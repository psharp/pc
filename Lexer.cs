using System;
using System.Collections.Generic;
using System.Text;

namespace PascalCompiler;

/// <summary>
/// Lexical analyzer (tokenizer) for the Pascal programming language.
/// Converts Pascal source code into a stream of tokens for parsing.
/// Supports all Pascal keywords, operators, literals, and special characters.
/// </summary>
public class Lexer
{
    /// <summary>
    /// The source code string being tokenized.
    /// </summary>
    private readonly string _source;

    /// <summary>
    /// Current position in the source code string (0-based index).
    /// </summary>
    private int _position;

    /// <summary>
    /// Current line number in the source code (1-based).
    /// Used for error reporting and token location tracking.
    /// </summary>
    private int _line = 1;

    /// <summary>
    /// Current column number in the source code (1-based).
    /// Used for error reporting and token location tracking.
    /// </summary>
    private int _column = 1;

    /// <summary>
    /// The current character being examined at _position.
    /// Set to '\0' when reaching the end of source.
    /// </summary>
    private char _currentChar;

    /// <summary>
    /// Dictionary mapping Pascal keywords (lowercase) to their corresponding token types.
    /// Used to distinguish between keywords and identifiers during tokenization.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the Lexer class with the given source code.
    /// Sets up initial position, line, and column tracking.
    /// </summary>
    /// <param name="source">The Pascal source code to tokenize.</param>
    public Lexer(string source)
    {
        _source = source;
        _position = 0;
        _currentChar = _source.Length > 0 ? _source[0] : '\0';
    }

    /// <summary>
    /// Advances the lexer to the next character in the source code.
    /// Automatically tracks line and column numbers for proper error reporting.
    /// </summary>
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

    /// <summary>
    /// Peeks ahead at a character in the source code without advancing the position.
    /// Useful for lookahead when distinguishing between operators like ':' and ':='.
    /// </summary>
    /// <param name="offset">Number of characters to look ahead (default is 1).</param>
    /// <returns>The character at the peek position, or '\0' if out of bounds.</returns>
    private char Peek(int offset = 1)
    {
        int peekPos = _position + offset;
        return peekPos < _source.Length ? _source[peekPos] : '\0';
    }

    /// <summary>
    /// Skips whitespace characters (spaces, tabs, newlines) in the source code.
    /// Advances the lexer position until a non-whitespace character is reached.
    /// </summary>
    private void SkipWhitespace()
    {
        while (_currentChar != '\0' && char.IsWhiteSpace(_currentChar))
        {
            Advance();
        }
    }

    /// <summary>
    /// Skips Pascal comments in all three supported formats:
    /// - { comment } (curly brace style)
    /// - (* comment *) (traditional Pascal style)
    /// - // comment (single-line style)
    /// Advances the lexer position to the character after the comment.
    /// </summary>
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

    /// <summary>
    /// Scans a numeric literal (integer or real) from the source code.
    /// Handles both integer literals (e.g., 42) and real literals (e.g., 3.14).
    /// Stops before '..' (range operator) to avoid confusion with real numbers.
    /// </summary>
    /// <returns>A token representing the numeric literal.</returns>
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

    /// <summary>
    /// Scans an identifier or keyword from the source code.
    /// Identifiers start with a letter or underscore and can contain letters, digits, and underscores.
    /// Pascal is case-insensitive, so keywords are matched in lowercase.
    /// </summary>
    /// <returns>A token representing either a keyword or an identifier.</returns>
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

    /// <summary>
    /// Scans a string literal from the source code.
    /// Supports both single quotes (') and double quotes (") for string delimiters.
    /// Handles escaped quotes using backslash (\' or \").
    /// </summary>
    /// <returns>A token representing the string literal.</returns>
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

    /// <summary>
    /// Gets the next token from the source code.
    /// Main tokenization logic that identifies token types and delegates to specialized methods.
    /// Automatically skips whitespace and comments.
    /// </summary>
    /// <returns>The next token in the source code, or an EOF token if at the end.</returns>
    /// <exception cref="Exception">Thrown when an unexpected character is encountered.</exception>
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

    /// <summary>
    /// Tokenizes the entire source code into a list of tokens.
    /// Repeatedly calls GetNextToken() until EOF is reached.
    /// This is the main entry point for lexical analysis.
    /// </summary>
    /// <returns>A list of all tokens in the source code, including the final EOF token.</returns>
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
