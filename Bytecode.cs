using System;
using System.Collections.Generic;

namespace PascalCompiler;

public enum OpCode : byte
{
    // Stack operations
    PUSH,           // Push constant value onto stack
    POP,            // Pop value from stack
    DUP,            // Duplicate top of stack

    // Variable operations
    LOAD_VAR,       // Load variable value onto stack
    STORE_VAR,      // Store top of stack into variable

    // Arithmetic operations
    ADD,            // Add two values
    SUB,            // Subtract two values
    MUL,            // Multiply two values
    DIV,            // Divide two values
    IDIV,           // Integer division
    MOD,            // Modulo operation
    NEG,            // Negate value

    // Comparison operations
    EQ,             // Equal
    NE,             // Not equal
    LT,             // Less than
    GT,             // Greater than
    LE,             // Less than or equal
    GE,             // Greater than or equal

    // Logical operations
    AND,            // Logical AND
    OR,             // Logical OR
    NOT,            // Logical NOT

    // Control flow
    JUMP,           // Unconditional jump
    JUMP_IF_FALSE,  // Jump if top of stack is false
    JUMP_IF_TRUE,   // Jump if top of stack is true

    // Function/Procedure operations
    CALL,           // Call function/procedure
    RETURN,         // Return from function/procedure

    // I/O operations
    WRITE,          // Write value
    WRITELN,        // Write value with newline
    READ,           // Read value

    // Pointer operations
    NEW,            // Allocate memory for pointer
    DISPOSE,        // Deallocate pointer memory
    DEREF,          // Dereference pointer (load value from heap)
    STORE_DEREF,    // Store value through pointer (ptr^ := value)
    ADDR_OF,        // Get address of variable
    PUSH_NIL,       // Push nil onto stack

    // File I/O operations
    FILE_ASSIGN,    // Assign(fileVar, filename)
    FILE_RESET,     // Reset(fileVar) - open for reading
    FILE_REWRITE,   // Rewrite(fileVar) - open for writing
    FILE_CLOSE,     // Close(fileVar)
    FILE_READ,      // Read(fileVar, var) or Readln(fileVar, var)
    FILE_WRITE,     // Write(fileVar, value) or Writeln(fileVar, value)
    FILE_EOF,       // EOF(fileVar) - check end of file

    // Set operations
    SET_LITERAL,    // Create set from N elements on stack
    SET_CONTAINS,   // Check if value is in set (in operator)

    // Array operations
    ARRAY_LOAD,     // Load array element: arr[i] or arr[i,j] (pops indices, pushes value)
    ARRAY_STORE,    // Store array element: arr[i] := val (pops value and indices)

    // Special
    HALT,           // Stop execution
    NOP             // No operation
}

public class Instruction
{
    public OpCode OpCode { get; }
    public object? Operand { get; }

    public Instruction(OpCode opCode, object? operand = null)
    {
        OpCode = opCode;
        Operand = operand;
    }

    public override string ToString()
    {
        return Operand != null ? $"{OpCode} {Operand}" : OpCode.ToString();
    }
}

public class BytecodeProgram
{
    public string Name { get; }
    public List<string> UsedUnits { get; }
    public List<Instruction> Instructions { get; }
    public Dictionary<string, int> Labels { get; }
    public Dictionary<string, object?> Constants { get; }
    public List<string> Variables { get; }
    public Dictionary<string, FunctionInfo> Functions { get; }
    public Dictionary<string, EnumInfo> EnumTypes { get; }
    public Dictionary<string, ArrayInfo> Arrays { get; }

    public BytecodeProgram(string name)
    {
        Name = name;
        UsedUnits = new List<string>();
        Instructions = new List<Instruction>();
        Labels = new Dictionary<string, int>();
        Constants = new Dictionary<string, object?>();
        Variables = new List<string>();
        Functions = new Dictionary<string, FunctionInfo>();
        EnumTypes = new Dictionary<string, EnumInfo>();
        Arrays = new Dictionary<string, ArrayInfo>();
    }

    public void AddInstruction(Instruction instruction)
    {
        Instructions.Add(instruction);
    }

    public void AddLabel(string label)
    {
        Labels[label] = Instructions.Count;
    }

    public int GetCurrentAddress()
    {
        return Instructions.Count;
    }

    public void AddVariable(string name)
    {
        if (!Variables.Contains(name))
        {
            Variables.Add(name);
        }
    }

    public int GetVariableIndex(string name)
    {
        return Variables.IndexOf(name);
    }
}

public class FunctionInfo
{
    public string Name { get; }
    public int Address { get; }
    public int ParameterCount { get; }
    public List<string> ParameterNames { get; }
    public List<bool> ParameterIsVar { get; }
    public string? ReturnType { get; }

    public FunctionInfo(string name, int address, int parameterCount, List<string> parameterNames, string? returnType = null, List<bool>? parameterIsVar = null)
    {
        Name = name;
        Address = address;
        ParameterCount = parameterCount;
        ParameterNames = parameterNames;
        ReturnType = returnType;
        ParameterIsVar = parameterIsVar ?? new List<bool>(new bool[parameterCount]);
    }
}

public class EnumInfo
{
    public string Name { get; }
    public List<string> Values { get; }

    public EnumInfo(string name, List<string> values)
    {
        Name = name;
        Values = values;
    }
}

public class ArrayInfo
{
    public string Name { get; }
    public List<(int LowerBound, int UpperBound)> Dimensions { get; }
    public string ElementType { get; }

    public ArrayInfo(string name, List<(int LowerBound, int UpperBound)> dimensions, string elementType)
    {
        Name = name;
        Dimensions = dimensions;
        ElementType = elementType;
    }

    public int DimensionCount => Dimensions.Count;

    public int TotalSize
    {
        get
        {
            int size = 1;
            foreach (var dim in Dimensions)
            {
                size *= (dim.UpperBound - dim.LowerBound + 1);
            }
            return size;
        }
    }
}
