# Pascal Compiler - Code Documentation

This document provides comprehensive documentation for all classes and methods in the Pascal Compiler project.

## Architecture Overview

The compiler follows a traditional multi-phase design:

1. **Lexical Analysis** (Lexer.cs) - Tokenization
2. **Syntactic Analysis** (Parser.cs) - AST Construction
3. **Semantic Analysis** (SemanticAnalyzer.cs) - Type Checking & Validation
4. **Execution Paths**:
   - Direct Interpretation (Interpreter.cs)
   - Bytecode Compilation (BytecodeCompiler.cs) → VM Execution (VirtualMachine.cs)

## Core Components

### Token.cs
**Purpose**: Defines token types and token data structure.

**Classes**:
- `TokenType` (enum) - All possible token types (keywords, operators, literals, etc.)
- `Token` - Represents a single token with type, value, line, and column information

**Key Methods**:
- `Token(TokenType, string, int, int)` - Constructor
- `ToString()` - Debug representation

---

### Lexer.cs
**Purpose**: Converts source code text into a sequence of tokens.

**Class**: `Lexer`

**Key Fields**:
- `_source` - Input source code
- `_position` - Current position in source
- `_line`, `_column` - Current line and column for error reporting
- `Keywords` - Dictionary mapping keyword strings to token types

**Key Methods**:
- `Lexer(string)` - Constructor, initializes with source code
- `Tokenize()` - Main entry point, returns List<Token>
- `NextToken()` - Returns next token from source
- `ScanIdentifierOrKeyword()` - Handles identifiers and keywords
- `ScanNumber()` - Handles integer and real literals
- `ScanString()` - Handles string literals
- `SkipWhitespace()` - Skips whitespace and tracks line/column
- `SkipComment()` - Skips Pascal comments ({ }, // , (* *))

---

### AST.cs
**Purpose**: Defines all Abstract Syntax Tree node types.

**Base Class**: `ASTNode` - Abstract base for all AST nodes

**Program Structure Nodes**:
- `ProgramNode` - Root node for a Pascal program
- `UnitNode` - Root node for a Pascal unit/module
- `BlockNode` - Block of statements (begin...end)

**Declaration Nodes**:
- `VarDeclarationNode` - Variable declaration
- `ArrayVarDeclarationNode` - Array variable declaration
- `RecordVarDeclarationNode` - Record variable declaration
- `PointerVarDeclarationNode` - Pointer variable declaration
- `FileVarDeclarationNode` - File variable declaration
- `SetVarDeclarationNode` - Set variable declaration
- `ProcedureDeclarationNode` - Procedure declaration
- `FunctionDeclarationNode` - Function declaration
- `ParameterNode` - Procedure/function parameter

**Type Nodes**:
- `RecordTypeNode` - Record type definition
- `EnumTypeNode` - Enumeration type definition
- `ArrayTypeNode` - Array type specification

**Statement Nodes**:
- `AssignmentNode` - Variable assignment
- `IfNode` - If-then-else statement
- `WhileNode` - While loop
- `ForNode` - For loop
- `ProcedureCallNode` - Procedure invocation
- `WriteNode` - Write/Writeln statement
- `ReadNode` - Read/Readln statement
- `NewNode` - Pointer allocation
- `DisposeNode` - Pointer deallocation
- File I/O nodes (FileAssignNode, FileResetNode, etc.)

**Expression Nodes**:
- `BinaryOpNode` - Binary operation (a + b, a < b, etc.)
- `UnaryOpNode` - Unary operation (not, -, etc.)
- `IntegerNode` - Integer literal
- `RealNode` - Real literal
- `StringNode` - String literal
- `BooleanNode` - Boolean literal
- `VariableNode` - Variable reference
- `FunctionCallNode` - Function invocation
- `ArrayIndexNode` - Array element access
- `RecordAccessNode` - Record field access
- `PointerDereferenceNode` - Pointer dereference (ptr^)
- `AddressOfNode` - Address-of operator (@var)
- `NilNode` - Nil pointer
- `SetLiteralNode` - Set literal ([a, b, c])
- `InNode` - Set membership test (a in set)

---

### Parser.cs
**Purpose**: Parses token stream into an Abstract Syntax Tree.

**Class**: `Parser`

**Key Fields**:
- `_tokens` - List of tokens from lexer
- `_position` - Current position in token list
- `_currentToken` - Current token being processed

**Key Methods**:
- `Parser(List<Token>)` - Constructor
- `ParseProgram()` - Parses a complete Pascal program, returns ProgramNode
- `ParseUnit()` - Parses a Pascal unit, returns UnitNode
- `ParseUsesClause()` - Parses uses statement
- `ParseBlock()` - Parses begin...end block
- `ParseStatement()` - Parses a single statement
- `ParseExpression()` - Parses an expression
- `ParseVariableDeclarations()` - Parses var section
- `ParseTypeDeclarations()` - Parses type section
- `ParseProcedure()` - Parses procedure declaration
- `ParseFunction()` - Parses function declaration
- `Expect(TokenType)` - Consumes token of expected type, throws on mismatch
- `Advance()` - Moves to next token

---

### SemanticAnalyzer.cs
**Purpose**: Performs semantic analysis and type checking on the AST.

**Class**: `SemanticAnalyzer`

**Key Fields**:
- `_symbolTable` - Maps variable names to types
- `_procedureTable` - Maps procedure names to declarations
- `_functionTable` - Maps function names to declarations
- `_recordTypeDefinitions` - Defined record types
- `_enumTypeDefinitions` - Defined enumeration types
- `_enumValues` - Maps enum value names to their types
- `_errors` - List of semantic errors found

**Key Methods**:
- `Analyze(ProgramNode)` - Analyzes a program
- `Analyze(UnitNode)` - Analyzes a unit
- `AnalyzeBlock(BlockNode)` - Analyzes a block
- `AnalyzeStatement(ASTNode)` - Analyzes a statement
- `AnalyzeExpression(ASTNode)` - Analyzes an expression
- `GetExpressionType(ASTNode)` - Infers type of expression
- `AreTypesCompatible(string, string)` - Checks type compatibility
- `RegisterProcedure/RegisterFunction` - Registers callable declarations
- `LoadUnitSymbols(UnitNode)` - Loads symbols from imported units

---

### Interpreter.cs
**Purpose**: Directly executes Pascal programs from the AST.

**Class**: `Interpreter`

**Key Fields**:
- `_variables` - Runtime variable storage (name → value)
- `_procedures` - Procedure definitions
- `_functions` - Function definitions
- `_heap` - Simulated heap for pointers
- `_arrays` - Array data storage
- `_sets` - Set data storage
- `_fileWriters`, `_fileReaders` - File I/O handles
- `_enumTypes`, `_enumValues` - Enumeration types and values

**Key Methods**:
- `Execute(ProgramNode)` - Executes a program
- `ExecuteBlock(BlockNode)` - Executes a block of statements
- `ExecuteStatement(ASTNode)` - Executes a single statement
- `EvaluateExpression(ASTNode)` - Evaluates an expression, returns value
- `ExecuteProcedure(ProcedureDeclarationNode, List<object>)` - Calls a procedure
- `ExecuteFunction(FunctionDeclarationNode, List<object>)` - Calls a function
- `GetDefaultValue(string)` - Returns default value for a type
- `LoadUnitSymbols(UnitNode)` - Loads and initializes unit symbols

---

### Bytecode.cs
**Purpose**: Defines bytecode instruction set and bytecode program structure.

**Enums**:
- `OpCode` - All bytecode operation codes

**Classes**:
- `Instruction` - Single bytecode instruction (opcode + operand)
- `BytecodeProgram` - Complete compiled program with instructions and metadata
- `FunctionInfo` - Metadata for compiled functions
- `EnumInfo` - Metadata for enumeration types
- `ArrayInfo` - Metadata for arrays

**Key OpCodes**:
- Stack: PUSH, POP, DUP
- Variables: LOAD_VAR, STORE_VAR
- Arithmetic: ADD, SUB, MUL, DIV, IDIV, MOD, NEG
- Comparison: EQ, NE, LT, GT, LE, GE
- Logical: AND, OR, NOT
- Control Flow: JUMP, JUMP_IF_FALSE, JUMP_IF_TRUE, CALL, RETURN
- I/O: WRITE, WRITELN, READLN
- Pointers: NEW, DISPOSE, DEREF, STORE_DEREF, ADDR_OF, PUSH_NIL
- Arrays: ARRAY_INDEX, ARRAY_STORE
- Records: RECORD_GET, RECORD_SET
- Files: FILE_ASSIGN, FILE_RESET, FILE_REWRITE, FILE_CLOSE, FILE_READ, FILE_WRITE, FILE_EOF
- Sets: SET_LITERAL, SET_CONTAINS

---

### BytecodeCompiler.cs
**Purpose**: Compiles AST to bytecode.

**Class**: `BytecodeCompiler`

**Key Methods**:
- `Compile(ProgramNode)` - Compiles program to BytecodeProgram
- `CompileUnit(UnitNode)` - Compiles unit to BytecodeUnit
- `CompileBlock(BlockNode)` - Compiles block of statements
- `CompileStatement(ASTNode)` - Compiles single statement
- `CompileExpression(ASTNode)` - Compiles expression
- `CompileProcedure(ProcedureDeclarationNode)` - Compiles procedure
- `CompileFunction(FunctionDeclarationNode)` - Compiles function

**Compilation Strategy**:
- Generates stack-based bytecode
- Functions become bytecode sections with entry points
- Labels resolved to instruction addresses
- Variables tracked by name

---

### BytecodeSerializer.cs
**Purpose**: Serializes and deserializes bytecode programs to/from .pbc files.

**Class**: `BytecodeSerializer`

**File Format**:
- Magic number: 0x50415343 ("PASC")
- Version: 1
- Sections: Variables, Labels, Enums, Arrays, Functions, Instructions

**Key Methods**:
- `SaveToFile(BytecodeProgram, string)` - Writes bytecode to file
- `LoadFromFile(string)` - Loads bytecode from file
- `DisassembleToString(BytecodeProgram)` - Human-readable disassembly

---

### VirtualMachine.cs
**Purpose**: Executes bytecode programs.

**Class**: `VirtualMachine`

**Key Fields**:
- `_stack` - Evaluation stack
- `_callStack` - Call frames for function calls
- `_variables` - Runtime variables
- `_instructionPointer` - Current instruction index
- `_program` - Bytecode being executed
- `_heap` - Simulated heap for pointers
- `_arrays` - Array storage
- `_fileWriters`, `_fileReaders` - File I/O

**Key Methods**:
- `Execute(BytecodeProgram)` - Executes a bytecode program
- `ExecuteInstruction(Instruction)` - Executes single instruction
- `LoadUnitSymbols(BytecodeUnit)` - Loads unit bytecode and merges with program
- `ExecuteUnitInitialization(BytecodeUnit)` - Runs unit initialization code

**Execution Model**:
- Stack-based architecture
- Call frames for procedure/function scope
- Variables stored by name
- Functions return via stack

---

### BytecodeUnit.cs
**Purpose**: Represents a compiled Pascal unit (module).

**Class**: `BytecodeUnit`

**Key Properties**:
- `Name` - Unit name
- `UsedUnits` - Dependencies
- `Variables` - Unit variables
- `Functions` - Unit functions
- `Instructions` - Compiled function code
- `InitializationCode` - Unit initialization block
- `FinalizationCode` - Unit finalization block
- `RecordTypes`, `EnumTypes`, `Arrays` - Type metadata

---

### BytecodeUnitSerializer.cs
**Purpose**: Serializes and deserializes unit bytecode to/from .pbu files.

**Class**: `BytecodeUnitSerializer`

**File Format**:
- Magic number: 0x50415355 ("PASU")
- Version: 1
- Sections: Used Units, Variables, Enums, Arrays, Functions, Instructions, Initialization, Finalization

**Key Methods**:
- `SaveToFile(BytecodeUnit, string)` - Writes unit to file
- `LoadFromFile(string)` - Loads unit from file
- `DisassembleToString(BytecodeUnit)` - Disassembly output

---

### BytecodeUnitLoader.cs
**Purpose**: Loads compiled unit bytecode files with dependency resolution.

**Class**: `BytecodeUnitLoader`

**Key Fields**:
- `_loadedUnits` - Cache of loaded units
- `_loadingUnits` - Tracks units being loaded (circular dependency detection)
- `_searchPath` - Directory to search for units

**Key Methods**:
- `LoadUnit(string)` - Loads unit by name, handles caching and dependencies
- `FindUnitFile(string)` - Locates .pbu file
- `GetLoadedUnits()` - Returns all loaded units
- `Clear()` - Clears cache

---

### UnitLoader.cs
**Purpose**: Loads Pascal source unit files (.pas) with dependency resolution.

**Class**: `UnitLoader`

**Key Fields**:
- `_loadedUnits` - Cache of parsed units
- `_loadingUnits` - Circular dependency detection
- `_searchPath` - Directory to search for units

**Key Methods**:
- `LoadUnit(string)` - Parses and loads unit from source
- `FindUnitFile(string)` - Locates .pas file
- `GetLoadedUnits()` - Returns cached units
- `Clear()` - Clears cache

**Features**:
- Recursive dependency loading
- Circular dependency detection
- Automatic parsing and semantic analysis
- Caching to avoid reparsing

---

### Program.cs
**Purpose**: Main entry point and command-line interface.

**Class**: `Program`

**Command-Line Options**:
- `-d, --debug` - Step-by-step debug mode
- `-c, --compile` - Compile to bytecode (.pbc)
- `-u, --compile-unit` - Compile unit to bytecode (.pbu)
- `-r, --run` - Run compiled bytecode
- `-s, --disassemble` - Disassemble bytecode
- `-o, --output` - Specify output file

**Key Methods**:
- `Main(string[])` - Entry point, parses args
- `RunDemo(bool)` - Runs demo program
- `CompileAndRun(string, bool)` - Interprets source
- `CompileToBytecode(string, string)` - Compiles program
- `CompileUnitToBytecode(string, string)` - Compiles unit
- `RunBytecodeFile(string)` - Executes bytecode
- `DisassembleBytecode(string)` - Disassembles bytecode

---

### PascalDebugger.cs
**Purpose**: Interactive debugger for step-by-step execution.

**Class**: `PascalDebugger`

**Features**:
- Step-by-step statement execution
- Variable inspection
- Breakpoint support
- Stack trace display

**Key Methods**:
- `Execute(ProgramNode)` - Starts debug session
- `DebugStatement(ASTNode)` - Steps through statement
- `PrintVariables()` - Displays current variables
- `WaitForCommand()` - Processes debugger commands

**Commands**:
- `s` - Step to next statement
- `c` - Continue execution
- `v` - Show variables
- `q` - Quit debugger

---

## Data Flow

### Interpretation Path
```
Source Code (.pas)
  ↓
Lexer → Tokens
  ↓
Parser → AST (ProgramNode)
  ↓
SemanticAnalyzer → Validated AST
  ↓
Interpreter → Execution
```

### Compilation Path
```
Source Code (.pas)
  ↓
Lexer → Tokens
  ↓
Parser → AST (ProgramNode)
  ↓
SemanticAnalyzer → Validated AST
  ↓
BytecodeCompiler → BytecodeProgram
  ↓
BytecodeSerializer → .pbc file
```

### Unit Compilation
```
Unit Source (.pas)
  ↓
Lexer → Tokens
  ↓
Parser → AST (UnitNode)
  ↓
SemanticAnalyzer → Validated AST
  ↓
BytecodeCompiler → BytecodeUnit
  ↓
BytecodeUnitSerializer → .pbu file
```

### Bytecode Execution
```
.pbc file
  ↓
BytecodeSerializer → BytecodeProgram
  ↓
BytecodeUnitLoader → Load dependencies (.pbu files)
  ↓
VirtualMachine → Execution
```

## Error Handling

- **Lexical Errors**: Thrown by Lexer with line/column info
- **Syntax Errors**: Thrown by Parser with token context
- **Semantic Errors**: Collected by SemanticAnalyzer in Errors list
- **Runtime Errors**: Thrown by Interpreter/VirtualMachine with context

## Testing

Test files located in `PascalCompiler.Tests/`:
- `LexerTests.cs` - Token generation tests
- `ParserTests.cs` - AST construction tests
- `SemanticAnalyzerTests.cs` - Type checking tests
- `InterpreterTests.cs` - Execution tests
- `IntegrationTests.cs` - End-to-end tests

## Performance Considerations

- **Lexer**: Single-pass, O(n) where n = source length
- **Parser**: Recursive descent, O(n) where n = tokens
- **Semantic Analysis**: Multiple passes, O(n) where n = AST nodes
- **Interpreter**: Direct AST traversal, slower than bytecode
- **Bytecode VM**: Stack-based, faster than interpretation
- **Unit Loading**: Cached to avoid reparsing/recompilation

## Extension Points

To add new language features:

1. Add tokens to `TokenType` enum and `Lexer.Keywords`
2. Create AST node class in `AST.cs`
3. Add parsing logic to `Parser.cs`
4. Add semantic checks to `SemanticAnalyzer.cs`
5. Add interpretation logic to `Interpreter.cs`
6. Add opcodes to `OpCode` enum (if needed)
7. Add compilation logic to `BytecodeCompiler.cs`
8. Add VM execution logic to `VirtualMachine.cs`
9. Update serializers if new metadata needed
10. Add tests

## File Extensions

- `.pas` - Pascal source files (programs and units)
- `.pbc` - Pascal Bytecode Compiled programs
- `.pbu` - Pascal Bytecode Unit (compiled modules)
