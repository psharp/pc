using System;
using System.Collections.Generic;
using System.Linq;

namespace PascalCompiler;

public class PascalDebugger
{
    private readonly Dictionary<string, object?> _variables = new();
    private bool _stepMode = true;
    private int _currentLine = 0;
    private readonly HashSet<int> _breakpoints = new();

    public void Execute(ProgramNode program)
    {
        Console.WriteLine("\n=== Pascal Debugger ===");
        Console.WriteLine("Commands:");
        Console.WriteLine("  [Enter]    - Step to next statement");
        Console.WriteLine("  c          - Continue until next breakpoint");
        Console.WriteLine("  v          - View all variables");
        Console.WriteLine("  v <name>   - View specific variable");
        Console.WriteLine("  b <line>   - Set breakpoint at line");
        Console.WriteLine("  bl         - List breakpoints");
        Console.WriteLine("  bc         - Clear all breakpoints");
        Console.WriteLine("  q          - Quit debugging");
        Console.WriteLine();

        foreach (var varDecl in program.Variables)
        {
            foreach (var name in varDecl.Names)
            {
                _variables[name.ToLower()] = GetDefaultValue(varDecl.Type);
            }
        }

        Console.WriteLine($"Program: {program.Name}");
        Console.WriteLine($"Variables initialized: {string.Join(", ", _variables.Keys)}");
        Console.WriteLine();

        try
        {
            ExecuteBlock(program.Block);
            Console.WriteLine("\n=== Program completed successfully ===");
        }
        catch (QuitDebuggingException)
        {
            Console.WriteLine("\n=== Debugging terminated by user ===");
        }
    }

    private object? GetDefaultValue(string type)
    {
        return type.ToLower() switch
        {
            "integer" => 0,
            "real" => 0.0,
            "string" => "",
            "boolean" => false,
            _ => null
        };
    }

    private void ExecuteBlock(BlockNode block)
    {
        foreach (var statement in block.Statements)
        {
            ExecuteStatement(statement);
        }
    }

    private void WaitForDebugCommand(string currentStatement)
    {
        _currentLine++;

        if (_breakpoints.Contains(_currentLine))
        {
            Console.WriteLine($"\n*** Breakpoint hit at line {_currentLine} ***");
            _stepMode = true;
        }

        if (!_stepMode)
        {
            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n[Line {_currentLine}] {currentStatement}");
        Console.ResetColor();

        while (true)
        {
            Console.Write("debug> ");
            string? input = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrEmpty(input))
            {
                // Step to next statement
                return;
            }

            string[] parts = input.Split(' ', 2);
            string command = parts[0].ToLower();

            switch (command)
            {
                case "c":
                case "continue":
                    _stepMode = false;
                    Console.WriteLine("Continuing...");
                    return;

                case "v":
                case "vars":
                    if (parts.Length > 1)
                    {
                        ViewVariable(parts[1]);
                    }
                    else
                    {
                        ViewAllVariables();
                    }
                    break;

                case "b":
                case "break":
                    if (parts.Length > 1 && int.TryParse(parts[1], out int line))
                    {
                        _breakpoints.Add(line);
                        Console.WriteLine($"Breakpoint set at line {line}");
                    }
                    else
                    {
                        Console.WriteLine("Usage: b <line_number>");
                    }
                    break;

                case "bl":
                case "breaklist":
                    if (_breakpoints.Count == 0)
                    {
                        Console.WriteLine("No breakpoints set");
                    }
                    else
                    {
                        Console.WriteLine("Breakpoints: " + string.Join(", ", _breakpoints.OrderBy(x => x)));
                    }
                    break;

                case "bc":
                case "breakclear":
                    _breakpoints.Clear();
                    Console.WriteLine("All breakpoints cleared");
                    break;

                case "q":
                case "quit":
                    throw new QuitDebuggingException();

                case "h":
                case "help":
                    Console.WriteLine("Commands:");
                    Console.WriteLine("  [Enter]    - Step to next statement");
                    Console.WriteLine("  c          - Continue until next breakpoint");
                    Console.WriteLine("  v          - View all variables");
                    Console.WriteLine("  v <name>   - View specific variable");
                    Console.WriteLine("  b <line>   - Set breakpoint at line");
                    Console.WriteLine("  bl         - List breakpoints");
                    Console.WriteLine("  bc         - Clear all breakpoints");
                    Console.WriteLine("  q          - Quit debugging");
                    break;

                default:
                    Console.WriteLine($"Unknown command: {command}. Type 'h' for help.");
                    break;
            }
        }
    }

    private void ViewAllVariables()
    {
        Console.WriteLine("\nVariables:");
        foreach (var kvp in _variables.OrderBy(x => x.Key))
        {
            Console.WriteLine($"  {kvp.Key} = {FormatValue(kvp.Value)}");
        }
    }

    private void ViewVariable(string name)
    {
        string varName = name.ToLower();
        if (_variables.TryGetValue(varName, out object? value))
        {
            Console.WriteLine($"{varName} = {FormatValue(value)}");
        }
        else
        {
            Console.WriteLine($"Variable '{name}' not found");
        }
    }

    private string FormatValue(object? value)
    {
        if (value == null) return "null";
        if (value is string s) return $"\"{s}\"";
        return value.ToString() ?? "null";
    }

    private void ExecuteStatement(StatementNode statement)
    {
        string description = GetStatementDescription(statement);
        WaitForDebugCommand(description);

        switch (statement)
        {
            case AssignmentNode assignment:
                object? value = EvaluateExpression(assignment.Expression);
                _variables[assignment.Variable.ToLower()] = value;
                Console.WriteLine($"  => {assignment.Variable} = {FormatValue(value)}");
                break;

            case IfNode ifNode:
                object? condition = EvaluateExpression(ifNode.Condition);
                bool conditionResult = IsTrue(condition);
                Console.WriteLine($"  => Condition evaluated to: {conditionResult}");

                if (conditionResult)
                {
                    ExecuteStatement(ifNode.ThenBranch);
                }
                else if (ifNode.ElseBranch != null)
                {
                    ExecuteStatement(ifNode.ElseBranch);
                }
                break;

            case WhileNode whileNode:
                while (IsTrue(EvaluateExpression(whileNode.Condition)))
                {
                    ExecuteStatement(whileNode.Body);
                }
                break;

            case ForNode forNode:
                int start = Convert.ToInt32(EvaluateExpression(forNode.Start));
                int end = Convert.ToInt32(EvaluateExpression(forNode.End));
                string varName = forNode.Variable.ToLower();

                Console.WriteLine($"  => Loop from {start} to {end}");

                if (forNode.IsDownTo)
                {
                    for (int i = start; i >= end; i--)
                    {
                        _variables[varName] = i;
                        ExecuteStatement(forNode.Body);
                    }
                }
                else
                {
                    for (int i = start; i <= end; i++)
                    {
                        _variables[varName] = i;
                        ExecuteStatement(forNode.Body);
                    }
                }
                break;

            case WriteNode writeNode:
                foreach (var expr in writeNode.Expressions)
                {
                    object? val = EvaluateExpression(expr);
                    Console.Write(val);
                }
                if (writeNode.NewLine)
                {
                    Console.WriteLine();
                }
                break;

            case ReadNode readNode:
                foreach (var variable in readNode.Variables)
                {
                    Console.Write($"Enter value for {variable}: ");
                    string? input = Console.ReadLine();
                    string readVarName = variable.ToLower();

                    if (_variables.ContainsKey(readVarName))
                    {
                        object? defaultVal = _variables[readVarName];
                        _variables[readVarName] = ParseInput(input, defaultVal);
                    }
                }
                break;

            case CompoundStatementNode compound:
                foreach (var stmt in compound.Statements)
                {
                    ExecuteStatement(stmt);
                }
                break;
        }
    }

    private string GetStatementDescription(StatementNode statement)
    {
        return statement switch
        {
            AssignmentNode a => $"{a.Variable} := {GetExpressionString(a.Expression)}",
            IfNode i => $"if {GetExpressionString(i.Condition)} then ...",
            WhileNode w => $"while {GetExpressionString(w.Condition)} do ...",
            ForNode f => $"for {f.Variable} := {GetExpressionString(f.Start)} to {GetExpressionString(f.End)} do ...",
            WriteNode w => $"write({string.Join(", ", w.Expressions.Select(GetExpressionString))})",
            ReadNode r => $"read({string.Join(", ", r.Variables)})",
            CompoundStatementNode => "begin...end",
            _ => statement.GetType().Name
        };
    }

    private string GetExpressionString(ExpressionNode expr)
    {
        return expr switch
        {
            NumberNode n => n.Value.ToString() ?? "",
            StringNode s => $"'{s.Value}'",
            BooleanNode b => b.Value.ToString().ToLower(),
            VariableNode v => v.Name,
            BinaryOpNode b => $"({GetExpressionString(b.Left)} {GetOperatorString(b.Operator)} {GetExpressionString(b.Right)})",
            UnaryOpNode u => $"{GetOperatorString(u.Operator)}({GetExpressionString(u.Operand)})",
            _ => "<?>"
        };
    }

    private string GetOperatorString(TokenType op)
    {
        return op switch
        {
            TokenType.PLUS => "+",
            TokenType.MINUS => "-",
            TokenType.MULTIPLY => "*",
            TokenType.DIVIDE => "/",
            TokenType.DIV => "div",
            TokenType.MOD => "mod",
            TokenType.EQUALS => "=",
            TokenType.NOT_EQUALS => "<>",
            TokenType.LESS_THAN => "<",
            TokenType.GREATER_THAN => ">",
            TokenType.LESS_EQUAL => "<=",
            TokenType.GREATER_EQUAL => ">=",
            TokenType.AND => "and",
            TokenType.OR => "or",
            TokenType.NOT => "not",
            _ => op.ToString()
        };
    }

    private object? ParseInput(string? input, object? defaultValue)
    {
        if (string.IsNullOrEmpty(input))
        {
            return defaultValue;
        }

        return defaultValue switch
        {
            int => int.TryParse(input, out int i) ? i : defaultValue,
            double => double.TryParse(input, out double d) ? d : defaultValue,
            bool => bool.TryParse(input, out bool b) ? b : defaultValue,
            _ => input
        };
    }

    private bool IsTrue(object? value)
    {
        if (value is bool b) return b;
        if (value is int i) return i != 0;
        if (value is double d) return d != 0.0;
        return value != null;
    }

    private object? EvaluateExpression(ExpressionNode expression)
    {
        switch (expression)
        {
            case NumberNode number:
                return number.Value;

            case StringNode str:
                return str.Value;

            case BooleanNode boolean:
                return boolean.Value;

            case VariableNode variable:
                string varName = variable.Name.ToLower();
                if (_variables.TryGetValue(varName, out object? value))
                {
                    return value;
                }
                throw new Exception($"Variable '{variable.Name}' not found");

            case BinaryOpNode binary:
                object? left = EvaluateExpression(binary.Left);
                object? right = EvaluateExpression(binary.Right);
                return EvaluateBinaryOp(left, binary.Operator, right);

            case UnaryOpNode unary:
                object? operand = EvaluateExpression(unary.Operand);
                return EvaluateUnaryOp(unary.Operator, operand);

            default:
                throw new Exception($"Unknown expression type: {expression.GetType()}");
        }
    }

    private object? EvaluateBinaryOp(object? left, TokenType op, object? right)
    {
        switch (op)
        {
            case TokenType.PLUS:
                if (left is string || right is string)
                    return left?.ToString() + right?.ToString();
                return ToDouble(left) + ToDouble(right);

            case TokenType.MINUS:
                return ToDouble(left) - ToDouble(right);

            case TokenType.MULTIPLY:
                return ToDouble(left) * ToDouble(right);

            case TokenType.DIVIDE:
                return ToDouble(left) / ToDouble(right);

            case TokenType.DIV:
                return (int)(ToDouble(left) / ToDouble(right));

            case TokenType.MOD:
                return ToDouble(left) % ToDouble(right);

            case TokenType.EQUALS:
                return AreEqual(left, right);

            case TokenType.NOT_EQUALS:
                return !AreEqual(left, right);

            case TokenType.LESS_THAN:
                return ToDouble(left) < ToDouble(right);

            case TokenType.GREATER_THAN:
                return ToDouble(left) > ToDouble(right);

            case TokenType.LESS_EQUAL:
                return ToDouble(left) <= ToDouble(right);

            case TokenType.GREATER_EQUAL:
                return ToDouble(left) >= ToDouble(right);

            case TokenType.AND:
                return IsTrue(left) && IsTrue(right);

            case TokenType.OR:
                return IsTrue(left) || IsTrue(right);

            default:
                throw new Exception($"Unknown binary operator: {op}");
        }
    }

    private object? EvaluateUnaryOp(TokenType op, object? operand)
    {
        switch (op)
        {
            case TokenType.PLUS:
                return ToDouble(operand);

            case TokenType.MINUS:
                return -ToDouble(operand);

            case TokenType.NOT:
                return !IsTrue(operand);

            default:
                throw new Exception($"Unknown unary operator: {op}");
        }
    }

    private double ToDouble(object? value)
    {
        if (value == null) return 0;
        if (value is int i) return i;
        if (value is double d) return d;
        if (value is bool b) return b ? 1 : 0;
        if (double.TryParse(value.ToString(), out double result))
            return result;
        return 0;
    }

    private bool AreEqual(object? left, object? right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;

        if (left is string || right is string)
            return left.ToString() == right.ToString();

        return Math.Abs(ToDouble(left) - ToDouble(right)) < 0.0001;
    }
}

public class QuitDebuggingException : Exception
{
}
