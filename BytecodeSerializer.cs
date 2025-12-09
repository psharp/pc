using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace PascalCompiler;

public class BytecodeSerializer
{
    private const uint MAGIC_NUMBER = 0x50415343; // "PASC"
    private const ushort VERSION = 1;

    public void SaveToFile(BytecodeProgram program, string filePath)
    {
        using var stream = File.Create(filePath);
        using var writer = new BinaryWriter(stream, Encoding.UTF8);

        // Write header
        writer.Write(MAGIC_NUMBER);
        writer.Write(VERSION);
        writer.Write(program.Name);

        // Write used units
        writer.Write(program.UsedUnits.Count);
        foreach (var usedUnit in program.UsedUnits)
        {
            writer.Write(usedUnit);
        }

        // Write variables
        writer.Write(program.Variables.Count);
        foreach (var variable in program.Variables)
        {
            writer.Write(variable);
        }

        // Write labels
        writer.Write(program.Labels.Count);
        foreach (var kvp in program.Labels)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value);
        }

        // Write enum types
        writer.Write(program.EnumTypes.Count);
        foreach (var kvp in program.EnumTypes)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value.Name);
            writer.Write(kvp.Value.Values.Count);
            foreach (var value in kvp.Value.Values)
            {
                writer.Write(value);
            }
        }

        // Write functions
        writer.Write(program.Functions.Count);
        foreach (var kvp in program.Functions)
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

            writer.Write(kvp.Value.ReturnType ?? "");
        }

        // Write instructions
        writer.Write(program.Instructions.Count);
        foreach (var instruction in program.Instructions)
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

    public BytecodeProgram LoadFromFile(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream, Encoding.UTF8);

        // Read and verify header
        uint magic = reader.ReadUInt32();
        if (magic != MAGIC_NUMBER)
        {
            throw new Exception("Invalid bytecode file format");
        }

        ushort version = reader.ReadUInt16();
        if (version != VERSION)
        {
            throw new Exception($"Unsupported bytecode version: {version}");
        }

        string programName = reader.ReadString();
        var program = new BytecodeProgram(programName);

        // Read used units
        int usedUnitsCount = reader.ReadInt32();
        for (int i = 0; i < usedUnitsCount; i++)
        {
            program.UsedUnits.Add(reader.ReadString());
        }

        // Read variables
        int variableCount = reader.ReadInt32();
        for (int i = 0; i < variableCount; i++)
        {
            program.Variables.Add(reader.ReadString());
        }

        // Read labels
        int labelCount = reader.ReadInt32();
        for (int i = 0; i < labelCount; i++)
        {
            string labelName = reader.ReadString();
            int address = reader.ReadInt32();
            program.Labels[labelName] = address;
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
            program.EnumTypes[key] = new EnumInfo(name, values);
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

            string returnType = reader.ReadString();

            program.Functions[key] = new FunctionInfo(
                name,
                address,
                paramCount,
                paramNames,
                string.IsNullOrEmpty(returnType) ? null : returnType
            );
        }

        // Read instructions
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

            program.Instructions.Add(new Instruction(opCode, operand));
        }

        return program;
    }

    public string DisassembleToString(BytecodeProgram program)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Program: {program.Name}");
        sb.AppendLine();

        if (program.UsedUnits.Count > 0)
        {
            sb.AppendLine("Used Units:");
            foreach (var unit in program.UsedUnits)
            {
                sb.AppendLine($"  {unit}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("Variables:");
        foreach (var variable in program.Variables)
        {
            sb.AppendLine($"  {variable}");
        }
        sb.AppendLine();

        if (program.Functions.Count > 0)
        {
            sb.AppendLine("Functions:");
            foreach (var func in program.Functions.Values)
            {
                sb.AppendLine($"  {func.Name} @ {func.Address} (params: {func.ParameterCount}, return: {func.ReturnType ?? "void"})");
            }
            sb.AppendLine();
        }

        sb.AppendLine("Code:");
        for (int i = 0; i < program.Instructions.Count; i++)
        {
            var instruction = program.Instructions[i];

            // Check if this address has a label
            foreach (var kvp in program.Labels)
            {
                if (kvp.Value == i)
                {
                    sb.AppendLine($"{kvp.Key}:");
                }
            }

            sb.AppendLine($"  {i,4}: {instruction}");
        }

        return sb.ToString();
    }
}
