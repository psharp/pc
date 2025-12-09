using Xunit;
using PascalCompiler;
using System.Collections.Generic;

namespace PascalCompiler.Tests;

public class ParserTests
{
    private Parser CreateParser(string source)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();
        return new Parser(tokens);
    }

    [Fact]
    public void ParseProgram_SimpleProgram_ReturnsValidProgramNode()
    {
        // Arrange
        var source = "program Test; begin end.";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        Assert.NotNull(program);
        Assert.Equal("Test", program.Name);
        Assert.Empty(program.Variables);
        Assert.Empty(program.Procedures);
        Assert.Empty(program.Functions);
        Assert.NotNull(program.Block);
    }

    [Fact]
    public void ParseProgram_WithVariables_ReturnsCorrectVariables()
    {
        // Arrange
        var source = @"
            program Test;
            var
                x, y : integer;
                name : string;
            begin
            end.
        ";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        Assert.Equal(2, program.Variables.Count);
        Assert.Equal(2, program.Variables[0].Names.Count);
        Assert.Contains("x", program.Variables[0].Names);
        Assert.Contains("y", program.Variables[0].Names);
        Assert.Equal("integer", program.Variables[0].Type);
        Assert.Single(program.Variables[1].Names);
        Assert.Contains("name", program.Variables[1].Names);
        Assert.Equal("string", program.Variables[1].Type);
    }

    [Fact]
    public void ParseProgram_WithProcedure_ReturnsProcedureDeclaration()
    {
        // Arrange
        var source = @"
            program Test;
            procedure Greet(name : string);
            begin
                writeln(name)
            end;
            begin
            end.
        ";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        Assert.Single(program.Procedures);
        Assert.Equal("Greet", program.Procedures[0].Name);
        Assert.Single(program.Procedures[0].Parameters);
        Assert.Single(program.Procedures[0].Parameters[0].Names);
        Assert.Equal("name", program.Procedures[0].Parameters[0].Names[0]);
        Assert.Equal("string", program.Procedures[0].Parameters[0].Type);
    }

    [Fact]
    public void ParseProgram_WithFunction_ReturnsFunctionDeclaration()
    {
        // Arrange
        var source = @"
            program Test;
            function Add(a, b : integer) : integer;
            begin
                Add := a + b
            end;
            begin
            end.
        ";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        Assert.Single(program.Functions);
        Assert.Equal("Add", program.Functions[0].Name);
        Assert.Equal("integer", program.Functions[0].ReturnType);
        Assert.Single(program.Functions[0].Parameters);
        Assert.Equal(2, program.Functions[0].Parameters[0].Names.Count);
    }

    [Fact]
    public void ParseStatement_Assignment_ReturnsAssignmentNode()
    {
        // Arrange
        var source = "program Test; begin x := 5 end.";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        Assert.Single(program.Block.Statements);
        var assignment = Assert.IsType<AssignmentNode>(program.Block.Statements[0]);
        Assert.Equal("x", assignment.Variable);
        Assert.IsType<NumberNode>(assignment.Expression);
    }

    [Fact]
    public void ParseStatement_IfStatement_ReturnsIfNode()
    {
        // Arrange
        var source = @"
            program Test;
            begin
                if x > 5 then
                    y := 1
                else
                    y := 0
            end.
        ";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        var ifNode = Assert.IsType<IfNode>(program.Block.Statements[0]);
        Assert.NotNull(ifNode.Condition);
        Assert.NotNull(ifNode.ThenBranch);
        Assert.NotNull(ifNode.ElseBranch);
    }

    [Fact]
    public void ParseStatement_WhileLoop_ReturnsWhileNode()
    {
        // Arrange
        var source = @"
            program Test;
            begin
                while x < 10 do
                    x := x + 1
            end.
        ";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        var whileNode = Assert.IsType<WhileNode>(program.Block.Statements[0]);
        Assert.NotNull(whileNode.Condition);
        Assert.NotNull(whileNode.Body);
    }

    [Fact]
    public void ParseStatement_ForLoop_ReturnsForNode()
    {
        // Arrange
        var source = @"
            program Test;
            begin
                for i := 1 to 10 do
                    writeln(i)
            end.
        ";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        var forNode = Assert.IsType<ForNode>(program.Block.Statements[0]);
        Assert.Equal("i", forNode.Variable);
        Assert.False(forNode.IsDownTo);
        Assert.NotNull(forNode.Start);
        Assert.NotNull(forNode.End);
    }

    [Fact]
    public void ParseStatement_ProcedureCall_ReturnsProcedureCallNode()
    {
        // Arrange
        var source = @"
            program Test;
            begin
                Greet('World')
            end.
        ";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        var procCall = Assert.IsType<ProcedureCallNode>(program.Block.Statements[0]);
        Assert.Equal("Greet", procCall.Name);
        Assert.Single(procCall.Arguments);
    }

    [Fact]
    public void ParseExpression_BinaryOperation_ReturnsBinaryOpNode()
    {
        // Arrange
        var source = "program Test; begin x := 5 + 3 end.";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        var assignment = Assert.IsType<AssignmentNode>(program.Block.Statements[0]);
        var binOp = Assert.IsType<BinaryOpNode>(assignment.Expression);
        Assert.Equal(TokenType.PLUS, binOp.Operator);
    }

    [Fact]
    public void ParseExpression_FunctionCall_ReturnsFunctionCallNode()
    {
        // Arrange
        var source = "program Test; begin x := Add(5, 3) end.";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        var assignment = Assert.IsType<AssignmentNode>(program.Block.Statements[0]);
        var funcCall = Assert.IsType<FunctionCallNode>(assignment.Expression);
        Assert.Equal("Add", funcCall.Name);
        Assert.Equal(2, funcCall.Arguments.Count);
    }

    [Fact]
    public void ParseExpression_LogicalOperators_ReturnsCorrectBinaryOp()
    {
        // Arrange
        var source = "program Test; begin if (x > 5) and (y < 10) then z := 1 end.";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        var ifNode = Assert.IsType<IfNode>(program.Block.Statements[0]);
        var binOp = Assert.IsType<BinaryOpNode>(ifNode.Condition);
        Assert.Equal(TokenType.AND, binOp.Operator);
    }

    [Fact]
    public void ParseStatement_CompoundStatement_ReturnsCompoundStatementNode()
    {
        // Arrange
        var source = @"
            program Test;
            begin
                begin
                    x := 1;
                    y := 2
                end
            end.
        ";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        var compound = Assert.IsType<CompoundStatementNode>(program.Block.Statements[0]);
        Assert.Equal(2, compound.Statements.Count);
    }

    [Fact]
    public void ParseStatement_WriteStatement_ReturnsWriteNode()
    {
        // Arrange
        var source = "program Test; begin writeln('Hello', x) end.";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        var writeNode = Assert.IsType<WriteNode>(program.Block.Statements[0]);
        Assert.True(writeNode.NewLine);
        Assert.Equal(2, writeNode.Expressions.Count);
    }

    [Fact]
    public void ParseStatement_ReadStatement_ReturnsReadNode()
    {
        // Arrange
        var source = "program Test; begin readln(x, y) end.";
        var parser = CreateParser(source);

        // Act
        var program = parser.ParseProgram();

        // Assert
        var readNode = Assert.IsType<ReadNode>(program.Block.Statements[0]);
        Assert.Equal(2, readNode.Variables.Count);
    }
}
