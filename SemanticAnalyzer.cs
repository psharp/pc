/// <summary>
/// Semantic analyzer for Pascal programs.
/// Performs type checking, symbol resolution, and semantic validation on the AST.
/// Collects errors rather than throwing exceptions for better error reporting.
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;

namespace PascalCompiler;

/// <summary>
/// Performs semantic analysis on Pascal programs and units.
/// Validates types, resolves symbols, and checks semantic correctness.
/// </summary>
public class SemanticAnalyzer
{
    /// <summary>Symbol table mapping variable names to types.</summary>
    private readonly Dictionary<string, string> _symbolTable = new();

    /// <summary>Maps array variable names to their type information.</summary>
    private readonly Dictionary<string, ArrayTypeNode> _arrayTypes = new();

    /// <summary>Maps record variable names to their record type names.</summary>
    private readonly Dictionary<string, string> _recordVariables = new();

    /// <summary>Maps record type names to their definitions.</summary>
    private readonly Dictionary<string, RecordTypeNode> _recordTypeDefinitions = new();

    /// <summary>Maps enumeration type names to their definitions.</summary>
    private readonly Dictionary<string, EnumTypeNode> _enumTypeDefinitions = new();

    /// <summary>Maps enumeration value names to their enum type names.</summary>
    private readonly Dictionary<string, string> _enumValues = new();

    /// <summary>Maps set variable names to their element types.</summary>
    private readonly Dictionary<string, string> _setVariables = new();

    /// <summary>Maps file variable names to their declarations.</summary>
    private readonly Dictionary<string, FileVarDeclarationNode> _fileVariables = new();

    /// <summary>Maps pointer variable names to their pointed-to types.</summary>
    private readonly Dictionary<string, string> _pointerVariables = new();

    /// <summary>Maps procedure names to their declarations.</summary>
    private readonly Dictionary<string, ProcedureDeclarationNode> _procedures = new();

    /// <summary>Maps function names to their declarations.</summary>
    private readonly Dictionary<string, FunctionDeclarationNode> _functions = new();

    /// <summary>Built-in math functions with their return types.</summary>
    private readonly Dictionary<string, string> _builtInFunctions = new()
    {
        // Absolute value and squaring
        { "abs", "auto" },      // Returns same type as input (int or real)
        { "sqr", "auto" },      // Returns same type as input (int or real)
        { "sqrt", "real" },     // Always returns real

        // Trigonometric functions
        { "sin", "real" },
        { "cos", "real" },
        { "arctan", "real" },

        // Logarithmic and exponential
        { "ln", "real" },
        { "exp", "real" },

        // Rounding functions
        { "trunc", "integer" },  // Truncate to integer
        { "round", "integer" },  // Round to nearest integer

        // Boolean function
        { "odd", "boolean" },    // Check if odd

        // String functions
        { "length", "integer" },   // Length of string
        { "copy", "string" },      // Copy substring (3 params: str, start, count)
        { "concat", "string" },    // Concatenate strings (2+ params)
        { "pos", "integer" },      // Find substring position (2 params)
        { "upcase", "string" },    // Convert to uppercase
        { "lowercase", "string" }, // Convert to lowercase
        { "chr", "string" },       // Convert integer to character
        { "ord", "integer" }       // Convert character to integer (ASCII value)
    };

    /// <summary>List of semantic errors found during analysis.</summary>
    private readonly List<string> _errors = new();

    /// <summary>Gets the list of semantic errors.</summary>
    public List<string> Errors => _errors;

    public void Analyze(UnitNode unit)
    {
        // Register interface and implementation types and symbols
        foreach (var recordType in unit.InterfaceRecordTypes)
        {
            string typeName = recordType.Name.ToLower();
            if (!_recordTypeDefinitions.ContainsKey(typeName))
            {
                _recordTypeDefinitions[typeName] = recordType;
            }
        }

        foreach (var recordType in unit.ImplementationRecordTypes)
        {
            string typeName = recordType.Name.ToLower();
            if (!_recordTypeDefinitions.ContainsKey(typeName))
            {
                _recordTypeDefinitions[typeName] = recordType;
            }
        }

        foreach (var enumType in unit.InterfaceEnumTypes)
        {
            string typeName = enumType.Name.ToLower();
            if (!_enumTypeDefinitions.ContainsKey(typeName))
            {
                _enumTypeDefinitions[typeName] = enumType;
                foreach (var value in enumType.Values)
                {
                    _enumValues[value.ToLower()] = typeName;
                }
            }
        }

        // Register procedures and functions from implementation
        foreach (var proc in unit.ImplementationProcedures)
        {
            RegisterProcedure(proc);
        }

        foreach (var func in unit.ImplementationFunctions)
        {
            RegisterFunction(func);
        }

        // Analyze initialization block if present
        if (unit.InitializationBlock != null)
        {
            AnalyzeBlock(unit.InitializationBlock);
        }

        // Analyze finalization block if present
        if (unit.FinalizationBlock != null)
        {
            AnalyzeBlock(unit.FinalizationBlock);
        }
    }

    public void Analyze(ProgramNode program, UnitLoader? unitLoader = null)
    {
        // Load and register units first
        if (unitLoader != null && program.UsedUnits.Count > 0)
        {
            foreach (var unitName in program.UsedUnits)
            {
                var unit = unitLoader.LoadUnit(unitName);
                LoadUnitSymbols(unit);
            }
        }

        // Register record type definitions
        foreach (var recordType in program.RecordTypes)
        {
            string typeName = recordType.Name.ToLower();
            if (_recordTypeDefinitions.ContainsKey(typeName))
            {
                _errors.Add($"Record type '{recordType.Name}' is already declared");
            }
            else
            {
                _recordTypeDefinitions[typeName] = recordType;
            }
        }

        // Register enum type definitions
        foreach (var enumType in program.EnumTypes)
        {
            string typeName = enumType.Name.ToLower();
            if (_enumTypeDefinitions.ContainsKey(typeName))
            {
                _errors.Add($"Enum type '{enumType.Name}' is already declared");
            }
            else
            {
                _enumTypeDefinitions[typeName] = enumType;
                // Register each enum value
                foreach (var value in enumType.Values)
                {
                    string valueName = value.ToLower();
                    if (_enumValues.ContainsKey(valueName))
                    {
                        _errors.Add($"Enum value '{value}' is already declared in another enum type");
                    }
                    else
                    {
                        _enumValues[valueName] = typeName;
                    }
                }
            }
        }

        // Register global variables
        foreach (var varDecl in program.Variables)
        {
            foreach (var name in varDecl.Names)
            {
                if (_symbolTable.ContainsKey(name.ToLower()))
                {
                    _errors.Add($"Variable '{name}' is already declared");
                }
                else
                {
                    _symbolTable[name.ToLower()] = varDecl.Type;
                }
            }
        }

        // Register array variables
        foreach (var arrayDecl in program.ArrayVariables)
        {
            foreach (var name in arrayDecl.Names)
            {
                if (_symbolTable.ContainsKey(name.ToLower()) || _arrayTypes.ContainsKey(name.ToLower()))
                {
                    _errors.Add($"Array '{name}' is already declared");
                }
                else
                {
                    _arrayTypes[name.ToLower()] = arrayDecl.ArrayType;
                    _symbolTable[name.ToLower()] = $"array[{arrayDecl.ArrayType.LowerBound}..{arrayDecl.ArrayType.UpperBound}] of {arrayDecl.ArrayType.ElementType}";
                }
            }
        }

        // Register record variables
        foreach (var recordDecl in program.RecordVariables)
        {
            foreach (var name in recordDecl.Names)
            {
                if (_symbolTable.ContainsKey(name.ToLower()))
                {
                    _errors.Add($"Record variable '{name}' is already declared");
                }
                else
                {
                    // Verify the record type exists
                    if (!_recordTypeDefinitions.ContainsKey(recordDecl.RecordTypeName.ToLower()))
                    {
                        _errors.Add($"Unknown record type '{recordDecl.RecordTypeName}'");
                    }
                    else
                    {
                        _recordVariables[name.ToLower()] = recordDecl.RecordTypeName.ToLower();
                        _symbolTable[name.ToLower()] = recordDecl.RecordTypeName;
                    }
                }
            }
        }

        // Register file variables
        foreach (var fileDecl in program.FileVariables)
        {
            foreach (var name in fileDecl.Names)
            {
                if (_symbolTable.ContainsKey(name.ToLower()))
                {
                    _errors.Add($"File variable '{name}' is already declared");
                }
                else
                {
                    _fileVariables[name.ToLower()] = fileDecl;
                    string fileType = fileDecl.IsTextFile ? "text" : $"file of {fileDecl.ElementType}";
                    _symbolTable[name.ToLower()] = fileType;
                }
            }
        }

        // Register pointer variables
        foreach (var ptrDecl in program.PointerVariables)
        {
            foreach (var name in ptrDecl.Names)
            {
                if (_symbolTable.ContainsKey(name.ToLower()))
                {
                    _errors.Add($"Pointer variable '{name}' is already declared");
                }
                else
                {
                    _pointerVariables[name.ToLower()] = ptrDecl.PointedType.ToLower();
                    _symbolTable[name.ToLower()] = "^" + ptrDecl.PointedType;
                }
            }
        }

        // Register set variables
        foreach (var setDecl in program.SetVariables)
        {
            foreach (var name in setDecl.Names)
            {
                if (_symbolTable.ContainsKey(name.ToLower()))
                {
                    _errors.Add($"Set variable '{name}' is already declared");
                }
                else
                {
                    // Verify the element type exists (basic type or enum type)
                    string elementType = setDecl.ElementType.ToLower();
                    bool isValidType = elementType == "integer" || elementType == "real" ||
                                       elementType == "string" || elementType == "boolean" ||
                                       _enumTypeDefinitions.ContainsKey(elementType);

                    if (!isValidType)
                    {
                        _errors.Add($"Unknown type '{setDecl.ElementType}' for set variable '{name}'");
                    }
                    else
                    {
                        _setVariables[name.ToLower()] = elementType;
                        _symbolTable[name.ToLower()] = "set of " + setDecl.ElementType;
                    }
                }
            }
        }

        // Register procedures (including nested ones)
        foreach (var proc in program.Procedures)
        {
            RegisterProcedure(proc);
        }

        // Register functions (including nested ones)
        foreach (var func in program.Functions)
        {
            RegisterFunction(func);
        }

        // Analyze procedure bodies
        foreach (var proc in program.Procedures)
        {
            AnalyzeProcedure(proc);
        }

        // Analyze function bodies
        foreach (var func in program.Functions)
        {
            AnalyzeFunction(func);
        }

        // Analyze main program block
        AnalyzeBlock(program.Block);
    }

    private void RegisterProcedure(ProcedureDeclarationNode procedure)
    {
        string procName = procedure.Name.ToLower();
        if (_procedures.ContainsKey(procName) || _functions.ContainsKey(procName))
        {
            _errors.Add($"Procedure '{procedure.Name}' is already declared");
        }
        else
        {
            _procedures[procName] = procedure;
        }

        // Recursively register nested procedures
        foreach (var nestedProc in procedure.NestedProcedures)
        {
            RegisterProcedure(nestedProc);
        }

        // Recursively register nested functions
        foreach (var nestedFunc in procedure.NestedFunctions)
        {
            RegisterFunction(nestedFunc);
        }
    }

    private void RegisterFunction(FunctionDeclarationNode function)
    {
        string funcName = function.Name.ToLower();
        if (_functions.ContainsKey(funcName) || _procedures.ContainsKey(funcName))
        {
            _errors.Add($"Function '{function.Name}' is already declared");
        }
        else
        {
            _functions[funcName] = function;
        }

        // Recursively register nested procedures
        foreach (var nestedProc in function.NestedProcedures)
        {
            RegisterProcedure(nestedProc);
        }

        // Recursively register nested functions
        foreach (var nestedFunc in function.NestedFunctions)
        {
            RegisterFunction(nestedFunc);
        }
    }

    private void AnalyzeBlock(BlockNode block)
    {
        foreach (var statement in block.Statements)
        {
            AnalyzeStatement(statement);
        }
    }

    private void AnalyzeStatement(StatementNode statement)
    {
        switch (statement)
        {
            case AssignmentNode assignment:
                if (!_symbolTable.ContainsKey(assignment.Variable.ToLower()))
                {
                    _errors.Add($"Variable '{assignment.Variable}' is not declared");
                }
                else
                {
                    AnalyzeExpression(assignment.Expression);
                    // Type check assignment
                    string varType = _symbolTable[assignment.Variable.ToLower()];
                    string exprType = GetExpressionType(assignment.Expression);
                    if (!AreTypesCompatible(varType, exprType))
                    {
                        _errors.Add($"Type mismatch in assignment to '{assignment.Variable}': cannot assign {exprType} to {varType}");
                    }
                }
                break;

            case ProcedureCallNode procCall:
                string procName = procCall.Name.ToLower();
                if (!_procedures.ContainsKey(procName))
                {
                    _errors.Add($"Procedure '{procCall.Name}' is not declared");
                }
                else
                {
                    var proc = _procedures[procName];
                    int expectedParamCount = 0;
                    foreach (var param in proc.Parameters)
                    {
                        expectedParamCount += param.Names.Count;
                    }
                    if (procCall.Arguments.Count != expectedParamCount)
                    {
                        _errors.Add($"Procedure '{procCall.Name}' expects {expectedParamCount} arguments but got {procCall.Arguments.Count}");
                    }
                }
                foreach (var arg in procCall.Arguments)
                {
                    AnalyzeExpression(arg);
                }
                break;

            case IfNode ifNode:
                AnalyzeExpression(ifNode.Condition);
                // Type check: if condition must be boolean
                string ifCondType = GetExpressionType(ifNode.Condition);
                if (ifCondType != "boolean" && ifCondType != "unknown")
                {
                    _errors.Add($"If condition must be boolean, got {ifCondType}");
                }
                AnalyzeStatement(ifNode.ThenBranch);
                if (ifNode.ElseBranch != null)
                {
                    AnalyzeStatement(ifNode.ElseBranch);
                }
                break;

            case WhileNode whileNode:
                AnalyzeExpression(whileNode.Condition);
                // Type check: while condition must be boolean
                string whileCondType = GetExpressionType(whileNode.Condition);
                if (whileCondType != "boolean" && whileCondType != "unknown")
                {
                    _errors.Add($"While condition must be boolean, got {whileCondType}");
                }
                AnalyzeStatement(whileNode.Body);
                break;

            case RepeatUntilNode repeatUntilNode:
                foreach (var stmt in repeatUntilNode.Statements)
                {
                    AnalyzeStatement(stmt);
                }
                AnalyzeExpression(repeatUntilNode.Condition);
                // Type check: repeat-until condition must be boolean
                string repeatCondType = GetExpressionType(repeatUntilNode.Condition);
                if (repeatCondType != "boolean" && repeatCondType != "unknown")
                {
                    _errors.Add($"Repeat-until condition must be boolean, got {repeatCondType}");
                }
                break;

            case WithNode withNode:
                // Check that the with variable is a record
                string withVarName = withNode.RecordVariable.ToLower();
                if (!_symbolTable.ContainsKey(withVarName))
                {
                    _errors.Add($"Record variable '{withNode.RecordVariable}' is not declared");
                }
                AnalyzeStatement(withNode.Statement);
                break;

            case GotoNode gotoNode:
                // No type checking needed for goto
                break;

            case LabeledStatementNode labeledStmt:
                AnalyzeStatement(labeledStmt.Statement);
                break;

            case ForNode forNode:
                if (!_symbolTable.ContainsKey(forNode.Variable.ToLower()))
                {
                    _errors.Add($"Variable '{forNode.Variable}' is not declared");
                }
                else
                {
                    // Type check: for loop variable must be integer
                    string loopVarType = _symbolTable[forNode.Variable.ToLower()];
                    if (loopVarType != "integer" && loopVarType != "unknown")
                    {
                        _errors.Add($"For loop variable must be integer, got {loopVarType}");
                    }
                }
                AnalyzeExpression(forNode.Start);
                AnalyzeExpression(forNode.End);
                // Type check: for loop bounds must be integer
                string startType = GetExpressionType(forNode.Start);
                string endType = GetExpressionType(forNode.End);
                if (startType != "integer" && startType != "unknown")
                {
                    _errors.Add($"For loop start value must be integer, got {startType}");
                }
                if (endType != "integer" && endType != "unknown")
                {
                    _errors.Add($"For loop end value must be integer, got {endType}");
                }
                AnalyzeStatement(forNode.Body);
                break;

            case WriteNode writeNode:
                foreach (var expr in writeNode.Expressions)
                {
                    AnalyzeExpression(expr);
                }
                break;

            case ReadNode readNode:
                foreach (var variable in readNode.Variables)
                {
                    if (!_symbolTable.ContainsKey(variable.ToLower()))
                    {
                        _errors.Add($"Variable '{variable}' is not declared");
                    }
                }
                break;

            case CompoundStatementNode compound:
                foreach (var stmt in compound.Statements)
                {
                    AnalyzeStatement(stmt);
                }
                break;

            case ArrayAssignmentNode arrayAssignment:
                if (!_arrayTypes.ContainsKey(arrayAssignment.ArrayName.ToLower()))
                {
                    _errors.Add($"Array '{arrayAssignment.ArrayName}' is not declared");
                }
                else
                {
                    AnalyzeExpression(arrayAssignment.Index);
                    AnalyzeExpression(arrayAssignment.Value);
                    // Type check array index
                    string indexType = GetExpressionType(arrayAssignment.Index);
                    if (indexType != "integer" && indexType != "unknown")
                    {
                        _errors.Add($"Array index must be integer, got {indexType}");
                    }
                    // Type check array element assignment
                    string elementType = GetArrayElementType(arrayAssignment.ArrayName);
                    string valueType = GetExpressionType(arrayAssignment.Value);
                    if (!AreTypesCompatible(elementType, valueType))
                    {
                        _errors.Add($"Type mismatch in array assignment: cannot assign {valueType} to array of {elementType}");
                    }
                }
                break;

            case RecordAssignmentNode recordAssignment:
                string recordName = recordAssignment.RecordName.ToLower();
                if (!_recordVariables.ContainsKey(recordName))
                {
                    _errors.Add($"Record variable '{recordAssignment.RecordName}' is not declared");
                }
                else
                {
                    // Verify the field exists in the record type
                    string recordTypeName = _recordVariables[recordName];
                    var recordType = _recordTypeDefinitions[recordTypeName];
                    bool fieldFound = false;
                    string fieldType = "unknown";
                    foreach (var field in recordType.Fields)
                    {
                        if (field.Names.Any(n => n.Equals(recordAssignment.FieldName, System.StringComparison.OrdinalIgnoreCase)))
                        {
                            fieldFound = true;
                            fieldType = field.Type;
                            break;
                        }
                    }
                    if (!fieldFound)
                    {
                        _errors.Add($"Field '{recordAssignment.FieldName}' does not exist in record type '{recordType.Name}'");
                    }
                    else
                    {
                        AnalyzeExpression(recordAssignment.Value);
                        // Type check field assignment
                        string valueType = GetExpressionType(recordAssignment.Value);
                        if (!AreTypesCompatible(fieldType, valueType))
                        {
                            _errors.Add($"Type mismatch in record field assignment: cannot assign {valueType} to {fieldType}");
                        }
                    }
                }
                break;

            case FileAssignNode fileAssign:
                if (!_fileVariables.ContainsKey(fileAssign.FileVariable.ToLower()))
                {
                    _errors.Add($"File variable '{fileAssign.FileVariable}' is not declared");
                }
                AnalyzeExpression(fileAssign.FileName);
                break;

            case FileResetNode fileReset:
                if (!_fileVariables.ContainsKey(fileReset.FileVariable.ToLower()))
                {
                    _errors.Add($"File variable '{fileReset.FileVariable}' is not declared");
                }
                break;

            case FileRewriteNode fileRewrite:
                if (!_fileVariables.ContainsKey(fileRewrite.FileVariable.ToLower()))
                {
                    _errors.Add($"File variable '{fileRewrite.FileVariable}' is not declared");
                }
                break;

            case FileCloseNode fileClose:
                if (!_fileVariables.ContainsKey(fileClose.FileVariable.ToLower()))
                {
                    _errors.Add($"File variable '{fileClose.FileVariable}' is not declared");
                }
                break;

            case PageNode pageNode:
                // ISO 7185: page procedure - optional file parameter
                if (pageNode.FileVariable != null && !_fileVariables.ContainsKey(pageNode.FileVariable.ToLower()))
                {
                    _errors.Add($"File variable '{pageNode.FileVariable}' is not declared");
                }
                break;

            case GetNode getNode:
                // ISO 7185: get procedure
                if (!_fileVariables.ContainsKey(getNode.FileVariable.ToLower()))
                {
                    _errors.Add($"File variable '{getNode.FileVariable}' is not declared");
                }
                break;

            case PutNode putNode:
                // ISO 7185: put procedure
                if (!_fileVariables.ContainsKey(putNode.FileVariable.ToLower()))
                {
                    _errors.Add($"File variable '{putNode.FileVariable}' is not declared");
                }
                break;

            case PackNode packNode:
                // ISO 7185: pack procedure - check arrays exist
                AnalyzeExpression(packNode.StartIndex);
                break;

            case UnpackNode unpackNode:
                // ISO 7185: unpack procedure - check arrays exist
                AnalyzeExpression(unpackNode.StartIndex);
                break;

            case FileReadNode fileRead:
                if (!_fileVariables.ContainsKey(fileRead.FileVariable.ToLower()))
                {
                    _errors.Add($"File variable '{fileRead.FileVariable}' is not declared");
                }
                foreach (var variable in fileRead.Variables)
                {
                    if (!_symbolTable.ContainsKey(variable.ToLower()))
                    {
                        _errors.Add($"Variable '{variable}' is not declared");
                    }
                }
                break;

            case FileWriteNode fileWrite:
                if (!_fileVariables.ContainsKey(fileWrite.FileVariable.ToLower()))
                {
                    _errors.Add($"File variable '{fileWrite.FileVariable}' is not declared");
                }
                foreach (var expr in fileWrite.Expressions)
                {
                    AnalyzeExpression(expr);
                }
                break;

            case ArrayRecordAssignmentNode arrayRecAssignment:
                if (!_arrayTypes.ContainsKey(arrayRecAssignment.ArrayName.ToLower()))
                {
                    _errors.Add($"Array '{arrayRecAssignment.ArrayName}' is not declared");
                }
                AnalyzeExpression(arrayRecAssignment.Index);
                AnalyzeExpression(arrayRecAssignment.Value);
                break;

            case NewNode newNode:
                string newVar = newNode.PointerVariable.ToLower();
                if (!_pointerVariables.ContainsKey(newVar))
                {
                    _errors.Add($"Variable '{newNode.PointerVariable}' is not a pointer");
                }
                break;

            case DisposeNode disposeNode:
                string disposeVar = disposeNode.PointerVariable.ToLower();
                if (!_pointerVariables.ContainsKey(disposeVar))
                {
                    _errors.Add($"Variable '{disposeNode.PointerVariable}' is not a pointer");
                }
                break;

            case PointerAssignmentNode ptrAssign:
                AnalyzeExpression(ptrAssign.Pointer);
                AnalyzeExpression(ptrAssign.Value);
                break;
        }
    }

    private void AnalyzeExpression(ExpressionNode expression)
    {
        switch (expression)
        {
            case BinaryOpNode binary:
                AnalyzeExpression(binary.Left);
                AnalyzeExpression(binary.Right);
                // Type check binary operations
                CheckBinaryOperation(binary);
                break;

            case UnaryOpNode unary:
                AnalyzeExpression(unary.Operand);
                // Type check unary operations
                CheckUnaryOperation(unary);
                break;

            case FunctionCallNode funcCall:
                string funcName = funcCall.Name.ToLower();

                // Check if it's a built-in function
                if (_builtInFunctions.ContainsKey(funcName))
                {
                    // Validate argument count for built-in functions
                    int expectedCount = GetBuiltInFunctionParamCount(funcName);
                    if (expectedCount >= 0 && funcCall.Arguments.Count != expectedCount)
                    {
                        _errors.Add($"Built-in function '{funcCall.Name}' expects {expectedCount} argument(s) but got {funcCall.Arguments.Count}");
                    }
                    else if (expectedCount == -1 && funcCall.Arguments.Count < 2)
                    {
                        // Variable argument count (concat) requires at least 2
                        _errors.Add($"Built-in function '{funcCall.Name}' requires at least 2 arguments but got {funcCall.Arguments.Count}");
                    }
                }
                else if (!_functions.ContainsKey(funcName))
                {
                    _errors.Add($"Function '{funcCall.Name}' is not declared");
                }
                else
                {
                    var func = _functions[funcName];
                    int expectedParamCount = 0;
                    foreach (var param in func.Parameters)
                    {
                        expectedParamCount += param.Names.Count;
                    }
                    if (funcCall.Arguments.Count != expectedParamCount)
                    {
                        _errors.Add($"Function '{funcCall.Name}' expects {expectedParamCount} arguments but got {funcCall.Arguments.Count}");
                    }
                }
                foreach (var arg in funcCall.Arguments)
                {
                    AnalyzeExpression(arg);
                }
                break;

            case VariableNode variable:
                string varLower = variable.Name.ToLower();
                // Check if it's a variable or enum value
                if (!_symbolTable.ContainsKey(varLower) && !_enumValues.ContainsKey(varLower))
                {
                    _errors.Add($"Variable or enum value '{variable.Name}' is not declared");
                }
                break;

            case ArrayAccessNode arrayAccess:
                if (!_arrayTypes.ContainsKey(arrayAccess.ArrayName.ToLower()))
                {
                    _errors.Add($"Array '{arrayAccess.ArrayName}' is not declared");
                }
                AnalyzeExpression(arrayAccess.Index);
                break;

            case RecordAccessNode recordAccess:
                string recName = recordAccess.RecordName.ToLower();
                if (!_recordVariables.ContainsKey(recName))
                {
                    _errors.Add($"Record variable '{recordAccess.RecordName}' is not declared");
                }
                else
                {
                    // Verify the field exists in the record type
                    string recTypeName = _recordVariables[recName];
                    var recType = _recordTypeDefinitions[recTypeName];
                    bool fieldFound = false;
                    foreach (var field in recType.Fields)
                    {
                        if (field.Names.Any(n => n.Equals(recordAccess.FieldName, System.StringComparison.OrdinalIgnoreCase)))
                        {
                            fieldFound = true;
                            break;
                        }
                    }
                    if (!fieldFound)
                    {
                        _errors.Add($"Field '{recordAccess.FieldName}' does not exist in record type '{recType.Name}'");
                    }
                }
                break;

            case FileEofNode fileEof:
                if (!_fileVariables.ContainsKey(fileEof.FileVariable.ToLower()))
                {
                    _errors.Add($"File variable '{fileEof.FileVariable}' is not declared");
                }
                break;

            case ArrayRecordAccessNode arrayRecAccess:
                if (!_arrayTypes.ContainsKey(arrayRecAccess.ArrayName.ToLower()))
                {
                    _errors.Add($"Array '{arrayRecAccess.ArrayName}' is not declared");
                }
                AnalyzeExpression(arrayRecAccess.Index);
                break;

            case NilNode:
                // Nil is always valid
                break;

            case PointerDereferenceNode ptrDeref:
                AnalyzeExpression(ptrDeref.Pointer);
                break;

            case AddressOfNode addressOf:
                if (!_symbolTable.ContainsKey(addressOf.VariableName.ToLower()))
                {
                    _errors.Add($"Variable '{addressOf.VariableName}' is not declared");
                }
                break;

            case SetLiteralNode setLiteral:
                // Analyze each element in the set
                foreach (var element in setLiteral.Elements)
                {
                    AnalyzeExpression(element);
                }
                break;

            case InNode inNode:
                // Analyze both the value and the set expression
                AnalyzeExpression(inNode.Value);
                AnalyzeExpression(inNode.SetExpression);
                // Type check: the set expression must be a set type
                string setType = GetExpressionType(inNode.SetExpression);
                if (!setType.StartsWith("set of") && setType != "unknown")
                {
                    _errors.Add($"'in' operator requires a set on the right side, got {setType}");
                }
                break;
        }
    }

    private void AnalyzeProcedure(ProcedureDeclarationNode procedure)
    {
        var savedSymbols = new Dictionary<string, string>(_symbolTable);
        var localNames = new HashSet<string>();

        // Add parameters to local scope
        foreach (var param in procedure.Parameters)
        {
            foreach (var name in param.Names)
            {
                localNames.Add(name.ToLower());
                _symbolTable[name.ToLower()] = param.Type;
            }
        }

        // Add local variables to scope (can shadow globals, but not parameters/other locals)
        foreach (var varDecl in procedure.LocalVariables)
        {
            foreach (var name in varDecl.Names)
            {
                if (localNames.Contains(name.ToLower()))
                {
                    _errors.Add($"Variable '{name}' is already declared in procedure '{procedure.Name}'");
                }
                else
                {
                    localNames.Add(name.ToLower());
                    _symbolTable[name.ToLower()] = varDecl.Type;
                }
            }
        }

        // Analyze nested procedures
        foreach (var nestedProc in procedure.NestedProcedures)
        {
            AnalyzeProcedure(nestedProc);
        }

        // Analyze nested functions
        foreach (var nestedFunc in procedure.NestedFunctions)
        {
            AnalyzeFunction(nestedFunc);
        }

        // Analyze procedure body
        AnalyzeBlock(procedure.Block);

        // Restore symbol table
        _symbolTable.Clear();
        foreach (var kvp in savedSymbols)
        {
            _symbolTable[kvp.Key] = kvp.Value;
        }
    }

    private void AnalyzeFunction(FunctionDeclarationNode function)
    {
        var savedSymbols = new Dictionary<string, string>(_symbolTable);
        var localNames = new HashSet<string>();

        // Add function name as a variable (for return value assignment)
        localNames.Add(function.Name.ToLower());
        _symbolTable[function.Name.ToLower()] = function.ReturnType;

        // Add parameters to local scope
        foreach (var param in function.Parameters)
        {
            foreach (var name in param.Names)
            {
                localNames.Add(name.ToLower());
                _symbolTable[name.ToLower()] = param.Type;
            }
        }

        // Add local variables to scope (can shadow globals, but not parameters/other locals/function name)
        foreach (var varDecl in function.LocalVariables)
        {
            foreach (var name in varDecl.Names)
            {
                if (localNames.Contains(name.ToLower()))
                {
                    _errors.Add($"Variable '{name}' is already declared in function '{function.Name}'");
                }
                else
                {
                    localNames.Add(name.ToLower());
                    _symbolTable[name.ToLower()] = varDecl.Type;
                }
            }
        }

        // Analyze nested procedures
        foreach (var nestedProc in function.NestedProcedures)
        {
            AnalyzeProcedure(nestedProc);
        }

        // Analyze nested functions
        foreach (var nestedFunc in function.NestedFunctions)
        {
            AnalyzeFunction(nestedFunc);
        }

        // Analyze function body
        AnalyzeBlock(function.Block);

        // Restore symbol table
        _symbolTable.Clear();
        foreach (var kvp in savedSymbols)
        {
            _symbolTable[kvp.Key] = kvp.Value;
        }
    }

    // Type inference: determine the type of an expression
    private string GetExpressionType(ExpressionNode expression)
    {
        return expression switch
        {
            NumberNode number => number.Value is int ? "integer" : "real",
            StringNode => "string",
            BooleanNode => "boolean",
            NilNode => "nil",
            VariableNode variable => GetVariableOrEnumType(variable.Name),
            ArrayAccessNode arrayAccess => GetArrayElementType(arrayAccess.ArrayName),
            RecordAccessNode recordAccess => GetRecordFieldType(recordAccess.RecordName, recordAccess.FieldName),
            ArrayRecordAccessNode arrayRecAccess => GetRecordFieldType(GetArrayElementType(arrayRecAccess.ArrayName), arrayRecAccess.FieldName),
            FunctionCallNode funcCall => GetFunctionReturnType(funcCall.Name),
            FileEofNode => "boolean",
            PointerDereferenceNode ptrDeref => GetPointerDereferenceType(ptrDeref.Pointer),
            AddressOfNode addressOf => GetAddressOfType(addressOf.VariableName),
            BinaryOpNode binary => GetBinaryOpResultType(binary),
            UnaryOpNode unary => GetUnaryOpResultType(unary),
            SetLiteralNode setLiteral => GetSetLiteralType(setLiteral),
            InNode => "boolean", // 'in' operator always returns boolean
            _ => "unknown"
        };
    }

    private string GetVariableOrEnumType(string name)
    {
        string lowerName = name.ToLower();

        // Check if it's a variable
        if (_symbolTable.TryGetValue(lowerName, out var type))
        {
            return type;
        }

        // Check if it's an enum value
        if (_enumValues.TryGetValue(lowerName, out var enumType))
        {
            return enumType;
        }

        return "unknown";
    }

    private string GetSetLiteralType(SetLiteralNode setLiteral)
    {
        // Infer set type from elements
        if (setLiteral.Elements.Count > 0)
        {
            string elementType = GetExpressionType(setLiteral.Elements[0]);
            return "set of " + elementType;
        }
        return "set of unknown";
    }

    private string GetArrayElementType(string arrayName)
    {
        if (_arrayTypes.TryGetValue(arrayName.ToLower(), out var arrayType))
        {
            return arrayType.ElementType;
        }
        return "unknown";
    }

    private string GetRecordFieldType(string recordName, string fieldName)
    {
        if (_recordVariables.TryGetValue(recordName.ToLower(), out var recordTypeName))
        {
            if (_recordTypeDefinitions.TryGetValue(recordTypeName, out var recordType))
            {
                foreach (var field in recordType.Fields)
                {
                    if (field.Names.Any(n => n.Equals(fieldName, System.StringComparison.OrdinalIgnoreCase)))
                    {
                        return field.Type;
                    }
                }
            }
        }
        return "unknown";
    }

    private string GetFunctionReturnType(string functionName)
    {
        string lowerName = functionName.ToLower();

        // Check built-in functions first
        if (_builtInFunctions.TryGetValue(lowerName, out var returnType))
        {
            // "auto" means it returns the same type as its argument
            // For now, we'll assume numeric input (int or real)
            // The actual type will be determined at runtime
            if (returnType == "auto")
            {
                return "auto"; // Type checker will handle this
            }
            return returnType;
        }

        if (_functions.TryGetValue(lowerName, out var function))
        {
            return function.ReturnType;
        }
        return "unknown";
    }

    private string GetPointerDereferenceType(ExpressionNode ptrExpr)
    {
        string ptrType = GetExpressionType(ptrExpr);
        // If it's a pointer type (^type), return the pointed-to type
        if (ptrType.StartsWith("^"))
        {
            return ptrType.Substring(1);
        }
        return "unknown";
    }

    private string GetAddressOfType(string variableName)
    {
        // @variable returns a pointer to the variable's type
        if (_symbolTable.TryGetValue(variableName.ToLower(), out var varType))
        {
            return "^" + varType;
        }
        return "unknown";
    }

    private string GetBinaryOpResultType(BinaryOpNode binary)
    {
        var leftType = GetExpressionType(binary.Left);
        var rightType = GetExpressionType(binary.Right);

        return binary.Operator switch
        {
            // Comparison operators always return boolean
            TokenType.EQUALS or TokenType.NOT_EQUALS or TokenType.LESS_THAN or
            TokenType.GREATER_THAN or TokenType.LESS_EQUAL or TokenType.GREATER_EQUAL => "boolean",

            // Logical operators work on booleans and return boolean
            TokenType.AND or TokenType.OR => "boolean",

            // Arithmetic operators: if either operand is real, result is real
            TokenType.PLUS or TokenType.MINUS or TokenType.MULTIPLY or TokenType.DIVIDE =>
                (leftType == "real" || rightType == "real") ? "real" : "integer",

            // Integer division and mod always return integer
            TokenType.DIV or TokenType.MOD => "integer",

            _ => "unknown"
        };
    }

    private string GetUnaryOpResultType(UnaryOpNode unary)
    {
        var operandType = GetExpressionType(unary.Operand);

        return unary.Operator switch
        {
            TokenType.NOT => "boolean",
            TokenType.MINUS => operandType, // Unary minus preserves type
            _ => "unknown"
        };
    }

    // Type compatibility checking
    private bool AreTypesCompatible(string targetType, string sourceType)
    {
        // Case-insensitive comparison for type names
        if (targetType.Equals(sourceType, StringComparison.OrdinalIgnoreCase))
            return true;

        if (targetType == "unknown" || sourceType == "unknown")
            return true; // Don't report errors for unknown types (already reported elsewhere)

        // "auto" type from built-in functions is compatible with numeric types
        if (sourceType == "auto" && (targetType == "integer" || targetType == "real"))
            return true;
        if (targetType == "auto" && (sourceType == "integer" || sourceType == "real"))
            return true;

        // Integer can be assigned to real (implicit conversion)
        if (targetType == "real" && sourceType == "integer")
            return true;

        // Nil can be assigned to any pointer type
        if (targetType.StartsWith('^') && sourceType == "nil")
            return true;

        // Check if both are set types with compatible element types
        if (targetType.StartsWith("set of") && sourceType.StartsWith("set of"))
        {
            string targetElement = targetType.Substring(7).Trim();
            string sourceElement = sourceType.Substring(7).Trim();
            return targetElement.Equals(sourceElement, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    // Type checking for binary operations
    private void CheckBinaryOperation(BinaryOpNode binary)
    {
        var leftType = GetExpressionType(binary.Left);
        var rightType = GetExpressionType(binary.Right);

        switch (binary.Operator)
        {
            case TokenType.PLUS:
            case TokenType.MINUS:
            case TokenType.MULTIPLY:
            case TokenType.DIVIDE:
                // Arithmetic operators require numeric types
                if (!IsNumericType(leftType))
                    _errors.Add($"Left operand of arithmetic operation must be numeric, got {leftType}");
                if (!IsNumericType(rightType))
                    _errors.Add($"Right operand of arithmetic operation must be numeric, got {rightType}");
                break;

            case TokenType.DIV:
            case TokenType.MOD:
                // Integer division requires integer types
                if (leftType != "integer" && leftType != "unknown")
                    _errors.Add($"Left operand of {binary.Operator} must be integer, got {leftType}");
                if (rightType != "integer" && rightType != "unknown")
                    _errors.Add($"Right operand of {binary.Operator} must be integer, got {rightType}");
                break;

            case TokenType.AND:
            case TokenType.OR:
                // Logical operators require boolean types
                if (leftType != "boolean" && leftType != "unknown")
                    _errors.Add($"Left operand of {binary.Operator} must be boolean, got {leftType}");
                if (rightType != "boolean" && rightType != "unknown")
                    _errors.Add($"Right operand of {binary.Operator} must be boolean, got {rightType}");
                break;

            case TokenType.EQUALS:
            case TokenType.NOT_EQUALS:
                // Equality operators require compatible types
                if (!AreTypesCompatible(leftType, rightType) && !AreTypesCompatible(rightType, leftType))
                    _errors.Add($"Cannot compare {leftType} with {rightType}");
                break;

            case TokenType.LESS_THAN:
            case TokenType.GREATER_THAN:
            case TokenType.LESS_EQUAL:
            case TokenType.GREATER_EQUAL:
                // Relational operators require comparable types (numeric or string)
                if (!IsComparableType(leftType))
                    _errors.Add($"Left operand of comparison must be numeric or string, got {leftType}");
                if (!IsComparableType(rightType))
                    _errors.Add($"Right operand of comparison must be numeric or string, got {rightType}");
                if (leftType != rightType && leftType != "unknown" && rightType != "unknown")
                {
                    // Allow integer/real comparisons
                    if (!((IsNumericType(leftType) && IsNumericType(rightType))))
                        _errors.Add($"Cannot compare {leftType} with {rightType}");
                }
                break;
        }
    }

    // Type checking for unary operations
    private void CheckUnaryOperation(UnaryOpNode unary)
    {
        var operandType = GetExpressionType(unary.Operand);

        switch (unary.Operator)
        {
            case TokenType.NOT:
                if (operandType != "boolean" && operandType != "unknown")
                    _errors.Add($"NOT operator requires boolean operand, got {operandType}");
                break;

            case TokenType.MINUS:
                if (!IsNumericType(operandType) && operandType != "unknown")
                    _errors.Add($"Unary minus requires numeric operand, got {operandType}");
                break;
        }
    }

    private bool IsNumericType(string type)
    {
        return type == "integer" || type == "real" || type == "auto";
    }

    private bool IsComparableType(string type)
    {
        return IsNumericType(type) || type == "string";
    }

    private void LoadUnitSymbols(UnitNode unit)
    {
        // Register unit's record types
        foreach (var recordType in unit.InterfaceRecordTypes)
        {
            string typeName = recordType.Name.ToLower();
            if (!_recordTypeDefinitions.ContainsKey(typeName))
            {
                _recordTypeDefinitions[typeName] = recordType;
            }
        }

        // Register unit's enum types
        foreach (var enumType in unit.InterfaceEnumTypes)
        {
            string typeName = enumType.Name.ToLower();
            if (!_enumTypeDefinitions.ContainsKey(typeName))
            {
                _enumTypeDefinitions[typeName] = enumType;
                // Register enum values
                foreach (var value in enumType.Values)
                {
                    _enumValues[value.ToLower()] = typeName;
                }
            }
        }

        // Register unit's interface procedures (use implementation if available)
        var implProcedures = unit.ImplementationProcedures.ToDictionary(p => p.Name.ToLower());
        foreach (var proc in unit.InterfaceProcedures)
        {
            string procName = proc.Name.ToLower();
            if (implProcedures.TryGetValue(procName, out var implProc))
            {
                RegisterProcedure(implProc);
            }
            else
            {
                RegisterProcedure(proc);
            }
        }

        // Register unit's interface functions (use implementation if available)
        var implFunctions = unit.ImplementationFunctions.ToDictionary(f => f.Name.ToLower());
        foreach (var func in unit.InterfaceFunctions)
        {
            string funcName = func.Name.ToLower();
            if (implFunctions.TryGetValue(funcName, out var implFunc))
            {
                RegisterFunction(implFunc);
            }
            else
            {
                RegisterFunction(func);
            }
        }
    }

    private int GetBuiltInFunctionParamCount(string funcName)
    {
        return funcName switch
        {
            // Single parameter functions
            "abs" or "sqr" or "sqrt" or "sin" or "cos" or "arctan" or
            "ln" or "exp" or "trunc" or "round" or "odd" or
            "length" or "upcase" or "lowercase" or "chr" or "ord" => 1,

            // Two parameter functions
            "pos" => 2,

            // Three parameter functions
            "copy" => 3,

            // Variable parameter functions (concat - 2 or more)
            "concat" => -1,

            _ => 1  // Default to 1 for safety
        };
    }
}
