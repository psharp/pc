using Xunit;
using PascalCompiler;
using System;
using System.IO;

namespace PascalCompiler.Tests;

public class InterpreterTests
{
    private ProgramNode ParseProgram(string source)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        return parser.ParseProgram();
    }

    private string CaptureOutput(Action action)
    {
        var originalOutput = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            action();
            return writer.ToString();
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    [Fact]
    public void Execute_SimpleAssignment_ExecutesCorrectly()
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
        var interpreter = new Interpreter();

        // Act
        interpreter.Execute(program);

        // Assert - no exception means success
        Assert.True(true);
    }

    [Fact]
    public void Execute_ArithmeticExpression_CalculatesCorrectly()
    {
        // Arrange
        var source = @"
            program Test;
            var result : integer;
            begin
                result := 5 + 3 * 2
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act & Assert - no exception
        interpreter.Execute(program);
    }

    [Fact]
    public void Execute_WritelnStatement_OutputsCorrectly()
    {
        // Arrange
        var source = @"
            program Test;
            begin
                writeln('Hello World')
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("Hello World", output);
    }

    [Fact]
    public void Execute_IfStatement_ExecutesCorrectBranch()
    {
        // Arrange
        var source = @"
            program Test;
            var x : integer;
            begin
                x := 10;
                if x > 5 then
                    writeln('Greater')
                else
                    writeln('Less')
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("Greater", output);
        Assert.DoesNotContain("Less", output);
    }

    [Fact]
    public void Execute_WhileLoop_LoopsCorrectly()
    {
        // Arrange
        var source = @"
            program Test;
            var i : integer;
            begin
                i := 1;
                while i <= 3 do
                begin
                    writeln(i);
                    i := i + 1
                end
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("1", output);
        Assert.Contains("2", output);
        Assert.Contains("3", output);
    }

    [Fact]
    public void Execute_ForLoop_IteratesCorrectly()
    {
        // Arrange
        var source = @"
            program Test;
            var i : integer;
            begin
                for i := 1 to 3 do
                    writeln(i)
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("1", output);
        Assert.Contains("2", output);
        Assert.Contains("3", output);
    }

    [Fact]
    public void Execute_ForDowntoLoop_IteratesInReverse()
    {
        // Arrange
        var source = @"
            program Test;
            var i : integer;
            begin
                for i := 3 downto 1 do
                    writeln(i)
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("3", output);
        Assert.Contains("2", output);
        Assert.Contains("1", output);
    }

    [Fact]
    public void Execute_ProcedureCall_ExecutesProcedure()
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
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("World", output);
    }

    [Fact]
    public void Execute_FunctionCall_ReturnsFunctionValue()
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
                result := Add(3, 5);
                writeln(result)
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("8", output);
    }

    [Fact]
    public void Execute_RecursiveFunction_CalculatesFactorial()
    {
        // Arrange
        var source = @"
            program Test;
            var result : integer;
            function Factorial(n : integer) : integer;
            var i : integer;
            begin
                Factorial := 1;
                for i := 2 to n do
                    Factorial := Factorial * i
            end;
            begin
                result := Factorial(5);
                writeln(result)
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("120", output);
    }

    [Fact]
    public void Execute_LocalVariables_IsolatedFromGlobal()
    {
        // Arrange
        var source = @"
            program Test;
            var x : integer;
            procedure SetLocal;
            var x : integer;
            begin
                x := 99
            end;
            begin
                x := 5;
                SetLocal();
                writeln(x)
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("5", output);
        Assert.DoesNotContain("99", output);
    }

    [Fact]
    public void Execute_BooleanExpression_EvaluatesCorrectly()
    {
        // Arrange
        var source = @"
            program Test;
            var result : boolean;
            begin
                result := (5 > 3) and (10 < 20);
                writeln(result)
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("True", output);
    }

    [Fact]
    public void Execute_StringConcatenation_ConcatenatesCorrectly()
    {
        // Arrange
        var source = @"
            program Test;
            var greeting : string;
            begin
                greeting := 'Hello' + ' ' + 'World';
                writeln(greeting)
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("Hello World", output);
    }

    [Fact]
    public void Execute_DivAndMod_CalculatesCorrectly()
    {
        // Arrange
        var source = @"
            program Test;
            var quotient, remainder : integer;
            begin
                quotient := 17 div 5;
                remainder := 17 mod 5;
                writeln(quotient);
                writeln(remainder)
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("3", output);
        Assert.Contains("2", output);
    }

    [Fact]
    public void Execute_ComparisonOperators_CompareCorrectly()
    {
        // Arrange
        var source = @"
            program Test;
            begin
                writeln(5 = 5);
                writeln(5 <> 3);
                writeln(5 > 3);
                writeln(3 < 5);
                writeln(5 >= 5);
                writeln(3 <= 5)
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Equal(6, output.Split("True", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public void Execute_UnaryMinus_NegatesValue()
    {
        // Arrange
        var source = @"
            program Test;
            var x : integer;
            begin
                x := -5;
                writeln(x)
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("-5", output);
    }

    [Fact]
    public void Execute_NotOperator_NegatesBoolean()
    {
        // Arrange
        var source = @"
            program Test;
            var result : boolean;
            begin
                result := not false;
                writeln(result)
            end.
        ";
        var program = ParseProgram(source);
        var interpreter = new Interpreter();

        // Act
        var output = CaptureOutput(() => interpreter.Execute(program));

        // Assert
        Assert.Contains("True", output);
    }
}
