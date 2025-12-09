using System;
using System.Collections.Generic;
using System.Text;

namespace PascalCompiler;

public class Lexer
{
    private readonly string _source;
    private int _position;
    private int _line = 1;
    private int _column = 1;
    private char _currentChar;

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "program", TokenType.PROGRAM },
        { "var", TokenType.VAR },
        { "begin", TokenType.BEGIN },
        { "end", TokenType.END },
        { "integer", TokenType.INTEGER },
        { "real", TokenType.REAL },
        { "string", TokenType.STRING },
        { "boolean", TokenType.BOOLEAN },
        { "if", TokenType.IF },
        { "then", TokenType.THEN },
        { "else", TokenType.ELSE },
        { "while", TokenType.WHILE },
        { "do", TokenType.DO },
        { "for", TokenType.FOR },
        { "to", TokenType.TO },
        { "downto", TokenType.DOWNTO },
        { "procedure", TokenType.PROCEDURE },
        { "function", TokenType.FUNCTION },
        { "const", TokenType.CONST },
        { "type", TokenType.TYPE },
        { "array", TokenType.ARRAY },
        { "of", TokenType.OF },
        { "record", TokenType.RECORD },
        { "true", TokenType.TRUE },
        { "false", TokenType.FALSE },
        { "div", TokenType.DIV },
        { "mod", TokenType.MOD },
        { "and", TokenType.AND },
        { "or", TokenType.OR },
        { "not", TokenType.NOT },
        { "file", TokenType.FILE },
        { "text", TokenType.TEXT },
        { "reset", TokenType.RESET },
        { "rewrite", TokenType.REWRITE },
        { "close", TokenType.CLOSE },
        { "eof", TokenType.EOF_FUNC },
        { "assign", TokenType.ASSIGN_FILE },
        { "nil", TokenType.NIL },
        { "new", TokenType.NEW },
        { "dispose", TokenType.DISPOSE },
        { "set", TokenType.SET },
        { "in", TokenType.IN },
        { "unit", TokenType.UNIT },
        { "interface", TokenType.INTERFACE },
        { "implementation", TokenType.IMPLEMENTATION },
        { "uses", TokenType.USES },
        { "initialization", TokenType.INITIALIZATION },
        { "finalization", TokenType.FINALIZATION }
    };

    public Lexer(string source)
    {
        _source = source;
        _position = 0;
        _currentChar = _source.Length > 0 ? _source[0] : '\0';
    }

    private void Advance()
    {
        if (_currentChar == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }

        _position++;
        _currentChar = _position < _source.Length ? _source[_position] : '\0';
    }

    private char Peek(int offset = 1)
    {
        int peekPos = _position + offset;
        return peekPos < _source.Length ? _source[peekPos] : '\0';
    }

    private void SkipWhitespace()
    {
        while (_currentChar != '\0' && char.IsWhiteSpace(_currentChar))
        {
            Advance();
        }
    }

    private void SkipComment()
    {
        if (_currentChar == '{')
        {
            while (_currentChar != '\0' && _currentChar != '}')
            {
                Advance();
            }
            if (_currentChar == '}')
            {
                Advance();
            }
        }
        else if (_currentChar == '(' && Peek() == '*')
        {
            Advance();
            Advance();
            while (_currentChar != '\0')
            {
                if (_currentChar == '*' && Peek() == ')')
                {
                    Advance();
                    Advance();
                    break;
                }
                Advance();
            }
        }
        else if (_currentChar == '/' && Peek() == '/')
        {
            while (_currentChar != '\0' && _currentChar != '\n')
            {
                Advance();
            }
        }
    }

    private Token Number()
    {
        var sb = new StringBuilder();
        int line = _line;
        int column = _column;
        bool isReal = false;

        while (_currentChar != '\0' && (char.IsDigit(_currentChar) || _currentChar == '.'))
        {
            if (_currentChar == '.')
            {
                if (isReal || Peek() == '.')
                {
                    break;
                }
                isReal = true;
            }
            sb.Append(_currentChar);
            Advance();
        }

        return new Token(
            isReal ? TokenType.REAL_LITERAL : TokenType.INTEGER_LITERAL,
            sb.ToString(),
            line,
            column
        );
    }

    private Token Identifier()
    {
        var sb = new StringBuilder();
        int line = _line;
        int column = _column;

        while (_currentChar != '\0' && (char.IsLetterOrDigit(_currentChar) || _currentChar == '_'))
        {
            sb.Append(_currentChar);
            Advance();
        }

        string value = sb.ToString();
        string lowerValue = value.ToLower();

        if (Keywords.TryGetValue(lowerValue, out TokenType type))
        {
            return new Token(type, lowerValue, line, column);
        }

        return new Token(TokenType.IDENTIFIER, value, line, column);
    }

    private Token StringLiteral()
    {
        var sb = new StringBuilder();
        int line = _line;
        int column = _column;
        char quote = _currentChar;

        Advance(); // Skip opening quote

        while (_currentChar != '\0' && _currentChar != quote)
        {
            if (_currentChar == '\\' && Peek() == quote)
            {
                Advance();
            }
            sb.Append(_currentChar);
            Advance();
        }

        if (_currentChar == quote)
        {
            Advance(); // Skip closing quote
        }

        return new Token(TokenType.STRING_LITERAL, sb.ToString(), line, column);
    }

    public Token GetNextToken()
    {
        while (_currentChar != '\0')
        {
            if (char.IsWhiteSpace(_currentChar))
            {
                SkipWhitespace();
                continue;
            }

            if (_currentChar == '{' || (_currentChar == '(' && Peek() == '*') || (_currentChar == '/' && Peek() == '/'))
            {
                SkipComment();
                continue;
            }

            if (char.IsDigit(_currentChar))
            {
                return Number();
            }

            if (char.IsLetter(_currentChar) || _currentChar == '_')
            {
                return Identifier();
            }

            if (_currentChar == '\'' || _currentChar == '"')
            {
                return StringLiteral();
            }

            int line = _line;
            int column = _column;

            switch (_currentChar)
            {
                case '+':
                    Advance();
                    return new Token(TokenType.PLUS, "+", line, column);
                case '-':
                    Advance();
                    return new Token(TokenType.MINUS, "-", line, column);
                case '*':
                    Advance();
                    return new Token(TokenType.MULTIPLY, "*", line, column);
                case '/':
                    Advance();
                    return new Token(TokenType.DIVIDE, "/", line, column);
                case '=':
                    Advance();
                    return new Token(TokenType.EQUALS, "=", line, column);
                case '<':
                    Advance();
                    if (_currentChar == '=')
                    {
                        Advance();
                        return new Token(TokenType.LESS_EQUAL, "<=", line, column);
                    }
                    else if (_currentChar == '>')
                    {
                        Advance();
                        return new Token(TokenType.NOT_EQUALS, "<>", line, column);
                    }
                    return new Token(TokenType.LESS_THAN, "<", line, column);
                case '>':
                    Advance();
                    if (_currentChar == '=')
                    {
                        Advance();
                        return new Token(TokenType.GREATER_EQUAL, ">=", line, column);
                    }
                    return new Token(TokenType.GREATER_THAN, ">", line, column);
                case ':':
                    Advance();
                    if (_currentChar == '=')
                    {
                        Advance();
                        return new Token(TokenType.ASSIGN, ":=", line, column);
                    }
                    return new Token(TokenType.COLON, ":", line, column);
                case ';':
                    Advance();
                    return new Token(TokenType.SEMICOLON, ";", line, column);
                case ',':
                    Advance();
                    return new Token(TokenType.COMMA, ",", line, column);
                case '.':
                    Advance();
                    if (_currentChar == '.')
                    {
                        Advance();
                        return new Token(TokenType.DOTDOT, "..", line, column);
                    }
                    return new Token(TokenType.DOT, ".", line, column);
                case '(':
                    Advance();
                    return new Token(TokenType.LPAREN, "(", line, column);
                case ')':
                    Advance();
                    return new Token(TokenType.RPAREN, ")", line, column);
                case '[':
                    Advance();
                    return new Token(TokenType.LBRACKET, "[", line, column);
                case ']':
                    Advance();
                    return new Token(TokenType.RBRACKET, "]", line, column);
                case '^':
                    Advance();
                    return new Token(TokenType.CARET, "^", line, column);
                case '@':
                    Advance();
                    return new Token(TokenType.AT, "@", line, column);
                default:
                    throw new Exception($"Unexpected character '{_currentChar}' at {line}:{column}");
            }
        }

        return new Token(TokenType.EOF, "", _line, _column);
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        Token token;

        do
        {
            token = GetNextToken();
            tokens.Add(token);
        } while (token.Type != TokenType.EOF);

        return tokens;
    }
}
