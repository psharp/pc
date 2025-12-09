using System.Collections.Generic;

namespace PascalCompiler;

public abstract class ASTNode
{
}

public class ProgramNode : ASTNode
{
    public string Name { get; }
    public List<string> UsedUnits { get; }
    public List<RecordTypeNode> RecordTypes { get; }
    public List<EnumTypeNode> EnumTypes { get; }
    public List<VarDeclarationNode> Variables { get; }
    public List<ArrayVarDeclarationNode> ArrayVariables { get; }
    public List<RecordVarDeclarationNode> RecordVariables { get; }
    public List<FileVarDeclarationNode> FileVariables { get; }
    public List<PointerVarDeclarationNode> PointerVariables { get; }
    public List<SetVarDeclarationNode> SetVariables { get; }
    public List<ProcedureDeclarationNode> Procedures { get; }
    public List<FunctionDeclarationNode> Functions { get; }
    public BlockNode Block { get; }

    public ProgramNode(string name, List<string> usedUnits, List<RecordTypeNode> recordTypes, List<EnumTypeNode> enumTypes,
        List<VarDeclarationNode> variables, List<ArrayVarDeclarationNode> arrayVariables,
        List<RecordVarDeclarationNode> recordVariables, List<FileVarDeclarationNode> fileVariables,
        List<PointerVarDeclarationNode> pointerVariables, List<SetVarDeclarationNode> setVariables,
        List<ProcedureDeclarationNode> procedures, List<FunctionDeclarationNode> functions,
        BlockNode block)
    {
        Name = name;
        UsedUnits = usedUnits;
        RecordTypes = recordTypes;
        EnumTypes = enumTypes;
        Variables = variables;
        ArrayVariables = arrayVariables;
        RecordVariables = recordVariables;
        FileVariables = fileVariables;
        PointerVariables = pointerVariables;
        SetVariables = setVariables;
        Procedures = procedures;
        Functions = functions;
        Block = block;
    }
}

public class BlockNode : ASTNode
{
    public List<StatementNode> Statements { get; }

    public BlockNode(List<StatementNode> statements)
    {
        Statements = statements;
    }
}

public abstract class StatementNode : ASTNode
{
}

public class AssignmentNode : StatementNode
{
    public string Variable { get; }
    public ExpressionNode Expression { get; }

    public AssignmentNode(string variable, ExpressionNode expression)
    {
        Variable = variable;
        Expression = expression;
    }
}

public class IfNode : StatementNode
{
    public ExpressionNode Condition { get; }
    public StatementNode ThenBranch { get; }
    public StatementNode? ElseBranch { get; }

    public IfNode(ExpressionNode condition, StatementNode thenBranch, StatementNode? elseBranch = null)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
    }
}

public class WhileNode : StatementNode
{
    public ExpressionNode Condition { get; }
    public StatementNode Body { get; }

    public WhileNode(ExpressionNode condition, StatementNode body)
    {
        Condition = condition;
        Body = body;
    }
}

public class ForNode : StatementNode
{
    public string Variable { get; }
    public ExpressionNode Start { get; }
    public ExpressionNode End { get; }
    public bool IsDownTo { get; }
    public StatementNode Body { get; }

    public ForNode(string variable, ExpressionNode start, ExpressionNode end, bool isDownTo, StatementNode body)
    {
        Variable = variable;
        Start = start;
        End = end;
        IsDownTo = isDownTo;
        Body = body;
    }
}

public class WriteNode : StatementNode
{
    public List<ExpressionNode> Expressions { get; }
    public bool NewLine { get; }

    public WriteNode(List<ExpressionNode> expressions, bool newLine = true)
    {
        Expressions = expressions;
        NewLine = newLine;
    }
}

public class ReadNode : StatementNode
{
    public List<string> Variables { get; }

    public ReadNode(List<string> variables)
    {
        Variables = variables;
    }
}

public class VarDeclarationNode : ASTNode
{
    public List<string> Names { get; }
    public string Type { get; }

    public VarDeclarationNode(List<string> names, string type)
    {
        Names = names;
        Type = type;
    }
}

public abstract class ExpressionNode : ASTNode
{
}

public class BinaryOpNode : ExpressionNode
{
    public ExpressionNode Left { get; }
    public TokenType Operator { get; }
    public ExpressionNode Right { get; }

    public BinaryOpNode(ExpressionNode left, TokenType op, ExpressionNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
}

public class UnaryOpNode : ExpressionNode
{
    public TokenType Operator { get; }
    public ExpressionNode Operand { get; }

    public UnaryOpNode(TokenType op, ExpressionNode operand)
    {
        Operator = op;
        Operand = operand;
    }
}

public class NumberNode : ExpressionNode
{
    public object Value { get; }

    public NumberNode(object value)
    {
        Value = value;
    }
}

public class StringNode : ExpressionNode
{
    public string Value { get; }

    public StringNode(string value)
    {
        Value = value;
    }
}

public class BooleanNode : ExpressionNode
{
    public bool Value { get; }

    public BooleanNode(bool value)
    {
        Value = value;
    }
}

public class VariableNode : ExpressionNode
{
    public string Name { get; }

    public VariableNode(string name)
    {
        Name = name;
    }
}

public class CompoundStatementNode : StatementNode
{
    public List<StatementNode> Statements { get; }

    public CompoundStatementNode(List<StatementNode> statements)
    {
        Statements = statements;
    }
}

// Parameter for procedure/function declarations
public class ParameterNode : ASTNode
{
    public List<string> Names { get; }
    public string Type { get; }
    public bool IsVar { get; }

    public ParameterNode(List<string> names, string type, bool isVar = false)
    {
        Names = names;
        Type = type;
        IsVar = isVar;
    }
}

// Procedure declaration
public class ProcedureDeclarationNode : ASTNode
{
    public string Name { get; }
    public List<ParameterNode> Parameters { get; }
    public List<VarDeclarationNode> LocalVariables { get; }
    public List<ProcedureDeclarationNode> NestedProcedures { get; }
    public List<FunctionDeclarationNode> NestedFunctions { get; }
    public BlockNode Block { get; }

    public ProcedureDeclarationNode(string name, List<ParameterNode> parameters,
        List<VarDeclarationNode> localVariables, BlockNode block,
        List<ProcedureDeclarationNode>? nestedProcedures = null,
        List<FunctionDeclarationNode>? nestedFunctions = null)
    {
        Name = name;
        Parameters = parameters;
        LocalVariables = localVariables;
        NestedProcedures = nestedProcedures ?? new List<ProcedureDeclarationNode>();
        NestedFunctions = nestedFunctions ?? new List<FunctionDeclarationNode>();
        Block = block;
    }
}

// Function declaration
public class FunctionDeclarationNode : ASTNode
{
    public string Name { get; }
    public List<ParameterNode> Parameters { get; }
    public string ReturnType { get; }
    public List<VarDeclarationNode> LocalVariables { get; }
    public List<ProcedureDeclarationNode> NestedProcedures { get; }
    public List<FunctionDeclarationNode> NestedFunctions { get; }
    public BlockNode Block { get; }

    public FunctionDeclarationNode(string name, List<ParameterNode> parameters,
        string returnType, List<VarDeclarationNode> localVariables, BlockNode block,
        List<ProcedureDeclarationNode>? nestedProcedures = null,
        List<FunctionDeclarationNode>? nestedFunctions = null)
    {
        Name = name;
        Parameters = parameters;
        ReturnType = returnType;
        LocalVariables = localVariables;
        NestedProcedures = nestedProcedures ?? new List<ProcedureDeclarationNode>();
        NestedFunctions = nestedFunctions ?? new List<FunctionDeclarationNode>();
        Block = block;
    }
}

// Procedure call statement
public class ProcedureCallNode : StatementNode
{
    public string Name { get; }
    public List<ExpressionNode> Arguments { get; }

    public ProcedureCallNode(string name, List<ExpressionNode> arguments)
    {
        Name = name;
        Arguments = arguments;
    }
}

// Function call expression
public class FunctionCallNode : ExpressionNode
{
    public string Name { get; }
    public List<ExpressionNode> Arguments { get; }

    public FunctionCallNode(string name, List<ExpressionNode> arguments)
    {
        Name = name;
        Arguments = arguments;
    }
}

// Array type declaration
public class ArrayTypeNode : ASTNode
{
    public List<(int LowerBound, int UpperBound)> Dimensions { get; }
    public string ElementType { get; }

    // Legacy constructor for single-dimensional arrays (backward compatibility)
    public ArrayTypeNode(int lowerBound, int upperBound, string elementType)
        : this(new List<(int, int)> { (lowerBound, upperBound) }, elementType)
    {
    }

    // New constructor for multi-dimensional arrays
    public ArrayTypeNode(List<(int LowerBound, int UpperBound)> dimensions, string elementType)
    {
        Dimensions = dimensions;
        ElementType = elementType;
    }

    // Helper properties for backward compatibility
    public int LowerBound => Dimensions.Count > 0 ? Dimensions[0].LowerBound : 0;
    public int UpperBound => Dimensions.Count > 0 ? Dimensions[0].UpperBound : 0;
    public int DimensionCount => Dimensions.Count;
}

// Array variable declaration (e.g., arr: array[1..10] of integer)
public class ArrayVarDeclarationNode : ASTNode
{
    public List<string> Names { get; }
    public ArrayTypeNode ArrayType { get; }

    public ArrayVarDeclarationNode(List<string> names, ArrayTypeNode arrayType)
    {
        Names = names;
        ArrayType = arrayType;
    }
}

// Array access expression (e.g., arr[5] or arr[2, 3])
public class ArrayAccessNode : ExpressionNode
{
    public string ArrayName { get; }
    public List<ExpressionNode> Indices { get; }

    // Legacy constructor for single index (backward compatibility)
    public ArrayAccessNode(string arrayName, ExpressionNode index)
        : this(arrayName, new List<ExpressionNode> { index })
    {
    }

    // New constructor for multiple indices
    public ArrayAccessNode(string arrayName, List<ExpressionNode> indices)
    {
        ArrayName = arrayName;
        Indices = indices;
    }

    // Helper property for backward compatibility
    public ExpressionNode Index => Indices.Count > 0 ? Indices[0] : new NumberNode(0);
}

// Array assignment statement (e.g., arr[5] := 10 or arr[2, 3] := 10)
public class ArrayAssignmentNode : StatementNode
{
    public string ArrayName { get; }
    public List<ExpressionNode> Indices { get; }
    public ExpressionNode Value { get; }

    // Legacy constructor for single index (backward compatibility)
    public ArrayAssignmentNode(string arrayName, ExpressionNode index, ExpressionNode value)
        : this(arrayName, new List<ExpressionNode> { index }, value)
    {
    }

    // New constructor for multiple indices
    public ArrayAssignmentNode(string arrayName, List<ExpressionNode> indices, ExpressionNode value)
    {
        ArrayName = arrayName;
        Indices = indices;
        Value = value;
    }

    // Helper property for backward compatibility
    public ExpressionNode Index => Indices.Count > 0 ? Indices[0] : new NumberNode(0);
}

// Record field declaration
public class RecordFieldNode : ASTNode
{
    public List<string> Names { get; }
    public string Type { get; }

    public RecordFieldNode(List<string> names, string type)
    {
        Names = names;
        Type = type;
    }
}

// Record type declaration
public class RecordTypeNode : ASTNode
{
    public string Name { get; }
    public List<RecordFieldNode> Fields { get; }

    public RecordTypeNode(string name, List<RecordFieldNode> fields)
    {
        Name = name;
        Fields = fields;
    }
}

// Record variable declaration (e.g., person: PersonRecord)
public class RecordVarDeclarationNode : ASTNode
{
    public List<string> Names { get; }
    public string RecordTypeName { get; }

    public RecordVarDeclarationNode(List<string> names, string recordTypeName)
    {
        Names = names;
        RecordTypeName = recordTypeName;
    }
}

// Record field access expression (e.g., person.name)
public class RecordAccessNode : ExpressionNode
{
    public string RecordName { get; }
    public string FieldName { get; }

    public RecordAccessNode(string recordName, string fieldName)
    {
        RecordName = recordName;
        FieldName = fieldName;
    }
}

// Record field assignment statement (e.g., person.name := 'John')
public class RecordAssignmentNode : StatementNode
{
    public string RecordName { get; }
    public string FieldName { get; }
    public ExpressionNode Value { get; }

    public RecordAssignmentNode(string recordName, string fieldName, ExpressionNode value)
    {
        RecordName = recordName;
        FieldName = fieldName;
        Value = value;
    }
}

// Array element field access (e.g., students[1].name)
public class ArrayRecordAccessNode : ExpressionNode
{
    public string ArrayName { get; }
    public ExpressionNode Index { get; }
    public string FieldName { get; }

    public ArrayRecordAccessNode(string arrayName, ExpressionNode index, string fieldName)
    {
        ArrayName = arrayName;
        Index = index;
        FieldName = fieldName;
    }
}

// Array element field assignment (e.g., students[1].name := 'John')
public class ArrayRecordAssignmentNode : StatementNode
{
    public string ArrayName { get; }
    public ExpressionNode Index { get; }
    public string FieldName { get; }
    public ExpressionNode Value { get; }

    public ArrayRecordAssignmentNode(string arrayName, ExpressionNode index, string fieldName, ExpressionNode value)
    {
        ArrayName = arrayName;
        Index = index;
        FieldName = fieldName;
        Value = value;
    }
}

// File variable declaration (e.g., f: text; or f: file of integer)
public class FileVarDeclarationNode : ASTNode
{
    public List<string> Names { get; }
    public bool IsTextFile { get; }
    public string? ElementType { get; }

    public FileVarDeclarationNode(List<string> names, bool isTextFile, string? elementType = null)
    {
        Names = names;
        IsTextFile = isTextFile;
        ElementType = elementType;
    }
}

// Assign file to filename (e.g., Assign(f, 'output.txt'))
public class FileAssignNode : StatementNode
{
    public string FileVariable { get; }
    public ExpressionNode FileName { get; }

    public FileAssignNode(string fileVariable, ExpressionNode fileName)
    {
        FileVariable = fileVariable;
        FileName = fileName;
    }
}

// Reset file for reading (e.g., Reset(f))
public class FileResetNode : StatementNode
{
    public string FileVariable { get; }

    public FileResetNode(string fileVariable)
    {
        FileVariable = fileVariable;
    }
}

// Rewrite file for writing (e.g., Rewrite(f))
public class FileRewriteNode : StatementNode
{
    public string FileVariable { get; }

    public FileRewriteNode(string fileVariable)
    {
        FileVariable = fileVariable;
    }
}

// Close file (e.g., Close(f))
public class FileCloseNode : StatementNode
{
    public string FileVariable { get; }

    public FileCloseNode(string fileVariable)
    {
        FileVariable = fileVariable;
    }
}

// Read from file (e.g., Read(f, x) or Readln(f, x))
public class FileReadNode : StatementNode
{
    public string FileVariable { get; }
    public List<string> Variables { get; }
    public bool ReadLine { get; }

    public FileReadNode(string fileVariable, List<string> variables, bool readLine = false)
    {
        FileVariable = fileVariable;
        Variables = variables;
        ReadLine = readLine;
    }
}

// Write to file (e.g., Write(f, x) or Writeln(f, x))
public class FileWriteNode : StatementNode
{
    public string FileVariable { get; }
    public List<ExpressionNode> Expressions { get; }
    public bool WriteLine { get; }

    public FileWriteNode(string fileVariable, List<ExpressionNode> expressions, bool writeLine = false)
    {
        FileVariable = fileVariable;
        Expressions = expressions;
        WriteLine = writeLine;
    }
}

// EOF function (e.g., EOF(f))
public class FileEofNode : ExpressionNode
{
    public string FileVariable { get; }

    public FileEofNode(string fileVariable)
    {
        FileVariable = fileVariable;
    }
}

// Pointer variable declaration (e.g., ptr : ^integer)
public class PointerVarDeclarationNode : ASTNode
{
    public List<string> Names { get; }
    public string PointedType { get; }

    public PointerVarDeclarationNode(List<string> names, string pointedType)
    {
        Names = names;
        PointedType = pointedType;
    }
}

// Nil literal
public class NilNode : ExpressionNode
{
}

// Pointer dereference (e.g., ptr^)
public class PointerDereferenceNode : ExpressionNode
{
    public ExpressionNode Pointer { get; }

    public PointerDereferenceNode(ExpressionNode pointer)
    {
        Pointer = pointer;
    }
}

// Address-of operator (e.g., @variable)
public class AddressOfNode : ExpressionNode
{
    public string VariableName { get; }

    public AddressOfNode(string variableName)
    {
        VariableName = variableName;
    }
}

// New(ptr) procedure call
public class NewNode : StatementNode
{
    public string PointerVariable { get; }

    public NewNode(string pointerVariable)
    {
        PointerVariable = pointerVariable;
    }
}

// Dispose(ptr) procedure call
public class DisposeNode : StatementNode
{
    public string PointerVariable { get; }

    public DisposeNode(string pointerVariable)
    {
        PointerVariable = pointerVariable;
    }
}

// Assignment to dereferenced pointer (e.g., ptr^ := value)
public class PointerAssignmentNode : StatementNode
{
    public ExpressionNode Pointer { get; }
    public ExpressionNode Value { get; }

    public PointerAssignmentNode(ExpressionNode pointer, ExpressionNode value)
    {
        Pointer = pointer;
        Value = value;
    }
}

// Enumeration type declaration (e.g., Color = (Red, Green, Blue))
public class EnumTypeNode : ASTNode
{
    public string Name { get; }
    public List<string> Values { get; }

    public EnumTypeNode(string name, List<string> values)
    {
        Name = name;
        Values = values;
    }
}

// Set type declaration (e.g., Colors : set of Color)
public class SetVarDeclarationNode : ASTNode
{
    public List<string> Names { get; }
    public string ElementType { get; }

    public SetVarDeclarationNode(List<string> names, string elementType)
    {
        Names = names;
        ElementType = elementType;
    }
}

// Set literal (e.g., [Red, Blue, Green])
public class SetLiteralNode : ExpressionNode
{
    public List<ExpressionNode> Elements { get; }

    public SetLiteralNode(List<ExpressionNode> elements)
    {
        Elements = elements;
    }
}

// Set membership test (e.g., value in setVar)
public class InNode : ExpressionNode
{
    public ExpressionNode Value { get; }
    public ExpressionNode SetExpression { get; }

    public InNode(ExpressionNode value, ExpressionNode setExpression)
    {
        Value = value;
        SetExpression = setExpression;
    }
}

// Unit declaration node
public class UnitNode : ASTNode
{
    public string Name { get; }
    public List<string> UsedUnits { get; }
    public List<RecordTypeNode> InterfaceRecordTypes { get; }
    public List<EnumTypeNode> InterfaceEnumTypes { get; }
    public List<VarDeclarationNode> InterfaceVariables { get; }
    public List<ProcedureDeclarationNode> InterfaceProcedures { get; }
    public List<FunctionDeclarationNode> InterfaceFunctions { get; }
    public List<RecordTypeNode> ImplementationRecordTypes { get; }
    public List<EnumTypeNode> ImplementationEnumTypes { get; }
    public List<VarDeclarationNode> ImplementationVariables { get; }
    public List<ArrayVarDeclarationNode> ImplementationArrayVariables { get; }
    public List<RecordVarDeclarationNode> ImplementationRecordVariables { get; }
    public List<FileVarDeclarationNode> ImplementationFileVariables { get; }
    public List<PointerVarDeclarationNode> ImplementationPointerVariables { get; }
    public List<SetVarDeclarationNode> ImplementationSetVariables { get; }
    public List<ProcedureDeclarationNode> ImplementationProcedures { get; }
    public List<FunctionDeclarationNode> ImplementationFunctions { get; }
    public BlockNode? InitializationBlock { get; }
    public BlockNode? FinalizationBlock { get; }

    public UnitNode(string name, List<string> usedUnits,
        List<RecordTypeNode> interfaceRecordTypes, List<EnumTypeNode> interfaceEnumTypes,
        List<VarDeclarationNode> interfaceVariables,
        List<ProcedureDeclarationNode> interfaceProcedures, List<FunctionDeclarationNode> interfaceFunctions,
        List<RecordTypeNode> implementationRecordTypes, List<EnumTypeNode> implementationEnumTypes,
        List<VarDeclarationNode> implementationVariables, List<ArrayVarDeclarationNode> implementationArrayVariables,
        List<RecordVarDeclarationNode> implementationRecordVariables, List<FileVarDeclarationNode> implementationFileVariables,
        List<PointerVarDeclarationNode> implementationPointerVariables, List<SetVarDeclarationNode> implementationSetVariables,
        List<ProcedureDeclarationNode> implementationProcedures, List<FunctionDeclarationNode> implementationFunctions,
        BlockNode? initializationBlock = null, BlockNode? finalizationBlock = null)
    {
        Name = name;
        UsedUnits = usedUnits;
        InterfaceRecordTypes = interfaceRecordTypes;
        InterfaceEnumTypes = interfaceEnumTypes;
        InterfaceVariables = interfaceVariables;
        InterfaceProcedures = interfaceProcedures;
        InterfaceFunctions = interfaceFunctions;
        ImplementationRecordTypes = implementationRecordTypes;
        ImplementationEnumTypes = implementationEnumTypes;
        ImplementationVariables = implementationVariables;
        ImplementationArrayVariables = implementationArrayVariables;
        ImplementationRecordVariables = implementationRecordVariables;
        ImplementationFileVariables = implementationFileVariables;
        ImplementationPointerVariables = implementationPointerVariables;
        ImplementationSetVariables = implementationSetVariables;
        ImplementationProcedures = implementationProcedures;
        ImplementationFunctions = implementationFunctions;
        InitializationBlock = initializationBlock;
        FinalizationBlock = finalizationBlock;
    }
}
