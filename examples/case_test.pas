program CaseStatementTest;

{ Comprehensive test of Pascal case statements }

var
    number, choice : integer;
    grade : string;
    passed, failed : integer;

procedure TestCase(name : string; expected, actual : string);
begin
    write('  ');
    write(name);
    write(': expected "');
    write(expected);
    write('", got "');
    write(actual);
    write('"');
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

function GetDayName(day : integer) : string;
begin
    case day of
        1: GetDayName := 'Monday';
        2: GetDayName := 'Tuesday';
        3: GetDayName := 'Wednesday';
        4: GetDayName := 'Thursday';
        5: GetDayName := 'Friday';
        6: GetDayName := 'Saturday';
        7: GetDayName := 'Sunday';
    else
        GetDayName := 'Invalid'
    end
end;

function GetMonthDays(month : integer) : integer;
begin
    case month of
        1, 3, 5, 7, 8, 10, 12: GetMonthDays := 31;
        4, 6, 9, 11: GetMonthDays := 30;
        2: GetMonthDays := 28;
    else
        GetMonthDays := 0
    end
end;

function GetGrade(score : integer) : string;
begin
    case score of
        90..100: GetGrade := 'A';
        80..89: GetGrade := 'B';
        70..79: GetGrade := 'C';
        60..69: GetGrade := 'D';
        0..59: GetGrade := 'F';
    else
        GetGrade := 'Invalid'
    end
end;

function ClassifyNumber(n : integer) : string;
begin
    case n of
        1..10: ClassifyNumber := 'Small';
        11..50: ClassifyNumber := 'Medium';
        51..100: ClassifyNumber := 'Large';
    else
        ClassifyNumber := 'Out of range'
    end
end;

begin
    passed := 0;
    failed := 0;

    writeln('=========================================');
    writeln('Pascal Case Statement Verification Test');
    writeln('=========================================');
    writeln();

    { ===== Simple Case (Single Values) ===== }
    writeln('Testing Simple Case (Single Values):');
    TestCase('Day 1', 'Monday', GetDayName(1));
    TestCase('Day 2', 'Tuesday', GetDayName(2));
    TestCase('Day 3', 'Wednesday', GetDayName(3));
    TestCase('Day 4', 'Thursday', GetDayName(4));
    TestCase('Day 5', 'Friday', GetDayName(5));
    TestCase('Day 6', 'Saturday', GetDayName(6));
    TestCase('Day 7', 'Sunday', GetDayName(7));
    TestCase('Day 0 (else)', 'Invalid', GetDayName(0));
    TestCase('Day 8 (else)', 'Invalid', GetDayName(8));
    writeln();

    { ===== Case with Multiple Values ===== }
    writeln('Testing Case with Multiple Values:');
    write('  Month 1 (31 days): ');
    if GetMonthDays(1) = 31 then
    begin
        writeln('PASS');
        passed := passed + 1
    end
    else
    begin
        writeln('FAIL');
        failed := failed + 1
    end;

    write('  Month 2 (28 days): ');
    if GetMonthDays(2) = 28 then
    begin
        writeln('PASS');
        passed := passed + 1
    end
    else
    begin
        writeln('FAIL');
        failed := failed + 1
    end;

    write('  Month 4 (30 days): ');
    if GetMonthDays(4) = 30 then
    begin
        writeln('PASS');
        passed := passed + 1
    end
    else
    begin
        writeln('FAIL');
        failed := failed + 1
    end;

    write('  Month 12 (31 days): ');
    if GetMonthDays(12) = 31 then
    begin
        writeln('PASS');
        passed := passed + 1
    end
    else
    begin
        writeln('FAIL');
        failed := failed + 1
    end;

    write('  Month 0 (else = 0): ');
    if GetMonthDays(0) = 0 then
    begin
        writeln('PASS');
        passed := passed + 1
    end
    else
    begin
        writeln('FAIL');
        failed := failed + 1
    end;
    writeln();

    { ===== Case with Ranges ===== }
    writeln('Testing Case with Ranges:');
    TestCase('Score 95 (A)', 'A', GetGrade(95));
    TestCase('Score 90 (A)', 'A', GetGrade(90));
    TestCase('Score 85 (B)', 'B', GetGrade(85));
    TestCase('Score 75 (C)', 'C', GetGrade(75));
    TestCase('Score 65 (D)', 'D', GetGrade(65));
    TestCase('Score 55 (F)', 'F', GetGrade(55));
    TestCase('Score 0 (F)', 'F', GetGrade(0));
    TestCase('Score 100 (A)', 'A', GetGrade(100));
    TestCase('Score 105 (Invalid)', 'Invalid', GetGrade(105));
    TestCase('Score -5 (Invalid)', 'Invalid', GetGrade(-5));
    writeln();

    { ===== Combined Ranges ===== }
    writeln('Testing Combined Ranges:');
    TestCase('Number 1 (Small)', 'Small', ClassifyNumber(1));
    TestCase('Number 10 (Small)', 'Small', ClassifyNumber(10));
    TestCase('Number 15 (Medium)', 'Medium', ClassifyNumber(15));
    TestCase('Number 50 (Medium)', 'Medium', ClassifyNumber(50));
    TestCase('Number 75 (Large)', 'Large', ClassifyNumber(75));
    TestCase('Number 100 (Large)', 'Large', ClassifyNumber(100));
    TestCase('Number 0 (Out of range)', 'Out of range', ClassifyNumber(0));
    TestCase('Number 101 (Out of range)', 'Out of range', ClassifyNumber(101));
    writeln();

    { ===== Inline Case Statements ===== }
    writeln('Testing Inline Case Statements:');

    number := 1;
    case number of
        1: grade := 'one';
        2: grade := 'two';
        3: grade := 'three';
    else
        grade := 'other'
    end;
    TestCase('Inline case 1', 'one', grade);

    number := 2;
    case number of
        1: grade := 'one';
        2: grade := 'two';
        3: grade := 'three';
    else
        grade := 'other'
    end;
    TestCase('Inline case 2', 'two', grade);

    number := 5;
    case number of
        1: grade := 'one';
        2: grade := 'two';
        3: grade := 'three';
    else
        grade := 'other'
    end;
    TestCase('Inline case else', 'other', grade);
    writeln();

    { ===== Case Without Else ===== }
    writeln('Testing Case Without Else:');
    number := 1;
    grade := 'unchanged';
    case number of
        1: grade := 'changed';
        2: grade := 'also changed'
    end;
    TestCase('Case without else (matched)', 'changed', grade);

    number := 3;
    grade := 'unchanged';
    case number of
        1: grade := 'changed';
        2: grade := 'also changed'
    end;
    TestCase('Case without else (no match)', 'unchanged', grade);
    writeln();

    { ===== Case with Begin-End Blocks ===== }
    writeln('Testing Case with Begin-End Blocks:');
    choice := 1;
    grade := 'none';
    number := 0;

    case choice of
        1: begin
            grade := 'option one';
            number := 10
        end;
        2: begin
            grade := 'option two';
            number := 20
        end;
        3: begin
            grade := 'option three';
            number := 30
        end;
    else
        begin
            grade := 'other option';
            number := 99
        end
    end;

    write('  Case with compound (choice=1): ');
    if (grade = 'option one') and (number = 10) then
    begin
        writeln('PASS');
        passed := passed + 1
    end
    else
    begin
        writeln('FAIL');
        failed := failed + 1
    end;

    choice := 5;
    case choice of
        1: begin
            grade := 'option one';
            number := 10
        end;
        2: begin
            grade := 'option two';
            number := 20
        end;
    else
        begin
            grade := 'else block';
            number := 999
        end
    end;

    write('  Case with compound else: ');
    if (grade = 'else block') and (number = 999) then
    begin
        writeln('PASS');
        passed := passed + 1
    end
    else
    begin
        writeln('FAIL');
        failed := failed + 1
    end;
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
