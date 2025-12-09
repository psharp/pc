/// <summary>
/// Compiles Pascal AST to bytecode.
/// Translates high-level Pascal constructs into stack-based bytecode instructions.
/// </summary>
using System;
using System.Collections.Generic;

namespace PascalCompiler;

/// <summary>
/// Bytecode compiler for Pascal programs.
/// Converts AST nodes to executable bytecode instructions for the virtual machine.
/// </summary>
public class BytecodeCompiler
{
    /// <summary>The bytecode program being built.</summary>
    private BytecodeProgram _program = null!;

    /// <summary>Counter for generating unique labels.</summary>
    private int _labelCounter;

    /// <summary>Stack of break target labels for loop compilation.</summary>
    private readonly Stack<string> _breakLabels = new();

    /// <summary>Stack of continue target labels for loop compilation.</summary>
    private readonly Stack<string> _continueLabels = new();

    /// <summary>
    /// Compiles a Pascal program to bytecode.
    /// </summary>
    /// <param name="program">The program AST to compile.</param>
    /// <returns>A BytecodeProgram ready for execution.</returns>
    public BytecodeProgram Compile(ProgramNode program)
    {
        _program = new BytecodeProgram(program.Name);
        _labelCounter = 0;

        // Store used units
        _program.UsedUnits.AddRange(program.UsedUnits);

        // Register enum types
        foreach (var enumType in program.EnumTypes)
        {
            var enumInfo = new EnumInfo(enumType.Name, enumType.Values);
            _program.EnumTypes[enumType.Name.ToLower()] = enumInfo;
        }

        // Register global variables
        foreach (var varDecl in program.Variables)
        {
            foreach (var name in varDecl.Names)
            {
                _program.AddVariable(name.ToLower());
            }
        }

        // Register pointer variables
        foreach (var ptrDecl in program.PointerVariables)
        {
            foreach (var name in ptrDecl.Names)
            {
                _program.AddVariable(name.ToLower());
            }
        }

        // Register file variables
        foreach (var fileDecl in program.FileVariables)
        {
            foreach (var name in fileDecl.Names)
            {
                _program.AddVariable(name.ToLower());
            }
        }

        // Register set variables
        foreach (var setDecl in program.SetVariables)
        {
            foreach (var name in setDecl.Names)
            {
                _program.AddVariable(name.ToLower());
            }
        }

        // Register array variables and metadata
        foreach (var arrayDecl in program.ArrayVariables)
        {
            foreach (var name in arrayDecl.Names)
            {
                string arrayName = name.ToLower();
                _program.AddVariable(arrayName);

                // Store array metadata
                var arrayInfo = new ArrayInfo(
                    name,
                    arrayDecl.ArrayType.Dimensions,
                    arrayDecl.ArrayType.ElementType
                );
                _program.Arrays[arrayName] = arrayInfo;
            }
        }

        // Compile procedures
        foreach (var proc in program.Procedures)
        {
            CompileProcedure(proc);
        }

        // Compile functions
        foreach (var func in program.Functions)
        {
            CompileFunction(func);
        }

        // Compile main program
        CompileBlock(program.Block);
        _program.AddInstruction(new Instruction(OpCode.HALT));

        return _program;
    }

    public BytecodeUnit CompileUnit(UnitNode unit)
    {
        var bytecodeUnit = new BytecodeUnit(unit.Name);
        _labelCounter = 0;

        // Store used units
        bytecodeUnit.UsedUnits.AddRange(unit.UsedUnits);

        // Register interface enum types
        foreach (var enumType in unit.InterfaceEnumTypes)
        {
            var enumInfo = new EnumInfo(enumType.Name, enumType.Values);
            bytecodeUnit.EnumTypes[enumType.Name.ToLower()] = enumInfo;
        }

        // Register implementation enum types
        foreach (var enumType in unit.ImplementationEnumTypes)
        {
            var enumInfo = new EnumInfo(enumType.Name, enumType.Values);
            bytecodeUnit.EnumTypes[enumType.Name.ToLower()] = enumInfo;
        }

        // Register interface record types
        foreach (var recordType in unit.InterfaceRecordTypes)
        {
            bytecodeUnit.RecordTypes[recordType.Name.ToLower()] = recordType;
        }

        // Register implementation record types
        foreach (var recordType in unit.ImplementationRecordTypes)
        {
            bytecodeUnit.RecordTypes[recordType.Name.ToLower()] = recordType;
        }

        // Register interface variables
        foreach (var varDecl in unit.InterfaceVariables)
        {
            foreach (var name in varDecl.Names)
            {
                bytecodeUnit.AddVariable(name.ToLower());
            }
        }

        // Register implementation variables
        foreach (var varDecl in unit.ImplementationVariables)
        {
            foreach (var name in varDecl.Names)
            {
                bytecodeUnit.AddVariable(name.ToLower());
            }
        }

        // Register implementation array variables
        foreach (var arrayDecl in unit.ImplementationArrayVariables)
        {
            foreach (var name in arrayDecl.Names)
            {
                string arrayName = name.ToLower();
                bytecodeUnit.AddVariable(arrayName);

                var arrayInfo = new ArrayInfo(
                    name,
                    arrayDecl.ArrayType.Dimensions,
                    arrayDecl.ArrayType.ElementType
                );
                bytecodeUnit.Arrays[arrayName] = arrayInfo;
            }
        }

        // Register implementation pointer, file, and set variables
        foreach (var ptrDecl in unit.ImplementationPointerVariables)
        {
            foreach (var name in ptrDecl.Names)
            {
                bytecodeUnit.AddVariable(name.ToLower());
            }
        }

        foreach (var fileDecl in unit.ImplementationFileVariables)
        {
            foreach (var name in fileDecl.Names)
            {
                bytecodeUnit.AddVariable(name.ToLower());
            }
        }

        foreach (var setDecl in unit.ImplementationSetVariables)
        {
            foreach (var name in setDecl.Names)
            {
                bytecodeUnit.AddVariable(name.ToLower());
            }
        }

        // Create a temporary BytecodeProgram to reuse the existing compilation methods
        _program = new BytecodeProgram(unit.Name);
        _program.Variables.AddRange(bytecodeUnit.Variables);
        foreach (var kvp in bytecodeUnit.EnumTypes)
        {
            _program.EnumTypes[kvp.Key] = kvp.Value;
        }
        foreach (var kvp in bytecodeUnit.Arrays)
        {
            _program.Arrays[kvp.Key] = kvp.Value;
        }

        // Compile implementation procedures (these include both interface and implementation-only)
        foreach (var proc in unit.ImplementationProcedures)
        {
            CompileProcedure(proc);
        }

        // Compile implementation functions
        foreach (var func in unit.ImplementationFunctions)
        {
            CompileFunction(func);
        }

        // Copy compiled functions to bytecode unit
        foreach (var kvp in _program.Functions)
        {
            bytecodeUnit.Functions[kvp.Key] = kvp.Value;
        }

        // Copy all compiled function bytecode
        bytecodeUnit.Instructions.AddRange(_program.Instructions);

        // Clear for initialization block
        int functionCodeLength = _program.Instructions.Count;
        _program.Instructions.Clear();

        // Compile initialization block if present
        if (unit.InitializationBlock != null)
        {
            CompileBlock(unit.InitializationBlock);
            bytecodeUnit.InitializationCode.AddRange(_program.Instructions);
            _program.Instructions.Clear();
        }

        // Compile finalization block if present
        if (unit.FinalizationBlock != null)
        {
            CompileBlock(unit.FinalizationBlock);
            foreach (var instruction in _program.Instructions)
            {
                bytecodeUnit.AddFinalizationInstruction(instruction);
            }
            _program.Instructions.Clear();
        }

        return bytecodeUnit;
    }

    private void CompileProcedure(ProcedureDeclarationNode procedure)
    {
        string funcLabel = $"proc_{procedure.Name.ToLower()}";
        _program.AddLabel(funcLabel);

        int paramCount = 0;
        var paramNames = new List<string>();
        var paramIsVar = new List<bool>();
        foreach (var param in procedure.Parameters)
        {
            foreach (var name in param.Names)
            {
                paramNames.Add(name.ToLower());
                paramIsVar.Add(param.IsVar);
                paramCount++;
            }
        }

        _program.Functions[procedure.Name.ToLower()] = new FunctionInfo(
            procedure.Name,
            _program.GetCurrentAddress(),
            paramCount,
            paramNames,
            null,
            paramIsVar
        );

        // Add parameters and local variables
        var savedVars = new List<string>(_program.Variables);

        foreach (var param in procedure.Parameters)
        {
            foreach (var name in param.Names)
            {
                _program.AddVariable(name.ToLower());
            }
        }

        foreach (var varDecl in procedure.LocalVariables)
        {
            foreach (var name in varDecl.Names)
            {
                _program.AddVariable(name.ToLower());
            }
        }

        // Compile nested procedures
        foreach (var nestedProc in procedure.NestedProcedures)
        {
            CompileProcedure(nestedProc);
        }

        // Compile nested functions
        foreach (var nestedFunc in procedure.NestedFunctions)
        {
            CompileFunction(nestedFunc);
        }

        CompileBlock(procedure.Block);
        _program.AddInstruction(new Instruction(OpCode.RETURN));

        // Restore variable list
        _program.Variables.Clear();
        _program.Variables.AddRange(savedVars);
    }

    private void CompileFunction(FunctionDeclarationNode function)
    {
        string funcLabel = $"func_{function.Name.ToLower()}";
        _program.AddLabel(funcLabel);

        int paramCount = 0;
        var paramNames = new List<string>();
        var paramIsVar = new List<bool>();
        foreach (var param in function.Parameters)
        {
            foreach (var name in param.Names)
            {
                paramNames.Add(name.ToLower());
                paramIsVar.Add(param.IsVar);
                paramCount++;
            }
        }

        _program.Functions[function.Name.ToLower()] = new FunctionInfo(
            function.Name,
            _program.GetCurrentAddress(),
            paramCount,
            paramNames,
            function.ReturnType,
            paramIsVar
        );

        // Add function name as variable for return value
        var savedVars = new List<string>(_program.Variables);
        _program.AddVariable(function.Name.ToLower());

        foreach (var param in function.Parameters)
        {
            foreach (var name in param.Names)
            {
                _program.AddVariable(name.ToLower());
            }
        }

        foreach (var varDecl in function.LocalVariables)
        {
            foreach (var name in varDecl.Names)
            {
                _program.AddVariable(name.ToLower());
            }
        }

        // Compile nested procedures
        foreach (var nestedProc in function.NestedProcedures)
        {
            CompileProcedure(nestedProc);
        }

        // Compile nested functions
        foreach (var nestedFunc in function.NestedFunctions)
        {
            CompileFunction(nestedFunc);
        }

        CompileBlock(function.Block);

        // Load return value (function name variable)
        _program.AddInstruction(new Instruction(OpCode.LOAD_VAR, function.Name.ToLower()));
        _program.AddInstruction(new Instruction(OpCode.RETURN));

        // Restore variable list
        _program.Variables.Clear();
        _program.Variables.AddRange(savedVars);
    }

    private void CompileBlock(BlockNode block)
    {
        foreach (var statement in block.Statements)
        {
            CompileStatement(statement);
        }
    }

    private void CompileStatement(StatementNode statement)
    {
        switch (statement)
        {
            case AssignmentNode assignment:
                CompileExpression(assignment.Expression);
                _program.AddInstruction(new Instruction(OpCode.STORE_VAR, assignment.Variable.ToLower()));
                break;

            case ArrayAssignmentNode arrayAssignment:
                // Compile value to store
                CompileExpression(arrayAssignment.Value);

                // Compile all indices
                foreach (var indexExpr in arrayAssignment.Indices)
                {
                    CompileExpression(indexExpr);
                }

                // ARRAY_STORE instruction with array name and dimension count
                string arrayName = arrayAssignment.ArrayName.ToLower();
                _program.AddInstruction(new Instruction(OpCode.ARRAY_STORE, $"{arrayName}:{arrayAssignment.Indices.Count}"));
                break;

            case ProcedureCallNode procCall:
                // Look up procedure info to check which parameters are var
                string procName = procCall.Name.ToLower();
                if (_program.Functions.TryGetValue(procName, out var procInfo))
                {
                    // Push arguments
                    for (int i = 0; i < procCall.Arguments.Count; i++)
                    {
                        if (i < procInfo.ParameterIsVar.Count && procInfo.ParameterIsVar[i])
                        {
                            // For var parameters, push variable index
                            if (procCall.Arguments[i] is VariableNode varNode)
                            {
                                int varIndex = _program.GetVariableIndex(varNode.Name.ToLower());
                                _program.AddInstruction(new Instruction(OpCode.PUSH, varIndex));
                            }
                            else
                            {
                                throw new Exception($"Var parameter requires a variable argument");
                            }
                        }
                        else
                        {
                            // For value parameters, push the evaluated value
                            CompileExpression(procCall.Arguments[i]);
                        }
                    }
                }
                else
                {
                    // If procedure not found yet (forward reference), assume all value params
                    foreach (var arg in procCall.Arguments)
                    {
                        CompileExpression(arg);
                    }
                }
                _program.AddInstruction(new Instruction(OpCode.CALL, procCall.Name.ToLower()));
                break;

            case IfNode ifNode:
                CompileIfStatement(ifNode);
                break;

            case WhileNode whileNode:
                CompileWhileStatement(whileNode);
                break;

            case ForNode forNode:
                CompileForStatement(forNode);
                break;

            case WriteNode writeNode:
                foreach (var expr in writeNode.Expressions)
                {
                    CompileExpression(expr);
                    _program.AddInstruction(new Instruction(OpCode.WRITE));
                }
                if (writeNode.NewLine)
                {
                    _program.AddInstruction(new Instruction(OpCode.PUSH, "\n"));
                    _program.AddInstruction(new Instruction(OpCode.WRITE));
                }
                break;

            case ReadNode readNode:
                foreach (var variable in readNode.Variables)
                {
                    _program.AddInstruction(new Instruction(OpCode.READ, variable.ToLower()));
                }
                break;

            case CompoundStatementNode compound:
                foreach (var stmt in compound.Statements)
                {
                    CompileStatement(stmt);
                }
                break;

            case NewNode newNode:
                _program.AddInstruction(new Instruction(OpCode.NEW, newNode.PointerVariable.ToLower()));
                break;

            case DisposeNode disposeNode:
                _program.AddInstruction(new Instruction(OpCode.DISPOSE, disposeNode.PointerVariable.ToLower()));
                break;

            case PointerAssignmentNode ptrAssign:
                CompileExpression(ptrAssign.Pointer);  // Get pointer address
                CompileExpression(ptrAssign.Value);    // Get value to store
                _program.AddInstruction(new Instruction(OpCode.STORE_DEREF));
                break;

            case FileAssignNode fileAssign:
                CompileExpression(fileAssign.FileName);  // Push filename onto stack
                _program.AddInstruction(new Instruction(OpCode.FILE_ASSIGN, fileAssign.FileVariable.ToLower()));
                break;

            case FileResetNode fileReset:
                _program.AddInstruction(new Instruction(OpCode.FILE_RESET, fileReset.FileVariable.ToLower()));
                break;

            case FileRewriteNode fileRewrite:
                _program.AddInstruction(new Instruction(OpCode.FILE_REWRITE, fileRewrite.FileVariable.ToLower()));
                break;

            case FileCloseNode fileClose:
                _program.AddInstruction(new Instruction(OpCode.FILE_CLOSE, fileClose.FileVariable.ToLower()));
                break;

            case FileReadNode fileRead:
                // Handle multiple variables - for now, just handle the first one
                foreach (var varName in fileRead.Variables)
                {
                    _program.AddInstruction(new Instruction(OpCode.FILE_READ,
                        new object[] { fileRead.FileVariable.ToLower(), varName.ToLower(), fileRead.ReadLine }));
                }
                break;

            case FileWriteNode fileWrite:
                foreach (var expr in fileWrite.Expressions)
                {
                    CompileExpression(expr);
                }
                _program.AddInstruction(new Instruction(OpCode.FILE_WRITE,
                    new object[] { fileWrite.FileVariable.ToLower(), fileWrite.Expressions.Count, fileWrite.WriteLine }));
                break;
        }
    }

    private void CompileIfStatement(IfNode ifNode)
    {
        string elseLabel = GenerateLabel("else");
        string endLabel = GenerateLabel("endif");

        CompileExpression(ifNode.Condition);
        _program.AddInstruction(new Instruction(OpCode.JUMP_IF_FALSE, elseLabel));

        CompileStatement(ifNode.ThenBranch);
        _program.AddInstruction(new Instruction(OpCode.JUMP, endLabel));

        _program.AddLabel(elseLabel);
        if (ifNode.ElseBranch != null)
        {
            CompileStatement(ifNode.ElseBranch);
        }

        _program.AddLabel(endLabel);
    }

    private void CompileWhileStatement(WhileNode whileNode)
    {
        string startLabel = GenerateLabel("while_start");
        string endLabel = GenerateLabel("while_end");

        _program.AddLabel(startLabel);
        CompileExpression(whileNode.Condition);
        _program.AddInstruction(new Instruction(OpCode.JUMP_IF_FALSE, endLabel));

        CompileStatement(whileNode.Body);
        _program.AddInstruction(new Instruction(OpCode.JUMP, startLabel));

        _program.AddLabel(endLabel);
    }

    private void CompileForStatement(ForNode forNode)
    {
        string startLabel = GenerateLabel("for_start");
        string endLabel = GenerateLabel("for_end");

        // Initialize loop variable
        CompileExpression(forNode.Start);
        _program.AddInstruction(new Instruction(OpCode.STORE_VAR, forNode.Variable.ToLower()));

        _program.AddLabel(startLabel);

        // Check condition
        _program.AddInstruction(new Instruction(OpCode.LOAD_VAR, forNode.Variable.ToLower()));
        CompileExpression(forNode.End);

        if (forNode.IsDownTo)
        {
            _program.AddInstruction(new Instruction(OpCode.LT)); // var < end
        }
        else
        {
            _program.AddInstruction(new Instruction(OpCode.GT)); // var > end
        }
        _program.AddInstruction(new Instruction(OpCode.JUMP_IF_TRUE, endLabel));

        // Execute body
        CompileStatement(forNode.Body);

        // Increment/Decrement
        _program.AddInstruction(new Instruction(OpCode.LOAD_VAR, forNode.Variable.ToLower()));
        _program.AddInstruction(new Instruction(OpCode.PUSH, 1));
        if (forNode.IsDownTo)
        {
            _program.AddInstruction(new Instruction(OpCode.SUB));
        }
        else
        {
            _program.AddInstruction(new Instruction(OpCode.ADD));
        }
        _program.AddInstruction(new Instruction(OpCode.STORE_VAR, forNode.Variable.ToLower()));

        _program.AddInstruction(new Instruction(OpCode.JUMP, startLabel));
        _program.AddLabel(endLabel);
    }

    private void CompileExpression(ExpressionNode expression)
    {
        switch (expression)
        {
            case NumberNode number:
                _program.AddInstruction(new Instruction(OpCode.PUSH, number.Value));
                break;

            case StringNode str:
                _program.AddInstruction(new Instruction(OpCode.PUSH, str.Value));
                break;

            case BooleanNode boolean:
                _program.AddInstruction(new Instruction(OpCode.PUSH, boolean.Value));
                break;

            case VariableNode variable:
                // Check if it's an enum value
                int? enumOrdinal = GetEnumValueOrdinal(variable.Name);
                if (enumOrdinal.HasValue)
                {
                    // It's an enum value - push its ordinal
                    _program.AddInstruction(new Instruction(OpCode.PUSH, enumOrdinal.Value));
                }
                else
                {
                    // It's a variable
                    _program.AddInstruction(new Instruction(OpCode.LOAD_VAR, variable.Name.ToLower()));
                }
                break;

            case ArrayAccessNode arrayAccess:
                // Compile all indices
                foreach (var indexExpr in arrayAccess.Indices)
                {
                    CompileExpression(indexExpr);
                }

                // ARRAY_LOAD instruction with array name and dimension count
                string arrName = arrayAccess.ArrayName.ToLower();
                _program.AddInstruction(new Instruction(OpCode.ARRAY_LOAD, $"{arrName}:{arrayAccess.Indices.Count}"));
                break;

            case FunctionCallNode funcCall:
                // Look up function info to check which parameters are var
                string funcName = funcCall.Name.ToLower();
                if (_program.Functions.TryGetValue(funcName, out var funcInfo))
                {
                    // Push arguments
                    for (int i = 0; i < funcCall.Arguments.Count; i++)
                    {
                        if (i < funcInfo.ParameterIsVar.Count && funcInfo.ParameterIsVar[i])
                        {
                            // For var parameters, push variable index
                            if (funcCall.Arguments[i] is VariableNode varNode)
                            {
                                int varIndex = _program.GetVariableIndex(varNode.Name.ToLower());
                                _program.AddInstruction(new Instruction(OpCode.PUSH, varIndex));
                            }
                            else
                            {
                                throw new Exception($"Var parameter requires a variable argument");
                            }
                        }
                        else
                        {
                            // For value parameters, push the evaluated value
                            CompileExpression(funcCall.Arguments[i]);
                        }
                    }
                }
                else
                {
                    // If function not found yet (forward reference), assume all value params
                    foreach (var arg in funcCall.Arguments)
                    {
                        CompileExpression(arg);
                    }
                }
                _program.AddInstruction(new Instruction(OpCode.CALL, funcCall.Name.ToLower()));
                break;

            case BinaryOpNode binary:
                CompileExpression(binary.Left);
                CompileExpression(binary.Right);
                CompileBinaryOperator(binary.Operator);
                break;

            case UnaryOpNode unary:
                CompileExpression(unary.Operand);
                CompileUnaryOperator(unary.Operator);
                break;

            case NilNode:
                _program.AddInstruction(new Instruction(OpCode.PUSH_NIL));
                break;

            case PointerDereferenceNode ptrDeref:
                CompileExpression(ptrDeref.Pointer);  // Get pointer address
                _program.AddInstruction(new Instruction(OpCode.DEREF));
                break;

            case AddressOfNode addressOf:
                _program.AddInstruction(new Instruction(OpCode.ADDR_OF, addressOf.VariableName.ToLower()));
                break;

            case FileEofNode fileEof:
                _program.AddInstruction(new Instruction(OpCode.FILE_EOF, fileEof.FileVariable.ToLower()));
                break;

            case SetLiteralNode setLiteral:
                // Compile each element expression
                foreach (var element in setLiteral.Elements)
                {
                    CompileExpression(element);
                }
                // Create set from N elements on stack
                _program.AddInstruction(new Instruction(OpCode.SET_LITERAL, setLiteral.Elements.Count));
                break;

            case InNode inNode:
                // Compile value and set expression
                CompileExpression(inNode.Value);
                CompileExpression(inNode.SetExpression);
                // Check membership
                _program.AddInstruction(new Instruction(OpCode.SET_CONTAINS));
                break;
        }
    }

    private void CompileBinaryOperator(TokenType op)
    {
        switch (op)
        {
            case TokenType.PLUS:
                _program.AddInstruction(new Instruction(OpCode.ADD));
                break;
            case TokenType.MINUS:
                _program.AddInstruction(new Instruction(OpCode.SUB));
                break;
            case TokenType.MULTIPLY:
                _program.AddInstruction(new Instruction(OpCode.MUL));
                break;
            case TokenType.DIVIDE:
                _program.AddInstruction(new Instruction(OpCode.DIV));
                break;
            case TokenType.DIV:
                _program.AddInstruction(new Instruction(OpCode.IDIV));
                break;
            case TokenType.MOD:
                _program.AddInstruction(new Instruction(OpCode.MOD));
                break;
            case TokenType.EQUALS:
                _program.AddInstruction(new Instruction(OpCode.EQ));
                break;
            case TokenType.NOT_EQUALS:
                _program.AddInstruction(new Instruction(OpCode.NE));
                break;
            case TokenType.LESS_THAN:
                _program.AddInstruction(new Instruction(OpCode.LT));
                break;
            case TokenType.GREATER_THAN:
                _program.AddInstruction(new Instruction(OpCode.GT));
                break;
            case TokenType.LESS_EQUAL:
                _program.AddInstruction(new Instruction(OpCode.LE));
                break;
            case TokenType.GREATER_EQUAL:
                _program.AddInstruction(new Instruction(OpCode.GE));
                break;
            case TokenType.AND:
                _program.AddInstruction(new Instruction(OpCode.AND));
                break;
            case TokenType.OR:
                _program.AddInstruction(new Instruction(OpCode.OR));
                break;
        }
    }

    private void CompileUnaryOperator(TokenType op)
    {
        switch (op)
        {
            case TokenType.MINUS:
                _program.AddInstruction(new Instruction(OpCode.NEG));
                break;
            case TokenType.NOT:
                _program.AddInstruction(new Instruction(OpCode.NOT));
                break;
            case TokenType.PLUS:
                // Unary plus is a no-op
                break;
        }
    }

    private string GenerateLabel(string prefix)
    {
        return $"{prefix}_{_labelCounter++}";
    }

    private int? GetEnumValueOrdinal(string name)
    {
        foreach (var enumType in _program.EnumTypes.Values)
        {
            for (int i = 0; i < enumType.Values.Count; i++)
            {
                if (enumType.Values[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
        }
        return null;
    }
}
