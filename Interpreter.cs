/// <summary>
/// Direct interpreter for Pascal programs.
/// Executes programs by walking the AST and evaluating nodes directly.
/// Supports all Pascal features including units, closures, pointers, files, and sets.
/// </summary>
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PascalCompiler;

/// <summary>
/// Tree-walking interpreter for Pascal programs.
/// Provides direct execution of AST nodes without compilation.
/// </summary>
public class Interpreter
{
    /// <summary>Maps variable names to their runtime values.</summary>
    private readonly Dictionary<string, object?> _variables = new();

    /// <summary>Maps array names to their element storage (linear index → value).</summary>
    private readonly Dictionary<string, Dictionary<int, object?>> _arrays = new();

    /// <summary>Maps array names to their type metadata (dimensions, bounds).</summary>
    private readonly Dictionary<string, ArrayTypeNode> _arrayTypes = new();

    /// <summary>Maps record variable names to their field storage (field name → value).</summary>
    private readonly Dictionary<string, Dictionary<string, object?>> _records = new();

    /// <summary>Maps record type names to their definitions.</summary>
    private readonly Dictionary<string, RecordTypeNode> _recordTypes = new();

    /// <summary>Maps enumeration type names to their definitions.</summary>
    private readonly Dictionary<string, EnumTypeNode> _enumTypes = new();

    /// <summary>Maps enumeration value names to their ordinal numbers.</summary>
    private readonly Dictionary<string, int> _enumValues = new();

    /// <summary>Maps set variable names to their element storage.</summary>
    private readonly Dictionary<string, HashSet<object>> _sets = new();

    /// <summary>Maps procedure names to their declarations.</summary>
    private readonly Dictionary<string, ProcedureDeclarationNode> _procedures = new();

    /// <summary>Maps function names to their declarations.</summary>
    private readonly Dictionary<string, FunctionDeclarationNode> _functions = new();

    /// <summary>Maps file variable names to their output streams.</summary>
    private readonly Dictionary<string, StreamWriter> _fileWriters = new();

    /// <summary>Maps file variable names to their input streams.</summary>
    private readonly Dictionary<string, StreamReader> _fileReaders = new();

    /// <summary>Maps file variable names to their filenames.</summary>
    private readonly Dictionary<string, string> _fileNames = new();

    /// <summary>Simulated heap storage for pointer values (address → value).</summary>
    private readonly Dictionary<int, object?> _heap = new();

    /// <summary>Next available heap address for pointer allocation.</summary>
    private int _nextAddress = 1000;

    /// <summary>Scope chain for closures (outer to inner scopes).</summary>
    private readonly Stack<Dictionary<string, object?>> _scopeChain = new();

    public void Execute(ProgramNode program, UnitLoader? unitLoader = null)
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

        // Register record types
        foreach (var recordType in program.RecordTypes)
        {
            _recordTypes[recordType.Name.ToLower()] = recordType;
        }

        // Register enum types
        foreach (var enumType in program.EnumTypes)
        {
            _enumTypes[enumType.Name.ToLower()] = enumType;
            // Register enum values with their ordinal positions
            for (int i = 0; i < enumType.Values.Count; i++)
            {
                _enumValues[enumType.Values[i].ToLower()] = i;
            }
        }

        // Initialize global variables
        foreach (var varDecl in program.Variables)
        {
            foreach (var name in varDecl.Names)
            {
                _variables[name.ToLower()] = GetDefaultValue(varDecl.Type);
            }
        }

        // Initialize arrays
        foreach (var arrayDecl in program.ArrayVariables)
        {
            foreach (var name in arrayDecl.Names)
            {
                var arrayData = new Dictionary<int, object?>();
                string elemType = arrayDecl.ArrayType.ElementType;

                // Store array type metadata
                _arrayTypes[name.ToLower()] = arrayDecl.ArrayType;

                // Calculate total size for multidimensional arrays
                int totalSize = 1;
                foreach (var dim in arrayDecl.ArrayType.Dimensions)
                {
                    totalSize *= (dim.UpperBound - dim.LowerBound + 1);
                }

                // Check if element type is a record
                if (_recordTypes.ContainsKey(elemType.ToLower()))
                {
                    // Initialize array of records
                    for (int i = 0; i < totalSize; i++)
                    {
                        var recordData = new Dictionary<string, object?>();
                        var recordType = _recordTypes[elemType.ToLower()];
                        foreach (var field in recordType.Fields)
                        {
                            foreach (var fieldName in field.Names)
                            {
                                recordData[fieldName.ToLower()] = GetDefaultValue(field.Type);
                            }
                        }
                        arrayData[i] = recordData;
                    }
                }
                else
                {
                    // Initialize array of basic types
                    for (int i = 0; i < totalSize; i++)
                    {
                        arrayData[i] = GetDefaultValue(elemType);
                    }
                }
                _arrays[name.ToLower()] = arrayData;
            }
        }

        // Initialize records
        foreach (var recordDecl in program.RecordVariables)
        {
            foreach (var name in recordDecl.Names)
            {
                var recordData = new Dictionary<string, object?>();
                var recordType = _recordTypes[recordDecl.RecordTypeName.ToLower()];
                // Initialize all fields to default values
                foreach (var field in recordType.Fields)
                {
                    foreach (var fieldName in field.Names)
                    {
                        recordData[fieldName.ToLower()] = GetDefaultValue(field.Type);
                    }
                }
                _records[name.ToLower()] = recordData;
            }
        }

        // Initialize file variables (track them but don't add to _variables as they're not regular vars)
        if (program.FileVariables != null)
        {
            foreach (var fileDecl in program.FileVariables)
            {
                foreach (var name in fileDecl.Names)
                {
                    _fileNames[name.ToLower()] = ""; // Empty string until Assign is called
                    // File variables don't have values, so we add a placeholder
                    _variables[name.ToLower()] = null;
                }
            }
        }

        // Initialize pointer variables to nil (null)
        if (program.PointerVariables != null)
        {
            foreach (var ptrDecl in program.PointerVariables)
            {
                foreach (var name in ptrDecl.Names)
                {
                    _variables[name.ToLower()] = null; // Pointers start as nil
                }
            }
        }

        // Initialize set variables
        if (program.SetVariables != null)
        {
            foreach (var setDecl in program.SetVariables)
            {
                foreach (var name in setDecl.Names)
                {
                    _sets[name.ToLower()] = new HashSet<object>(); // Empty set
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

        // Execute main program block
        ExecuteBlock(program.Block);

        // Clean up open files
        CleanupFiles();
    }

    private void CleanupFiles()
    {
        foreach (var writer in _fileWriters.Values)
        {
            try { writer?.Close(); } catch { /* Ignore cleanup errors */ }
        }
        foreach (var reader in _fileReaders.Values)
        {
            try { reader?.Close(); } catch { /* Ignore cleanup errors */ }
        }
        _fileWriters.Clear();
        _fileReaders.Clear();
    }

    private void RegisterProcedure(ProcedureDeclarationNode procedure)
    {
        _procedures[procedure.Name.ToLower()] = procedure;

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
        _functions[function.Name.ToLower()] = function;

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

    private object? GetDefaultValue(string type)
    {
        return type.ToLower() switch
        {
            "integer" => 0,
            "real" => 0.0,
            "string" => "",
            "boolean" => false,
            _ => null
        };
    }

    private void ExecuteBlock(BlockNode block)
    {
        foreach (var statement in block.Statements)
        {
            ExecuteStatement(statement);
        }
    }

    private void ExecuteStatement(StatementNode statement)
    {
        switch (statement)
        {
            case AssignmentNode assignment:
                object? value = EvaluateExpression(assignment.Expression);
                string assignVarName = assignment.Variable.ToLower();

                // Check if this is a set variable
                if (_sets.ContainsKey(assignVarName))
                {
                    // Assigning to a set variable
                    if (value is HashSet<object> setValue)
                    {
                        _sets[assignVarName] = setValue;
                    }
                    else
                    {
                        throw new Exception($"Cannot assign non-set value to set variable '{assignment.Variable}'");
                    }
                }
                else
                {
                    // Try to assign to a variable in the scope chain
                    bool assignedInScope = false;
                    foreach (var scope in _scopeChain)
                    {
                        if (scope.ContainsKey(assignVarName))
                        {
                            scope[assignVarName] = value;
                            assignedInScope = true;
                            break;
                        }
                    }

                    // If not found in scope chain, assign to global scope
                    if (!assignedInScope)
                    {
                        _variables[assignVarName] = value;
                    }
                }
                break;

            case ProcedureCallNode procCall:
                ExecuteProcedure(procCall.Name, procCall.Arguments);
                break;

            case IfNode ifNode:
                object? condition = EvaluateExpression(ifNode.Condition);
                if (IsTrue(condition))
                {
                    ExecuteStatement(ifNode.ThenBranch);
                }
                else if (ifNode.ElseBranch != null)
                {
                    ExecuteStatement(ifNode.ElseBranch);
                }
                break;

            case WhileNode whileNode:
                while (IsTrue(EvaluateExpression(whileNode.Condition)))
                {
                    ExecuteStatement(whileNode.Body);
                }
                break;

            case ForNode forNode:
                int start = Convert.ToInt32(EvaluateExpression(forNode.Start));
                int end = Convert.ToInt32(EvaluateExpression(forNode.End));
                string varName = forNode.Variable.ToLower();

                if (forNode.IsDownTo)
                {
                    for (int i = start; i >= end; i--)
                    {
                        _variables[varName] = i;
                        ExecuteStatement(forNode.Body);
                    }
                }
                else
                {
                    for (int i = start; i <= end; i++)
                    {
                        _variables[varName] = i;
                        ExecuteStatement(forNode.Body);
                    }
                }
                break;

            case WriteNode writeNode:
                foreach (var expr in writeNode.Expressions)
                {
                    object? val = EvaluateExpression(expr);
                    Console.Write(val);
                }
                if (writeNode.NewLine)
                {
                    Console.WriteLine();
                }
                break;

            case ReadNode readNode:
                foreach (var variable in readNode.Variables)
                {
                    string? input = Console.ReadLine();
                    string readVarName = variable.ToLower();

                    if (_variables.ContainsKey(readVarName))
                    {
                        object? defaultVal = _variables[readVarName];
                        _variables[readVarName] = ParseInput(input, defaultVal);
                    }
                }
                break;

            case CompoundStatementNode compound:
                foreach (var stmt in compound.Statements)
                {
                    ExecuteStatement(stmt);
                }
                break;

            case ArrayAssignmentNode arrayAssignment:
                object? arrayValue = EvaluateExpression(arrayAssignment.Value);
                string arrayName = arrayAssignment.ArrayName.ToLower();
                if (_arrays.ContainsKey(arrayName))
                {
                    // Evaluate all indices
                    var indices = new List<int>();
                    foreach (var indexExpr in arrayAssignment.Indices)
                    {
                        indices.Add(Convert.ToInt32(EvaluateExpression(indexExpr)));
                    }

                    // Get array type info
                    var arrayType = _arrayTypes[arrayName];

                    // Calculate linear index
                    int linearIndex = CalculateLinearIndex(arrayType, indices);

                    _arrays[arrayName][linearIndex] = arrayValue;
                }
                break;

            case RecordAssignmentNode recordAssignment:
                object? recordValue = EvaluateExpression(recordAssignment.Value);
                string recName = recordAssignment.RecordName.ToLower();
                string fieldName = recordAssignment.FieldName.ToLower();
                if (_records.ContainsKey(recName))
                {
                    _records[recName][fieldName] = recordValue;
                }
                break;

            case ArrayRecordAssignmentNode arrayRecAssignment:
                int arrIdx = Convert.ToInt32(EvaluateExpression(arrayRecAssignment.Index));
                object? arrRecValue = EvaluateExpression(arrayRecAssignment.Value);
                string arrName = arrayRecAssignment.ArrayName.ToLower();
                string fldName = arrayRecAssignment.FieldName.ToLower();
                if (_arrays.ContainsKey(arrName))
                {
                    var element = _arrays[arrName][arrIdx];
                    if (element is Dictionary<string, object?> recordDict)
                    {
                        recordDict[fldName] = arrRecValue;
                    }
                }
                break;

            case FileAssignNode fileAssign:
                string fileVar = fileAssign.FileVariable.ToLower();
                string fileName = Convert.ToString(EvaluateExpression(fileAssign.FileName)) ?? "";
                _fileNames[fileVar] = fileName;
                break;

            case FileResetNode fileReset:
                string resetFileVar = fileReset.FileVariable.ToLower();
                if (_fileNames.ContainsKey(resetFileVar) && !string.IsNullOrEmpty(_fileNames[resetFileVar]))
                {
                    _fileReaders[resetFileVar] = new StreamReader(_fileNames[resetFileVar]);
                }
                break;

            case FileRewriteNode fileRewrite:
                string rewriteFileVar = fileRewrite.FileVariable.ToLower();
                if (_fileNames.ContainsKey(rewriteFileVar) && !string.IsNullOrEmpty(_fileNames[rewriteFileVar]))
                {
                    _fileWriters[rewriteFileVar] = new StreamWriter(_fileNames[rewriteFileVar]);
                }
                break;

            case FileCloseNode fileClose:
                string closeFileVar = fileClose.FileVariable.ToLower();
                if (_fileWriters.ContainsKey(closeFileVar))
                {
                    _fileWriters[closeFileVar].Close();
                    _fileWriters.Remove(closeFileVar);
                }
                if (_fileReaders.ContainsKey(closeFileVar))
                {
                    _fileReaders[closeFileVar].Close();
                    _fileReaders.Remove(closeFileVar);
                }
                break;

            case FileReadNode fileRead:
                string readFileVar = fileRead.FileVariable.ToLower();
                if (_fileReaders.ContainsKey(readFileVar))
                {
                    var reader = _fileReaders[readFileVar];
                    foreach (var variable in fileRead.Variables)
                    {
                        string? line = reader.ReadLine();
                        string readVarName = variable.ToLower();
                        if (_variables.ContainsKey(readVarName))
                        {
                            object? defaultVal = _variables[readVarName];
                            _variables[readVarName] = ParseInput(line, defaultVal);
                        }
                    }
                }
                break;

            case FileWriteNode fileWrite:
                string writeFileVar = fileWrite.FileVariable.ToLower();
                if (_fileWriters.ContainsKey(writeFileVar))
                {
                    var writer = _fileWriters[writeFileVar];
                    foreach (var expr in fileWrite.Expressions)
                    {
                        object? val = EvaluateExpression(expr);
                        writer.Write(val);
                    }
                    if (fileWrite.WriteLine)
                    {
                        writer.WriteLine();
                    }
                }
                break;

            case NewNode newNode:
                string ptrVar = newNode.PointerVariable.ToLower();
                // Allocate memory by assigning a unique address
                int address = _nextAddress++;
                _heap[address] = 0; // Initialize with default value
                _variables[ptrVar] = address; // Store the address in the pointer variable
                break;

            case DisposeNode disposeNode:
                string disposeVar = disposeNode.PointerVariable.ToLower();
                if (_variables.ContainsKey(disposeVar) && _variables[disposeVar] is int addr)
                {
                    _heap.Remove(addr); // Free the memory
                    _variables[disposeVar] = null; // Set pointer to nil
                }
                break;

            case PointerAssignmentNode ptrAssign:
                // Get the pointer address
                object? ptrValue = EvaluateExpression(ptrAssign.Pointer);
                if (ptrValue is int ptrAddr)
                {
                    // Evaluate the value and store it at the pointed-to address
                    object? assignValue = EvaluateExpression(ptrAssign.Value);
                    _heap[ptrAddr] = assignValue;
                }
                break;
        }
    }

    private object? ParseInput(string? input, object? defaultValue)
    {
        if (string.IsNullOrEmpty(input))
        {
            return defaultValue;
        }

        return defaultValue switch
        {
            int => int.TryParse(input, out int i) ? i : defaultValue,
            double => double.TryParse(input, out double d) ? d : defaultValue,
            bool => bool.TryParse(input, out bool b) ? b : defaultValue,
            _ => input
        };
    }

    private bool IsTrue(object? value)
    {
        if (value is bool b)
        {
            return b;
        }
        if (value is int i)
        {
            return i != 0;
        }
        if (value is double d)
        {
            return d != 0.0;
        }
        return value != null;
    }

    private object? EvaluateExpression(ExpressionNode expression)
    {
        switch (expression)
        {
            case NumberNode number:
                return number.Value;

            case StringNode str:
                return str.Value;

            case BooleanNode boolean:
                return boolean.Value;

            case VariableNode variable:
                string varName = variable.Name.ToLower();
                // Check if it's a set variable
                if (_sets.TryGetValue(varName, out HashSet<object>? setData))
                {
                    return setData;
                }
                // Search for variable in scope chain (from innermost to outermost)
                foreach (var scope in _scopeChain)
                {
                    if (scope.TryGetValue(varName, out object? scopeValue))
                    {
                        return scopeValue;
                    }
                }
                // Check if it's a regular variable (global scope)
                if (_variables.TryGetValue(varName, out object? value))
                {
                    return value;
                }
                // Check if it's an enum value
                if (_enumValues.TryGetValue(varName, out int enumOrdinal))
                {
                    return enumOrdinal; // Return the ordinal value
                }
                throw new Exception($"Variable or enum value '{variable.Name}' not found");

            case FunctionCallNode funcCall:
                return ExecuteFunction(funcCall.Name, funcCall.Arguments);

            case BinaryOpNode binary:
                object? left = EvaluateExpression(binary.Left);
                object? right = EvaluateExpression(binary.Right);
                return EvaluateBinaryOp(left, binary.Operator, right);

            case UnaryOpNode unary:
                object? operand = EvaluateExpression(unary.Operand);
                return EvaluateUnaryOp(unary.Operator, operand);

            case ArrayAccessNode arrayAccess:
                string arrName = arrayAccess.ArrayName.ToLower();
                if (_arrays.TryGetValue(arrName, out var array))
                {
                    // Evaluate all indices
                    var indices = new List<int>();
                    foreach (var indexExpr in arrayAccess.Indices)
                    {
                        indices.Add(Convert.ToInt32(EvaluateExpression(indexExpr)));
                    }

                    // Get array type info
                    var arrayType = _arrayTypes[arrName];

                    // Calculate linear index
                    int linearIndex = CalculateLinearIndex(arrayType, indices);

                    if (array.TryGetValue(linearIndex, out object? arrValue))
                    {
                        return arrValue;
                    }
                    throw new Exception($"Array index out of bounds for array '{arrayAccess.ArrayName}'");
                }
                throw new Exception($"Array '{arrayAccess.ArrayName}' not found");

            case RecordAccessNode recordAccess:
                string recVarName = recordAccess.RecordName.ToLower();
                string fldName = recordAccess.FieldName.ToLower();
                if (_records.TryGetValue(recVarName, out var record))
                {
                    if (record.TryGetValue(fldName, out object? fldValue))
                    {
                        return fldValue;
                    }
                    throw new Exception($"Field '{recordAccess.FieldName}' not found in record '{recordAccess.RecordName}'");
                }
                throw new Exception($"Record variable '{recordAccess.RecordName}' not found");

            case ArrayRecordAccessNode arrayRecAccess:
                int arrayIdx = Convert.ToInt32(EvaluateExpression(arrayRecAccess.Index));
                string arrayName = arrayRecAccess.ArrayName.ToLower();
                string fieldName = arrayRecAccess.FieldName.ToLower();
                if (_arrays.TryGetValue(arrayName, out var arr))
                {
                    if (arr.TryGetValue(arrayIdx, out object? elem))
                    {
                        if (elem is Dictionary<string, object?> recDict)
                        {
                            if (recDict.TryGetValue(fieldName, out object? fieldVal))
                            {
                                return fieldVal;
                            }
                            throw new Exception($"Field '{arrayRecAccess.FieldName}' not found in array element");
                        }
                        throw new Exception($"Array element is not a record");
                    }
                    throw new Exception($"Array index {arrayIdx} out of bounds for array '{arrayRecAccess.ArrayName}'");
                }
                throw new Exception($"Array '{arrayRecAccess.ArrayName}' not found");

            case FileEofNode fileEof:
                string eofFileVar = fileEof.FileVariable.ToLower();
                if (_fileReaders.ContainsKey(eofFileVar))
                {
                    return _fileReaders[eofFileVar].EndOfStream;
                }
                return true; // If file not open, consider it at EOF

            case NilNode:
                return null; // Nil evaluates to null

            case PointerDereferenceNode ptrDeref:
                // Evaluate the pointer to get the address
                object? ptrAddr = EvaluateExpression(ptrDeref.Pointer);
                if (ptrAddr == null)
                {
                    throw new Exception("Cannot dereference nil pointer");
                }
                if (ptrAddr is int address && _heap.TryGetValue(address, out object? derefValue))
                {
                    return derefValue;
                }
                throw new Exception("Invalid pointer dereference");

            case AddressOfNode addressOf:
                // For simplicity, we'll create a pseudo-address for variables
                // This allows @variable to work without changing the variable storage model
                string addrVarName = addressOf.VariableName.ToLower();
                if (_variables.ContainsKey(addrVarName))
                {
                    // Return a special marker that indicates this is an address-of
                    // In a real implementation, this would need more sophisticated handling
                    int pseudoAddr = _nextAddress++;
                    _heap[pseudoAddr] = _variables[addrVarName];
                    return pseudoAddr;
                }
                throw new Exception($"Variable '{addressOf.VariableName}' not found for address-of operation");

            case SetLiteralNode setLiteral:
                // Evaluate a set literal like [Red, Blue, Green]
                var setElements = new HashSet<object>();
                foreach (var element in setLiteral.Elements)
                {
                    object? elemValue = EvaluateExpression(element);
                    if (elemValue != null)
                    {
                        setElements.Add(elemValue);
                    }
                }
                return setElements;

            case InNode inNode:
                // Evaluate 'value in setExpression'
                object? testValue = EvaluateExpression(inNode.Value);
                object? setExpr = EvaluateExpression(inNode.SetExpression);

                // setExpr should be a HashSet (from set literal or set variable)
                if (setExpr is HashSet<object> setCollection)
                {
                    return testValue != null && setCollection.Contains(testValue);
                }
                return false;

            default:
                throw new Exception($"Unknown expression type: {expression.GetType()}");
        }
    }

    private object? EvaluateBinaryOp(object? left, TokenType op, object? right)
    {
        switch (op)
        {
            case TokenType.PLUS:
                if (left is string || right is string)
                {
                    return left?.ToString() + right?.ToString();
                }
                return ToDouble(left) + ToDouble(right);

            case TokenType.MINUS:
                return ToDouble(left) - ToDouble(right);

            case TokenType.MULTIPLY:
                return ToDouble(left) * ToDouble(right);

            case TokenType.DIVIDE:
                return ToDouble(left) / ToDouble(right);

            case TokenType.DIV:
                return (int)(ToDouble(left) / ToDouble(right));

            case TokenType.MOD:
                return ToDouble(left) % ToDouble(right);

            case TokenType.EQUALS:
                return AreEqual(left, right);

            case TokenType.NOT_EQUALS:
                return !AreEqual(left, right);

            case TokenType.LESS_THAN:
                return ToDouble(left) < ToDouble(right);

            case TokenType.GREATER_THAN:
                return ToDouble(left) > ToDouble(right);

            case TokenType.LESS_EQUAL:
                return ToDouble(left) <= ToDouble(right);

            case TokenType.GREATER_EQUAL:
                return ToDouble(left) >= ToDouble(right);

            case TokenType.AND:
                return IsTrue(left) && IsTrue(right);

            case TokenType.OR:
                return IsTrue(left) || IsTrue(right);

            default:
                throw new Exception($"Unknown binary operator: {op}");
        }
    }

    private object? EvaluateUnaryOp(TokenType op, object? operand)
    {
        switch (op)
        {
            case TokenType.PLUS:
                return ToDouble(operand);

            case TokenType.MINUS:
                return -ToDouble(operand);

            case TokenType.NOT:
                return !IsTrue(operand);

            default:
                throw new Exception($"Unknown unary operator: {op}");
        }
    }

    private double ToDouble(object? value)
    {
        if (value == null) return 0;
        if (value is int i) return i;
        if (value is double d) return d;
        if (value is bool b) return b ? 1 : 0;
        if (double.TryParse(value.ToString(), out double result))
        {
            return result;
        }
        return 0;
    }

    private bool AreEqual(object? left, object? right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;

        if (left is string || right is string)
        {
            return left.ToString() == right.ToString();
        }

        return Math.Abs(ToDouble(left) - ToDouble(right)) < 0.0001;
    }

    private void ExecuteProcedure(string name, List<ExpressionNode> arguments)
    {
        string procName = name.ToLower();
        if (!_procedures.TryGetValue(procName, out var procedure))
        {
            throw new Exception($"Procedure '{name}' not found");
        }

        // Create a new local scope for this procedure
        var localScope = new Dictionary<string, object?>();

        // Track var parameter mappings (param name -> caller var name)
        var varParamMappings = new Dictionary<string, string>();

        // Bind arguments to parameters
        int argIndex = 0;
        foreach (var param in procedure.Parameters)
        {
            foreach (var paramName in param.Names)
            {
                if (argIndex < arguments.Count)
                {
                    if (param.IsVar)
                    {
                        // For var parameters, argument must be a variable
                        if (arguments[argIndex] is not VariableNode varNode)
                        {
                            throw new Exception($"Var parameter '{paramName}' requires a variable argument");
                        }

                        string callerVarName = varNode.Name.ToLower();
                        string localParamName = paramName.ToLower();

                        // Map the parameter to the caller's variable
                        varParamMappings[localParamName] = callerVarName;

                        // Get the initial value from caller's variable (search scope chain)
                        object? callerValue = null;
                        bool found = false;

                        // Search in scope chain
                        foreach (var scope in _scopeChain)
                        {
                            if (scope.TryGetValue(callerVarName, out callerValue))
                            {
                                found = true;
                                break;
                            }
                        }

                        // Search in global scope
                        if (!found && _variables.TryGetValue(callerVarName, out callerValue))
                        {
                            found = true;
                        }

                        if (!found)
                        {
                            throw new Exception($"Variable '{varNode.Name}' not found");
                        }

                        localScope[localParamName] = callerValue;
                    }
                    else
                    {
                        // For value parameters, evaluate and copy
                        object? argValue = EvaluateExpression(arguments[argIndex]);
                        localScope[paramName.ToLower()] = argValue;
                    }
                    argIndex++;
                }
            }
        }

        // Initialize local variables in the local scope
        foreach (var varDecl in procedure.LocalVariables)
        {
            foreach (var varName in varDecl.Names)
            {
                localScope[varName.ToLower()] = GetDefaultValue(varDecl.Type);
            }
        }

        // Push the local scope onto the scope chain
        _scopeChain.Push(localScope);

        try
        {
            // Execute procedure body
            ExecuteBlock(procedure.Block);
        }
        finally
        {
            // Pop the local scope from the scope chain
            _scopeChain.Pop();

            // Copy var parameter values back to caller's variables
            foreach (var mapping in varParamMappings)
            {
                string localParamName = mapping.Key;
                string callerVarName = mapping.Value;

                if (localScope.TryGetValue(localParamName, out var paramValue))
                {
                    // Update the variable in the appropriate scope
                    bool updated = false;
                    foreach (var scope in _scopeChain)
                    {
                        if (scope.ContainsKey(callerVarName))
                        {
                            scope[callerVarName] = paramValue;
                            updated = true;
                            break;
                        }
                    }

                    if (!updated)
                    {
                        _variables[callerVarName] = paramValue;
                    }
                }
            }
        }
    }

    private object? ExecuteFunction(string name, List<ExpressionNode> arguments)
    {
        string funcName = name.ToLower();
        if (!_functions.TryGetValue(funcName, out var function))
        {
            throw new Exception($"Function '{name}' not found");
        }

        // Create a new local scope for this function
        var localScope = new Dictionary<string, object?>();

        // Track var parameter mappings (param name -> caller var name)
        var varParamMappings = new Dictionary<string, string>();

        // Initialize function return variable in local scope
        localScope[funcName] = GetDefaultValue(function.ReturnType);

        // Bind arguments to parameters
        int argIndex = 0;
        foreach (var param in function.Parameters)
        {
            foreach (var paramName in param.Names)
            {
                if (argIndex < arguments.Count)
                {
                    if (param.IsVar)
                    {
                        // For var parameters, argument must be a variable
                        if (arguments[argIndex] is not VariableNode varNode)
                        {
                            throw new Exception($"Var parameter '{paramName}' requires a variable argument");
                        }

                        string callerVarName = varNode.Name.ToLower();
                        string localParamName = paramName.ToLower();

                        // Map the parameter to the caller's variable
                        varParamMappings[localParamName] = callerVarName;

                        // Get the initial value from caller's variable (search scope chain)
                        object? callerValue = null;
                        bool found = false;

                        // Search in scope chain
                        foreach (var scope in _scopeChain)
                        {
                            if (scope.TryGetValue(callerVarName, out callerValue))
                            {
                                found = true;
                                break;
                            }
                        }

                        // Search in global scope
                        if (!found && _variables.TryGetValue(callerVarName, out callerValue))
                        {
                            found = true;
                        }

                        if (!found)
                        {
                            throw new Exception($"Variable '{varNode.Name}' not found");
                        }

                        localScope[localParamName] = callerValue;
                    }
                    else
                    {
                        // For value parameters, evaluate and copy
                        object? argValue = EvaluateExpression(arguments[argIndex]);
                        localScope[paramName.ToLower()] = argValue;
                    }
                    argIndex++;
                }
            }
        }

        // Initialize local variables in the local scope
        foreach (var varDecl in function.LocalVariables)
        {
            foreach (var varName in varDecl.Names)
            {
                localScope[varName.ToLower()] = GetDefaultValue(varDecl.Type);
            }
        }

        // Push the local scope onto the scope chain
        _scopeChain.Push(localScope);

        object? returnValue;
        try
        {
            // Execute function body
            ExecuteBlock(function.Block);

            // Get return value (function name is used as return variable)
            returnValue = localScope[funcName];
        }
        finally
        {
            // Pop the local scope from the scope chain
            _scopeChain.Pop();

            // Copy var parameter values back to caller's variables
            foreach (var mapping in varParamMappings)
            {
                string localParamName = mapping.Key;
                string callerVarName = mapping.Value;

                if (localScope.TryGetValue(localParamName, out var paramValue))
                {
                    // Update the variable in the appropriate scope
                    bool updated = false;
                    foreach (var scope in _scopeChain)
                    {
                        if (scope.ContainsKey(callerVarName))
                        {
                            scope[callerVarName] = paramValue;
                            updated = true;
                            break;
                        }
                    }

                    if (!updated)
                    {
                        _variables[callerVarName] = paramValue;
                    }
                }
            }
        }

        return returnValue;
    }

    // Helper method to calculate linear index from multidimensional indices
    private int CalculateLinearIndex(ArrayTypeNode arrayType, List<int> indices)
    {
        if (indices.Count != arrayType.DimensionCount)
        {
            throw new Exception($"Array requires {arrayType.DimensionCount} indices but got {indices.Count}");
        }

        int linearIndex = 0;
        int multiplier = 1;

        // Calculate linear index using row-major order
        for (int d = arrayType.DimensionCount - 1; d >= 0; d--)
        {
            var dim = arrayType.Dimensions[d];
            int adjustedIndex = indices[d] - dim.LowerBound;

            if (indices[d] < dim.LowerBound || indices[d] > dim.UpperBound)
            {
                throw new Exception($"Array index {indices[d]} out of bounds for dimension {d + 1} [{dim.LowerBound}..{dim.UpperBound}]");
            }

            linearIndex += adjustedIndex * multiplier;
            multiplier *= (dim.UpperBound - dim.LowerBound + 1);
        }

        return linearIndex;
    }

    private void LoadUnitSymbols(UnitNode unit)
    {
        // Register unit's record types
        foreach (var recordType in unit.InterfaceRecordTypes)
        {
            _recordTypes[recordType.Name.ToLower()] = recordType;
        }

        // Register unit's enum types
        foreach (var enumType in unit.InterfaceEnumTypes)
        {
            _enumTypes[enumType.Name.ToLower()] = enumType;
            for (int i = 0; i < enumType.Values.Count; i++)
            {
                _enumValues[enumType.Values[i].ToLower()] = i;
            }
        }

        // Initialize unit's interface variables
        foreach (var varDecl in unit.InterfaceVariables)
        {
            foreach (var name in varDecl.Names)
            {
                _variables[name.ToLower()] = GetDefaultValue(varDecl.Type);
            }
        }

        // Register unit's interface procedures (use implementation if available)
        var implProcedures = unit.ImplementationProcedures.ToDictionary(p => p.Name.ToLower());
        foreach (var proc in unit.InterfaceProcedures)
        {
            string procName = proc.Name.ToLower();
            // Use implementation if available, otherwise use interface declaration
            if (implProcedures.TryGetValue(procName, out var implProc))
            {
                _procedures[procName] = implProc;
                RegisterProcedure(implProc);
            }
            else
            {
                _procedures[procName] = proc;
                RegisterProcedure(proc);
            }
        }

        // Register unit's interface functions (use implementation if available)
        var implFunctions = unit.ImplementationFunctions.ToDictionary(f => f.Name.ToLower());
        foreach (var func in unit.InterfaceFunctions)
        {
            string funcName = func.Name.ToLower();
            // Use implementation if available, otherwise use interface declaration
            if (implFunctions.TryGetValue(funcName, out var implFunc))
            {
                _functions[funcName] = implFunc;
                RegisterFunction(implFunc);
            }
            else
            {
                _functions[funcName] = func;
                RegisterFunction(func);
            }
        }

        // Execute unit initialization block if present
        if (unit.InitializationBlock != null)
        {
            ExecuteBlock(unit.InitializationBlock);
        }
    }
}
