/// <summary>
/// Token and TokenType definitions for the Pascal lexical analyzer.
/// Represents all possible token types in the Pascal language including keywords,
/// operators, delimiters, literals, and identifiers.
/// </summary>
namespace PascalCompiler;

/// <summary>
/// Enumeration of all token types recognized by the Pascal lexer.
/// </summary>
public enum TokenType
{
    // Keywords
    PROGRAM, VAR, BEGIN, END, INTEGER, REAL, STRING, BOOLEAN,
    IF, THEN, ELSE, WHILE, DO, FOR, TO, DOWNTO, REPEAT, UNTIL,
    CASE, WITH,
    PROCEDURE, FUNCTION, CONST, TYPE, ARRAY, OF, RECORD,
    TRUE, FALSE, DIV, MOD, AND, OR, NOT,
    FILE, TEXT, RESET, REWRITE, READ, WRITE, CLOSE, EOF_FUNC, ASSIGN_FILE,
    PAGE, GET, PUT, PACK, UNPACK,
    NIL, NEW, DISPOSE,
    SET, IN,
    UNIT, INTERFACE, IMPLEMENTATION, USES, INITIALIZATION, FINALIZATION,
    GOTO, LABEL,

    // Operators
    PLUS, MINUS, MULTIPLY, DIVIDE, ASSIGN,
    EQUALS, NOT_EQUALS, LESS_THAN, GREATER_THAN,
    LESS_EQUAL, GREATER_EQUAL,
    CARET, AT,

    // Delimiters
    SEMICOLON, COLON, COMMA, DOT, DOTDOT, LPAREN, RPAREN,
    LBRACKET, RBRACKET,

    // Literals
    INTEGER_LITERAL, REAL_LITERAL, STRING_LITERAL,

    // Identifiers
    IDENTIFIER,

    // Special
    EOF
}

/// <summary>
/// Represents a single token in the Pascal source code.
/// Contains the token type, lexeme value, and position information for error reporting.
/// </summary>
public class Token
{
    /// <summary>
    /// Gets the type of this token.
    /// </summary>
    public TokenType Type { get; }

    /// <summary>
    /// Gets the string value (lexeme) of this token.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the line number where this token appears in the source code (1-based).
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Gets the column number where this token appears in the source code (1-based).
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Initializes a new instance of the Token class.
    /// </summary>
    /// <param name="type">The type of the token.</param>
    /// <param name="value">The string value (lexeme) of the token.</param>
    /// <param name="line">The line number where the token appears.</param>
    /// <param name="column">The column number where the token appears.</param>
    public Token(TokenType type, string value, int line, int column)
    {
        Type = type;
        Value = value;
        Line = line;
        Column = column;
    }

    /// <summary>
    /// Returns a string representation of the token for debugging purposes.
    /// </summary>
    /// <returns>A string in the format "Token(Type, 'Value', Line:Column)".</returns>
    public override string ToString() => $"Token({Type}, '{Value}', {Line}:{Column})";
}
