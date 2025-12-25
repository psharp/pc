program MathFunctionsTest;

{ Comprehensive test of all standard Pascal math functions }

var
    intVal, intResult : integer;
    realVal, realResult : real;
    passed, failed : integer;

procedure TestInteger(name : string; expected, actual : integer);
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

procedure TestReal(name : string; expected, actual : real);
var
    diff : real;
begin
    write('  ');
    write(name);
    write(': expected ');
    write(expected);
    write(', got ');
    write(actual);

    { Check if values are close enough (within 0.0001) }
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

procedure TestBoolean(name : string; expected, actual : boolean);
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

    writeln('========================================');
    writeln('Pascal Math Functions Verification Test');
    writeln('========================================');
    writeln();

    { ===== ABS Function ===== }
    writeln('Testing ABS (Absolute Value):');
    TestInteger('abs(-5)', 5, abs(-5));
    TestInteger('abs(5)', 5, abs(5));
    TestInteger('abs(0)', 0, abs(0));
    TestReal('abs(-3.14)', 3.14, abs(-3.14));
    TestReal('abs(2.718)', 2.718, abs(2.718));
    writeln();

    { ===== SQR Function ===== }
    writeln('Testing SQR (Square):');
    TestInteger('sqr(5)', 25, sqr(5));
    TestInteger('sqr(-4)', 16, sqr(-4));
    TestInteger('sqr(0)', 0, sqr(0));
    TestReal('sqr(3.0)', 9.0, sqr(3.0));
    TestReal('sqr(2.5)', 6.25, sqr(2.5));
    writeln();

    { ===== SQRT Function ===== }
    writeln('Testing SQRT (Square Root):');
    TestReal('sqrt(25.0)', 5.0, sqrt(25.0));
    TestReal('sqrt(16.0)', 4.0, sqrt(16.0));
    TestReal('sqrt(2.0)', 1.4142, sqrt(2.0));
    TestReal('sqrt(9.0)', 3.0, sqrt(9.0));
    TestReal('sqrt(0.25)', 0.5, sqrt(0.25));
    writeln();

    { ===== SIN Function ===== }
    writeln('Testing SIN (Sine):');
    TestReal('sin(0.0)', 0.0, sin(0.0));
    TestReal('sin(1.5708)', 1.0, sin(1.5708));     { pi/2 }
    TestReal('sin(3.14159)', 0.0, sin(3.14159));   { pi }
    TestReal('sin(0.5236)', 0.5, sin(0.5236));     { pi/6 }
    writeln();

    { ===== COS Function ===== }
    writeln('Testing COS (Cosine):');
    TestReal('cos(0.0)', 1.0, cos(0.0));
    TestReal('cos(1.5708)', 0.0, cos(1.5708));     { pi/2 }
    TestReal('cos(3.14159)', -1.0, cos(3.14159));  { pi }
    TestReal('cos(1.0472)', 0.5, cos(1.0472));     { pi/3 }
    writeln();

    { ===== ARCTAN Function ===== }
    writeln('Testing ARCTAN (Arctangent):');
    TestReal('arctan(0.0)', 0.0, arctan(0.0));
    TestReal('arctan(1.0)', 0.7854, arctan(1.0));  { pi/4 }
    TestReal('arctan(-1.0)', -0.7854, arctan(-1.0));
    writeln();

    { ===== LN Function ===== }
    writeln('Testing LN (Natural Logarithm):');
    TestReal('ln(1.0)', 0.0, ln(1.0));
    TestReal('ln(2.71828)', 1.0, ln(2.71828));     { e }
    TestReal('ln(7.38906)', 2.0, ln(7.38906));     { e^2 }
    TestReal('ln(10.0)', 2.3026, ln(10.0));
    writeln();

    { ===== EXP Function ===== }
    writeln('Testing EXP (Exponential e^x):');
    TestReal('exp(0.0)', 1.0, exp(0.0));
    TestReal('exp(1.0)', 2.71828, exp(1.0));       { e }
    TestReal('exp(2.0)', 7.38906, exp(2.0));       { e^2 }
    TestReal('exp(-1.0)', 0.36788, exp(-1.0));     { 1/e }
    writeln();

    { ===== TRUNC Function ===== }
    writeln('Testing TRUNC (Truncate to Integer):');
    TestInteger('trunc(3.7)', 3, trunc(3.7));
    TestInteger('trunc(3.2)', 3, trunc(3.2));
    TestInteger('trunc(-2.8)', -2, trunc(-2.8));
    TestInteger('trunc(5.0)', 5, trunc(5.0));
    TestInteger('trunc(0.9)', 0, trunc(0.9));
    writeln();

    { ===== ROUND Function ===== }
    writeln('Testing ROUND (Round to Nearest Integer):');
    TestInteger('round(3.7)', 4, round(3.7));
    TestInteger('round(3.2)', 3, round(3.2));
    TestInteger('round(3.5)', 4, round(3.5));
    TestInteger('round(-2.3)', -2, round(-2.3));
    TestInteger('round(-2.8)', -3, round(-2.8));
    writeln();

    { ===== ODD Function ===== }
    writeln('Testing ODD (Check if Odd):');
    TestBoolean('odd(3)', true, odd(3));
    TestBoolean('odd(4)', false, odd(4));
    TestBoolean('odd(0)', false, odd(0));
    TestBoolean('odd(-5)', true, odd(-5));
    TestBoolean('odd(1)', true, odd(1));
    writeln();

    { ===== Combined Operations ===== }
    writeln('Testing Combined Operations:');
    realResult := sqrt(sqr(3.0) + sqr(4.0));
    TestReal('sqrt(3^2 + 4^2)', 5.0, realResult);

    realResult := sin(0.5236) * sin(0.5236) + cos(0.5236) * cos(0.5236);
    TestReal('sin^2 + cos^2', 1.0, realResult);

    intResult := abs(sqr(-7));
    TestInteger('abs((-7)^2)', 49, intResult);

    realResult := exp(ln(5.0));
    TestReal('exp(ln(5))', 5.0, realResult);

    intResult := round(sqrt(64.0));
    TestInteger('round(sqrt(64))', 8, intResult);
    writeln();

    { ===== Test with Variables ===== }
    writeln('Testing with Variables:');
    intVal := -42;
    TestInteger('abs(intVal) where intVal=-42', 42, abs(intVal));

    realVal := 5.5;
    TestReal('sqr(realVal) where realVal=5.5', 30.25, sqr(realVal));

    realVal := 100.0;
    TestReal('sqrt(realVal) where realVal=100.0', 10.0, sqrt(realVal));
    writeln();

    { ===== Edge Cases ===== }
    writeln('Testing Edge Cases:');
    TestInteger('abs(0)', 0, abs(0));
    TestInteger('sqr(0)', 0, sqr(0));
    TestReal('sqrt(0.0)', 0.0, sqrt(0.0));
    TestReal('sin(0.0)', 0.0, sin(0.0));
    TestReal('cos(0.0)', 1.0, cos(0.0));
    TestReal('ln(1.0)', 0.0, ln(1.0));
    TestReal('exp(0.0)', 1.0, exp(0.0));
    TestInteger('trunc(0.0)', 0, trunc(0.0));
    TestInteger('round(0.0)', 0, round(0.0));
    TestBoolean('odd(0)', false, odd(0));
    writeln();

    { ===== Final Results ===== }
    writeln('========================================');
    writeln('Test Results:');
    write('  Passed: ');
    writeln(passed);
    write('  Failed: ');
    writeln(failed);
    write('  Total:  ');
    writeln(passed + failed);
    writeln('========================================');

    if failed = 0 then
        writeln('All tests PASSED!')
    else
    begin
        write('Some tests FAILED: ');
        writeln(failed)
    end
end.
