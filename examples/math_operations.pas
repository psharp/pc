program MathOperationsTest;

{ Test program for basic mathematical operations }
{ Note: Standard math functions (abs, sqrt, sin, etc.) not yet implemented }

var
    a, b, c : integer;
    x, y, z : real;
    result : boolean;
    passed, failed : integer;

procedure TestIntegerOp(name : string; expected, actual : integer);
begin
    write('  ');
    write(name);
    write(': expected ');
    write(expected);
    write(', got ');
    write(actual);
    if expected = actual then
    begin
        writeln(' - PASS');
        passed := passed + 1
    end
    else
    begin
        writeln(' - FAIL');
        failed := failed + 1
    end
end;

procedure TestRealOp(name : string; expected, actual : real);
var
    diff : real;
begin
    write('  ');
    write(name);
    write(': expected ');
    write(expected);
    write(', got ');
    write(actual);

    diff := expected - actual;
    if diff < 0.0 then
        diff := 0.0 - diff;

    if diff < 0.0001 then
    begin
        writeln(' - PASS');
        passed := passed + 1
    end
    else
    begin
        writeln(' - FAIL');
        failed := failed + 1
    end
end;

procedure TestBooleanOp(name : string; expected, actual : boolean);
begin
    write('  ');
    write(name);
    write(': expected ');
    write(expected);
    write(', got ');
    write(actual);
    if expected = actual then
    begin
        writeln(' - PASS');
        passed := passed + 1
    end
    else
    begin
        writeln(' - FAIL');
        failed := failed + 1
    end
end;

begin
    passed := 0;
    failed := 0;

    writeln('=======================================');
    writeln('Pascal Basic Math Operations Test');
    writeln('=======================================');
    writeln();

    { ===== Integer Addition ===== }
    writeln('Testing Integer Addition:');
    TestIntegerOp('5 + 3', 8, 5 + 3);
    TestIntegerOp('10 + 20', 30, 10 + 20);
    TestIntegerOp('-5 + 3', -2, -5 + 3);
    TestIntegerOp('0 + 0', 0, 0 + 0);
    writeln();

    { ===== Integer Subtraction ===== }
    writeln('Testing Integer Subtraction:');
    TestIntegerOp('10 - 3', 7, 10 - 3);
    TestIntegerOp('5 - 10', -5, 5 - 10);
    TestIntegerOp('0 - 5', -5, 0 - 5);
    TestIntegerOp('100 - 100', 0, 100 - 100);
    writeln();

    { ===== Integer Multiplication ===== }
    writeln('Testing Integer Multiplication:');
    TestIntegerOp('5 * 3', 15, 5 * 3);
    TestIntegerOp('7 * 8', 56, 7 * 8);
    TestIntegerOp('-4 * 3', -12, -4 * 3);
    TestIntegerOp('0 * 100', 0, 0 * 100);
    writeln();

    { ===== Integer Division (div) ===== }
    writeln('Testing Integer Division (div):');
    TestIntegerOp('10 div 2', 5, 10 div 2);
    TestIntegerOp('15 div 4', 3, 15 div 4);
    TestIntegerOp('7 div 3', 2, 7 div 3);
    TestIntegerOp('100 div 10', 10, 100 div 10);
    writeln();

    { ===== Integer Modulo (mod) ===== }
    writeln('Testing Integer Modulo (mod):');
    TestIntegerOp('10 mod 3', 1, 10 mod 3);
    TestIntegerOp('15 mod 4', 3, 15 mod 4);
    TestIntegerOp('7 mod 7', 0, 7 mod 7);
    TestIntegerOp('20 mod 6', 2, 20 mod 6);
    writeln();

    { ===== Real Addition ===== }
    writeln('Testing Real Addition:');
    TestRealOp('3.5 + 2.5', 6.0, 3.5 + 2.5);
    TestRealOp('1.25 + 2.75', 4.0, 1.25 + 2.75);
    TestRealOp('-1.5 + 3.5', 2.0, -1.5 + 3.5);
    TestRealOp('0.0 + 0.0', 0.0, 0.0 + 0.0);
    writeln();

    { ===== Real Subtraction ===== }
    writeln('Testing Real Subtraction:');
    TestRealOp('5.5 - 2.5', 3.0, 5.5 - 2.5);
    TestRealOp('10.0 - 3.5', 6.5, 10.0 - 3.5);
    TestRealOp('2.5 - 5.5', -3.0, 2.5 - 5.5);
    TestRealOp('7.5 - 7.5', 0.0, 7.5 - 7.5);
    writeln();

    { ===== Real Multiplication ===== }
    writeln('Testing Real Multiplication:');
    TestRealOp('2.5 * 4.0', 10.0, 2.5 * 4.0);
    TestRealOp('3.5 * 2.0', 7.0, 3.5 * 2.0);
    TestRealOp('-2.5 * 3.0', -7.5, -2.5 * 3.0);
    TestRealOp('0.0 * 100.0', 0.0, 0.0 * 100.0);
    writeln();

    { ===== Real Division ===== }
    writeln('Testing Real Division:');
    TestRealOp('10.0 / 2.0', 5.0, 10.0 / 2.0);
    TestRealOp('15.0 / 4.0', 3.75, 15.0 / 4.0);
    TestRealOp('7.5 / 2.5', 3.0, 7.5 / 2.5);
    TestRealOp('1.0 / 8.0', 0.125, 1.0 / 8.0);
    writeln();

    { ===== Mixed Integer and Real Operations ===== }
    writeln('Testing Mixed Integer/Real Operations:');
    TestRealOp('5 + 2.5', 7.5, 5 + 2.5);
    TestRealOp('10.5 - 3', 7.5, 10.5 - 3);
    TestRealOp('4 * 2.5', 10.0, 4 * 2.5);
    TestRealOp('15.0 / 4', 3.75, 15.0 / 4);
    writeln();

    { ===== Comparison Operations ===== }
    writeln('Testing Comparison Operations:');
    TestBooleanOp('5 = 5', true, 5 = 5);
    TestBooleanOp('5 = 6', false, 5 = 6);
    TestBooleanOp('3 < 5', true, 3 < 5);
    TestBooleanOp('5 < 3', false, 5 < 3);
    TestBooleanOp('5 > 3', true, 5 > 3);
    TestBooleanOp('3 > 5', false, 3 > 5);
    TestBooleanOp('5 <= 5', true, 5 <= 5);
    TestBooleanOp('5 >= 5', true, 5 >= 5);
    TestBooleanOp('5 <> 6', true, 5 <> 6);
    TestBooleanOp('5 <> 5', false, 5 <> 5);
    writeln();

    { ===== Unary Minus ===== }
    writeln('Testing Unary Minus:');
    a := 5;
    TestIntegerOp('-5', -5, -a);
    a := -10;
    TestIntegerOp('-(-10)', 10, -a);
    x := 3.5;
    TestRealOp('-3.5', -3.5, -x);
    writeln();

    { ===== Complex Expressions ===== }
    writeln('Testing Complex Expressions:');
    TestIntegerOp('2 + 3 * 4', 14, 2 + 3 * 4);
    TestIntegerOp('(2 + 3) * 4', 20, (2 + 3) * 4);
    TestIntegerOp('10 - 2 * 3', 4, 10 - 2 * 3);
    TestRealOp('(5.0 + 3.0) / 2.0', 4.0, (5.0 + 3.0) / 2.0);
    TestRealOp('2.0 * 3.0 + 4.0 * 5.0', 26.0, 2.0 * 3.0 + 4.0 * 5.0);
    writeln();

    { ===== Variables in Operations ===== }
    writeln('Testing Variables in Operations:');
    a := 10;
    b := 3;
    TestIntegerOp('a + b (10 + 3)', 13, a + b);
    TestIntegerOp('a - b (10 - 3)', 7, a - b);
    TestIntegerOp('a * b (10 * 3)', 30, a * b);
    TestIntegerOp('a div b (10 div 3)', 3, a div b);
    TestIntegerOp('a mod b (10 mod 3)', 1, a mod b);

    x := 6.5;
    y := 2.0;
    TestRealOp('x + y (6.5 + 2.0)', 8.5, x + y);
    TestRealOp('x - y (6.5 - 2.0)', 4.5, x - y);
    TestRealOp('x * y (6.5 * 2.0)', 13.0, x * y);
    TestRealOp('x / y (6.5 / 2.0)', 3.25, x / y);
    writeln();

    { ===== Boolean Operations ===== }
    writeln('Testing Boolean Operations:');
    TestBooleanOp('true and true', true, true and true);
    TestBooleanOp('true and false', false, true and false);
    TestBooleanOp('false and false', false, false and false);
    TestBooleanOp('true or false', true, true or false);
    TestBooleanOp('false or false', false, false or false);
    TestBooleanOp('not true', false, not true);
    TestBooleanOp('not false', true, not false);
    writeln();

    { ===== Final Results ===== }
    writeln('=======================================');
    writeln('Test Results:');
    write('  Passed: ');
    writeln(passed);
    write('  Failed: ');
    writeln(failed);
    write('  Total:  ');
    writeln(passed + failed);
    writeln('=======================================');

    if failed = 0 then
        writeln('All tests PASSED!')
    else
    begin
        write('Some tests FAILED: ');
        writeln(failed)
    end
end.
