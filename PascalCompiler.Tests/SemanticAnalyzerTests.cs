using Xunit;
using PascalCompiler;
using System.Linq;

namespace PascalCompiler.Tests;

public class SemanticAnalyzerTests
{
    private ProgramNode ParseProgram(string source)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        return parser.ParseProgram();
    }

    [Fact]
    public void Analyze_ValidProgram_NoErrors()
    {
        // Arrange
        var source = @"
            program Test;
            var x : integer;
            begin
                x := 5
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.Empty(analyzer.Errors);
    }

    [Fact]
    public void Analyze_UndeclaredVariable_ReturnsError()
    {
        // Arrange
        var source = @"
            program Test;
            begin
                x := 5
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.NotEmpty(analyzer.Errors);
        Assert.Contains(analyzer.Errors, e => e.Contains("not declared"));
    }

    [Fact]
    public void Analyze_DuplicateVariable_ReturnsError()
    {
        // Arrange
        var source = @"
            program Test;
            var
                x : integer;
                x : string;
            begin
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.NotEmpty(analyzer.Errors);
        Assert.Contains(analyzer.Errors, e => e.Contains("already declared"));
    }

    [Fact]
    public void Analyze_ValidProcedureCall_NoErrors()
    {
        // Arrange
        var source = @"
            program Test;
            procedure Greet(name : string);
            begin
                writeln(name)
            end;
            begin
                Greet('World')
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.Empty(analyzer.Errors);
    }

    [Fact]
    public void Analyze_UndeclaredProcedure_ReturnsError()
    {
        // Arrange
        var source = @"
            program Test;
            begin
                DoSomething()
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.NotEmpty(analyzer.Errors);
        Assert.Contains(analyzer.Errors, e => e.Contains("Procedure") && e.Contains("not declared"));
    }

    [Fact]
    public void Analyze_ProcedureWithWrongArgumentCount_ReturnsError()
    {
        // Arrange
        var source = @"
            program Test;
            procedure Greet(name : string);
            begin
                writeln(name)
            end;
            begin
                Greet('Hello', 'World')
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.NotEmpty(analyzer.Errors);
        Assert.Contains(analyzer.Errors, e => e.Contains("expects") && e.Contains("arguments"));
    }

    [Fact]
    public void Analyze_ValidFunctionCall_NoErrors()
    {
        // Arrange
        var source = @"
            program Test;
            var result : integer;
            function Add(a, b : integer) : integer;
            begin
                Add := a + b
            end;
            begin
                result := Add(3, 5)
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.Empty(analyzer.Errors);
    }

    [Fact]
    public void Analyze_UndeclaredFunction_ReturnsError()
    {
        // Arrange
        var source = @"
            program Test;
            var result : integer;
            begin
                result := Calculate(5)
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.NotEmpty(analyzer.Errors);
        Assert.Contains(analyzer.Errors, e => e.Contains("Function") && e.Contains("not declared"));
    }

    [Fact]
    public void Analyze_FunctionWithWrongArgumentCount_ReturnsError()
    {
        // Arrange
        var source = @"
            program Test;
            var result : integer;
            function Add(a, b : integer) : integer;
            begin
                Add := a + b
            end;
            begin
                result := Add(3)
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.NotEmpty(analyzer.Errors);
        Assert.Contains(analyzer.Errors, e => e.Contains("expects") && e.Contains("arguments"));
    }

    [Fact]
    public void Analyze_LocalVariablesInProcedure_NoErrors()
    {
        // Arrange
        var source = @"
            program Test;
            procedure Calculate;
            var temp : integer;
            begin
                temp := 5
            end;
            begin
                Calculate()
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.Empty(analyzer.Errors);
    }

    [Fact]
    public void Analyze_DuplicateLocalVariable_ReturnsError()
    {
        // Arrange
        var source = @"
            program Test;
            procedure Calculate(x : integer);
            var x : integer;
            begin
            end;
            begin
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.NotEmpty(analyzer.Errors);
        Assert.Contains(analyzer.Errors, e => e.Contains("already declared"));
    }

    [Fact]
    public void Analyze_FunctionAssignmentToSelf_NoErrors()
    {
        // Arrange
        var source = @"
            program Test;
            function GetValue : integer;
            begin
                GetValue := 42
            end;
            begin
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.Empty(analyzer.Errors);
    }

    [Fact]
    public void Analyze_VariableInExpression_NoErrors()
    {
        // Arrange
        var source = @"
            program Test;
            var x, y, z : integer;
            begin
                z := x + y
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.Empty(analyzer.Errors);
    }

    [Fact]
    public void Analyze_UndeclaredVariableInExpression_ReturnsError()
    {
        // Arrange
        var source = @"
            program Test;
            var x : integer;
            begin
                x := y + 5
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.NotEmpty(analyzer.Errors);
        Assert.Contains(analyzer.Errors, e => e.Contains("'y'") && e.Contains("not declared"));
    }

    [Fact]
    public void Analyze_DuplicateProcedureName_ReturnsError()
    {
        // Arrange
        var source = @"
            program Test;
            procedure Greet;
            begin
            end;
            procedure Greet;
            begin
            end;
            begin
            end.
        ";
        var program = ParseProgram(source);
        var analyzer = new SemanticAnalyzer();

        // Act
        analyzer.Analyze(program);

        // Assert
        Assert.NotEmpty(analyzer.Errors);
        Assert.Contains(analyzer.Errors, e => e.Contains("already declared"));
    }
}
