program RepeatUntilTest;

{ Comprehensive test of Pascal repeat-until loops }

var
    n, sum, factorial, count : integer;
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

begin
    passed := 0;
    failed := 0;

    writeln('=========================================');
    writeln('Pascal Repeat-Until Loop Verification Test');
    writeln('=========================================');
    writeln();

    { ===== Basic Repeat-Until ===== }
    writeln('Testing Basic Repeat-Until:');
    n := 0;
    repeat
        n := n + 1
    until n >= 5;
    TestInteger('Count to 5', 5, n);

    n := 0;
    repeat
        n := n + 1
    until n = 10;
    TestInteger('Count to 10', 10, n);
    writeln();

    { ===== Repeat-Until Always Executes Once ===== }
    writeln('Testing Post-Test Behavior (always executes once):');
    n := 100;
    repeat
        n := n + 1
    until n > 50;
    TestInteger('Loop executes once even when condition is initially true', 101, n);

    n := 0;
    repeat
        n := 99
    until true;
    TestInteger('Single execution with immediate true condition', 99, n);
    writeln();

    { ===== Repeat-Until with Multiple Statements ===== }
    writeln('Testing Repeat-Until with Multiple Statements:');
    n := 1;
    sum := 0;
    repeat
        sum := sum + n;
        n := n + 1
    until n > 5;
    TestInteger('Sum 1+2+3+4+5', 15, sum);

    n := 1;
    sum := 0;
    repeat
        sum := sum + n;
        n := n + 1
    until n > 10;
    TestInteger('Sum 1 to 10', 55, sum);
    writeln();

    { ===== Factorial Using Repeat-Until ===== }
    writeln('Testing Factorial Calculation:');
    n := 5;
    factorial := 1;
    count := 1;
    repeat
        factorial := factorial * count;
        count := count + 1
    until count > n;
    TestInteger('5! (factorial)', 120, factorial);

    n := 6;
    factorial := 1;
    count := 1;
    repeat
        factorial := factorial * count;
        count := count + 1
    until count > n;
    TestInteger('6! (factorial)', 720, factorial);
    writeln();

    { ===== Nested Repeat-Until ===== }
    writeln('Testing Nested Repeat-Until:');
    sum := 0;
    n := 1;
    repeat
        count := 1;
        repeat
            sum := sum + 1;
            count := count + 1
        until count > 3;
        n := n + 1
    until n > 4;
    TestInteger('Nested loop (4 * 3 iterations)', 12, sum);
    writeln();

    { ===== Repeat-Until with Compound Condition ===== }
    writeln('Testing Repeat-Until with Compound Conditions:');
    n := 0;
    sum := 0;
    repeat
        n := n + 1;
        sum := sum + n
    until (n >= 5) and (sum >= 10);
    TestInteger('Stop at n>=5 AND sum>=10', 5, n);
    TestInteger('Sum should be 15', 15, sum);

    n := 0;
    repeat
        n := n + 1
    until (n = 3) or (n = 7);
    TestInteger('Stop at n=3 OR n=7 (stops at 3)', 3, n);
    writeln();

    { ===== Repeat-Until vs While Comparison ===== }
    writeln('Testing Repeat-Until vs While Behavior:');

    { Repeat-until executes at least once }
    n := 0;
    repeat
        n := n + 1
    until 1 > 0;  { Condition is immediately true }
    TestInteger('Repeat executes once (condition already true)', 1, n);

    { While would not execute at all }
    n := 0;
    while 1 < 0 do  { Condition is immediately false }
    begin
        n := n + 1
    end;
    TestInteger('While does not execute (condition false)', 0, n);
    writeln();

    { ===== Countdown Using Repeat-Until ===== }
    writeln('Testing Countdown:');
    n := 10;
    repeat
        n := n - 1
    until n <= 0;
    TestInteger('Countdown from 10 to 0', 0, n);

    n := 5;
    sum := 0;
    repeat
        sum := sum + n;
        n := n - 1
    until n < 1;
    TestInteger('Sum countdown 5+4+3+2+1', 15, sum);
    writeln();

    { ===== Power Calculation ===== }
    writeln('Testing Power Calculation:');
    n := 1;
    count := 0;
    repeat
        n := n * 2;
        count := count + 1
    until count >= 8;
    TestInteger('2^8 = 256', 256, n);
    writeln();

    { ===== Final Results ===== }
    writeln('=========================================');
    writeln('Test Results:');
    write('  Passed: ');
    writeln(passed);
    write('  Failed: ');
    writeln(failed);
    write('  Total:  ');
    writeln(passed + failed);
    writeln('=========================================');

    if failed = 0 then
        writeln('All tests PASSED!')
    else
    begin
        write('Some tests FAILED: ');
        writeln(failed)
    end
end.
