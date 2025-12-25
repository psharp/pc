/// <summary>
/// Bytecode instruction set and program structure definitions for the Pascal virtual machine.
/// Defines opcodes, instruction format, and compiled program representation.
/// </summary>
using System;
using System.Collections.Generic;

namespace PascalCompiler;

/// <summary>
/// Enumeration of all bytecode operation codes supported by the Pascal virtual machine.
/// Each opcode represents a single VM instruction.
/// </summary>
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
    CASE_JUMP,      // Jump table for case statement (operand = branch count)
    CASE_RANGE,     // Check if value is in range (used for case ranges)

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

    // Math functions
    ABS,            // Absolute value
    SQR,            // Square
    SQRT,           // Square root
    SIN,            // Sine
    COS,            // Cosine
    ARCTAN,         // Arctangent
    LN,             // Natural logarithm
    EXP,            // Exponential
    TRUNC,          // Truncate to integer
    ROUND,          // Round to nearest integer
    ODD,            // Check if odd

    // String functions
    LENGTH,         // String length
    COPY,           // Copy substring (3 params: str, start, count)
    CONCAT,         // Concatenate strings (operand = count of strings on stack)
    POS,            // Find substring position (2 params: substr, str)
    UPCASE,         // Convert to uppercase
    LOWERCASE,      // Convert to lowercase
    CHR,            // Convert integer to character
    ORD,            // Convert character to integer (ASCII value)

    // Special
    HALT,           // Stop execution
    NOP             // No operation
}

/// <summary>
/// Represents a single bytecode instruction with an opcode and optional operand.
/// Instructions are executed sequentially by the virtual machine.
/// </summary>
public class Instruction
{
    /// <summary>Gets the operation code for this instruction.</summary>
    public OpCode OpCode { get; }

    /// <summary>Gets the optional operand value (can be int, double, string, bool, array, etc.).</summary>
    public object? Operand { get; }

    /// <summary>
    /// Initializes a new bytecode instruction.
    /// </summary>
    /// <param name="opCode">The operation code.</param>
    /// <param name="operand">Optional operand value.</param>
    public Instruction(OpCode opCode, object? operand = null)
    {
        OpCode = opCode;
        Operand = operand;
    }

    /// <summary>
    /// Returns a string representation of the instruction for disassembly output.
    /// </summary>
    public override string ToString()
    {
        return Operand != null ? $"{OpCode} {Operand}" : OpCode.ToString();
    }
}

/// <summary>
/// Represents a compiled Pascal program in bytecode format.
/// Contains all instructions, metadata, and symbol information needed for execution.
/// </summary>
public class BytecodeProgram
{
    /// <summary>Gets the name of the program.</summary>
    public string Name { get; }

    /// <summary>Gets the list of units used by this program (for automatic loading).</summary>
    public List<string> UsedUnits { get; }

    /// <summary>Gets the list of bytecode instructions to execute.</summary>
    public List<Instruction> Instructions { get; }

    /// <summary>Gets the dictionary of labels mapped to instruction addresses.</summary>
    public Dictionary<string, int> Labels { get; }

    /// <summary>Gets the dictionary of constant values.</summary>
    public Dictionary<string, object?> Constants { get; }

    /// <summary>Gets the list of variable names.</summary>
    public List<string> Variables { get; }

    /// <summary>Gets the dictionary of function/procedure metadata.</summary>
    public Dictionary<string, FunctionInfo> Functions { get; }

    /// <summary>Gets the dictionary of enumeration type definitions.</summary>
    public Dictionary<string, EnumInfo> EnumTypes { get; }

    /// <summary>Gets the dictionary of array metadata.</summary>
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
    public List<string> LocalVariableNames { get; }

    public FunctionInfo(string name, int address, int parameterCount, List<string> parameterNames, string? returnType = null, List<bool>? parameterIsVar = null, List<string>? localVariableNames = null)
    {
        Name = name;
        Address = address;
        ParameterCount = parameterCount;
        ParameterNames = parameterNames;
        ReturnType = returnType;
        ParameterIsVar = parameterIsVar ?? new List<bool>(new bool[parameterCount]);
        LocalVariableNames = localVariableNames ?? new List<string>();
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
