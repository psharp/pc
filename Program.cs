/// <summary>
/// Main entry point and command-line interface for the Pascal compiler.
/// Supports interpretation, bytecode compilation, and execution of Pascal programs and units.
/// </summary>
using System;
using System.IO;

namespace PascalCompiler;

/// <summary>
/// Command-line interface for the Pascal compiler and virtual machine.
/// Provides options for interpretation, compilation, execution, and debugging.
/// </summary>
class Program
{
    /// <summary>
    /// Main entry point for the Pascal compiler application.
    /// Parses command-line arguments and dispatches to appropriate handlers.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    static void Main(string[] args)
    {
        bool debugMode = false;
        bool compileOnly = false;
        bool runBytecode = false;
        bool disassemble = false;
        bool compileUnit = false;
        string? filename = null;
        string? outputFile = null;

        // Parse arguments
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--debug" || args[i] == "-d")
            {
                debugMode = true;
            }
            else if (args[i] == "--compile" || args[i] == "-c")
            {
                compileOnly = true;
            }
            else if (args[i] == "--compile-unit" || args[i] == "-u")
            {
                compileUnit = true;
            }
            else if (args[i] == "--run" || args[i] == "-r")
            {
                runBytecode = true;
            }
            else if (args[i] == "--disassemble" || args[i] == "-s")
            {
                disassemble = true;
            }
            else if (args[i] == "--output" || args[i] == "-o")
            {
                if (i + 1 < args.Length)
                {
                    outputFile = args[++i];
                }
            }
            else if (!args[i].StartsWith("-"))
            {
                filename = args[i];
            }
        }

        if (filename == null)
        {
            Console.WriteLine("Pascal Compiler with Bytecode Support");
            Console.WriteLine("Usage: PascalCompiler [options] <filename>");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("  --debug, -d              Run in step-by-step debug mode");
            Console.WriteLine("  --compile, -c            Compile to bytecode (.pbc file)");
            Console.WriteLine("  --compile-unit, -u       Compile unit to bytecode (.pbu file)");
            Console.WriteLine("  --run, -r                Run pre-compiled bytecode");
            Console.WriteLine("  --disassemble, -s        Disassemble bytecode file");
            Console.WriteLine("  --output, -o <file>      Specify output file for bytecode");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("  PascalCompiler program.pas              # Interpret program");
            Console.WriteLine("  PascalCompiler -c program.pas           # Compile to program.pbc");
            Console.WriteLine("  PascalCompiler -u MathUtils.pas         # Compile unit to MathUtils.pbu");
            Console.WriteLine("  PascalCompiler -c -o out.pbc prog.pas   # Compile to out.pbc");
            Console.WriteLine("  PascalCompiler -r program.pbc           # Run bytecode");
            Console.WriteLine("  PascalCompiler -s program.pbc           # Disassemble bytecode");
            Console.WriteLine("\nRunning demo program...\n");
            RunDemo(debugMode);
            return;
        }

        if (!File.Exists(filename))
        {
            Console.WriteLine($"Error: File '{filename}' not found");
            return;
        }

        if (runBytecode)
        {
            RunBytecodeFile(filename);
        }
        else if (disassemble)
        {
            DisassembleBytecode(filename);
        }
        else if (compileUnit)
        {
            string source = File.ReadAllText(filename);
            CompileUnitToBytecode(source, outputFile ?? Path.ChangeExtension(filename, ".pbu"));
        }
        else
        {
            string source = File.ReadAllText(filename);
            if (compileOnly)
            {
                CompileToBytecode(source, outputFile ?? Path.ChangeExtension(filename, ".pbc"));
            }
            else
            {
                CompileAndRun(source, debugMode);
            }
        }
    }

    static void RunDemo(bool debugMode = false)
    {
        string demoProgram = @"
program Fibonacci;
var
    n, i, a, b, temp : integer;
begin
    writeln('Fibonacci Sequence Generator');
    writeln('Enter number of terms: ');
    readln(n);

    a := 0;
    b := 1;

    writeln('Fibonacci sequence:');
    write(a);
    write(' ');
    write(b);

    for i := 3 to n do
    begin
        temp := a + b;
        write(' ');
        write(temp);
        a := b;
        b := temp
    end;

    writeln()
end.
";
        CompileAndRun(demoProgram, debugMode);
    }

    static void CompileAndRun(string source, bool debugMode = false)
    {
        try
        {
            Console.WriteLine("=== Lexical Analysis ===");
            var lexer = new Lexer(source);
            var tokens = lexer.Tokenize();
            Console.WriteLine($"Generated {tokens.Count} tokens");

            Console.WriteLine("\n=== Parsing ===");
            var parser = new Parser(tokens);
            var ast = parser.ParseProgram();
            Console.WriteLine($"Program: {ast.Name}");
            Console.WriteLine($"Variables: {ast.Variables.Count}");
            Console.WriteLine($"Statements: {ast.Block.Statements.Count}");

            Console.WriteLine("\n=== Semantic Analysis ===");
            var unitLoader = new UnitLoader();
            var analyzer = new SemanticAnalyzer();
            analyzer.Analyze(ast, unitLoader);

            if (analyzer.Errors.Count > 0)
            {
                Console.WriteLine("Semantic errors found:");
                foreach (var error in analyzer.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
                return;
            }
            Console.WriteLine("No semantic errors");

            if (debugMode)
            {
                var debugger = new PascalDebugger();
                debugger.Execute(ast);
            }
            else
            {
                Console.WriteLine("\n=== Execution ===");
                var interpreter = new Interpreter();
                interpreter.Execute(ast, unitLoader);
                Console.WriteLine("\n=== Program completed ===");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    static void CompileToBytecode(string source, string outputFile)
    {
        try
        {
            Console.WriteLine("=== Compiling to Bytecode ===");

            var lexer = new Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Parser(tokens);
            var ast = parser.ParseProgram();

            // Create unit loader to handle source units (for semantic analysis)
            var unitLoader = new UnitLoader();
            var analyzer = new SemanticAnalyzer();
            analyzer.Analyze(ast, unitLoader);

            if (analyzer.Errors.Count > 0)
            {
                Console.WriteLine("Semantic errors found:");
                foreach (var error in analyzer.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
                return;
            }

            var compiler = new BytecodeCompiler();
            var bytecode = compiler.Compile(ast);

            var serializer = new BytecodeSerializer();
            serializer.SaveToFile(bytecode, outputFile);

            Console.WriteLine($"Successfully compiled to: {outputFile}");
            Console.WriteLine($"Instructions: {bytecode.Instructions.Count}");
            Console.WriteLine($"Variables: {bytecode.Variables.Count}");
            Console.WriteLine($"Functions: {bytecode.Functions.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Compilation error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    static void RunBytecodeFile(string filename)
    {
        try
        {
            Console.WriteLine("=== Loading Bytecode ===");

            var serializer = new BytecodeSerializer();
            var bytecode = serializer.LoadFromFile(filename);

            Console.WriteLine($"Loaded program: {bytecode.Name}");
            Console.WriteLine($"Instructions: {bytecode.Instructions.Count}");

            if (bytecode.UsedUnits.Count > 0)
            {
                Console.WriteLine($"Used units: {string.Join(", ", bytecode.UsedUnits)}");
            }

            Console.WriteLine("\n=== Executing Bytecode ===");
            var vm = new VirtualMachine();
            vm.Execute(bytecode);

            Console.WriteLine("\n=== Program completed ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Execution error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    static void CompileUnitToBytecode(string source, string outputFile)
    {
        try
        {
            Console.WriteLine("=== Compiling Unit to Bytecode ===");

            var lexer = new Lexer(source);
            var tokens = lexer.Tokenize();

            var parser = new Parser(tokens);
            var unit = parser.ParseUnit();

            var analyzer = new SemanticAnalyzer();
            analyzer.Analyze(unit);

            if (analyzer.Errors.Count > 0)
            {
                Console.WriteLine("Semantic errors found:");
                foreach (var error in analyzer.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
                return;
            }

            var compiler = new BytecodeCompiler();
            var bytecodeUnit = compiler.CompileUnit(unit);

            var serializer = new BytecodeUnitSerializer();
            serializer.SaveToFile(bytecodeUnit, outputFile);

            Console.WriteLine($"Successfully compiled unit to: {outputFile}");
            Console.WriteLine($"Variables: {bytecodeUnit.Variables.Count}");
            Console.WriteLine($"Functions: {bytecodeUnit.Functions.Count}");
            Console.WriteLine($"Initialization instructions: {bytecodeUnit.InitializationCode.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Compilation error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    static void DisassembleBytecode(string filename)
    {
        try
        {
            // Try to load as program bytecode first
            if (filename.EndsWith(".pbc"))
            {
                var serializer = new BytecodeSerializer();
                var bytecode = serializer.LoadFromFile(filename);
                Console.WriteLine(serializer.DisassembleToString(bytecode));
            }
            // Try to load as unit bytecode
            else if (filename.EndsWith(".pbu"))
            {
                var serializer = new BytecodeUnitSerializer();
                var bytecode = serializer.LoadFromFile(filename);
                Console.WriteLine(serializer.DisassembleToString(bytecode));
            }
            else
            {
                // Try both formats
                try
                {
                    var serializer = new BytecodeSerializer();
                    var bytecode = serializer.LoadFromFile(filename);
                    Console.WriteLine(serializer.DisassembleToString(bytecode));
                }
                catch
                {
                    var serializer = new BytecodeUnitSerializer();
                    var bytecode = serializer.LoadFromFile(filename);
                    Console.WriteLine(serializer.DisassembleToString(bytecode));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Disassembly error: {ex.Message}");
        }
    }
}

