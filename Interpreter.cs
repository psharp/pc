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

        // Initialize constants
        foreach (var constDecl in program.Constants)
        {
            object? value = EvaluateExpression(constDecl.Value);
            _variables[constDecl.Name.ToLower()] = value;
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
        string lowerType = type.ToLower();

        // Check for basic types first
        switch (lowerType)
        {
            case "integer":
                return 0;
            case "real":
                return 0.0;
            case "string":
                return "";
            case "boolean":
                return false;
        }

        // Check if it's a record type
        if (_recordTypes.ContainsKey(lowerType))
        {
            // Return a placeholder - actual record initialization will be done separately
            return null;
        }

        // Check if it's an enum type
        if (_enumTypes.ContainsKey(lowerType))
        {
            // Return the first enum value
            var enumType = _enumTypes[lowerType];
            return enumType.Values.First();
        }

        return null;
    }

    private void ExecuteBlock(BlockNode block)
    {
        int i = 0;
        while (i < block.Statements.Count)
        {
            try
            {
                ExecuteStatement(block.Statements[i]);
                i++;
            }
            catch (GotoException gotoEx)
            {
                // Find the label in the remaining statements
                bool labelFound = false;
                for (int j = i + 1; j < block.Statements.Count; j++)
                {
                    if (block.Statements[j] is LabeledStatementNode labeledStmt &&
                        labeledStmt.Label.Equals(gotoEx.Label, StringComparison.OrdinalIgnoreCase))
                    {
                        // Jump to this label and continue execution
                        i = j;
                        labelFound = true;
                        break;
                    }
                }

                if (!labelFound)
                {
                    // Label not found in this block, propagate exception to outer block
                    throw;
                }
            }
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

            case CaseNode caseNode:
                ExecuteCaseStatement(caseNode);
                break;

            case WhileNode whileNode:
                while (IsTrue(EvaluateExpression(whileNode.Condition)))
                {
                    ExecuteStatement(whileNode.Body);
                }
                break;

            case RepeatUntilNode repeatUntilNode:
                do
                {
                    foreach (var stmt in repeatUntilNode.Statements)
                    {
                        ExecuteStatement(stmt);
                    }
                } while (!IsTrue(EvaluateExpression(repeatUntilNode.Condition)));
                break;

            case WithNode withNode:
                ExecuteWithStatement(withNode);
                break;

            case GotoNode gotoNode:
                throw new GotoException(gotoNode.Label);

            case LabeledStatementNode labeledStmt:
                ExecuteStatement(labeledStmt.Statement);
                break;

            case ForNode forNode:
                int start = Convert.ToInt32(EvaluateExpression(forNode.Start));
                int end = Convert.ToInt32(EvaluateExpression(forNode.End));
                string varName = forNode.Variable.ToLower();

                if (forNode.IsDownTo)
                {
                    for (int i = start; i >= end; i--)
                    {
                        // Try to assign to a variable in the scope chain
                        bool assignedInScope = false;
                        foreach (var scope in _scopeChain)
                        {
                            if (scope.ContainsKey(varName))
                            {
                                scope[varName] = i;
                                assignedInScope = true;
                                break;
                            }
                        }

                        // If not found in scope chain, assign to global scope
                        if (!assignedInScope)
                        {
                            _variables[varName] = i;
                        }

                        ExecuteStatement(forNode.Body);
                    }
                }
                else
                {
                    for (int i = start; i <= end; i++)
                    {
                        // Try to assign to a variable in the scope chain
                        bool assignedInScope = false;
                        foreach (var scope in _scopeChain)
                        {
                            if (scope.ContainsKey(varName))
                            {
                                scope[varName] = i;
                                assignedInScope = true;
                                break;
                            }
                        }

                        // If not found in scope chain, assign to global scope
                        if (!assignedInScope)
                        {
                            _variables[varName] = i;
                        }

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

            case RecordFieldArrayAssignmentNode recFldArrAssignment:
                string recFieldArrName = recFldArrAssignment.RecordName.ToLower();
                string recFldName = recFldArrAssignment.FieldName.ToLower();
                object? recFldValue = EvaluateExpression(recFldArrAssignment.Value);
                if (_records.ContainsKey(recFieldArrName))
                {
                    var recDict = _records[recFieldArrName];
                    if (recDict.ContainsKey(recFldName))
                    {
                        // The field should be an array
                        if (recDict[recFldName] is Dictionary<int, object?> fieldArray)
                        {
                            int index = Convert.ToInt32(EvaluateExpression(recFldArrAssignment.Indices[0]));
                            fieldArray[index] = recFldValue;
                        }
                    }
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

            case PageNode pageNode:
                // ISO 7185: page procedure outputs form feed character
                if (pageNode.FileVariable != null)
                {
                    string pageFileVar = pageNode.FileVariable.ToLower();
                    if (_fileWriters.ContainsKey(pageFileVar))
                    {
                        _fileWriters[pageFileVar].Write('\f');  // Form feed
                    }
                }
                else
                {
                    Console.Write('\f');  // Form feed to stdout
                }
                break;

            case GetNode getNode:
                // ISO 7185: get procedure - advance file buffer (simplified: read next line)
                string getFileVar = getNode.FileVariable.ToLower();
                if (_fileReaders.ContainsKey(getFileVar))
                {
                    _fileReaders[getFileVar].ReadLine();  // Advance buffer
                }
                break;

            case PutNode putNode:
                // ISO 7185: put procedure - write file buffer (simplified: flush)
                string putFileVar = putNode.FileVariable.ToLower();
                if (_fileWriters.ContainsKey(putFileVar))
                {
                    _fileWriters[putFileVar].Flush();
                }
                break;

            case PackNode packNode:
                // ISO 7185: pack procedure - copies elements from unpacked array to packed array
                // Simplified: just copy array elements starting from index
                ExecutePackProcedure(packNode);
                break;

            case UnpackNode unpackNode:
                // ISO 7185: unpack procedure - copies elements from packed array to unpacked array
                // Simplified: just copy array elements starting from index
                ExecuteUnpackProcedure(unpackNode);
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

            case RecordFieldArrayAccessNode recFieldArrAccess:
                string recName = recFieldArrAccess.RecordName.ToLower();
                string recFieldName = recFieldArrAccess.FieldName.ToLower();
                if (_records.TryGetValue(recName, out var rec))
                {
                    if (rec.TryGetValue(recFieldName, out object? fieldValue))
                    {
                        // The field should be an array
                        if (fieldValue is Dictionary<int, object?> fieldArray)
                        {
                            int index = Convert.ToInt32(EvaluateExpression(recFieldArrAccess.Indices[0]));
                            if (fieldArray.TryGetValue(index, out object? elemValue))
                            {
                                return elemValue;
                            }
                            throw new Exception($"Array index {index} out of bounds for field '{recFieldArrAccess.FieldName}'");
                        }
                        throw new Exception($"Field '{recFieldArrAccess.FieldName}' is not an array");
                    }
                    throw new Exception($"Field '{recFieldArrAccess.FieldName}' not found in record '{recFieldArrAccess.RecordName}'");
                }
                throw new Exception($"Record variable '{recFieldArrAccess.RecordName}' not found");

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
                string varNameLower = varName.ToLower();
                string varType = varDecl.Type.ToLower();

                // Check if it's a record type
                if (_recordTypes.ContainsKey(varType))
                {
                    // Create a new record instance
                    var recordData = new Dictionary<string, object?>();
                    var recordType = _recordTypes[varType];
                    // Initialize all fields to default values
                    foreach (var field in recordType.Fields)
                    {
                        foreach (var fieldName in field.Names)
                        {
                            recordData[fieldName.ToLower()] = GetDefaultValue(field.Type);
                        }
                    }
                    _records[varNameLower] = recordData;
                    localScope[varNameLower] = null; // Record is stored in _records, not in local scope
                }
                else
                {
                    localScope[varNameLower] = GetDefaultValue(varDecl.Type);
                }
            }
        }

        // Push the local scope onto the scope chain
        _scopeChain.Push(localScope);

        // Track local record variables for cleanup
        var localRecordVars = new List<string>();
        foreach (var varDecl in procedure.LocalVariables)
        {
            string varType = varDecl.Type.ToLower();
            if (_recordTypes.ContainsKey(varType))
            {
                foreach (var varName in varDecl.Names)
                {
                    localRecordVars.Add(varName.ToLower());
                }
            }
        }

        try
        {
            // Execute procedure body
            ExecuteBlock(procedure.Block);
        }
        finally
        {
            // Pop the local scope from the scope chain
            _scopeChain.Pop();

            // Clean up local record variables
            foreach (var recordVar in localRecordVars)
            {
                _records.Remove(recordVar);
            }

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

        // Check if it's a built-in math function
        object? builtInResult = TryExecuteBuiltInFunction(funcName, arguments);
        if (builtInResult != null || IsBuiltInFunction(funcName))
        {
            return builtInResult;
        }

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
                string varNameLower = varName.ToLower();
                string varType = varDecl.Type.ToLower();

                // Check if it's a record type
                if (_recordTypes.ContainsKey(varType))
                {
                    // Create a new record instance
                    var recordData = new Dictionary<string, object?>();
                    var recordType = _recordTypes[varType];
                    // Initialize all fields to default values
                    foreach (var field in recordType.Fields)
                    {
                        foreach (var fieldName in field.Names)
                        {
                            recordData[fieldName.ToLower()] = GetDefaultValue(field.Type);
                        }
                    }
                    _records[varNameLower] = recordData;
                    localScope[varNameLower] = null; // Record is stored in _records, not in local scope
                }
                else
                {
                    localScope[varNameLower] = GetDefaultValue(varDecl.Type);
                }
            }
        }

        // Push the local scope onto the scope chain
        _scopeChain.Push(localScope);

        // Track local record variables for cleanup
        var localRecordVars = new List<string>();
        foreach (var varDecl in function.LocalVariables)
        {
            string varType = varDecl.Type.ToLower();
            if (_recordTypes.ContainsKey(varType))
            {
                foreach (var varName in varDecl.Names)
                {
                    localRecordVars.Add(varName.ToLower());
                }
            }
        }

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

            // Clean up local record variables
            foreach (var recordVar in localRecordVars)
            {
                _records.Remove(recordVar);
            }

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

    private void ExecuteCaseStatement(CaseNode caseNode)
    {
        // Evaluate the case expression
        object? caseValue = EvaluateExpression(caseNode.Expression);

        // Try to match against each case branch
        foreach (var branch in caseNode.Branches)
        {
            foreach (var label in branch.Labels)
            {
                bool matched = false;

                if (label.IsRange)
                {
                    // Range matching: check if caseValue is between start and end (inclusive)
                    object? startValue = EvaluateExpression(label.StartValue);
                    object? endValue = EvaluateExpression(label.EndValue!);

                    // Convert to comparable values
                    if (caseValue is int intCaseValue && startValue is int intStart && endValue is int intEnd)
                    {
                        matched = intCaseValue >= intStart && intCaseValue <= intEnd;
                    }
                    else if (caseValue is double doubleCaseValue && startValue is double doubleStart && endValue is double doubleEnd)
                    {
                        matched = doubleCaseValue >= doubleStart && doubleCaseValue <= doubleEnd;
                    }
                    else if (caseValue is string stringCaseValue && startValue is string stringStart && endValue is string stringEnd)
                    {
                        // String range comparison (lexicographic)
                        matched = string.CompareOrdinal(stringCaseValue, stringStart) >= 0 &&
                                 string.CompareOrdinal(stringCaseValue, stringEnd) <= 0;
                    }
                }
                else
                {
                    // Single value matching
                    object? labelValue = EvaluateExpression(label.StartValue);

                    // Use Equals for comparison to handle different types
                    if (caseValue != null && labelValue != null)
                    {
                        // Handle numeric comparisons
                        if ((caseValue is int || caseValue is double) && (labelValue is int || labelValue is double))
                        {
                            double cv = Convert.ToDouble(caseValue);
                            double lv = Convert.ToDouble(labelValue);
                            matched = Math.Abs(cv - lv) < 0.0001;
                        }
                        else
                        {
                            matched = caseValue.Equals(labelValue);
                        }
                    }
                    else
                    {
                        matched = caseValue == labelValue;
                    }
                }

                // If we found a match, execute the statement and return
                if (matched)
                {
                    ExecuteStatement(branch.Statement);
                    return;
                }
            }
        }

        // No match found, execute else branch if present
        if (caseNode.ElseBranch != null)
        {
            ExecuteStatement(caseNode.ElseBranch);
        }
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

    private bool IsBuiltInFunction(string funcName)
    {
        return funcName switch
        {
            "abs" or "sqr" or "sqrt" or "sin" or "cos" or "arctan" or
            "ln" or "exp" or "trunc" or "round" or "odd" or
            "length" or "copy" or "concat" or "pos" or "upcase" or
            "lowercase" or "chr" or "ord" => true,
            _ => false
        };
    }

    private object? TryExecuteBuiltInFunction(string funcName, List<ExpressionNode> arguments)
    {
        if (!IsBuiltInFunction(funcName))
        {
            return null;
        }

        // Handle math functions (1 parameter)
        if (arguments.Count == 1)
        {
            object? arg = EvaluateExpression(arguments[0]);
            if (arg == null && funcName != "chr")
            {
                throw new Exception($"Argument to {funcName} cannot be null");
            }

            return funcName switch
            {
                "abs" => ExecuteAbs(arg!),
                "sqr" => ExecuteSqr(arg!),
                "sqrt" => Math.Sqrt(ToDouble(arg)),
                "sin" => Math.Sin(ToDouble(arg)),
                "cos" => Math.Cos(ToDouble(arg)),
                "arctan" => Math.Atan(ToDouble(arg)),
                "ln" => Math.Log(ToDouble(arg)),
                "exp" => Math.Exp(ToDouble(arg)),
                "trunc" => (int)Math.Truncate(ToDouble(arg)),
                "round" => (int)Math.Round(ToDouble(arg)),
                "odd" => Convert.ToInt32(arg) % 2 != 0,
                "length" => arg!.ToString()!.Length,
                "upcase" => arg!.ToString()!.ToUpper(),
                "lowercase" => arg!.ToString()!.ToLower(),
                "chr" => ((char)Convert.ToInt32(arg)).ToString(),
                "ord" => arg!.ToString()!.Length > 0 ? (int)arg.ToString()![0] : 0,
                _ => null
            };
        }

        // Handle string functions with multiple parameters
        return funcName switch
        {
            "copy" when arguments.Count == 3 => ExecuteCopy(arguments),
            "pos" when arguments.Count == 2 => ExecutePos(arguments),
            "concat" when arguments.Count >= 2 => ExecuteConcat(arguments),
            _ => null
        };
    }

    private object ExecuteAbs(object value)
    {
        // Return same type as input
        if (value is int intVal)
        {
            return Math.Abs(intVal);
        }
        return Math.Abs(ToDouble(value));
    }

    private object ExecuteSqr(object value)
    {
        // Return same type as input
        if (value is int intVal)
        {
            return intVal * intVal;
        }
        double dVal = ToDouble(value);
        return dVal * dVal;
    }

    private string ExecuteCopy(List<ExpressionNode> arguments)
    {
        // copy(s, index, count) - Extract substring
        string str = EvaluateExpression(arguments[0])?.ToString() ?? "";
        int start = Convert.ToInt32(EvaluateExpression(arguments[1]));
        int count = Convert.ToInt32(EvaluateExpression(arguments[2]));

        // Pascal uses 1-based indexing
        start = start - 1;
        if (start < 0) start = 0;
        if (start >= str.Length) return "";
        if (count < 0) count = 0;
        if (start + count > str.Length) count = str.Length - start;

        return str.Substring(start, count);
    }

    private int ExecutePos(List<ExpressionNode> arguments)
    {
        // pos(substr, s) - Find position of substring (1-based, 0 if not found)
        string substr = EvaluateExpression(arguments[0])?.ToString() ?? "";
        string str = EvaluateExpression(arguments[1])?.ToString() ?? "";

        int index = str.IndexOf(substr);
        return index >= 0 ? index + 1 : 0;  // Convert to 1-based, or 0 if not found
    }

    private string ExecuteConcat(List<ExpressionNode> arguments)
    {
        // concat(s1, s2, ...) - Concatenate multiple strings
        var sb = new System.Text.StringBuilder();
        foreach (var arg in arguments)
        {
            sb.Append(EvaluateExpression(arg)?.ToString() ?? "");
        }
        return sb.ToString();
    }

    private void ExecuteWithStatement(WithNode withNode)
    {
        // The with statement allows accessing record fields without qualification
        // Create a temporary scope with the record fields
        string recordName = withNode.RecordVariable.ToLower();

        if (!_records.ContainsKey(recordName))
        {
            throw new Exception($"Record variable '{recordName}' not found");
        }

        // Push a new scope with the record fields
        var withScope = new Dictionary<string, object?>(_records[recordName]);
        _scopeChain.Push(withScope);

        try
        {
            // Execute the statement within the with scope
            ExecuteStatement(withNode.Statement);

            // Copy any changes back to the record
            foreach (var kvp in withScope)
            {
                _records[recordName][kvp.Key] = kvp.Value;
            }
        }
        finally
        {
            // Pop the with scope
            _scopeChain.Pop();
        }
    }

    private void ExecutePackProcedure(PackNode packNode)
    {
        // ISO 7185: pack(unpacked, index, packed)
        // Copies elements from unpacked array to packed array starting at index
        string unpackedName = packNode.UnpackedArray.ToLower();
        string packedName = packNode.PackedArray.ToLower();
        int startIndex = Convert.ToInt32(EvaluateExpression(packNode.StartIndex));

        if (!_arrays.ContainsKey(unpackedName) || !_arrays.ContainsKey(packedName))
        {
            return; // Arrays not found
        }

        var unpackedArray = _arrays[unpackedName];
        var packedArray = _arrays[packedName];

        // Copy all elements from unpacked to packed
        int packedIndex = 0;
        foreach (var kvp in unpackedArray.OrderBy(x => x.Key))
        {
            if (kvp.Key >= startIndex)
            {
                packedArray[packedIndex] = kvp.Value;
                packedIndex++;
            }
        }
    }

    private void ExecuteUnpackProcedure(UnpackNode unpackNode)
    {
        // ISO 7185: unpack(packed, unpacked, index)
        // Copies elements from packed array to unpacked array starting at index
        string packedName = unpackNode.PackedArray.ToLower();
        string unpackedName = unpackNode.UnpackedArray.ToLower();
        int startIndex = Convert.ToInt32(EvaluateExpression(unpackNode.StartIndex));

        if (!_arrays.ContainsKey(packedName) || !_arrays.ContainsKey(unpackedName))
        {
            return; // Arrays not found
        }

        var packedArray = _arrays[packedName];
        var unpackedArray = _arrays[unpackedName];

        // Copy all elements from packed to unpacked
        int unpackedIndex = startIndex;
        foreach (var kvp in packedArray.OrderBy(x => x.Key))
        {
            unpackedArray[unpackedIndex] = kvp.Value;
            unpackedIndex++;
        }
    }
}

/// <summary>
/// Exception used to implement goto statement control flow.
/// Thrown when a goto is executed and caught by the label handler.
/// </summary>
public class GotoException : Exception
{
    public string Label { get; }

    public GotoException(string label) : base($"goto {label}")
    {
        Label = label;
    }
}
