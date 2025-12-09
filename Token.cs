namespace PascalCompiler;

public enum TokenType
{
    // Keywords
    PROGRAM, VAR, BEGIN, END, INTEGER, REAL, STRING, BOOLEAN,
    IF, THEN, ELSE, WHILE, DO, FOR, TO, DOWNTO,
    PROCEDURE, FUNCTION, CONST, TYPE, ARRAY, OF, RECORD,
    TRUE, FALSE, DIV, MOD, AND, OR, NOT,
    FILE, TEXT, RESET, REWRITE, READ, WRITE, CLOSE, EOF_FUNC, ASSIGN_FILE,
    NIL, NEW, DISPOSE,
    SET, IN,
    UNIT, INTERFACE, IMPLEMENTATION, USES, INITIALIZATION, FINALIZATION,

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

public class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public int Line { get; }
    public int Column { get; }

    public Token(TokenType type, string value, int line, int column)
    {
        Type = type;
        Value = value;
        Line = line;
        Column = column;
    }

    public override string ToString() => $"Token({Type}, '{Value}', {Line}:{Column})";
}
