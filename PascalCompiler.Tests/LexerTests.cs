using Xunit;
using PascalCompiler;
using System.Collections.Generic;
using System.Linq;

namespace PascalCompiler.Tests;

public class LexerTests
{
    [Fact]
    public void Tokenize_SimpleProgram_ReturnsCorrectTokens()
    {
        // Arrange
        var source = "program Test; begin end.";
        var lexer = new Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(7, tokens.Count); // program, Test, ;, begin, end, ., EOF
        Assert.Equal(TokenType.PROGRAM, tokens[0].Type);
        Assert.Equal(TokenType.IDENTIFIER, tokens[1].Type);
        Assert.Equal("Test", tokens[1].Value);
        Assert.Equal(TokenType.SEMICOLON, tokens[2].Type);
        Assert.Equal(TokenType.BEGIN, tokens[3].Type);
        Assert.Equal(TokenType.END, tokens[4].Type);
        Assert.Equal(TokenType.DOT, tokens[5].Type);
        Assert.Equal(TokenType.EOF, tokens[6].Type);
    }

    [Fact]
    public void Tokenize_IntegerLiteral_ReturnsIntegerToken()
    {
        // Arrange
        var source = "123";
        var lexer = new Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count); // number + EOF
        Assert.Equal(TokenType.INTEGER_LITERAL, tokens[0].Type);
        Assert.Equal("123", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_RealLiteral_ReturnsRealToken()
    {
        // Arrange
        var source = "3.14";
        var lexer = new Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.REAL_LITERAL, tokens[0].Type);
        Assert.Equal("3.14", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_StringLiteral_ReturnsStringToken()
    {
        // Arrange
        var source = "'Hello World'";
        var lexer = new Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.STRING_LITERAL, tokens[0].Type);
        Assert.Equal("Hello World", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_Keywords_ReturnsKeywordTokens()
    {
        // Arrange
        var source = "if then else while do for procedure function";
        var lexer = new Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(TokenType.IF, tokens[0].Type);
        Assert.Equal(TokenType.THEN, tokens[1].Type);
        Assert.Equal(TokenType.ELSE, tokens[2].Type);
        Assert.Equal(TokenType.WHILE, tokens[3].Type);
        Assert.Equal(TokenType.DO, tokens[4].Type);
        Assert.Equal(TokenType.FOR, tokens[5].Type);
        Assert.Equal(TokenType.PROCEDURE, tokens[6].Type);
        Assert.Equal(TokenType.FUNCTION, tokens[7].Type);
    }

    [Fact]
    public void Tokenize_Operators_ReturnsOperatorTokens()
    {
        // Arrange
        var source = "+ - * / := = <> < > <= >=";
        var lexer = new Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(TokenType.PLUS, tokens[0].Type);
        Assert.Equal(TokenType.MINUS, tokens[1].Type);
        Assert.Equal(TokenType.MULTIPLY, tokens[2].Type);
        Assert.Equal(TokenType.DIVIDE, tokens[3].Type);
        Assert.Equal(TokenType.ASSIGN, tokens[4].Type);
        Assert.Equal(TokenType.EQUALS, tokens[5].Type);
        Assert.Equal(TokenType.NOT_EQUALS, tokens[6].Type);
        Assert.Equal(TokenType.LESS_THAN, tokens[7].Type);
        Assert.Equal(TokenType.GREATER_THAN, tokens[8].Type);
        Assert.Equal(TokenType.LESS_EQUAL, tokens[9].Type);
        Assert.Equal(TokenType.GREATER_EQUAL, tokens[10].Type);
    }

    [Fact]
    public void Tokenize_Comments_SkipsComments()
    {
        // Arrange
        var source = "program { comment } Test // another comment\nbegin end.";
        var lexer = new Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(6, tokens.Count); // program, Test, begin, end, ., EOF (no semicolon)
        Assert.Equal(TokenType.PROGRAM, tokens[0].Type);
        Assert.Equal(TokenType.IDENTIFIER, tokens[1].Type);
        Assert.Equal("Test", tokens[1].Value);
        Assert.Equal(TokenType.BEGIN, tokens[2].Type);
        Assert.Equal(TokenType.END, tokens[3].Type);
    }

    [Fact]
    public void Tokenize_MultilineComment_SkipsComment()
    {
        // Arrange
        var source = "program (* multi\nline\ncomment *) Test; begin end.";
        var lexer = new Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(7, tokens.Count); // program, Test, ;, begin, end, ., EOF (wait no - program, Test is actually 2, not 3)
        Assert.Equal(TokenType.PROGRAM, tokens[0].Type);
        Assert.Equal(TokenType.IDENTIFIER, tokens[1].Type);
        Assert.Equal("Test", tokens[1].Value);
        Assert.Equal(TokenType.SEMICOLON, tokens[2].Type);
    }

    [Fact]
    public void Tokenize_VariableDeclaration_ReturnsCorrectTokens()
    {
        // Arrange
        var source = "var x, y : integer;";
        var lexer = new Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(TokenType.VAR, tokens[0].Type);
        Assert.Equal(TokenType.IDENTIFIER, tokens[1].Type);
        Assert.Equal("x", tokens[1].Value);
        Assert.Equal(TokenType.COMMA, tokens[2].Type);
        Assert.Equal(TokenType.IDENTIFIER, tokens[3].Type);
        Assert.Equal("y", tokens[3].Value);
        Assert.Equal(TokenType.COLON, tokens[4].Type);
        Assert.Equal(TokenType.INTEGER, tokens[5].Type);
        Assert.Equal(TokenType.SEMICOLON, tokens[6].Type);
    }

    [Fact]
    public void Tokenize_BooleanLiterals_ReturnsBooleanTokens()
    {
        // Arrange
        var source = "true false";
        var lexer = new Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(TokenType.TRUE, tokens[0].Type);
        Assert.Equal(TokenType.FALSE, tokens[1].Type);
    }

    [Fact]
    public void Tokenize_LogicalOperators_ReturnsLogicalTokens()
    {
        // Arrange
        var source = "and or not";
        var lexer = new Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(TokenType.AND, tokens[0].Type);
        Assert.Equal(TokenType.OR, tokens[1].Type);
        Assert.Equal(TokenType.NOT, tokens[2].Type);
    }

    [Fact]
    public void Tokenize_LineAndColumnTracking_ReturnsCorrectPositions()
    {
        // Arrange
        var source = "program\nTest;";
        var lexer = new Lexer(source);

        // Act
        var tokens = lexer.Tokenize();

        // Assert
        Assert.Equal(1, tokens[0].Line);
        Assert.Equal(1, tokens[0].Column);
        Assert.Equal(2, tokens[1].Line); // "Test" is on line 2
        Assert.Equal(1, tokens[1].Column);
    }
}
