using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PascalCompiler;

public class BytecodeUnitSerializer
{
    private const uint MAGIC_NUMBER = 0x50415355; // "PASU" (Pascal Unit)
    private const ushort VERSION = 1;

    public void SaveToFile(BytecodeUnit unit, string filePath)
    {
        using var stream = File.Create(filePath);
        using var writer = new BinaryWriter(stream, Encoding.UTF8);

        // Write header
        writer.Write(MAGIC_NUMBER);
        writer.Write(VERSION);
        writer.Write(unit.Name);

        // Write used units
        writer.Write(unit.UsedUnits.Count);
        foreach (var usedUnit in unit.UsedUnits)
        {
            writer.Write(usedUnit);
        }

        // Write variables
        writer.Write(unit.Variables.Count);
        foreach (var variable in unit.Variables)
        {
            writer.Write(variable);
        }

        // Write labels
        writer.Write(unit.Labels.Count);
        foreach (var kvp in unit.Labels)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value);
        }

        // Write enum types
        writer.Write(unit.EnumTypes.Count);
        foreach (var kvp in unit.EnumTypes)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value.Name);
            writer.Write(kvp.Value.Values.Count);
            foreach (var value in kvp.Value.Values)
            {
                writer.Write(value);
            }
        }

        // Write array metadata
        writer.Write(unit.Arrays.Count);
        foreach (var kvp in unit.Arrays)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value.Name);
            writer.Write(kvp.Value.Dimensions.Count);
            foreach (var dim in kvp.Value.Dimensions)
            {
                writer.Write(dim.LowerBound);
                writer.Write(dim.UpperBound);
            }
            writer.Write(kvp.Value.ElementType);
        }

        // Write functions
        writer.Write(unit.Functions.Count);
        foreach (var kvp in unit.Functions)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value.Name);
            writer.Write(kvp.Value.Address);
            writer.Write(kvp.Value.ParameterCount);

            // Write parameter names
            writer.Write(kvp.Value.ParameterNames.Count);
            foreach (var paramName in kvp.Value.ParameterNames)
            {
                writer.Write(paramName);
            }

            // Write parameter isVar flags
            writer.Write(kvp.Value.ParameterIsVar.Count);
            foreach (var isVar in kvp.Value.ParameterIsVar)
            {
                writer.Write(isVar);
            }

            writer.Write(kvp.Value.ReturnType ?? "");
        }

        // Write all function bytecode instructions
        WriteInstructions(writer, unit.Instructions);

        // Write initialization code
        WriteInstructions(writer, unit.InitializationCode);

        // Write finalization code
        WriteInstructions(writer, unit.FinalizationCode);
    }

    public BytecodeUnit LoadFromFile(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream, Encoding.UTF8);

        // Read and verify header
        uint magic = reader.ReadUInt32();
        if (magic != MAGIC_NUMBER)
        {
            throw new Exception("Invalid unit bytecode file format");
        }

        ushort version = reader.ReadUInt16();
        if (version != VERSION)
        {
            throw new Exception($"Unsupported unit bytecode version: {version}");
        }

        string unitName = reader.ReadString();
        var unit = new BytecodeUnit(unitName);

        // Read used units
        int usedUnitsCount = reader.ReadInt32();
        for (int i = 0; i < usedUnitsCount; i++)
        {
            unit.UsedUnits.Add(reader.ReadString());
        }

        // Read variables
        int variableCount = reader.ReadInt32();
        for (int i = 0; i < variableCount; i++)
        {
            unit.Variables.Add(reader.ReadString());
        }

        // Read labels
        int labelCount = reader.ReadInt32();
        for (int i = 0; i < labelCount; i++)
        {
            string labelName = reader.ReadString();
            int address = reader.ReadInt32();
            unit.Labels[labelName] = address;
        }

        // Read enum types
        int enumCount = reader.ReadInt32();
        for (int i = 0; i < enumCount; i++)
        {
            string key = reader.ReadString();
            string name = reader.ReadString();
            int valuesCount = reader.ReadInt32();
            var values = new List<string>();
            for (int j = 0; j < valuesCount; j++)
            {
                values.Add(reader.ReadString());
            }
            unit.EnumTypes[key] = new EnumInfo(name, values);
        }

        // Read array metadata
        int arrayCount = reader.ReadInt32();
        for (int i = 0; i < arrayCount; i++)
        {
            string key = reader.ReadString();
            string name = reader.ReadString();
            int dimensionCount = reader.ReadInt32();
            var dimensions = new List<(int LowerBound, int UpperBound)>();
            for (int j = 0; j < dimensionCount; j++)
            {
                int lowerBound = reader.ReadInt32();
                int upperBound = reader.ReadInt32();
                dimensions.Add((lowerBound, upperBound));
            }
            string elementType = reader.ReadString();
            unit.Arrays[key] = new ArrayInfo(name, dimensions, elementType);
        }

        // Read functions
        int functionCount = reader.ReadInt32();
        for (int i = 0; i < functionCount; i++)
        {
            string key = reader.ReadString();
            string name = reader.ReadString();
            int address = reader.ReadInt32();
            int paramCount = reader.ReadInt32();

            // Read parameter names
            int paramNamesCount = reader.ReadInt32();
            var paramNames = new List<string>();
            for (int j = 0; j < paramNamesCount; j++)
            {
                paramNames.Add(reader.ReadString());
            }

            // Read parameter isVar flags
            int paramIsVarCount = reader.ReadInt32();
            var paramIsVar = new List<bool>();
            for (int j = 0; j < paramIsVarCount; j++)
            {
                paramIsVar.Add(reader.ReadBoolean());
            }

            string returnType = reader.ReadString();

            unit.Functions[key] = new FunctionInfo(
                name,
                address,
                paramCount,
                paramNames,
                string.IsNullOrEmpty(returnType) ? null : returnType,
                paramIsVar
            );
        }

        // Read all function bytecode instructions
        unit.Instructions.AddRange(ReadInstructions(reader));

        // Read initialization code
        unit.InitializationCode.AddRange(ReadInstructions(reader));

        // Read finalization code
        unit.FinalizationCode.AddRange(ReadInstructions(reader));

        return unit;
    }

    private void WriteInstructions(BinaryWriter writer, List<Instruction> instructions)
    {
        writer.Write(instructions.Count);
        foreach (var instruction in instructions)
        {
            writer.Write((byte)instruction.OpCode);

            // Write operand based on type
            if (instruction.Operand == null)
            {
                writer.Write((byte)0); // Null
            }
            else if (instruction.Operand is int intValue)
            {
                writer.Write((byte)1); // Int
                writer.Write(intValue);
            }
            else if (instruction.Operand is double doubleValue)
            {
                writer.Write((byte)2); // Double
                writer.Write(doubleValue);
            }
            else if (instruction.Operand is string stringValue)
            {
                writer.Write((byte)3); // String
                writer.Write(stringValue);
            }
            else if (instruction.Operand is bool boolValue)
            {
                writer.Write((byte)4); // Bool
                writer.Write(boolValue);
            }
            else if (instruction.Operand is string[] stringArray)
            {
                writer.Write((byte)5); // String array
                writer.Write(stringArray.Length);
                foreach (var str in stringArray)
                {
                    writer.Write(str);
                }
            }
            else if (instruction.Operand is object[] objArray)
            {
                writer.Write((byte)6); // Object array
                writer.Write(objArray.Length);
                foreach (var obj in objArray)
                {
                    if (obj is string str)
                    {
                        writer.Write((byte)3); // String
                        writer.Write(str);
                    }
                    else if (obj is int intVal)
                    {
                        writer.Write((byte)1); // Int
                        writer.Write(intVal);
                    }
                    else if (obj is bool boolVal)
                    {
                        writer.Write((byte)4); // Bool
                        writer.Write(boolVal);
                    }
                    else
                    {
                        throw new Exception($"Unsupported object array element type: {obj?.GetType()}");
                    }
                }
            }
            else
            {
                throw new Exception($"Unsupported operand type: {instruction.Operand.GetType()}");
            }
        }
    }

    private List<Instruction> ReadInstructions(BinaryReader reader)
    {
        var instructions = new List<Instruction>();
        int instructionCount = reader.ReadInt32();

        for (int i = 0; i < instructionCount; i++)
        {
            OpCode opCode = (OpCode)reader.ReadByte();
            byte operandType = reader.ReadByte();

            object? operand = null;
            switch (operandType)
            {
                case 0:
                    operand = null;
                    break;
                case 1:
                    operand = reader.ReadInt32();
                    break;
                case 2:
                    operand = reader.ReadDouble();
                    break;
                case 3:
                    operand = reader.ReadString();
                    break;
                case 4:
                    operand = reader.ReadBoolean();
                    break;
                case 5: // String array
                    {
                        int arrayLength = reader.ReadInt32();
                        var strArray = new string[arrayLength];
                        for (int j = 0; j < arrayLength; j++)
                        {
                            strArray[j] = reader.ReadString();
                        }
                        operand = strArray;
                    }
                    break;
                case 6: // Object array
                    {
                        int arrayLength = reader.ReadInt32();
                        var objArray = new object[arrayLength];
                        for (int j = 0; j < arrayLength; j++)
                        {
                            byte elemType = reader.ReadByte();
                            objArray[j] = elemType switch
                            {
                                1 => reader.ReadInt32(),
                                3 => reader.ReadString(),
                                4 => reader.ReadBoolean(),
                                _ => throw new Exception($"Unknown object array element type: {elemType}")
                            };
                        }
                        operand = objArray;
                    }
                    break;
                default:
                    throw new Exception($"Unknown operand type: {operandType}");
            }

            instructions.Add(new Instruction(opCode, operand));
        }

        return instructions;
    }

    public string DisassembleToString(BytecodeUnit unit)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Unit: {unit.Name}");
        sb.AppendLine();

        if (unit.UsedUnits.Count > 0)
        {
            sb.AppendLine("Used Units:");
            foreach (var usedUnit in unit.UsedUnits)
            {
                sb.AppendLine($"  {usedUnit}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("Variables:");
        foreach (var variable in unit.Variables)
        {
            sb.AppendLine($"  {variable}");
        }
        sb.AppendLine();

        if (unit.Functions.Count > 0)
        {
            sb.AppendLine("Functions:");
            foreach (var func in unit.Functions.Values)
            {
                sb.AppendLine($"  {func.Name} @ {func.Address} (params: {func.ParameterCount}, return: {func.ReturnType ?? "void"})");
            }
            sb.AppendLine();
        }

        if (unit.Instructions.Count > 0)
        {
            sb.AppendLine("Function Code:");
            for (int i = 0; i < unit.Instructions.Count; i++)
            {
                sb.AppendLine($"  {i,4}: {unit.Instructions[i]}");
            }
            sb.AppendLine();
        }

        if (unit.InitializationCode.Count > 0)
        {
            sb.AppendLine("Initialization Code:");
            for (int i = 0; i < unit.InitializationCode.Count; i++)
            {
                sb.AppendLine($"  {i,4}: {unit.InitializationCode[i]}");
            }
            sb.AppendLine();
        }

        if (unit.FinalizationCode.Count > 0)
        {
            sb.AppendLine("Finalization Code:");
            for (int i = 0; i < unit.FinalizationCode.Count; i++)
            {
                sb.AppendLine($"  {i,4}: {unit.FinalizationCode[i]}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
