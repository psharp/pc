using System;
using System.Collections.Generic;
using System.IO;

namespace PascalCompiler;

public class VirtualMachine
{
    private readonly Stack<object?> _stack = new();
    private readonly Dictionary<string, object?> _variables = new();
    private readonly Stack<CallFrame> _callStack = new();
    private readonly Dictionary<int, object?> _heap = new();
    private readonly Dictionary<string, StreamWriter> _fileWriters = new();
    private readonly Dictionary<string, StreamReader> _fileReaders = new();
    private readonly Dictionary<string, string> _fileNames = new();
    private readonly Dictionary<string, Dictionary<int, object?>> _arrays = new();
    private int _nextAddress = 1000;
    private int _instructionPointer;
    private BytecodeProgram _program = null!;

    // Scope chain for closures: stack of local variable scopes from outer to inner
    private readonly Stack<Dictionary<string, object?>> _scopeChain = new();

    private class CallFrame
    {
        public int ReturnAddress { get; }
        public Dictionary<string, object?> LocalScope { get; }
        public Dictionary<string, string> VarParamMappings { get; }

        public CallFrame(int returnAddress, Dictionary<string, object?> localScope, Dictionary<string, string>? varParamMappings = null)
        {
            ReturnAddress = returnAddress;
            LocalScope = localScope;
            VarParamMappings = varParamMappings ?? new Dictionary<string, string>();
        }
    }

    public void Execute(BytecodeProgram program, BytecodeUnitLoader? unitLoader = null)
    {
        _program = program;
        _instructionPointer = 0;

        // Load and initialize units based on program's used units list
        if (program.UsedUnits.Count > 0)
        {
            // Create unit loader if not provided
            unitLoader ??= new BytecodeUnitLoader();

            // Load each used unit
            foreach (var unitName in program.UsedUnits)
            {
                var unit = unitLoader.LoadUnit(unitName);
                LoadUnitSymbols(unit);
            }
        }
        // Also load any manually pre-loaded units
        else if (unitLoader != null)
        {
            var loadedUnits = unitLoader.GetLoadedUnits();
            foreach (var unit in loadedUnits.Values)
            {
                LoadUnitSymbols(unit);
            }
        }

        // Initialize variables
        foreach (var varName in program.Variables)
        {
            _variables[varName] = GetDefaultValue();
        }

        // Initialize arrays
        foreach (var arrayInfo in program.Arrays.Values)
        {
            string arrayName = arrayInfo.Name.ToLower();
            var arrayData = new Dictionary<int, object?>();

            // Initialize all elements to default value
            for (int i = 0; i < arrayInfo.TotalSize; i++)
            {
                arrayData[i] = GetDefaultValue();
            }

            _arrays[arrayName] = arrayData;
        }

        // Execute instructions
        while (_instructionPointer < program.Instructions.Count)
        {
            var instruction = program.Instructions[_instructionPointer];
            ExecuteInstruction(instruction);
        }
    }

    private void LoadUnitSymbols(BytecodeUnit unit)
    {
        // Initialize unit variables
        foreach (var varName in unit.Variables)
        {
            if (!_variables.ContainsKey(varName))
            {
                _variables[varName] = GetDefaultValue();
            }
        }

        // Initialize unit arrays
        foreach (var arrayInfo in unit.Arrays.Values)
        {
            string arrayName = arrayInfo.Name.ToLower();
            if (!_arrays.ContainsKey(arrayName))
            {
                var arrayData = new Dictionary<int, object?>();

                // Initialize all elements to default value
                for (int i = 0; i < arrayInfo.TotalSize; i++)
                {
                    arrayData[i] = GetDefaultValue();
                }

                _arrays[arrayName] = arrayData;
            }
        }

        // Merge unit instructions into program
        // The function addresses need to be adjusted to point to the merged instruction stream
        int baseAddress = _program.Instructions.Count;

        // Append unit instructions to program
        _program.Instructions.AddRange(unit.Instructions);

        // Register unit functions with adjusted addresses
        foreach (var funcInfo in unit.Functions.Values)
        {
            string funcName = funcInfo.Name.ToLower();
            if (!_program.Functions.ContainsKey(funcName))
            {
                // Adjust function address to account for merged instructions
                var adjustedFuncInfo = new FunctionInfo(
                    funcInfo.Name,
                    funcInfo.Address + baseAddress,
                    funcInfo.ParameterCount,
                    funcInfo.ParameterNames,
                    funcInfo.ReturnType,
                    funcInfo.ParameterIsVar
                );
                _program.Functions[funcName] = adjustedFuncInfo;
            }
        }

        // Register unit array metadata
        foreach (var arrayInfo in unit.Arrays.Values)
        {
            string arrayName = arrayInfo.Name.ToLower();
            if (!_program.Arrays.ContainsKey(arrayName))
            {
                _program.Arrays[arrayName] = arrayInfo;
            }
        }

        // Execute unit initialization code if present
        if (unit.InitializationCode.Count > 0)
        {
            ExecuteUnitInitialization(unit);
        }
    }

    private void ExecuteUnitInitialization(BytecodeUnit unit)
    {
        // Save current program and instruction pointer
        var savedProgram = _program;
        var savedIP = _instructionPointer;

        // Create a temporary program for unit initialization
        var tempProgram = new BytecodeProgram(unit.Name);
        tempProgram.Variables.AddRange(_program.Variables);
        tempProgram.Variables.AddRange(unit.Variables);

        foreach (var kvp in _program.Functions)
            tempProgram.Functions[kvp.Key] = kvp.Value;
        foreach (var kvp in unit.Functions)
            tempProgram.Functions[kvp.Key] = kvp.Value;

        foreach (var kvp in _program.Arrays)
            tempProgram.Arrays[kvp.Key] = kvp.Value;
        foreach (var kvp in unit.Arrays)
            tempProgram.Arrays[kvp.Key] = kvp.Value;

        tempProgram.Instructions.AddRange(unit.InitializationCode);

        // Execute initialization code
        _program = tempProgram;
        _instructionPointer = 0;

        while (_instructionPointer < tempProgram.Instructions.Count)
        {
            var instruction = tempProgram.Instructions[_instructionPointer];
            ExecuteInstruction(instruction);
        }

        // Restore original program and instruction pointer
        _program = savedProgram;
        _instructionPointer = savedIP;
    }

    private void ExecuteInstruction(Instruction instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.PUSH:
                _stack.Push(instruction.Operand);
                _instructionPointer++;
                break;

            case OpCode.POP:
                _stack.Pop();
                _instructionPointer++;
                break;

            case OpCode.DUP:
                _stack.Push(_stack.Peek());
                _instructionPointer++;
                break;

            case OpCode.LOAD_VAR:
                {
                    string varName = (string)instruction.Operand!;

                    // Search in scope chain (from innermost to outermost)
                    bool found = false;
                    foreach (var scope in _scopeChain)
                    {
                        if (scope.TryGetValue(varName, out var scopeValue))
                        {
                            _stack.Push(scopeValue);
                            found = true;
                            break;
                        }
                    }

                    // If not in scope chain, check global variables
                    if (!found)
                    {
                        if (_variables.TryGetValue(varName, out var value))
                        {
                            _stack.Push(value);
                        }
                        else
                        {
                            throw new Exception($"Variable '{varName}' not found");
                        }
                    }
                    _instructionPointer++;
                }
                break;

            case OpCode.STORE_VAR:
                {
                    string varName = (string)instruction.Operand!;
                    object? value = _stack.Pop();

                    // Try to store in scope chain
                    bool stored = false;
                    foreach (var scope in _scopeChain)
                    {
                        if (scope.ContainsKey(varName))
                        {
                            scope[varName] = value;
                            stored = true;
                            break;
                        }
                    }

                    // If not in scope chain, store in global variables
                    if (!stored)
                    {
                        _variables[varName] = value;
                    }
                    _instructionPointer++;
                }
                break;

            case OpCode.ADD:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    if (left is string || right is string)
                    {
                        _stack.Push(left?.ToString() + right?.ToString());
                    }
                    else
                    {
                        _stack.Push(ToDouble(left) + ToDouble(right));
                    }
                    _instructionPointer++;
                }
                break;

            case OpCode.SUB:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    _stack.Push(ToDouble(left) - ToDouble(right));
                    _instructionPointer++;
                }
                break;

            case OpCode.MUL:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    _stack.Push(ToDouble(left) * ToDouble(right));
                    _instructionPointer++;
                }
                break;

            case OpCode.DIV:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    _stack.Push(ToDouble(left) / ToDouble(right));
                    _instructionPointer++;
                }
                break;

            case OpCode.IDIV:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    _stack.Push((int)(ToDouble(left) / ToDouble(right)));
                    _instructionPointer++;
                }
                break;

            case OpCode.MOD:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    _stack.Push(ToDouble(left) % ToDouble(right));
                    _instructionPointer++;
                }
                break;

            case OpCode.NEG:
                {
                    var operand = _stack.Pop();
                    _stack.Push(-ToDouble(operand));
                    _instructionPointer++;
                }
                break;

            case OpCode.EQ:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    _stack.Push(AreEqual(left, right));
                    _instructionPointer++;
                }
                break;

            case OpCode.NE:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    _stack.Push(!AreEqual(left, right));
                    _instructionPointer++;
                }
                break;

            case OpCode.LT:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    _stack.Push(ToDouble(left) < ToDouble(right));
                    _instructionPointer++;
                }
                break;

            case OpCode.GT:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    _stack.Push(ToDouble(left) > ToDouble(right));
                    _instructionPointer++;
                }
                break;

            case OpCode.LE:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    _stack.Push(ToDouble(left) <= ToDouble(right));
                    _instructionPointer++;
                }
                break;

            case OpCode.GE:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    _stack.Push(ToDouble(left) >= ToDouble(right));
                    _instructionPointer++;
                }
                break;

            case OpCode.AND:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    _stack.Push(IsTrue(left) && IsTrue(right));
                    _instructionPointer++;
                }
                break;

            case OpCode.OR:
                {
                    var right = _stack.Pop();
                    var left = _stack.Pop();
                    _stack.Push(IsTrue(left) || IsTrue(right));
                    _instructionPointer++;
                }
                break;

            case OpCode.NOT:
                {
                    var operand = _stack.Pop();
                    _stack.Push(!IsTrue(operand));
                    _instructionPointer++;
                }
                break;

            case OpCode.JUMP:
                {
                    string label = (string)instruction.Operand!;
                    _instructionPointer = _program.Labels[label];
                }
                break;

            case OpCode.JUMP_IF_FALSE:
                {
                    var condition = _stack.Pop();
                    if (!IsTrue(condition))
                    {
                        string label = (string)instruction.Operand!;
                        _instructionPointer = _program.Labels[label];
                    }
                    else
                    {
                        _instructionPointer++;
                    }
                }
                break;

            case OpCode.JUMP_IF_TRUE:
                {
                    var condition = _stack.Pop();
                    if (IsTrue(condition))
                    {
                        string label = (string)instruction.Operand!;
                        _instructionPointer = _program.Labels[label];
                    }
                    else
                    {
                        _instructionPointer++;
                    }
                }
                break;

            case OpCode.CALL:
                {
                    string funcName = (string)instruction.Operand!;
                    if (_program.Functions.TryGetValue(funcName, out var funcInfo))
                    {
                        // Create a new local scope for this function
                        var localScope = new Dictionary<string, object?>();

                        // Track var parameter mappings (param name -> caller var name)
                        var varParamMappings = new Dictionary<string, string>();

                        // Pop arguments (in reverse order since they were pushed in order)
                        var args = new Stack<object?>();
                        for (int i = 0; i < funcInfo.ParameterCount; i++)
                        {
                            args.Push(_stack.Pop());
                        }

                        // Assign arguments to parameter variables (in correct order)
                        for (int i = 0; i < funcInfo.ParameterNames.Count; i++)
                        {
                            string paramName = funcInfo.ParameterNames[i];
                            object? argValue = args.Pop();

                            if (i < funcInfo.ParameterIsVar.Count && funcInfo.ParameterIsVar[i])
                            {
                                // For var parameters, argValue is actually a variable index
                                int varIndex = Convert.ToInt32(argValue);
                                string callerVarName = _program.Variables[varIndex];

                                // Track the mapping
                                varParamMappings[paramName] = callerVarName;

                                // Get the caller's variable value (search scope chain)
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

                                // Copy the value to local scope
                                localScope[paramName] = callerValue;
                            }
                            else
                            {
                                // For value parameters, assign to local scope
                                localScope[paramName] = argValue;
                            }
                        }

                        // Create call frame with local scope
                        _callStack.Push(new CallFrame(_instructionPointer + 1, localScope, varParamMappings));

                        // Push the local scope onto the scope chain
                        _scopeChain.Push(localScope);

                        _instructionPointer = funcInfo.Address;
                    }
                    else
                    {
                        throw new Exception($"Function '{funcName}' not found");
                    }
                }
                break;

            case OpCode.RETURN:
                {
                    if (_callStack.Count > 0)
                    {
                        var frame = _callStack.Pop();

                        // Pop the local scope from the scope chain
                        if (_scopeChain.Count > 0)
                        {
                            _scopeChain.Pop();
                        }

                        // For functions, the return value is on top of stack
                        object? returnValue = null;
                        if (_stack.Count > 0)
                        {
                            returnValue = _stack.Pop();
                        }

                        // Copy var parameter values back to caller's variables
                        foreach (var mapping in frame.VarParamMappings)
                        {
                            string paramName = mapping.Key;
                            string callerVarName = mapping.Value;

                            if (frame.LocalScope.TryGetValue(paramName, out var paramValue))
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

                                // If not in scope chain, update global
                                if (!updated)
                                {
                                    _variables[callerVarName] = paramValue;
                                }
                            }
                        }

                        // Push return value back
                        if (returnValue != null)
                        {
                            _stack.Push(returnValue);
                        }

                        _instructionPointer = frame.ReturnAddress;
                    }
                    else
                    {
                        // Return from main program
                        _instructionPointer = _program.Instructions.Count;
                    }
                }
                break;

            case OpCode.WRITE:
                {
                    var value = _stack.Pop();
                    Console.Write(value);
                    _instructionPointer++;
                }
                break;

            case OpCode.WRITELN:
                {
                    var value = _stack.Pop();
                    Console.WriteLine(value);
                    _instructionPointer++;
                }
                break;

            case OpCode.READ:
                {
                    string varName = (string)instruction.Operand!;
                    string? input = Console.ReadLine();
                    _variables[varName] = ParseInput(input, _variables.GetValueOrDefault(varName));
                    _instructionPointer++;
                }
                break;

            case OpCode.NEW:
                {
                    string ptrVar = (string)instruction.Operand!;
                    int address = _nextAddress++;
                    _heap[address] = 0; // Initialize with default value
                    _variables[ptrVar] = address; // Store address in pointer variable
                    _instructionPointer++;
                }
                break;

            case OpCode.DISPOSE:
                {
                    string disposeVar = (string)instruction.Operand!;
                    if (_variables.TryGetValue(disposeVar, out var ptrValue) && ptrValue is int addr)
                    {
                        _heap.Remove(addr);
                        _variables[disposeVar] = null; // Set to nil
                    }
                    _instructionPointer++;
                }
                break;

            case OpCode.DEREF:
                {
                    var ptrAddr = _stack.Pop();
                    if (ptrAddr == null)
                        throw new Exception("Cannot dereference nil pointer");
                    if (ptrAddr is int address && _heap.TryGetValue(address, out object? derefValue))
                    {
                        _stack.Push(derefValue);
                    }
                    else
                    {
                        throw new Exception("Invalid pointer dereference");
                    }
                    _instructionPointer++;
                }
                break;

            case OpCode.STORE_DEREF:
                {
                    var storeValue = _stack.Pop();      // Value to store
                    var ptrAddr = _stack.Pop();         // Pointer address
                    if (ptrAddr == null)
                        throw new Exception("Cannot dereference nil pointer");
                    if (ptrAddr is int address)
                    {
                        _heap[address] = storeValue;
                    }
                    else
                    {
                        throw new Exception("Invalid pointer for assignment");
                    }
                    _instructionPointer++;
                }
                break;

            case OpCode.ADDR_OF:
                {
                    string addrVarName = (string)instruction.Operand!;
                    if (_variables.TryGetValue(addrVarName, out var varValue))
                    {
                        int pseudoAddr = _nextAddress++;
                        _heap[pseudoAddr] = varValue;
                        _stack.Push(pseudoAddr);
                    }
                    else
                    {
                        throw new Exception($"Variable '{addrVarName}' not found");
                    }
                    _instructionPointer++;
                }
                break;

            case OpCode.PUSH_NIL:
                _stack.Push(null);
                _instructionPointer++;
                break;

            case OpCode.FILE_ASSIGN:
                {
                    string fileVar = (string)instruction.Operand!;
                    string fileName = _stack.Pop()?.ToString() ?? "";
                    _fileNames[fileVar] = fileName;
                    _instructionPointer++;
                }
                break;

            case OpCode.FILE_RESET:
                {
                    string fileVar = (string)instruction.Operand!;
                    if (_fileNames.TryGetValue(fileVar, out var fileName))
                    {
                        _fileReaders[fileVar] = new StreamReader(fileName);
                    }
                    _instructionPointer++;
                }
                break;

            case OpCode.FILE_REWRITE:
                {
                    string fileVar = (string)instruction.Operand!;
                    if (_fileNames.TryGetValue(fileVar, out var fileName))
                    {
                        _fileWriters[fileVar] = new StreamWriter(fileName);
                    }
                    _instructionPointer++;
                }
                break;

            case OpCode.FILE_CLOSE:
                {
                    string fileVar = (string)instruction.Operand!;
                    if (_fileWriters.TryGetValue(fileVar, out var writer))
                    {
                        writer.Close();
                        _fileWriters.Remove(fileVar);
                    }
                    if (_fileReaders.TryGetValue(fileVar, out var reader))
                    {
                        reader.Close();
                        _fileReaders.Remove(fileVar);
                    }
                    _instructionPointer++;
                }
                break;

            case OpCode.FILE_READ:
                {
                    var readInfo = (object[])instruction.Operand!;
                    string fileVar = (string)readInfo[0];
                    string varName = (string)readInfo[1];
                    // bool readLine = (bool)readInfo[2]; // Not currently used - always use ReadLine

                    if (_fileReaders.TryGetValue(fileVar, out var reader))
                    {
                        string? line = reader.ReadLine();
                        _variables[varName] = ParseInput(line, _variables.GetValueOrDefault(varName));
                    }
                    _instructionPointer++;
                }
                break;

            case OpCode.FILE_WRITE:
                {
                    var writeInfo = (object[])instruction.Operand!;
                    string fileVar = (string)writeInfo[0];
                    int exprCount = (int)writeInfo[1];
                    bool newLine = (bool)writeInfo[2];

                    if (_fileWriters.TryGetValue(fileVar, out var writer))
                    {
                        // Pop expressions in reverse order (they were pushed in order)
                        var values = new Stack<object?>();
                        for (int i = 0; i < exprCount; i++)
                        {
                            values.Push(_stack.Pop());
                        }

                        // Write them in correct order
                        while (values.Count > 0)
                        {
                            writer.Write(values.Pop());
                        }

                        if (newLine)
                        {
                            writer.WriteLine();
                        }
                        writer.Flush();
                    }
                    _instructionPointer++;
                }
                break;

            case OpCode.FILE_EOF:
                {
                    string fileVar = (string)instruction.Operand!;
                    bool isEof = false;
                    if (_fileReaders.TryGetValue(fileVar, out var reader))
                    {
                        isEof = reader.EndOfStream;
                    }
                    _stack.Push(isEof);
                    _instructionPointer++;
                }
                break;

            case OpCode.SET_LITERAL:
                {
                    int elementCount = (int)instruction.Operand!;
                    var setElements = new HashSet<object>();
                    // Pop elements in reverse order and add to set
                    var tempStack = new Stack<object?>();
                    for (int i = 0; i < elementCount; i++)
                    {
                        tempStack.Push(_stack.Pop());
                    }
                    // Add to set in correct order
                    while (tempStack.Count > 0)
                    {
                        object? elem = tempStack.Pop();
                        if (elem != null)
                        {
                            setElements.Add(elem);
                        }
                    }
                    _stack.Push(setElements);
                    _instructionPointer++;
                }
                break;

            case OpCode.SET_CONTAINS:
                {
                    object? setExpr = _stack.Pop();
                    object? testValue = _stack.Pop();
                    bool result = false;
                    if (setExpr is HashSet<object> setData)
                    {
                        result = testValue != null && setData.Contains(testValue);
                    }
                    _stack.Push(result);
                    _instructionPointer++;
                }
                break;

            case OpCode.ARRAY_LOAD:
                {
                    // Operand format: "arrayName:dimensionCount"
                    string operand = (string)instruction.Operand!;
                    string[] parts = operand.Split(':');
                    string arrayName = parts[0];
                    int dimensionCount = int.Parse(parts[1]);

                    // Pop indices from stack (in reverse order)
                    var indices = new List<int>();
                    for (int i = 0; i < dimensionCount; i++)
                    {
                        indices.Insert(0, Convert.ToInt32(_stack.Pop()));
                    }

                    // Get array info and data
                    var arrayInfo = _program.Arrays[arrayName];
                    var arrayData = _arrays[arrayName];

                    // Calculate linear index
                    int linearIndex = CalculateLinearIndex(arrayInfo, indices);

                    // Push value onto stack
                    if (arrayData.TryGetValue(linearIndex, out object? value))
                    {
                        _stack.Push(value);
                    }
                    else
                    {
                        throw new Exception($"Array index out of bounds");
                    }

                    _instructionPointer++;
                }
                break;

            case OpCode.ARRAY_STORE:
                {
                    // Operand format: "arrayName:dimensionCount"
                    string operand = (string)instruction.Operand!;
                    string[] parts = operand.Split(':');
                    string arrayName = parts[0];
                    int dimensionCount = int.Parse(parts[1]);

                    // Pop indices from stack (in reverse order)
                    var indices = new List<int>();
                    for (int i = 0; i < dimensionCount; i++)
                    {
                        indices.Insert(0, Convert.ToInt32(_stack.Pop()));
                    }

                    // Pop value to store
                    object? value = _stack.Pop();

                    // Get array info and data
                    var arrayInfo = _program.Arrays[arrayName];
                    var arrayData = _arrays[arrayName];

                    // Calculate linear index
                    int linearIndex = CalculateLinearIndex(arrayInfo, indices);

                    // Store value
                    arrayData[linearIndex] = value;

                    _instructionPointer++;
                }
                break;

            case OpCode.HALT:
                _instructionPointer = _program.Instructions.Count;
                break;

            case OpCode.NOP:
                _instructionPointer++;
                break;

            default:
                throw new Exception($"Unknown opcode: {instruction.OpCode}");
        }
    }

    private object? GetDefaultValue()
    {
        return ""; // Default to empty string so ParseInput treats unknowns as strings
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

    private bool IsTrue(object? value)
    {
        if (value is bool b) return b;
        if (value is int i) return i != 0;
        if (value is double d) return d != 0.0;
        return value != null;
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

    // Helper method to calculate linear index from multidimensional indices
    private int CalculateLinearIndex(ArrayInfo arrayInfo, List<int> indices)
    {
        if (indices.Count != arrayInfo.DimensionCount)
        {
            throw new Exception($"Array requires {arrayInfo.DimensionCount} indices but got {indices.Count}");
        }

        int linearIndex = 0;
        int multiplier = 1;

        // Calculate linear index using row-major order
        for (int d = arrayInfo.DimensionCount - 1; d >= 0; d--)
        {
            var dim = arrayInfo.Dimensions[d];
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
}
