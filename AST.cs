/// <summary>
/// Abstract Syntax Tree (AST) node definitions for the Pascal compiler.
/// This file contains all AST node types used to represent Pascal programs in memory.
/// The AST is built by the Parser and consumed by the SemanticAnalyzer, Interpreter, and BytecodeCompiler.
/// </summary>
using System.Collections.Generic;

namespace PascalCompiler;

/// <summary>
/// Abstract base class for all AST nodes.
/// All nodes in the syntax tree inherit from this class.
/// </summary>
public abstract class ASTNode
{
}

/// <summary>
/// Represents a Pascal program (the root node of the AST).
/// Contains all top-level declarations and the main program block.
/// Syntax: program ProgramName; uses Units; var/type declarations; begin...end.
/// </summary>
public class ProgramNode : ASTNode
{
    /// <summary>Gets the name of the program.</summary>
    public string Name { get; }

    /// <summary>Gets the list of units referenced in the 'uses' clause.</summary>
    public List<string> UsedUnits { get; }

    /// <summary>Gets the list of record type definitions.</summary>
    public List<RecordTypeNode> RecordTypes { get; }

    /// <summary>Gets the list of enumeration type definitions.</summary>
    public List<EnumTypeNode> EnumTypes { get; }

    /// <summary>Gets the list of simple variable declarations.</summary>
    public List<VarDeclarationNode> Variables { get; }

    /// <summary>Gets the list of array variable declarations.</summary>
    public List<ArrayVarDeclarationNode> ArrayVariables { get; }

    /// <summary>Gets the list of record variable declarations.</summary>
    public List<RecordVarDeclarationNode> RecordVariables { get; }

    /// <summary>Gets the list of file variable declarations.</summary>
    public List<FileVarDeclarationNode> FileVariables { get; }

    /// <summary>Gets the list of pointer variable declarations.</summary>
    public List<PointerVarDeclarationNode> PointerVariables { get; }

    /// <summary>Gets the list of set variable declarations.</summary>
    public List<SetVarDeclarationNode> SetVariables { get; }

    /// <summary>Gets the list of procedure declarations.</summary>
    public List<ProcedureDeclarationNode> Procedures { get; }

    /// <summary>Gets the list of function declarations.</summary>
    public List<FunctionDeclarationNode> Functions { get; }

    /// <summary>Gets the main program block (begin...end).</summary>
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

/// <summary>
/// Represents a block of statements (begin...end).
/// A block contains a sequence of statements executed sequentially.
/// </summary>
public class BlockNode : ASTNode
{
    /// <summary>Gets the list of statements in this block.</summary>
    public List<StatementNode> Statements { get; }

    public BlockNode(List<StatementNode> statements)
    {
        Statements = statements;
    }
}

/// <summary>
/// Abstract base class for all statement nodes.
/// Statements represent executable code (assignments, loops, conditionals, procedure calls, etc.).
/// </summary>
public abstract class StatementNode : ASTNode
{
}

/// <summary>
/// Represents a variable assignment statement.
/// Syntax: Variable := Expression;
/// </summary>
public class AssignmentNode : StatementNode
{
    /// <summary>Gets the name of the variable being assigned to.</summary>
    public string Variable { get; }

    /// <summary>Gets the expression whose value is assigned to the variable.</summary>
    public ExpressionNode Expression { get; }

    public AssignmentNode(string variable, ExpressionNode expression)
    {
        Variable = variable;
        Expression = expression;
    }
}

/// <summary>
/// Represents an if-then-else conditional statement.
/// Syntax: if Condition then ThenBranch else ElseBranch;
/// The else branch is optional.
/// </summary>
public class IfNode : StatementNode
{
    /// <summary>Gets the boolean condition to evaluate.</summary>
    public ExpressionNode Condition { get; }

    /// <summary>Gets the statement executed when the condition is true.</summary>
    public StatementNode ThenBranch { get; }

    /// <summary>Gets the optional statement executed when the condition is false.</summary>
    public StatementNode? ElseBranch { get; }

    public IfNode(ExpressionNode condition, StatementNode thenBranch, StatementNode? elseBranch = null)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElseBranch = elseBranch;
    }
}

/// <summary>
/// Represents a while loop statement.
/// Syntax: while Condition do Body;
/// Executes Body repeatedly while Condition is true.
/// </summary>
public class WhileNode : StatementNode
{
    /// <summary>Gets the loop condition.</summary>
    public ExpressionNode Condition { get; }

    /// <summary>Gets the statement executed in each iteration.</summary>
    public StatementNode Body { get; }

    public WhileNode(ExpressionNode condition, StatementNode body)
    {
        Condition = condition;
        Body = body;
    }
}

/// <summary>
/// Represents a repeat-until loop statement.
/// Syntax: repeat Statements until Condition;
/// Executes Statements repeatedly until Condition becomes true.
/// The loop body is always executed at least once (post-test loop).
/// </summary>
public class RepeatUntilNode : StatementNode
{
    /// <summary>Gets the list of statements in the loop body.</summary>
    public List<StatementNode> Statements { get; }

    /// <summary>Gets the termination condition (loop exits when this is true).</summary>
    public ExpressionNode Condition { get; }

    public RepeatUntilNode(List<StatementNode> statements, ExpressionNode condition)
    {
        Statements = statements;
        Condition = condition;
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

/// <summary>
/// Represents a case statement (multi-way conditional branch).
/// Syntax: case Expression of Value1: Statement1; Value2: Statement2; else ElseStatement end;
/// The else branch is optional.
/// </summary>
public class CaseNode : StatementNode
{
    /// <summary>Gets the expression to evaluate and match against case values.</summary>
    public ExpressionNode Expression { get; }

    /// <summary>Gets the list of case branches (value/range and corresponding statement).</summary>
    public List<CaseBranch> Branches { get; }

    /// <summary>Gets the optional else statement executed when no case matches.</summary>
    public StatementNode? ElseBranch { get; }

    public CaseNode(ExpressionNode expression, List<CaseBranch> branches, StatementNode? elseBranch = null)
    {
        Expression = expression;
        Branches = branches;
        ElseBranch = elseBranch;
    }
}

/// <summary>
/// Represents a with statement for record field access.
/// Syntax: with RecordVar do Statement;
/// Allows accessing record fields without qualifying them with the record variable name.
/// </summary>
public class WithNode : StatementNode
{
    /// <summary>Gets the record variable to access.</summary>
    public string RecordVariable { get; }

    /// <summary>Gets the statement executed within the with scope.</summary>
    public StatementNode Statement { get; }

    public WithNode(string recordVariable, StatementNode statement)
    {
        RecordVariable = recordVariable;
        Statement = statement;
    }
}

/// <summary>
/// Represents a goto statement that jumps to a labeled statement.
/// Syntax: goto LabelName;
/// </summary>
public class GotoNode : StatementNode
{
    /// <summary>Gets the label to jump to.</summary>
    public string Label { get; }

    public GotoNode(string label)
    {
        Label = label;
    }
}

/// <summary>
/// Represents a labeled statement.
/// Syntax: LabelName: Statement;
/// </summary>
public class LabeledStatementNode : StatementNode
{
    /// <summary>Gets the label name.</summary>
    public string Label { get; }

    /// <summary>Gets the statement after the label.</summary>
    public StatementNode Statement { get; }

    public LabeledStatementNode(string label, StatementNode statement)
    {
        Label = label;
        Statement = statement;
    }
}

/// <summary>
/// Represents a single branch in a case statement.
/// Each branch contains one or more values/ranges and the statement to execute if matched.
/// </summary>
public class CaseBranch
{
    /// <summary>Gets the list of values or ranges to match against.</summary>
    public List<CaseLabel> Labels { get; }

    /// <summary>Gets the statement to execute if any label matches.</summary>
    public StatementNode Statement { get; }

    public CaseBranch(List<CaseLabel> labels, StatementNode statement)
    {
        Labels = labels;
        Statement = statement;
    }
}

/// <summary>
/// Represents a single value or range in a case branch.
/// Can be either a single value (e.g., 1) or a range (e.g., 1..5).
/// </summary>
public class CaseLabel
{
    /// <summary>Gets the start value of the range (or the single value if not a range).</summary>
    public ExpressionNode StartValue { get; }

    /// <summary>Gets the end value of the range (null for single values).</summary>
    public ExpressionNode? EndValue { get; }

    /// <summary>Gets whether this label represents a range (start..end).</summary>
    public bool IsRange => EndValue != null;

    public CaseLabel(ExpressionNode startValue, ExpressionNode? endValue = null)
    {
        StartValue = startValue;
        EndValue = endValue;
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

/// <summary>
/// Abstract base class for all expression nodes.
/// Expressions represent values and computations (literals, variables, operations, function calls, etc.).
/// </summary>
public abstract class ExpressionNode : ASTNode
{
}

/// <summary>
/// Represents a binary operation (e.g., a + b, x < y, p and q).
/// Binary operations have two operands and an operator.
/// </summary>
public class BinaryOpNode : ExpressionNode
{
    /// <summary>Gets the left operand expression.</summary>
    public ExpressionNode Left { get; }

    /// <summary>Gets the operator token type (PLUS, MINUS, MULTIPLY, LESS_THAN, etc.).</summary>
    public TokenType Operator { get; }

    /// <summary>Gets the right operand expression.</summary>
    public ExpressionNode Right { get; }

    public BinaryOpNode(ExpressionNode left, TokenType op, ExpressionNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
}

/// <summary>
/// Represents a unary operation (e.g., -x, not b).
/// Unary operations have one operand and an operator.
/// </summary>
public class UnaryOpNode : ExpressionNode
{
    /// <summary>Gets the operator token type (MINUS, NOT).</summary>
    public TokenType Operator { get; }

    /// <summary>Gets the operand expression.</summary>
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

// Page procedure - outputs form feed character (ISO 7185)
public class PageNode : StatementNode
{
    public string? FileVariable { get; }

    public PageNode(string? fileVariable = null)
    {
        FileVariable = fileVariable;
    }
}

// Get procedure - advances file buffer (ISO 7185)
public class GetNode : StatementNode
{
    public string FileVariable { get; }

    public GetNode(string fileVariable)
    {
        FileVariable = fileVariable;
    }
}

// Put procedure - writes file buffer (ISO 7185)
public class PutNode : StatementNode
{
    public string FileVariable { get; }

    public PutNode(string fileVariable)
    {
        FileVariable = fileVariable;
    }
}

// Pack procedure - packs unpacked array into packed array (ISO 7185)
public class PackNode : StatementNode
{
    public string UnpackedArray { get; }
    public ExpressionNode StartIndex { get; }
    public string PackedArray { get; }

    public PackNode(string unpackedArray, ExpressionNode startIndex, string packedArray)
    {
        UnpackedArray = unpackedArray;
        StartIndex = startIndex;
        PackedArray = packedArray;
    }
}

// Unpack procedure - unpacks packed array into unpacked array (ISO 7185)
public class UnpackNode : StatementNode
{
    public string PackedArray { get; }
    public string UnpackedArray { get; }
    public ExpressionNode StartIndex { get; }

    public UnpackNode(string packedArray, string unpackedArray, ExpressionNode startIndex)
    {
        PackedArray = packedArray;
        UnpackedArray = unpackedArray;
        StartIndex = startIndex;
    }
}

// File buffer variable access (e.g., f^) (ISO 7185)
public class FileBufferNode : ExpressionNode
{
    public string FileVariable { get; }

    public FileBufferNode(string fileVariable)
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
/// <summary>
/// Represents a Pascal unit/module (the root node for unit files).
/// Units provide code organization with interface (public) and implementation (private) sections.
/// Syntax: unit UnitName; interface uses...; declarations; implementation declarations; end.
/// </summary>
public class UnitNode : ASTNode
{
    /// <summary>Gets the name of the unit.</summary>
    public string Name { get; }

    /// <summary>Gets the list of units referenced in the 'uses' clause.</summary>
    public List<string> UsedUnits { get; }

    /// <summary>Gets record types defined in the interface section (public).</summary>
    public List<RecordTypeNode> InterfaceRecordTypes { get; }

    /// <summary>Gets enumeration types defined in the interface section (public).</summary>
    public List<EnumTypeNode> InterfaceEnumTypes { get; }

    /// <summary>Gets variables declared in the interface section (public).</summary>
    public List<VarDeclarationNode> InterfaceVariables { get; }

    /// <summary>Gets procedure headers declared in the interface section (public).</summary>
    public List<ProcedureDeclarationNode> InterfaceProcedures { get; }

    /// <summary>Gets function headers declared in the interface section (public).</summary>
    public List<FunctionDeclarationNode> InterfaceFunctions { get; }

    /// <summary>Gets record types defined in the implementation section (private).</summary>
    public List<RecordTypeNode> ImplementationRecordTypes { get; }

    /// <summary>Gets enumeration types defined in the implementation section (private).</summary>
    public List<EnumTypeNode> ImplementationEnumTypes { get; }

    /// <summary>Gets variables declared in the implementation section (private).</summary>
    public List<VarDeclarationNode> ImplementationVariables { get; }

    /// <summary>Gets array variables declared in the implementation section (private).</summary>
    public List<ArrayVarDeclarationNode> ImplementationArrayVariables { get; }

    /// <summary>Gets record variables declared in the implementation section (private).</summary>
    public List<RecordVarDeclarationNode> ImplementationRecordVariables { get; }

    /// <summary>Gets file variables declared in the implementation section (private).</summary>
    public List<FileVarDeclarationNode> ImplementationFileVariables { get; }

    /// <summary>Gets pointer variables declared in the implementation section (private).</summary>
    public List<PointerVarDeclarationNode> ImplementationPointerVariables { get; }

    /// <summary>Gets set variables declared in the implementation section (private).</summary>
    public List<SetVarDeclarationNode> ImplementationSetVariables { get; }

    /// <summary>Gets procedure implementations in the implementation section.</summary>
    public List<ProcedureDeclarationNode> ImplementationProcedures { get; }

    /// <summary>Gets function implementations in the implementation section.</summary>
    public List<FunctionDeclarationNode> ImplementationFunctions { get; }

    /// <summary>Gets the optional initialization block executed when the unit is loaded.</summary>
    public BlockNode? InitializationBlock { get; }

    /// <summary>Gets the optional finalization block executed when the program exits.</summary>
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
