using Xunit;
using PascalCompiler;
using System;
using System.IO;

namespace PascalCompiler.Tests;

public class IntegrationTests
{
    private string ExecuteProgram(string source, string? input = null)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();
        var parser = new Parser(tokens);
        var program = parser.ParseProgram();

        var analyzer = new SemanticAnalyzer();
        analyzer.Analyze(program);

        if (analyzer.Errors.Count > 0)
        {
            throw new Exception($"Semantic errors: {string.Join(", ", analyzer.Errors)}");
        }

        var originalOutput = Console.Out;
        var originalInput = Console.In;
        using var outputWriter = new StringWriter();

        try
        {
            Console.SetOut(outputWriter);

            if (input != null)
            {
                using var inputReader = new StringReader(input);
                Console.SetIn(inputReader);
            }

            var interpreter = new Interpreter();
            interpreter.Execute(program);

            return outputWriter.ToString();
        }
        finally
        {
            Console.SetOut(originalOutput);
            Console.SetIn(originalInput);
        }
    }

    [Fact]
    public void FullPipeline_SimpleProgram_ExecutesSuccessfully()
    {
        // Arrange
        var source = @"
            program Simple;
            var x, y, sum : integer;
            begin
                x := 5;
                y := 10;
                sum := x + y;
                writeln('The sum is: ');
                writeln(sum)
            end.
        ";

        // Act
        var output = ExecuteProgram(source);

        // Assert
        Assert.Contains("The sum is:", output);
        Assert.Contains("15", output);
    }

    [Fact]
    public void FullPipeline_FibonacciWithProcedures_ExecutesSuccessfully()
    {
        // Arrange
        var source = @"
            program Fibonacci;
            var a, b, i, n, temp : integer;

            procedure PrintNumber(num : integer);
            begin
                write(num);
                write(' ')
            end;

            begin
                n := 10;
                a := 0;
                b := 1;

                PrintNumber(a);
                PrintNumber(b);

                for i := 3 to n do
                begin
                    temp := a + b;
                    PrintNumber(temp);
                    a := b;
                    b := temp
                end;

                writeln()
            end.
        ";

        // Act
        var output = ExecuteProgram(source);

        // Assert
        Assert.Contains("0 1 1 2 3 5 8 13 21 34", output);
    }

    [Fact]
    public void FullPipeline_NestedFunctions_ExecutesSuccessfully()
    {
        // Arrange
        var source = @"
            program NestedCalls;
            var result : integer;

            function Double(n : integer) : integer;
            begin
                Double := n * 2
            end;

            function Triple(n : integer) : integer;
            begin
                Triple := n * 3
            end;

            function Calculate(x : integer) : integer;
            begin
                Calculate := Double(x) + Triple(x)
            end;

            begin
                result := Calculate(5);
                writeln(result)
            end.
        ";

        // Act
        var output = ExecuteProgram(source);

        // Assert
        Assert.Contains("25", output); // 2*5 + 3*5 = 25
    }

    [Fact]
    public void FullPipeline_ComplexLogic_ExecutesSuccessfully()
    {
        // Arrange
        var source = @"
            program ComplexLogic;
            var i : integer;

            function IsEven(n : integer) : boolean;
            begin
                IsEven := (n mod 2) = 0
            end;

            function IsPrime(n : integer) : boolean;
            var i : integer;
            begin
                if n < 2 then
                    IsPrime := false
                else
                begin
                    IsPrime := true;
                    for i := 2 to (n - 1) do
                    begin
                        if (n mod i) = 0 then
                            IsPrime := false
                    end
                end
            end;

            begin
                for i := 1 to 10 do
                begin
                    if IsEven(i) and IsPrime(i) then
                    begin
                        write(i);
                        writeln(' is even and prime')
                    end
                end
            end.
        ";

        // Act
        var output = ExecuteProgram(source);

        // Assert
        Assert.Contains("2 is even and prime", output);
    }

    [Fact]
    public void FullPipeline_StringManipulation_ExecutesSuccessfully()
    {
        // Arrange
        var source = @"
            program StringTest;
            var greeting : string;

            procedure PrintGreeting(name : string);
            var message : string;
            begin
                message := 'Hello, ' + name + '!';
                writeln(message)
            end;

            begin
                PrintGreeting('World');
                PrintGreeting('Pascal')
            end.
        ";

        // Act
        var output = ExecuteProgram(source);

        // Assert
        Assert.Contains("Hello, World!", output);
        Assert.Contains("Hello, Pascal!", output);
    }

    [Fact]
    public void FullPipeline_MathematicalFunctions_ExecutesSuccessfully()
    {
        // Arrange
        var source = @"
            program MathFunctions;
            var result : integer;

            function Max(a, b : integer) : integer;
            begin
                if a > b then
                    Max := a
                else
                    Max := b
            end;

            function Min(a, b : integer) : integer;
            begin
                if a < b then
                    Min := a
                else
                    Min := b
            end;

            function Abs(n : integer) : integer;
            begin
                if n < 0 then
                    Abs := -n
                else
                    Abs := n
            end;

            begin
                result := Max(10, 20);
                writeln(result);

                result := Min(10, 20);
                writeln(result);

                result := Abs(-15);
                writeln(result)
            end.
        ";

        // Act
        var output = ExecuteProgram(source);

        // Assert
        Assert.Contains("20", output);
        Assert.Contains("10", output);
        Assert.Contains("15", output);
    }

    [Fact]
    public void FullPipeline_ComplexControlFlow_ExecutesSuccessfully()
    {
        // Arrange
        var source = @"
            program ControlFlow;
            var i, j, count : integer;

            begin
                count := 0;

                for i := 1 to 3 do
                begin
                    for j := 1 to 3 do
                    begin
                        count := count + 1
                    end
                end;

                writeln(count);

                while count > 5 do
                begin
                    count := count - 1
                end;

                writeln(count)
            end.
        ";

        // Act
        var output = ExecuteProgram(source);

        // Assert
        Assert.Contains("9", output);
        Assert.Contains("5", output);
    }

    [Fact]
    public void FullPipeline_LocalVariableScoping_MaintainsCorrectScope()
    {
        // Arrange
        var source = @"
            program ScopingTest;
            var x : integer;

            procedure ModifyLocal;
            var x : integer;
            begin
                x := 999
            end;

            function GetLocal : integer;
            var x : integer;
            begin
                x := 777;
                GetLocal := x
            end;

            begin
                x := 100;
                ModifyLocal();
                writeln(x);
                writeln(GetLocal());
                writeln(x)
            end.
        ";

        // Act
        var output = ExecuteProgram(source);

        // Assert
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Contains("100", lines[0]);
        Assert.Contains("777", lines[1]);
        Assert.Contains("100", lines[2]);
    }

    [Fact]
    public void FullPipeline_ProcedureWithMultipleParameters_ExecutesSuccessfully()
    {
        // Arrange
        var source = @"
            program MultiParam;

            procedure PrintSum(a, b, c : integer);
            var sum : integer;
            begin
                sum := a + b + c;
                writeln(sum)
            end;

            begin
                PrintSum(10, 20, 30)
            end.
        ";

        // Act
        var output = ExecuteProgram(source);

        // Assert
        Assert.Contains("60", output);
    }

    [Fact]
    public void FullPipeline_BooleanLogic_ExecutesSuccessfully()
    {
        // Arrange
        var source = @"
            program BooleanTest;
            var result : boolean;

            begin
                result := true and false;
                writeln(result);

                result := true or false;
                writeln(result);

                result := not false;
                writeln(result)
            end.
        ";

        // Act
        var output = ExecuteProgram(source);

        // Assert
        Assert.Contains("False", output);
        Assert.Contains("True", output);
    }
}
