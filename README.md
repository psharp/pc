# Pascal Compiler

A Pascal compiler written in C# that includes a lexer, parser, semantic analyzer, interpreter, and bytecode compiler/VM.

## Features

- **Lexical Analysis**: Tokenizes Pascal source code
- **Parsing**: Builds an Abstract Syntax Tree (AST)
- **Semantic Analysis**: Validates variable declarations and usage with comprehensive type checking
- **Type Checking**: Enforces type safety for assignments, operations, and control flow
- **Interpreter**: Executes the Pascal program directly from AST
- **Bytecode Compiler**: Compiles Pascal to portable bytecode (.pbc files)
- **Virtual Machine**: Executes compiled bytecode programs
- **Debugger**: Step-by-step execution with variable inspection

## Supported Pascal Features

### Data Types
- `integer` - Integer numbers
- `real` - Floating-point numbers
- `string` - Text strings
- `boolean` - True/false values
- `array[lower..upper] of type` - Single-dimensional arrays with integer indices
- `array[lower1..upper1, lower2..upper2, ...] of type` - Multidimensional arrays
- `record` - Structured data types with named fields
- `^type` - Pointer types (e.g., `^integer`, `^record`)
- `enumeration` - Named constant types (e.g., `Color = (Red, Green, Blue)`)
- `set of type` - Sets of enumeration or ordinal values

### Control Structures
- `if-then-else` - Conditional statements
- `case-of-end` - Multi-way conditional (switch/match statement)
- `while-do` - While loops (pre-test)
- `repeat-until` - Repeat-until loops (post-test, always executes at least once)
- `for-to-do` - For loops (ascending)
- `for-downto-do` - For loops (descending)
- `with-do` - With statement for simplified record field access
- `goto` and `label` - Unconditional jumps (ISO 7185 compliance)

### Operators
- Arithmetic: `+`, `-`, `*`, `/`, `div`, `mod`
- Comparison: `=`, `<>`, `<`, `>`, `<=`, `>=`
- Logical: `and`, `or`, `not`
- Pointer: `^` (dereference), `@` (address-of)
- Set: `in` (membership testing)

### Mathematical Functions
All standard Pascal math functions are supported in both interpreter and bytecode VM:

- **Absolute Value & Squaring**
  - `abs(x)` - Absolute value (preserves type: int→int, real→real)
  - `sqr(x)` - Square (x²) (preserves type: int→int, real→real)
  - `sqrt(x)` - Square root (always returns real)

- **Trigonometric Functions**
  - `sin(x)` - Sine (x in radians, returns real)
  - `cos(x)` - Cosine (x in radians, returns real)
  - `arctan(x)` - Arctangent (returns real)

- **Logarithmic & Exponential**
  - `ln(x)` - Natural logarithm (returns real)
  - `exp(x)` - Exponential e^x (returns real)

- **Rounding Functions**
  - `trunc(x)` - Truncate to integer (returns integer)
  - `round(x)` - Round to nearest integer (returns integer)

- **Boolean Function**
  - `odd(x)` - Returns true if x is odd (returns boolean)

**Example:**
```pascal
var
  x, y: real;
  n: integer;
begin
  x := sqrt(16.0);        // x = 4.0
  y := sin(0.0);          // y = 0.0
  n := abs(-42);          // n = 42 (integer preserved)
  if odd(5) then          // true
    writeln('5 is odd');
  n := round(3.7);        // n = 4
end.
```

### String Functions
All standard Pascal string functions are supported in both interpreter and bytecode VM:

- **String Inspection**
  - `length(s)` - Returns the length of string s (returns integer)
  - `pos(substr, s)` - Finds position of substr in s, 1-based (returns integer, 0 if not found)

- **String Manipulation**
  - `copy(s, start, count)` - Extracts substring from s starting at position start (1-based), count characters
  - `concat(s1, s2, ...)` - Concatenates 2 or more strings (variable arguments)
  - `upcase(s)` - Converts string to uppercase (returns string)
  - `lowercase(s)` - Converts string to lowercase (returns string)

- **Character Conversion**
  - `chr(n)` - Converts integer ASCII value to character string (returns string)
  - `ord(s)` - Converts first character of string to ASCII value (returns integer)

**Example:**
```pascal
var
  name, upper: string;
  ext: string;
  filename: string;
  dotPos, len: integer;
  ch: string;
begin
  // String length
  name := 'Pascal';
  len := length(name);              // len = 6

  // Case conversion
  upper := upcase(name);            // upper = 'PASCAL'
  name := lowercase('HELLO');       // name = 'hello'

  // Substring extraction
  filename := 'program.pas';
  dotPos := pos('.', filename);     // dotPos = 8
  ext := copy(filename, dotPos + 1, length(filename));  // ext = 'pas'

  // String concatenation
  name := concat('Hello', ' ', 'World');  // name = 'Hello World'

  // Character/ASCII conversion
  ch := chr(65);                    // ch = 'A'
  len := ord('A');                  // len = 65
end.
```

### Procedures and Functions
- `procedure` - Declare procedures (subroutines without return values)
- `function` - Declare functions (subroutines with return values)
- Support for parameters and local variables
- Function return values assigned via function name
- **Nested Procedures/Functions**: Declare procedures and functions inside other procedures/functions
- **Full Closure Support**: Nested procedures/functions can access variables from their enclosing scope
- **Var Parameters**: Pass arguments by reference using the `var` keyword
  - Value parameters (default): Pass a copy of the value
  - Var parameters: Allow procedures/functions to modify the caller's variable
  ```pascal
  procedure Swap(var a, b: integer);
  var temp: integer;
  begin
    temp := a;
    a := b;
    b := temp;
  end;

  procedure Increment(var n: integer);
  begin
    n := n + 1;
  end;

  // Can mix var and value parameters
  function AddAndModify(var x: integer; y: integer): integer;
  begin
    x := x + 10;  // Modifies caller's variable
    AddAndModify := x + y;
  end;

  // Nested procedures and functions with closure support
  procedure Outer(x: integer);
  var
    y: integer;

    procedure Inner;
    begin
      // Can access both x and y from outer scope
      writeln('x = ', x, ', y = ', y);
      y := y + 1;  // Can modify outer scope variables
    end;
  begin
    y := 10;
    writeln('Outer procedure');
    Inner();
    writeln('After Inner: y = ', y);  // y is now 11
  end;
  ```

**Note**: Var parameters, nested procedures/functions, and full closures are all supported in both the interpreter and bytecode VM.

### I/O Operations
- `writeln()` - Write to console with newline
- `write()` - Write to console without newline
- `readln()` - Read input from console

### File I/O Operations
- `text` - Text file type declaration
- `Assign(fileVar, filename)` - Associate file variable with filename
- `Reset(fileVar)` - Open file for reading
- `Rewrite(fileVar)` - Open file for writing (creates/overwrites)
- `Close(fileVar)` - Close an open file
- `EOF(fileVar)` - Check if end of file reached (returns boolean)
- `Read(f, var)` - Read from file into variable
- `Readln(f, var)` - Read line from file into variable
- `Write(f, value)` - Write value to file
- `Writeln(f, value)` - Write value to file with newline

**Note**: File I/O operations are fully supported in both the interpreter and bytecode VM.

### Enumerations and Sets
- **Enumeration Types**: Define named constants
  ```pascal
  type
    Color = (Red, Green, Blue, Yellow);
    Day = (Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday);
  ```
- **Set Types**: Collections of enumeration values
  ```pascal
  var
    favoriteColor : Color;
    primaryColors : set of Color;
    workDays : set of Day;
  ```
- **Set Literals**: Create sets with square brackets
  ```pascal
  primaryColors := [Red, Blue, Yellow];
  workDays := [Monday, Tuesday, Wednesday, Thursday, Friday];
  ```
- **Set Membership**: Test if value is in set using `in` operator
  ```pascal
  if Red in primaryColors then
    writeln('Red is a primary color');

  if today in workDays then
    writeln('Today is a work day');
  ```

**Note**: Enumerations and sets are fully supported in both the interpreter and bytecode VM.

### Arrays
- **Single-dimensional arrays**: Fixed-size arrays with a single index
  ```pascal
  var
    numbers: array[1..10] of integer;
    names: array[0..4] of string;
  begin
    numbers[1] := 100;
    numbers[5] := numbers[1] + 50;
    names[0] := 'Alice';
  end.
  ```
- **Multidimensional arrays**: Arrays with two or more dimensions
  ```pascal
  var
    matrix: array[1..3, 1..4] of integer;     // 2D array (3x4 matrix)
    cube: array[0..2, 0..2, 0..2] of integer; // 3D array (3x3x3 cube)
    i, j, k: integer;
  begin
    // Access 2D array
    matrix[2, 3] := 42;

    // Loop through 2D array
    for i := 1 to 3 do
      for j := 1 to 4 do
        matrix[i, j] := i * 10 + j;

    // Access 3D array
    cube[1, 2, 0] := 120;
  end.
  ```
- Arrays can have custom lower bounds (e.g., `array[5..15]`, `array[-10..10]`)
- Array indices are validated at runtime
- Supports arrays of any type: integers, reals, strings, records, etc.

**Note**: Both single and multidimensional arrays are fully supported in the interpreter and bytecode VM.

### Comments
- `{ comment }` - Curly brace comments
- `(* comment *)` - Parenthesis-asterisk comments
- `// comment` - Single-line comments

## Building and Running

### Prerequisites
- .NET 8.0 SDK or later
- (Optional) Visual Studio Code with C# Dev Kit extension for IDE support

### Build the project:
```bash
dotnet build
```

### Run the tests:
```bash
dotnet test
```

The test suite includes 69 tests covering:
- **Lexer Tests**: Tokenization and lexical analysis
- **Parser Tests**: AST construction and syntax parsing
- **Semantic Analyzer Tests**: Variable declaration and scope validation
- **Interpreter Tests**: Code execution and runtime behavior
- **Integration Tests**: End-to-end program execution

### Using with Visual Studio Code

The project includes VSCode configuration files for debugging and IntelliSense support.

1. Open the project folder in VSCode:
   ```bash
   code .
   ```

2. Install recommended extension:
   - **C# Dev Kit** (Microsoft) - Provides IntelliSense, debugging, and project management

3. Press `F5` to build and run with debugging, or use the Run menu to select:
   - "Run Pascal Compiler" - Runs the demo Fibonacci program
   - "Run with Hello Example" - Runs the hello.pas example
   - "Run with Loops Example" - Runs the loops.pas example

4. Set breakpoints in the compiler code to debug the lexer, parser, or interpreter

### Run the compiler (normal mode):

```bash
# Run with demo Fibonacci program (interactive)
dotnet run

# Run Hello World example
dotnet run examples/hello.pas

# Run factorial calculator (interactive - enter a number)
dotnet run examples/factorial.pas

# Run conditionals demo (interactive - enter name and age)
dotnet run examples/conditionals.pas

# Run loops demonstration
dotnet run examples/loops.pas

# Run arithmetic operations (interactive - enter two numbers)
dotnet run examples/arithmetic.pas

# Run comprehensive math functions test (67 tests)
dotnet run examples/math_functions.pas

# Run basic math operations test (74 tests)
dotnet run examples/math_operations.pas

# Run comprehensive string functions test (79 tests)
dotnet run examples/string_functions.pas
```

### Bytecode Compilation and Execution

The compiler supports compiling Pascal programs to bytecode (.pbc files) for faster execution and portability.

#### Compiling to Bytecode

To compile a Pascal program to bytecode, use the `-c` or `--compile` flag:

```bash
# Compile a program - creates a .pbc file with the same name
dotnet run -- -c examples/simple.pas
# Output: examples/simple.pbc

# Compile with a custom output filename
dotnet run -- -c -o myprogram.pbc examples/simple.pas
# Output: myprogram.pbc

# Compile any Pascal program
dotnet run -- -c examples/loops.pas
# Output: examples/loops.pbc
```

The compiler will:
1. Parse and validate your Pascal source code
2. Generate bytecode instructions
3. Save the bytecode to a `.pbc` (Pascal Bytecode) file
4. Display compilation statistics (number of instructions, variables, functions)

**Example compilation output:**
```
=== Compiling to Bytecode ===
Successfully compiled to: examples/simple.pbc
Instructions: 17
Variables: 3
Functions: 0
```

#### Running Compiled Bytecode

To execute a compiled bytecode file, use the `-r` or `--run` flag:

```bash
# Run a compiled bytecode program
dotnet run -- -r examples/simple.pbc

# Run any .pbc file
dotnet run -- -r myprogram.pbc
```

**Example execution:**
```
=== Loading Bytecode ===
Loaded program: Simple
Instructions: 17

=== Executing Bytecode ===
The sum is: 15

=== Program completed ===
```

#### Inspecting Bytecode (Disassembly)

To view the bytecode instructions in a human-readable format, use the `-s` or `--disassemble` flag:

```bash
# Disassemble bytecode to see instructions
dotnet run -- -s examples/simple.pbc
```

**Example disassembly output:**
```
Program: Simple
Variables: x, y, sum

Instructions:
     0: PUSH 5
     1: STORE_VAR x
     2: LOAD_VAR x
     3: LOAD_VAR y
     4: ADD
     5: STORE_VAR sum
     6: WRITE The sum is:
     7: WRITELN
     8: LOAD_VAR sum
     9: WRITELN
    10: HALT
```

#### Bytecode Workflow Example

Here's a complete workflow from source to execution:

```bash
# Step 1: Write your Pascal program (or use an example)
# examples/simple.pas

# Step 2: Compile to bytecode
dotnet run -- -c examples/simple.pas
# Creates: examples/simple.pbc

# Step 3: (Optional) Inspect the bytecode
dotnet run -- -s examples/simple.pbc

# Step 4: Run the compiled program
dotnet run -- -r examples/simple.pbc
```

#### Benefits of Bytecode

- **Faster Execution**: No need to re-parse source code each time
- **Portable Format**: Bytecode files can be distributed and run anywhere
- **Smaller File Size**: Compiled bytecode is more compact than source
- **Distribution**: Share compiled programs without source code
- **Separation of Concerns**: Compile once, run many times
- **Full Feature Support**: Pointers, file I/O, enumerations, and sets all work in bytecode mode

#### Current Limitations

- **Best For**: All Pascal programs (full feature parity with interpreter)

**Programs that work well with bytecode:**
- [examples/simple.pas](examples/simple.pas) - Variable assignments ✓
- [examples/hello.pas](examples/hello.pas) - Basic I/O ✓
- [examples/loops.pas](examples/loops.pas) - For/while loops ✓
- [examples/arithmetic.pas](examples/arithmetic.pas) - Arithmetic operations ✓
- [examples/pointer_basic.pas](examples/pointer_basic.pas) - Pointer operations ✓
- [examples/pointer_demo.pas](examples/pointer_demo.pas) - Advanced pointers ✓
- [examples/fileio_complete.pas](examples/fileio_complete.pas) - File I/O ✓
- [examples/fileio_showcase.pas](examples/fileio_showcase.pas) - Advanced file I/O ✓
- [enum_set_demo.pas](enum_set_demo.pas) - Enumerations and sets ✓
- [param_test.pas](param_test.pas) - Function/procedure parameters ✓
- [complex_param_test.pas](complex_param_test.pas) - Advanced parameters ✓

### Debug Pascal programs (step-by-step mode):

To step through Pascal programs line by line with variable inspection:

```bash
# Debug mode - step through the program
dotnet run --debug examples/simple.pas

# Or use short form
dotnet run -d examples/hello.pas

# Debug the demo Fibonacci program
dotnet run --debug
```

**Debug Mode Commands:**
- `[Enter]` - Step to next statement
- `c` - Continue until next breakpoint
- `v` - View all variables and their values
- `v <name>` - View specific variable
- `b <line>` - Set breakpoint at line number
- `bl` - List all breakpoints
- `bc` - Clear all breakpoints
- `h` - Show help
- `q` - Quit debugging

**Example Debug Session:**
```
dotnet run --debug examples/simple.pas

=== Pascal Debugger ===
Commands:
  [Enter]    - Step to next statement
  c          - Continue until next breakpoint
  v          - View all variables
  ...

Program: Simple
Variables initialized: x, y, sum

[Line 1] x := 5
debug> v
Variables:
  sum = 0
  x = 0
  y = 0

debug> [press Enter to step]
  => x = 5

[Line 2] y := 10
debug> c
Continuing...
```

## Example Programs

Several example programs are included in the `examples/` directory:

- **simple.pas** - Simple variable assignment (great for debugging)
  ```bash
  dotnet run examples/simple.pas
  dotnet run --debug examples/simple.pas  # Debug mode
  ```

- **hello.pas** - Simple Hello World program
  ```bash
  dotnet run examples/hello.pas
  ```

- **factorial.pas** - Calculate factorial of a number (interactive)
  ```bash
  dotnet run examples/factorial.pas
  ```

- **conditionals.pas** - Demonstrate if-else statements (interactive)
  ```bash
  dotnet run examples/conditionals.pas
  ```

- **loops.pas** - Demonstrate for and while loops
  ```bash
  dotnet run examples/loops.pas
  ```

- **arithmetic.pas** - Basic arithmetic operations (interactive)
  ```bash
  dotnet run examples/arithmetic.pas
  ```

- **arrays.pas** - Array demonstration
  ```bash
  dotnet run examples/arrays.pas
  ```

- **records.pas** - Record (struct) demonstration
  ```bash
  dotnet run examples/records.pas
  ```

- **students.pas** - Arrays of records example
  ```bash
  dotnet run examples/students.pas
  ```

## Pascal Program Structure

```pascal
program ProgramName;
var
    variable1, variable2 : integer;
    variable3 : string;
begin
    { Your code here }
    writeln('Hello, World!')
end.
```

## Architecture

The compiler consists of eight main components:

1. **Lexer** ([Lexer.cs](Lexer.cs)) - Converts source code into tokens
2. **Parser** ([Parser.cs](Parser.cs)) - Builds an AST from tokens
3. **AST** ([AST.cs](AST.cs)) - Defines the abstract syntax tree node types
4. **Semantic Analyzer** ([SemanticAnalyzer.cs](SemanticAnalyzer.cs)) - Validates the program
5. **Interpreter** ([Interpreter.cs](Interpreter.cs)) - Executes the program directly from AST
6. **Bytecode Compiler** ([BytecodeCompiler.cs](BytecodeCompiler.cs)) - Compiles AST to bytecode instructions
7. **Virtual Machine** ([VirtualMachine.cs](VirtualMachine.cs)) - Stack-based VM that executes bytecode
8. **Bytecode Serializer** ([BytecodeSerializer.cs](BytecodeSerializer.cs)) - Saves/loads bytecode to/from .pbc files

## Example Usage

```pascal
program Fibonacci;
var
    n, i, a, b, temp : integer;
begin
    writeln('Enter number of terms: ');
    readln(n);

    a := 0;
    b := 1;

    for i := 3 to n do
    begin
        temp := a + b;
        write(temp);
        write(' ');
        a := b;
        b := temp
    end;

    writeln()
end.
```

## Example: Procedures and Functions

```pascal
program FunctionsExample;
var
    num, result : integer;

function Square(n : integer) : integer;
begin
    Square := n * n
end;

procedure PrintResult(value : integer);
begin
    write('Result: ');
    writeln(value)
end;

begin
    num := 5;
    result := Square(num);
    PrintResult(result)
end.
```

Run with:
```bash
dotnet run examples/procedures.pas
dotnet run examples/functions.pas
```

## Example: Arrays

```pascal
program ArrayExample;
var
    numbers : array[1..5] of integer;
    i : integer;
begin
    { Initialize array }
    numbers[1] := 10;
    numbers[2] := 20;
    numbers[3] := 30;

    { Print array elements }
    for i := 1 to 3 do
    begin
        writeln(numbers[i])
    end
end.
```

## Example: Records

```pascal
program RecordExample;
type
    Person = record
        name : string;
        age : integer;
    end;

var
    john : Person;
begin
    { Initialize record fields }
    john.name := 'John Doe';
    john.age := 30;

    { Access record fields }
    writeln(john.name);
    writeln(john.age)
end.
```

## Example: Arrays of Records

```pascal
program StudentDatabase;
type
    Student = record
        name : string;
        grade : integer;
    end;

var
    students : array[1..3] of Student;
    i : integer;
begin
    { Initialize array of records }
    students[1].name := 'Alice';
    students[1].grade := 85;

    students[2].name := 'Bob';
    students[2].grade := 92;

    { Access elements }
    for i := 1 to 2 do
    begin
        writeln(students[i].name);
        writeln(students[i].grade)
    end
end.
```

## Example: Case Statement

```pascal
program CaseExample;
var
    choice : integer;
    grade : string;
begin
    choice := 2;

    { Simple case with single values }
    case choice of
        1: writeln('Option One');
        2: writeln('Option Two');
        3: writeln('Option Three');
    else
        writeln('Invalid Option')
    end;

    { Case with multiple values per branch }
    choice := 7;
    case choice of
        1, 3, 5, 7, 9: writeln('Odd digit');
        2, 4, 6, 8: writeln('Even digit');
        0: writeln('Zero');
    else
        writeln('Not a digit')
    end;

    { Case with ranges }
    choice := 85;
    case choice of
        90..100: grade := 'A';
        80..89: grade := 'B';
        70..79: grade := 'C';
        60..69: grade := 'D';
        0..59: grade := 'F';
    else
        grade := 'Invalid'
    end;
    writeln('Grade: ', grade);

    { Case with compound statements }
    choice := 1;
    case choice of
        1: begin
            writeln('Multiple');
            writeln('Statements');
        end;
        2: writeln('Single Statement');
    else
        writeln('Default')
    end;
end.
```

Run with:
```bash
dotnet run examples/case_test.pas
```

## Example: Repeat-Until Loop

The `repeat-until` loop is a post-test loop that always executes at least once:

```pascal
program RepeatUntilExample;
var
    n, sum : integer;
begin
    { Basic repeat-until - counts from 1 to 5 }
    n := 0;
    repeat
        n := n + 1;
        writeln('Count: ', n)
    until n >= 5;

    { Calculate sum of numbers 1 to 10 }
    n := 1;
    sum := 0;
    repeat
        sum := sum + n;
        n := n + 1
    until n > 10;
    writeln('Sum 1 to 10: ', sum);  { Outputs: 55 }

    { Always executes at least once (post-test) }
    n := 100;
    repeat
        writeln('Executed once despite condition being true');
        n := n + 1
    until n > 50;  { Condition is already true, but body runs once }

    { Factorial calculation }
    n := 5;
    sum := 1;
    repeat
        sum := sum * n;
        n := n - 1
    until n = 0;
    writeln('5! = ', sum);  { Outputs: 120 }
end.
```

Run with:
```bash
dotnet run examples/repeat_until_test.pas
```

## Example: File I/O

```pascal
program FileIOExample;
var
    f : text;
    line : string;
begin
    { Write to file }
    Assign(f, 'output.txt');
    Rewrite(f);

    Write(f, 'Hello, ');
    Writeln(f, 'World!');
    Write(f, 'Line ');
    Writeln(f, 2);

    Close(f);

    { Read from file }
    Assign(f, 'output.txt');
    Reset(f);

    while not EOF(f) do
    begin
        Readln(f, line);
        writeln(line)
    end;

    Close(f)
end.
```

Run with:
```bash
dotnet run examples/fileio_complete.pas
dotnet run examples/fileio_advanced.pas
```

## Example: Pointers

```pascal
program PointerExample;
var
    ptr : ^integer;
    value : integer;
begin
    { Allocate memory }
    New(ptr);

    { Assign value through pointer }
    ptr^ := 42;

    { Read value through pointer }
    value := ptr^;
    writeln(value);  { Outputs: 42 }

    { Free memory }
    Dispose(ptr);

    { Nil pointer }
    ptr := nil;
end.
```

Pointer operations:
- `^type` - Declare pointer type (e.g., `ptr : ^integer`)
- `New(ptr)` - Allocate memory
- `Dispose(ptr)` - Free memory
- `ptr^` - Dereference pointer to access value
- `@variable` - Get address of variable
- `nil` - Null pointer constant

Run with:
```bash
dotnet run examples/pointer_basic.pas
dotnet run examples/pointer_demo.pas
```

## Type Checking

The compiler includes comprehensive type checking that validates:

### Type Safety in Assignments
- Variables can only be assigned values of compatible types
- Integer values can be implicitly converted to real
- Arrays and record fields are type-checked

```pascal
var
    x : integer;
    z : real;
begin
    x := 10;      { OK }
    z := 3.14;    { OK }
    z := x;       { OK - implicit int to real conversion }
    x := 'text';  { ERROR: cannot assign string to integer }
end.
```

### Type Safety in Operations
- **Arithmetic operators** (`+`, `-`, `*`, `/`) require numeric operands
- **Integer division** (`div`, `mod`) requires integer operands
- **Logical operators** (`and`, `or`, `not`) require boolean operands
- **Comparison operators** enforce type compatibility

```pascal
var
    x : integer;
    flag : boolean;
begin
    x := 5 + 3;           { OK }
    x := 10 div 3;        { OK }
    flag := (x > 5);      { OK }
    flag := true and false; { OK }
    x := flag * 2;        { ERROR: boolean in arithmetic }
    flag := x and true;   { ERROR: integer in logical operation }
end.
```

### Type Safety in Control Flow
- **If/While conditions** must be boolean
- **For loop** variables and bounds must be integer

```pascal
var
    x : integer;
    flag : boolean;
begin
    if flag then          { OK }
        writeln('true');

    if x then            { ERROR: condition must be boolean }
        writeln('error');

    for x := 1 to 10 do  { OK }
        writeln(x);
end.
```

### Array and Record Type Checking
- Array indices must be integer
- Array elements must match the declared type
- Record fields must match their declared types

```pascal
type
    Person = record
        name : string;
        age : integer;
    end;
var
    people : array[1..10] of Person;
    i : integer;
begin
    people[1].name := 'John';  { OK }
    people[1].age := 25;       { OK }
    people[1].age := 'text';   { ERROR: type mismatch }
    people['x'].name := 'Bob'; { ERROR: array index must be integer }
end.
```

To see type checking in action:
```bash
# This will report type errors
dotnet run examples/typecheck_errors.pas

# This will pass all type checks
dotnet run examples/typecheck_valid.pas
```

## Units and Modules

This compiler supports Pascal units for organizing code into reusable modules:

### Unit Structure
```pascal
unit MathUtils;

interface
  // Public declarations (visible to programs that use this unit)
  function Square(x: integer): integer;
  function Cube(x: integer): integer;

implementation
  // Implementation of interface functions
  function Square(x: integer): integer;
  begin
    Square := x * x;
  end;

  function Cube(x: integer): integer;
  begin
    Cube := x * x * x;
  end;

  // Private functions (not visible outside this unit)
  function HelperFunction: integer;
  begin
    HelperFunction := 42;
  end;

end.
```

### Using Units in Programs
```pascal
program MyProgram;

uses MathUtils;  // Import the unit

var
  result: integer;

begin
  result := Square(5);
  writeln('Square of 5 is: ', result);
end.
```

### Unit Features
- **Interface section**: Public declarations (types, variables, procedures, functions)
- **Implementation section**: Full implementations of interface declarations plus private declarations
- **Uses clause**: Import other units
- **Optional initialization/finalization blocks**: Code that runs when unit is loaded/unloaded
- **Automatic unit loading**: Units are loaded automatically when referenced
- **Symbol resolution**: Unit symbols are properly resolved in programs
- **Initialization execution**: Unit initialization blocks run before the main program

**Example output:**
```bash
$ dotnet run test_with_unit.pas
=== Execution ===
Testing program with unit support

Square(5) = 25
Cube(5) = 125

Square of 7 is 49

Program completed successfully!
```

### Separate Unit Compilation

Units can now be compiled separately to bytecode (.pbu files) for better performance and code distribution:

```bash
# Compile a unit to bytecode
$ dotnet run -- -u MathUtils.pas
=== Compiling Unit to Bytecode ===
Successfully compiled unit to: MathUtils.pbu
Variables: 0
Functions: 3
Initialization instructions: 0

# Compile a program to bytecode (automatically finds source units)
$ dotnet run -- -c test_with_unit.pas
=== Compiling to Bytecode ===
Successfully compiled to: test_with_unit.pbc
Instructions: 45
Variables: 2
Functions: 0

# Run the compiled program (automatically loads compiled units)
$ dotnet run -- -r test_with_unit.pbc
=== Loading Bytecode ===
Loaded program: TestWithUnit
Instructions: 45

=== Executing Bytecode ===
Testing program with unit support

Square(5) = 25
Cube(5) = 125

Square of 7 is 49

Program completed successfully!
```

**Bytecode Unit Features:**
- **Separate compilation**: Compile units once, use them in multiple programs
- **Automatic unit discovery**: VM automatically discovers and loads required .pbu files
- **Dependency tracking**: Program bytecode stores list of used units
- **Function address resolution**: Unit functions are properly linked at runtime
- **Initialization support**: Unit initialization blocks execute before main program
- **Faster loading**: Pre-compiled units load faster than parsing source files
- **No manual configuration**: Units are loaded automatically based on `uses` clause

**Command-line options:**
- `-u, --compile-unit`: Compile a unit to .pbu bytecode file
- `-c, --compile`: Compile a program to .pbc bytecode file
- `-r, --run`: Run compiled bytecode (automatically loads required units)
- `-s, --disassemble`: Disassemble bytecode file (shows used units for programs)
- `-o, --output`: Specify custom output filename

**How it works:**
1. When compiling a program, the compiler stores the list of used units in the bytecode
2. When running the bytecode, the VM reads the used units list and automatically loads the corresponding .pbu files
3. Unit functions are merged into the program's instruction stream with adjusted addresses
4. No manual unit loading or configuration required

## Limitations

This is a basic Pascal compiler with the following limitations:

- Unit search path is limited to current directory
- Future enhancement: support for multiple search paths and package directories

## Future Enhancements

- Support for multiple unit search paths and package directories
- Add code optimization
- Generate native code or IL
- Unit versioning and compatibility checking
