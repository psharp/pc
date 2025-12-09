using System.Collections.Generic;

namespace PascalCompiler;

public class BytecodeUnit
{
    public string Name { get; }
    public List<string> UsedUnits { get; } = new();
    public List<string> Variables { get; } = new();
    public Dictionary<string, int> Labels { get; } = new();
    public Dictionary<string, EnumInfo> EnumTypes { get; } = new();
    public Dictionary<string, FunctionInfo> Functions { get; } = new();
    public Dictionary<string, ArrayInfo> Arrays { get; } = new();
    public Dictionary<string, RecordTypeNode> RecordTypes { get; } = new();
    public List<Instruction> Instructions { get; } = new(); // All function code
    public List<Instruction> InitializationCode { get; } = new();
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
