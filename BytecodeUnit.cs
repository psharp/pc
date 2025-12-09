/// <summary>
/// Represents a compiled Pascal unit/module in bytecode format.
/// Units can be loaded and linked with programs, providing reusable code modules.
/// </summary>
using System.Collections.Generic;

namespace PascalCompiler;

/// <summary>
/// Represents a compiled Pascal unit with interface and implementation sections.
/// Contains bytecode for all unit functions, plus optional initialization/finalization code.
/// </summary>
public class BytecodeUnit
{
    /// <summary>Gets the name of the unit.</summary>
    public string Name { get; }

    /// <summary>Gets the list of units used by this unit.</summary>
    public List<string> UsedUnits { get; } = new();

    /// <summary>Gets the list of variable names defined in this unit.</summary>
    public List<string> Variables { get; } = new();

    /// <summary>Gets the dictionary of labels mapped to instruction addresses.</summary>
    public Dictionary<string, int> Labels { get; } = new();

    /// <summary>Gets the dictionary of enumeration type definitions.</summary>
    public Dictionary<string, EnumInfo> EnumTypes { get; } = new();

    /// <summary>Gets the dictionary of function/procedure metadata.</summary>
    public Dictionary<string, FunctionInfo> Functions { get; } = new();

    /// <summary>Gets the dictionary of array metadata.</summary>
    public Dictionary<string, ArrayInfo> Arrays { get; } = new();

    /// <summary>Gets the dictionary of record type definitions.</summary>
    public Dictionary<string, RecordTypeNode> RecordTypes { get; } = new();

    /// <summary>Gets the list of bytecode instructions for all unit functions.</summary>
    public List<Instruction> Instructions { get; } = new();

    /// <summary>Gets the bytecode for the unit's initialization block (executed when loaded).</summary>
    public List<Instruction> InitializationCode { get; } = new();

    /// <summary>Gets the bytecode for the unit's finalization block (executed at program exit).</summary>
    public List<Instruction> FinalizationCode { get; } = new();

    public BytecodeUnit(string name)
    {
        Name = name;
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

    public void AddLabel(string label)
    {
        Labels[label] = InitializationCode.Count;
    }

    public int GetCurrentAddress()
    {
        return InitializationCode.Count;
    }

    public void AddInstruction(Instruction instruction)
    {
        InitializationCode.Add(instruction);
    }

    public void AddFinalizationInstruction(Instruction instruction)
    {
        FinalizationCode.Add(instruction);
    }
}
