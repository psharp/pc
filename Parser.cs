using System;
using System.Collections.Generic;
using System.Linq;

namespace PascalCompiler;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _position;
    private Token _currentToken;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
        _position = 0;
        _currentToken = _tokens[0];
    }

    private void Advance()
    {
        _position++;
        _currentToken = _position < _tokens.Count ? _tokens[_position] : _tokens[^1];
    }

    private void Expect(TokenType type)
    {
        if (_currentToken.Type != type)
        {
            throw new Exception($"Expected {type} but got {_currentToken.Type} at {_currentToken.Line}:{_currentToken.Column}");
        }
        Advance();
    }

    private bool Match(params TokenType[] types)
    {
        return types.Contains(_currentToken.Type);
    }

    public ProgramNode ParseProgram()
    {
        Expect(TokenType.PROGRAM);
        string name = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);
        Expect(TokenType.SEMICOLON);

        // Parse uses clause
        var usedUnits = new List<string>();
        if (_currentToken.Type == TokenType.USES)
        {
            usedUnits = ParseUsesClause();
        }

        // Parse type declarations (records and enums)
        var recordTypes = new List<RecordTypeNode>();
        var enumTypes = new List<EnumTypeNode>();
        if (_currentToken.Type == TokenType.TYPE)
        {
            ParseTypeDeclarations(recordTypes, enumTypes);
        }

        // Parse variable declarations
        var variables = new List<VarDeclarationNode>();
        var arrayVariables = new List<ArrayVarDeclarationNode>();
        var recordVariables = new List<RecordVarDeclarationNode>();
        var fileVariables = new List<FileVarDeclarationNode>();
        var pointerVariables = new List<PointerVarDeclarationNode>();
        var setVariables = new List<SetVarDeclarationNode>();

        if (_currentToken.Type == TokenType.VAR)
        {
            ParseVariableDeclarations(variables, arrayVariables, recordVariables, fileVariables, pointerVariables, setVariables, recordTypes, enumTypes);
        }

        var procedures = new List<ProcedureDeclarationNode>();
        var functions = new List<FunctionDeclarationNode>();

        while (_currentToken.Type == TokenType.PROCEDURE || _currentToken.Type == TokenType.FUNCTION)
        {
            if (_currentToken.Type == TokenType.PROCEDURE)
            {
                procedures.Add(ParseProcedureDeclaration());
            }
            else
            {
                functions.Add(ParseFunctionDeclaration());
            }
        }

        BlockNode block = ParseBlock();
        Expect(TokenType.DOT);

        return new ProgramNode(name, usedUnits, recordTypes, enumTypes, variables, arrayVariables, recordVariables, fileVariables, pointerVariables, setVariables, procedures, functions, block);
    }

    private List<string> ParseUsesClause()
    {
        Expect(TokenType.USES);
        var units = new List<string> { _currentToken.Value };
        Expect(TokenType.IDENTIFIER);

        while (_currentToken.Type == TokenType.COMMA)
        {
            Advance();
            units.Add(_currentToken.Value);
            Expect(TokenType.IDENTIFIER);
        }

        Expect(TokenType.SEMICOLON);
        return units;
    }

    private List<VarDeclarationNode> ParseVarDeclarations()
    {
        var declarations = new List<VarDeclarationNode>();
        Expect(TokenType.VAR);

        while (_currentToken.Type == TokenType.IDENTIFIER)
        {
            var names = new List<string> { _currentToken.Value };
            Advance();

            while (_currentToken.Type == TokenType.COMMA)
            {
                Advance();
                names.Add(_currentToken.Value);
                Expect(TokenType.IDENTIFIER);
            }

            Expect(TokenType.COLON);
            string type = _currentToken.Value;

            if (!Match(TokenType.INTEGER, TokenType.REAL, TokenType.STRING, TokenType.BOOLEAN))
            {
                throw new Exception($"Expected type but got {_currentToken.Type}");
            }
            Advance();
            Expect(TokenType.SEMICOLON);

            declarations.Add(new VarDeclarationNode(names, type));
        }

        return declarations;
    }

    private BlockNode ParseBlock()
    {
        Expect(TokenType.BEGIN);
        var statements = new List<StatementNode>();

        while (_currentToken.Type != TokenType.END)
        {
            statements.Add(ParseStatement());

            if (_currentToken.Type == TokenType.SEMICOLON)
            {
                Advance();
            }
        }

        Expect(TokenType.END);
        return new BlockNode(statements);
    }

    private StatementNode ParseStatement()
    {
        switch (_currentToken.Type)
        {
            case TokenType.IDENTIFIER:
                string lowerValue = _currentToken.Value.ToLower();
                if (lowerValue == "writeln" || lowerValue == "write")
                {
                    return ParseWrite();
                }
                else if (lowerValue == "readln" || lowerValue == "read")
                {
                    return ParseRead();
                }
                // Check if this might be a procedure call or assignment
                return ParseAssignmentOrProcedureCall();

            case TokenType.ASSIGN_FILE:
                return ParseFileAssign();

            case TokenType.RESET:
                return ParseFileReset();

            case TokenType.REWRITE:
                return ParseFileRewrite();

            case TokenType.CLOSE:
                return ParseFileClose();

            case TokenType.NEW:
                return ParseNew();

            case TokenType.DISPOSE:
                return ParseDispose();

            case TokenType.IF:
                return ParseIf();

            case TokenType.WHILE:
                return ParseWhile();

            case TokenType.FOR:
                return ParseFor();

            case TokenType.BEGIN:
                return ParseCompoundStatement();

            default:
                throw new Exception($"Unexpected statement starting with {_currentToken.Type}");
        }
    }

    private StatementNode ParseAssignmentOrProcedureCall()
    {
        string name = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);

        // Check for array indexing: arr[index] or arr[index].field
        if (_currentToken.Type == TokenType.LBRACKET)
        {
            Advance();
            var indices = new List<ExpressionNode>();
            indices.Add(ParseExpression());

            // Check for additional indices (multidimensional array)
            while (_currentToken.Type == TokenType.COMMA)
            {
                Advance();
                indices.Add(ParseExpression());
            }

            Expect(TokenType.RBRACKET);

            // Check for field access after array index: arr[index].field
            if (_currentToken.Type == TokenType.DOT)
            {
                Advance();
                string fieldName = _currentToken.Value;
                Expect(TokenType.IDENTIFIER);
                Expect(TokenType.ASSIGN);
                ExpressionNode value = ParseExpression();
                if (indices.Count > 1)
                {
                    throw new Exception("Multidimensional arrays with record elements not supported");
                }
                return new ArrayRecordAssignmentNode(name, indices[0], fieldName, value);
            }
            else
            {
                Expect(TokenType.ASSIGN);
                ExpressionNode value = ParseExpression();
                return new ArrayAssignmentNode(name, indices, value);
            }
        }
        // Check for pointer dereference: ptr^ := value
        else if (_currentToken.Type == TokenType.CARET)
        {
            Advance();
            Expect(TokenType.ASSIGN);
            ExpressionNode value = ParseExpression();
            return new PointerAssignmentNode(new VariableNode(name), value);
        }
        // Check for record field access: record.field
        else if (_currentToken.Type == TokenType.DOT)
        {
            Advance();
            string fieldName = _currentToken.Value;
            Expect(TokenType.IDENTIFIER);
            Expect(TokenType.ASSIGN);
            ExpressionNode value = ParseExpression();
            return new RecordAssignmentNode(name, fieldName, value);
        }
        // Check if this is a procedure call (has parentheses)
        else if (_currentToken.Type == TokenType.LPAREN)
        {
            Advance();
            var arguments = new List<ExpressionNode>();
            if (_currentToken.Type != TokenType.RPAREN)
            {
                arguments.Add(ParseExpression());
                while (_currentToken.Type == TokenType.COMMA)
                {
                    Advance();
                    arguments.Add(ParseExpression());
                }
            }
            Expect(TokenType.RPAREN);
            return new ProcedureCallNode(name, arguments);
        }
        // Otherwise it's a simple assignment
        else if (_currentToken.Type == TokenType.ASSIGN)
        {
            Expect(TokenType.ASSIGN);
            ExpressionNode expression = ParseExpression();
            return new AssignmentNode(name, expression);
        }
        else
        {
            throw new Exception($"Expected :=, [, ., or ( after identifier {name}");
        }
    }

    private CompoundStatementNode ParseCompoundStatement()
    {
        var block = ParseBlock();
        return new CompoundStatementNode(block.Statements);
    }

    private IfNode ParseIf()
    {
        Expect(TokenType.IF);
        ExpressionNode condition = ParseExpression();
        Expect(TokenType.THEN);
        StatementNode thenBranch = ParseStatement();
        StatementNode? elseBranch = null;

        if (_currentToken.Type == TokenType.ELSE)
        {
            Advance();
            elseBranch = ParseStatement();
        }

        return new IfNode(condition, thenBranch, elseBranch);
    }

    private WhileNode ParseWhile()
    {
        Expect(TokenType.WHILE);
        ExpressionNode condition = ParseExpression();
        Expect(TokenType.DO);
        StatementNode body = ParseStatement();
        return new WhileNode(condition, body);
    }

    private ForNode ParseFor()
    {
        Expect(TokenType.FOR);
        string variable = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);
        Expect(TokenType.ASSIGN);
        ExpressionNode start = ParseExpression();

        bool isDownTo = false;
        if (_currentToken.Type == TokenType.TO)
        {
            Advance();
        }
        else if (_currentToken.Type == TokenType.DOWNTO)
        {
            isDownTo = true;
            Advance();
        }
        else
        {
            throw new Exception($"Expected TO or DOWNTO in for loop");
        }

        ExpressionNode end = ParseExpression();
        Expect(TokenType.DO);
        StatementNode body = ParseStatement();

        return new ForNode(variable, start, end, isDownTo, body);
    }

    private StatementNode ParseWrite()
    {
        bool newLine = _currentToken.Value.ToLower() == "writeln";
        Advance();

        // Handle writeln; without parentheses (just newline)
        if (_currentToken.Type == TokenType.SEMICOLON)
        {
            return new WriteNode(new List<ExpressionNode>(), newLine);
        }

        Expect(TokenType.LPAREN);

        // Check if first parameter is a file variable
        // If it's an identifier and we have a comma, it might be Write(f, value)
        if (_currentToken.Type == TokenType.IDENTIFIER)
        {
            int savedPos = _position;
            Token savedToken = _currentToken;
            string firstParam = _currentToken.Value;
            Advance();

            // Check if this is file I/O: Write(fileVar, ...)
            if (_currentToken.Type == TokenType.COMMA)
            {
                // This is file write syntax: Write(f, expr1, expr2, ...)
                Advance(); // skip comma

                var expressions = new List<ExpressionNode>();
                expressions.Add(ParseExpression());

                while (_currentToken.Type == TokenType.COMMA)
                {
                    Advance();
                    expressions.Add(ParseExpression());
                }

                Expect(TokenType.RPAREN);
                return new FileWriteNode(firstParam, expressions, newLine);
            }
            else
            {
                // Not file I/O, restore position and parse as regular write
                _position = savedPos;
                _currentToken = savedToken;
            }
        }

        // Parse as regular console write
        var consoleExpressions = new List<ExpressionNode>();
        if (_currentToken.Type != TokenType.RPAREN)
        {
            consoleExpressions.Add(ParseExpression());

            while (_currentToken.Type == TokenType.COMMA)
            {
                Advance();
                consoleExpressions.Add(ParseExpression());
            }
        }

        Expect(TokenType.RPAREN);
        return new WriteNode(consoleExpressions, newLine);
    }

    private StatementNode ParseRead()
    {
        bool readLine = _currentToken.Value.ToLower() == "readln";
        Advance();
        Expect(TokenType.LPAREN);

        // Check if first parameter is a file variable
        // If we have Read(f, var1, var2, ...) it's file I/O
        if (_currentToken.Type == TokenType.IDENTIFIER)
        {
            int savedPos = _position;
            Token savedToken = _currentToken;
            string firstParam = _currentToken.Value;
            Advance();

            // Check if this is file I/O: Read(fileVar, var1, ...)
            if (_currentToken.Type == TokenType.COMMA)
            {
                // This is file read syntax: Read(f, var1, var2, ...)
                Advance(); // skip comma

                var variables = new List<string>();
                variables.Add(_currentToken.Value);
                Expect(TokenType.IDENTIFIER);

                while (_currentToken.Type == TokenType.COMMA)
                {
                    Advance();
                    variables.Add(_currentToken.Value);
                    Expect(TokenType.IDENTIFIER);
                }

                Expect(TokenType.RPAREN);
                return new FileReadNode(firstParam, variables, readLine);
            }
            else
            {
                // Not file I/O, restore position and parse as regular read
                _position = savedPos;
                _currentToken = savedToken;
            }
        }

        // Parse as regular console read
        var consoleVariables = new List<string>();
        if (_currentToken.Type == TokenType.IDENTIFIER)
        {
            consoleVariables.Add(_currentToken.Value);
            Advance();

            while (_currentToken.Type == TokenType.COMMA)
            {
                Advance();
                consoleVariables.Add(_currentToken.Value);
                Expect(TokenType.IDENTIFIER);
            }
        }

        Expect(TokenType.RPAREN);
        return new ReadNode(consoleVariables);
    }

    private ExpressionNode ParseExpression()
    {
        return ParseLogicalOr();
    }

    private ExpressionNode ParseLogicalOr()
    {
        ExpressionNode left = ParseLogicalAnd();

        while (_currentToken.Type == TokenType.OR)
        {
            TokenType op = _currentToken.Type;
            Advance();
            ExpressionNode right = ParseLogicalAnd();
            left = new BinaryOpNode(left, op, right);
        }

        return left;
    }

    private ExpressionNode ParseLogicalAnd()
    {
        ExpressionNode left = ParseComparison();

        while (_currentToken.Type == TokenType.AND)
        {
            TokenType op = _currentToken.Type;
            Advance();
            ExpressionNode right = ParseComparison();
            left = new BinaryOpNode(left, op, right);
        }

        return left;
    }

    private ExpressionNode ParseComparison()
    {
        ExpressionNode left = ParseAdditive();

        while (Match(TokenType.EQUALS, TokenType.NOT_EQUALS, TokenType.LESS_THAN,
                     TokenType.GREATER_THAN, TokenType.LESS_EQUAL, TokenType.GREATER_EQUAL, TokenType.IN))
        {
            TokenType op = _currentToken.Type;
            Advance();
            ExpressionNode right = ParseAdditive();

            // Special handling for 'in' operator
            if (op == TokenType.IN)
            {
                left = new InNode(left, right);
            }
            else
            {
                left = new BinaryOpNode(left, op, right);
            }
        }

        return left;
    }

    private ExpressionNode ParseAdditive()
    {
        ExpressionNode left = ParseMultiplicative();

        while (Match(TokenType.PLUS, TokenType.MINUS))
        {
            TokenType op = _currentToken.Type;
            Advance();
            ExpressionNode right = ParseMultiplicative();
            left = new BinaryOpNode(left, op, right);
        }

        return left;
    }

    private ExpressionNode ParseMultiplicative()
    {
        ExpressionNode left = ParseUnary();

        while (Match(TokenType.MULTIPLY, TokenType.DIVIDE, TokenType.DIV, TokenType.MOD))
        {
            TokenType op = _currentToken.Type;
            Advance();
            ExpressionNode right = ParseUnary();
            left = new BinaryOpNode(left, op, right);
        }

        return left;
    }

    private ExpressionNode ParseUnary()
    {
        if (Match(TokenType.PLUS, TokenType.MINUS, TokenType.NOT))
        {
            TokenType op = _currentToken.Type;
            Advance();
            ExpressionNode operand = ParseUnary();
            return new UnaryOpNode(op, operand);
        }

        // Address-of operator: @variable
        if (_currentToken.Type == TokenType.AT)
        {
            Advance();
            string varName = _currentToken.Value;
            Expect(TokenType.IDENTIFIER);
            return new AddressOfNode(varName);
        }

        return ParsePostfix();
    }

    private ExpressionNode ParsePostfix()
    {
        ExpressionNode expr = ParsePrimary();

        // Pointer dereference: ptr^
        while (_currentToken.Type == TokenType.CARET)
        {
            Advance();
            expr = new PointerDereferenceNode(expr);
        }

        return expr;
    }

    private ExpressionNode ParsePrimary()
    {
        switch (_currentToken.Type)
        {
            case TokenType.INTEGER_LITERAL:
                int intValue = int.Parse(_currentToken.Value);
                Advance();
                return new NumberNode(intValue);

            case TokenType.REAL_LITERAL:
                double realValue = double.Parse(_currentToken.Value);
                Advance();
                return new NumberNode(realValue);

            case TokenType.STRING_LITERAL:
                string strValue = _currentToken.Value;
                Advance();
                return new StringNode(strValue);

            case TokenType.TRUE:
                Advance();
                return new BooleanNode(true);

            case TokenType.FALSE:
                Advance();
                return new BooleanNode(false);

            case TokenType.NIL:
                Advance();
                return new NilNode();

            case TokenType.EOF_FUNC:
                Advance();
                Expect(TokenType.LPAREN);
                string fileVar = _currentToken.Value;
                Expect(TokenType.IDENTIFIER);
                Expect(TokenType.RPAREN);
                return new FileEofNode(fileVar);

            case TokenType.IDENTIFIER:
                string name = _currentToken.Value;
                Advance();

                // Check for array indexing: arr[index] or arr[i, j] or arr[index].field
                if (_currentToken.Type == TokenType.LBRACKET)
                {
                    Advance();
                    var indices = new List<ExpressionNode>();
                    indices.Add(ParseExpression());

                    // Check for additional indices (multidimensional array)
                    while (_currentToken.Type == TokenType.COMMA)
                    {
                        Advance();
                        indices.Add(ParseExpression());
                    }

                    Expect(TokenType.RBRACKET);

                    // Check for field access after array index: arr[index].field
                    // Note: Only single-dimensional arrays can have record elements
                    if (_currentToken.Type == TokenType.DOT)
                    {
                        Advance();
                        string fieldName = _currentToken.Value;
                        Expect(TokenType.IDENTIFIER);
                        if (indices.Count > 1)
                        {
                            throw new Exception("Multidimensional arrays with record elements not supported");
                        }
                        return new ArrayRecordAccessNode(name, indices[0], fieldName);
                    }
                    return new ArrayAccessNode(name, indices);
                }
                // Check for record field access: record.field
                else if (_currentToken.Type == TokenType.DOT)
                {
                    Advance();
                    string fieldName = _currentToken.Value;
                    Expect(TokenType.IDENTIFIER);
                    return new RecordAccessNode(name, fieldName);
                }
                // Check if this is a function call
                else if (_currentToken.Type == TokenType.LPAREN)
                {
                    Advance();
                    var arguments = new List<ExpressionNode>();
                    if (_currentToken.Type != TokenType.RPAREN)
                    {
                        arguments.Add(ParseExpression());
                        while (_currentToken.Type == TokenType.COMMA)
                        {
                            Advance();
                            arguments.Add(ParseExpression());
                        }
                    }
                    Expect(TokenType.RPAREN);
                    return new FunctionCallNode(name, arguments);
                }
                return new VariableNode(name);

            case TokenType.LPAREN:
                Advance();
                ExpressionNode expr = ParseExpression();
                Expect(TokenType.RPAREN);
                return expr;

            case TokenType.LBRACKET:
                // Set literal: [value1, value2, ...]
                Advance();
                var elements = new List<ExpressionNode>();
                if (_currentToken.Type != TokenType.RBRACKET)
                {
                    elements.Add(ParseExpression());
                    while (_currentToken.Type == TokenType.COMMA)
                    {
                        Advance();
                        elements.Add(ParseExpression());
                    }
                }
                Expect(TokenType.RBRACKET);
                return new SetLiteralNode(elements);

            default:
                throw new Exception($"Unexpected token {_currentToken.Type} at {_currentToken.Line}:{_currentToken.Column}");
        }
    }

    private ProcedureDeclarationNode ParseProcedureDeclaration()
    {
        Expect(TokenType.PROCEDURE);
        string name = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);

        var parameters = new List<ParameterNode>();
        if (_currentToken.Type == TokenType.LPAREN)
        {
            parameters = ParseParameters();
        }

        Expect(TokenType.SEMICOLON);

        var localVariables = new List<VarDeclarationNode>();
        if (_currentToken.Type == TokenType.VAR)
        {
            localVariables = ParseVarDeclarations();
        }

        // Parse nested procedures and functions
        var nestedProcedures = new List<ProcedureDeclarationNode>();
        var nestedFunctions = new List<FunctionDeclarationNode>();

        while (_currentToken.Type == TokenType.PROCEDURE || _currentToken.Type == TokenType.FUNCTION)
        {
            if (_currentToken.Type == TokenType.PROCEDURE)
            {
                nestedProcedures.Add(ParseProcedureDeclaration());
            }
            else
            {
                nestedFunctions.Add(ParseFunctionDeclaration());
            }
        }

        BlockNode block = ParseBlock();
        Expect(TokenType.SEMICOLON);

        return new ProcedureDeclarationNode(name, parameters, localVariables, block, nestedProcedures, nestedFunctions);
    }

    private FunctionDeclarationNode ParseFunctionDeclaration()
    {
        Expect(TokenType.FUNCTION);
        string name = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);

        var parameters = new List<ParameterNode>();
        if (_currentToken.Type == TokenType.LPAREN)
        {
            parameters = ParseParameters();
        }

        Expect(TokenType.COLON);
        string returnType = _currentToken.Value;
        if (!Match(TokenType.INTEGER, TokenType.REAL, TokenType.STRING, TokenType.BOOLEAN))
        {
            throw new Exception($"Expected type but got {_currentToken.Type}");
        }
        Advance();
        Expect(TokenType.SEMICOLON);

        var localVariables = new List<VarDeclarationNode>();
        if (_currentToken.Type == TokenType.VAR)
        {
            localVariables = ParseVarDeclarations();
        }

        // Parse nested procedures and functions
        var nestedProcedures = new List<ProcedureDeclarationNode>();
        var nestedFunctions = new List<FunctionDeclarationNode>();

        while (_currentToken.Type == TokenType.PROCEDURE || _currentToken.Type == TokenType.FUNCTION)
        {
            if (_currentToken.Type == TokenType.PROCEDURE)
            {
                nestedProcedures.Add(ParseProcedureDeclaration());
            }
            else
            {
                nestedFunctions.Add(ParseFunctionDeclaration());
            }
        }

        BlockNode block = ParseBlock();
        Expect(TokenType.SEMICOLON);

        return new FunctionDeclarationNode(name, parameters, returnType, localVariables, block, nestedProcedures, nestedFunctions);
    }

    private List<ParameterNode> ParseParameters()
    {
        var parameters = new List<ParameterNode>();
        Expect(TokenType.LPAREN);

        if (_currentToken.Type != TokenType.RPAREN)
        {
            // Check for optional 'var' keyword
            bool isVar = false;
            if (_currentToken.Type == TokenType.VAR)
            {
                isVar = true;
                Advance();
            }

            var names = new List<string> { _currentToken.Value };
            Expect(TokenType.IDENTIFIER);

            while (_currentToken.Type == TokenType.COMMA)
            {
                Advance();
                names.Add(_currentToken.Value);
                Expect(TokenType.IDENTIFIER);
            }

            Expect(TokenType.COLON);
            string type = _currentToken.Value;
            if (!Match(TokenType.INTEGER, TokenType.REAL, TokenType.STRING, TokenType.BOOLEAN))
            {
                throw new Exception($"Expected type but got {_currentToken.Type}");
            }
            Advance();

            parameters.Add(new ParameterNode(names, type, isVar));

            while (_currentToken.Type == TokenType.SEMICOLON)
            {
                Advance();

                // Check for optional 'var' keyword for next parameter group
                isVar = false;
                if (_currentToken.Type == TokenType.VAR)
                {
                    isVar = true;
                    Advance();
                }

                names = new List<string> { _currentToken.Value };
                Expect(TokenType.IDENTIFIER);

                while (_currentToken.Type == TokenType.COMMA)
                {
                    Advance();
                    names.Add(_currentToken.Value);
                    Expect(TokenType.IDENTIFIER);
                }

                Expect(TokenType.COLON);
                type = _currentToken.Value;
                if (!Match(TokenType.INTEGER, TokenType.REAL, TokenType.STRING, TokenType.BOOLEAN))
                {
                    throw new Exception($"Expected type but got {_currentToken.Type}");
                }
                Advance();

                parameters.Add(new ParameterNode(names, type, isVar));
            }
        }

        Expect(TokenType.RPAREN);
        return parameters;
    }

    // Parse type section (for record and enum definitions)
    private void ParseTypeDeclarations(List<RecordTypeNode> recordTypes, List<EnumTypeNode> enumTypes)
    {
        Expect(TokenType.TYPE);

        while (_currentToken.Type == TokenType.IDENTIFIER)
        {
            string typeName = _currentToken.Value;
            Advance();
            Expect(TokenType.EQUALS);

            // Check if it's an enumeration: (Value1, Value2, ...)
            if (_currentToken.Type == TokenType.LPAREN)
            {
                Advance();
                var enumValues = new List<string> { _currentToken.Value };
                Expect(TokenType.IDENTIFIER);

                while (_currentToken.Type == TokenType.COMMA)
                {
                    Advance();
                    enumValues.Add(_currentToken.Value);
                    Expect(TokenType.IDENTIFIER);
                }

                Expect(TokenType.RPAREN);
                Expect(TokenType.SEMICOLON);
                enumTypes.Add(new EnumTypeNode(typeName, enumValues));
            }
            // Check if it's a record
            else if (_currentToken.Type == TokenType.RECORD)
            {
                Advance();
                var fields = new List<RecordFieldNode>();
                while (_currentToken.Type == TokenType.IDENTIFIER)
                {
                    var fieldNames = new List<string> { _currentToken.Value };
                    Advance();

                    while (_currentToken.Type == TokenType.COMMA)
                    {
                        Advance();
                        fieldNames.Add(_currentToken.Value);
                        Expect(TokenType.IDENTIFIER);
                    }

                    Expect(TokenType.COLON);
                    string fieldType = _currentToken.Value;
                    if (!Match(TokenType.INTEGER, TokenType.REAL, TokenType.STRING, TokenType.BOOLEAN))
                    {
                        throw new Exception($"Expected type but got {_currentToken.Type}");
                    }
                    Advance();
                    Expect(TokenType.SEMICOLON);

                    fields.Add(new RecordFieldNode(fieldNames, fieldType));
                }

                Expect(TokenType.END);
                Expect(TokenType.SEMICOLON);
                recordTypes.Add(new RecordTypeNode(typeName, fields));
            }
            else
            {
                throw new Exception($"Expected RECORD or ( for type declaration but got {_currentToken.Type}");
            }
        }
    }

    // Parse variable declarations (including arrays, records, files, pointers, and sets)
    private void ParseVariableDeclarations(List<VarDeclarationNode> variables,
        List<ArrayVarDeclarationNode> arrayVariables,
        List<RecordVarDeclarationNode> recordVariables,
        List<FileVarDeclarationNode> fileVariables,
        List<PointerVarDeclarationNode> pointerVariables,
        List<SetVarDeclarationNode> setVariables,
        List<RecordTypeNode> recordTypes,
        List<EnumTypeNode> enumTypes)
    {
        Expect(TokenType.VAR);

        while (_currentToken.Type == TokenType.IDENTIFIER)
        {
            var names = new List<string> { _currentToken.Value };
            Advance();

            while (_currentToken.Type == TokenType.COMMA)
            {
                Advance();
                names.Add(_currentToken.Value);
                Expect(TokenType.IDENTIFIER);
            }

            Expect(TokenType.COLON);

            // Check if it's a set type (set of TypeName)
            if (_currentToken.Type == TokenType.SET)
            {
                Advance();
                Expect(TokenType.OF);
                string elementType = _currentToken.Value;

                // Element type can be a basic type or an enum type
                if (!Match(TokenType.INTEGER, TokenType.REAL, TokenType.STRING, TokenType.BOOLEAN) &&
                    !enumTypes.Any(et => et.Name.Equals(_currentToken.Value, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new Exception($"Expected set element type but got {_currentToken.Type}");
                }
                Advance();
                Expect(TokenType.SEMICOLON);
                setVariables.Add(new SetVarDeclarationNode(names, elementType));
            }
            // Check if it's a pointer type (^TypeName)
            else if (_currentToken.Type == TokenType.CARET)
            {
                Advance();
                string pointedType = _currentToken.Value;
                if (!Match(TokenType.INTEGER, TokenType.REAL, TokenType.STRING, TokenType.BOOLEAN, TokenType.IDENTIFIER))
                {
                    throw new Exception($"Expected type after ^ but got {_currentToken.Type}");
                }
                Advance();
                Expect(TokenType.SEMICOLON);
                pointerVariables.Add(new PointerVarDeclarationNode(names, pointedType));
            }
            // Check if it's a text file
            else if (_currentToken.Type == TokenType.TEXT)
            {
                Advance();
                Expect(TokenType.SEMICOLON);
                fileVariables.Add(new FileVarDeclarationNode(names, true));
            }
            // Check if it's a typed file (file of type)
            else if (_currentToken.Type == TokenType.FILE)
            {
                Advance();
                Expect(TokenType.OF);
                string elementType = _currentToken.Value;
                if (!Match(TokenType.INTEGER, TokenType.REAL, TokenType.STRING, TokenType.BOOLEAN))
                {
                    throw new Exception($"Expected file element type but got {_currentToken.Type}");
                }
                Advance();
                Expect(TokenType.SEMICOLON);
                fileVariables.Add(new FileVarDeclarationNode(names, false, elementType));
            }
            // Check if it's an array type
            else if (_currentToken.Type == TokenType.ARRAY)
            {
                Advance();
                Expect(TokenType.LBRACKET);

                // Parse dimensions (can be multiple: array[1..10, 1..20] of integer)
                var dimensions = new List<(int, int)>();

                int lowerBound = int.Parse(_currentToken.Value);
                Expect(TokenType.INTEGER_LITERAL);
                Expect(TokenType.DOTDOT);
                int upperBound = int.Parse(_currentToken.Value);
                Expect(TokenType.INTEGER_LITERAL);
                dimensions.Add((lowerBound, upperBound));

                // Check for additional dimensions
                while (_currentToken.Type == TokenType.COMMA)
                {
                    Advance();
                    lowerBound = int.Parse(_currentToken.Value);
                    Expect(TokenType.INTEGER_LITERAL);
                    Expect(TokenType.DOTDOT);
                    upperBound = int.Parse(_currentToken.Value);
                    Expect(TokenType.INTEGER_LITERAL);
                    dimensions.Add((lowerBound, upperBound));
                }

                Expect(TokenType.RBRACKET);
                Expect(TokenType.OF);

                string elementType = _currentToken.Value;
                // Allow basic types or record types as array elements
                if (!Match(TokenType.INTEGER, TokenType.REAL, TokenType.STRING, TokenType.BOOLEAN) &&
                    !recordTypes.Any(rt => rt.Name.Equals(_currentToken.Value, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new Exception($"Expected array element type but got {_currentToken.Type}");
                }
                Advance();
                Expect(TokenType.SEMICOLON);

                var arrayType = new ArrayTypeNode(dimensions, elementType);
                arrayVariables.Add(new ArrayVarDeclarationNode(names, arrayType));
            }
            // Check if it's a record type
            else if (recordTypes.Any(rt => rt.Name.Equals(_currentToken.Value, StringComparison.OrdinalIgnoreCase)))
            {
                string recordTypeName = _currentToken.Value;
                Advance();
                Expect(TokenType.SEMICOLON);
                recordVariables.Add(new RecordVarDeclarationNode(names, recordTypeName));
            }
            // Check if it's an enum type
            else if (enumTypes.Any(et => et.Name.Equals(_currentToken.Value, StringComparison.OrdinalIgnoreCase)))
            {
                string enumTypeName = _currentToken.Value;
                Advance();
                Expect(TokenType.SEMICOLON);
                variables.Add(new VarDeclarationNode(names, enumTypeName));
            }
            // Regular variable
            else
            {
                string type = _currentToken.Value;
                if (!Match(TokenType.INTEGER, TokenType.REAL, TokenType.STRING, TokenType.BOOLEAN))
                {
                    throw new Exception($"Expected type but got {_currentToken.Type}");
                }
                Advance();
                Expect(TokenType.SEMICOLON);
                variables.Add(new VarDeclarationNode(names, type));
            }
        }
    }

    // Parse Assign(fileVar, filename)
    private StatementNode ParseFileAssign()
    {
        Expect(TokenType.ASSIGN_FILE);
        Expect(TokenType.LPAREN);
        string fileVar = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);
        Expect(TokenType.COMMA);
        ExpressionNode fileName = ParseExpression();
        Expect(TokenType.RPAREN);
        return new FileAssignNode(fileVar, fileName);
    }

    // Parse Reset(fileVar)
    private StatementNode ParseFileReset()
    {
        Expect(TokenType.RESET);
        Expect(TokenType.LPAREN);
        string fileVar = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);
        Expect(TokenType.RPAREN);
        return new FileResetNode(fileVar);
    }

    // Parse Rewrite(fileVar)
    private StatementNode ParseFileRewrite()
    {
        Expect(TokenType.REWRITE);
        Expect(TokenType.LPAREN);
        string fileVar = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);
        Expect(TokenType.RPAREN);
        return new FileRewriteNode(fileVar);
    }

    // Parse Close(fileVar)
    private StatementNode ParseFileClose()
    {
        Expect(TokenType.CLOSE);
        Expect(TokenType.LPAREN);
        string fileVar = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);
        Expect(TokenType.RPAREN);
        return new FileCloseNode(fileVar);
    }

    // Parse New(ptr)
    private StatementNode ParseNew()
    {
        Expect(TokenType.NEW);
        Expect(TokenType.LPAREN);
        string ptrVar = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);
        Expect(TokenType.RPAREN);
        return new NewNode(ptrVar);
    }

    // Parse Dispose(ptr)
    private StatementNode ParseDispose()
    {
        Expect(TokenType.DISPOSE);
        Expect(TokenType.LPAREN);
        string ptrVar = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);
        Expect(TokenType.RPAREN);
        return new DisposeNode(ptrVar);
    }

    public UnitNode ParseUnit()
    {
        Expect(TokenType.UNIT);
        string name = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);
        Expect(TokenType.SEMICOLON);

        // Parse interface section
        Expect(TokenType.INTERFACE);

        // Parse uses clause in interface (optional)
        var usedUnits = new List<string>();
        if (_currentToken.Type == TokenType.USES)
        {
            usedUnits = ParseUsesClause();
        }

        // Parse interface type declarations
        var interfaceRecordTypes = new List<RecordTypeNode>();
        var interfaceEnumTypes = new List<EnumTypeNode>();
        if (_currentToken.Type == TokenType.TYPE)
        {
            ParseTypeDeclarations(interfaceRecordTypes, interfaceEnumTypes);
        }

        // Parse interface variable declarations
        var interfaceVariables = new List<VarDeclarationNode>();
        if (_currentToken.Type == TokenType.VAR)
        {
            interfaceVariables = ParseVarDeclarations();
        }

        // Parse interface procedure/function declarations (headers only)
        var interfaceProcedures = new List<ProcedureDeclarationNode>();
        var interfaceFunctions = new List<FunctionDeclarationNode>();

        while (_currentToken.Type == TokenType.PROCEDURE || _currentToken.Type == TokenType.FUNCTION)
        {
            if (_currentToken.Type == TokenType.PROCEDURE)
            {
                interfaceProcedures.Add(ParseProcedureHeader());
            }
            else
            {
                interfaceFunctions.Add(ParseFunctionHeader());
            }
        }

        // Parse implementation section
        Expect(TokenType.IMPLEMENTATION);

        // Parse implementation type declarations
        var implRecordTypes = new List<RecordTypeNode>();
        var implEnumTypes = new List<EnumTypeNode>();
        if (_currentToken.Type == TokenType.TYPE)
        {
            ParseTypeDeclarations(implRecordTypes, implEnumTypes);
        }

        // Parse implementation variable declarations
        var implVariables = new List<VarDeclarationNode>();
        var implArrayVariables = new List<ArrayVarDeclarationNode>();
        var implRecordVariables = new List<RecordVarDeclarationNode>();
        var implFileVariables = new List<FileVarDeclarationNode>();
        var implPointerVariables = new List<PointerVarDeclarationNode>();
        var implSetVariables = new List<SetVarDeclarationNode>();

        if (_currentToken.Type == TokenType.VAR)
        {
            // Combine interface and implementation types for variable resolution
            var allRecordTypes = new List<RecordTypeNode>(interfaceRecordTypes);
            allRecordTypes.AddRange(implRecordTypes);
            var allEnumTypes = new List<EnumTypeNode>(interfaceEnumTypes);
            allEnumTypes.AddRange(implEnumTypes);

            ParseVariableDeclarations(implVariables, implArrayVariables, implRecordVariables,
                implFileVariables, implPointerVariables, implSetVariables, allRecordTypes, allEnumTypes);
        }

        // Parse implementation procedures/functions (full definitions)
        var implProcedures = new List<ProcedureDeclarationNode>();
        var implFunctions = new List<FunctionDeclarationNode>();

        while (_currentToken.Type == TokenType.PROCEDURE || _currentToken.Type == TokenType.FUNCTION)
        {
            if (_currentToken.Type == TokenType.PROCEDURE)
            {
                implProcedures.Add(ParseProcedureDeclaration());
            }
            else
            {
                implFunctions.Add(ParseFunctionDeclaration());
            }
        }

        // Parse optional initialization section
        BlockNode? initializationBlock = null;
        if (_currentToken.Type == TokenType.INITIALIZATION)
        {
            Advance();
            initializationBlock = ParseBlock();
        }

        // Parse optional finalization section
        BlockNode? finalizationBlock = null;
        if (_currentToken.Type == TokenType.FINALIZATION)
        {
            Advance();
            finalizationBlock = ParseBlock();
        }

        // Expect END.
        Expect(TokenType.END);
        Expect(TokenType.DOT);

        return new UnitNode(name, usedUnits,
            interfaceRecordTypes, interfaceEnumTypes, interfaceVariables,
            interfaceProcedures, interfaceFunctions,
            implRecordTypes, implEnumTypes, implVariables, implArrayVariables,
            implRecordVariables, implFileVariables, implPointerVariables, implSetVariables,
            implProcedures, implFunctions,
            initializationBlock, finalizationBlock);
    }

    private ProcedureDeclarationNode ParseProcedureHeader()
    {
        Expect(TokenType.PROCEDURE);
        string name = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);

        var parameters = new List<ParameterNode>();
        if (_currentToken.Type == TokenType.LPAREN)
        {
            parameters = ParseParameters();
        }

        Expect(TokenType.SEMICOLON);

        // Return a procedure header with empty body
        return new ProcedureDeclarationNode(name, parameters,
            new List<VarDeclarationNode>(),
            new BlockNode(new List<StatementNode>()));
    }

    private FunctionDeclarationNode ParseFunctionHeader()
    {
        Expect(TokenType.FUNCTION);
        string name = _currentToken.Value;
        Expect(TokenType.IDENTIFIER);

        var parameters = new List<ParameterNode>();
        if (_currentToken.Type == TokenType.LPAREN)
        {
            parameters = ParseParameters();
        }

        Expect(TokenType.COLON);
        string returnType = _currentToken.Value;

        if (!Match(TokenType.INTEGER, TokenType.REAL, TokenType.STRING, TokenType.BOOLEAN, TokenType.IDENTIFIER))
        {
            throw new Exception($"Expected return type but got {_currentToken.Type}");
        }
        Advance();
        Expect(TokenType.SEMICOLON);

        // Return a function header with empty body
        return new FunctionDeclarationNode(name, parameters, returnType,
            new List<VarDeclarationNode>(),
            new BlockNode(new List<StatementNode>()));
    }
}

