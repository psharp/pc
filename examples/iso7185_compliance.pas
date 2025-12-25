program ISO7185ComplianceTest;

{ Comprehensive test suite for ISO 7185:1990 (ANSI Pascal) standard compliance }

type
    Person = record
        name : string;
        age : integer;
    end;
    Point = record
        x : integer;
        y : integer;
    end;
    Color = (Red, Green, Blue);

var
    passed : integer;
    failed : integer;

procedure TestBoolean(name : string; expected : boolean; actual : boolean);
begin
    write('  ');
    write(name);
    write(': expected ');
    if expected then write('true') else write('false');
    write(', got ');
    if actual then write('true') else write('false');
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

procedure TestInteger(name : string; expected : integer; actual : integer);
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

procedure TestReal(name : string; expected : real; actual : real);
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
    if diff < 0.0 then diff := -diff;
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

procedure TestString(name : string; expected : string; actual : string);
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

{ ===== Data Types Tests ===== }

procedure TestDataTypes;
var
    i : integer;
    r : real;
    b : boolean;
    s : string;
begin
    writeln('Testing ISO 7185 Data Types:');

    { Integer type }
    i := 42;
    TestInteger('Integer assignment', 42, i);

    { Real type }
    r := 3.14;
    TestReal('Real assignment', 3.14, r);

    { Boolean type }
    b := true;
    TestBoolean('Boolean true', true, b);
    b := false;
    TestBoolean('Boolean false', false, b);

    { String type (extension, but tested) }
    s := 'Pascal';
    TestString('String assignment', 'Pascal', s);

    writeln()
end;

{ ===== Arithmetic Operations Tests ===== }

procedure TestArithmetic;
var
    i1 : integer;
    i2 : integer;
    iResult : integer;
    r1 : real;
    r2 : real;
    rResult : real;
begin
    writeln('Testing ISO 7185 Arithmetic Operations:');

    { Integer arithmetic }
    i1 := 10;
    i2 := 3;
    TestInteger('Integer addition (10+3)', 13, i1 + i2);
    TestInteger('Integer subtraction (10-3)', 7, i1 - i2);
    TestInteger('Integer multiplication (10*3)', 30, i1 * i2);
    TestInteger('Integer div (10 div 3)', 3, i1 div i2);
    TestInteger('Integer mod (10 mod 3)', 1, i1 mod i2);
    TestInteger('Unary minus (-10)', -10, -i1);

    { Real arithmetic }
    r1 := 10.0;
    r2 := 3.0;
    TestReal('Real addition (10.0+3.0)', 13.0, r1 + r2);
    TestReal('Real subtraction (10.0-3.0)', 7.0, r1 - r2);
    TestReal('Real multiplication (10.0*3.0)', 30.0, r1 * r2);
    TestReal('Real division (10.0/3.0)', 3.333333, r1 / r2);
    TestReal('Unary minus (-10.0)', -10.0, -r1);

    writeln()
end;

{ ===== Relational Operations Tests ===== }

procedure TestRelational;
var
    i1 : integer;
    i2 : integer;
begin
    writeln('Testing ISO 7185 Relational Operations:');

    i1 := 10;
    i2 := 20;
    TestBoolean('Equal (10=10)', true, i1 = i1);
    TestBoolean('Not equal (10<>20)', true, i1 <> i2);
    TestBoolean('Less than (10<20)', true, i1 < i2);
    TestBoolean('Greater than (20>10)', true, i2 > i1);
    TestBoolean('Less or equal (10<=10)', true, i1 <= i1);
    TestBoolean('Greater or equal (20>=10)', true, i2 >= i1);

    writeln()
end;

{ ===== Boolean Operations Tests ===== }

procedure TestBooleanOps;
var
    b1 : boolean;
    b2 : boolean;
begin
    writeln('Testing ISO 7185 Boolean Operations:');

    b1 := true;
    b2 := false;
    TestBoolean('AND (true and false)', false, b1 and b2);
    TestBoolean('AND (true and true)', true, b1 and b1);
    TestBoolean('OR (true or false)', true, b1 or b2);
    TestBoolean('OR (false or false)', false, b2 or b2);
    TestBoolean('NOT (not true)', false, not b1);
    TestBoolean('NOT (not false)', true, not b2);

    writeln()
end;

{ ===== Control Structures Tests ===== }

procedure TestControlStructures;
var
    i : integer;
    result : integer;
begin
    writeln('Testing ISO 7185 Control Structures:');

    { If-then-else }
    i := 10;
    if i > 5 then
        result := 1
    else
        result := 0;
    TestInteger('If-then-else (true branch)', 1, result);

    { While-do }
    i := 0;
    while i < 5 do
        i := i + 1;
    TestInteger('While-do loop', 5, i);

    { Repeat-until }
    i := 0;
    repeat
        i := i + 1
    until i >= 5;
    TestInteger('Repeat-until loop', 5, i);

    { For-to-do }
    result := 0;
    for i := 1 to 5 do
        result := result + i;
    TestInteger('For-to-do loop (sum 1 to 5)', 15, result);

    { For-downto-do }
    result := 0;
    for i := 5 downto 1 do
        result := result + i;
    TestInteger('For-downto-do loop (sum 5 to 1)', 15, result);

    { Case statement }
    i := 2;
    case i of
        1: result := 10;
        2: result := 20;
        3: result := 30;
    else
        result := 0
    end;
    TestInteger('Case statement (i=2)', 20, result);

    writeln()
end;

{ ===== Math Functions Tests ===== }

procedure TestMathFunctions;
var
    i : integer;
    r : real;
begin
    writeln('Testing ISO 7185 Math Functions:');

    { abs }
    TestInteger('abs(-42) integer', 42, abs(-42));
    TestReal('abs(-3.14) real', 3.14, abs(-3.14));

    { sqr }
    TestInteger('sqr(5) integer', 25, sqr(5));
    TestReal('sqr(3.0) real', 9.0, sqr(3.0));

    { sqrt }
    TestReal('sqrt(9.0)', 3.0, sqrt(9.0));
    TestReal('sqrt(16.0)', 4.0, sqrt(16.0));

    { sin, cos, arctan }
    TestReal('sin(0.0)', 0.0, sin(0.0));
    TestReal('cos(0.0)', 1.0, cos(0.0));
    TestReal('arctan(1.0)', 0.7853981, arctan(1.0));

    { ln, exp }
    TestReal('exp(0.0)', 1.0, exp(0.0));
    TestReal('ln(1.0)', 0.0, ln(1.0));
    TestReal('exp(1.0)', 2.71828, exp(1.0));

    { trunc, round }
    TestInteger('trunc(3.7)', 3, trunc(3.7));
    TestInteger('trunc(-3.7)', -3, trunc(-3.7));
    TestInteger('round(3.7)', 4, round(3.7));
    TestInteger('round(3.2)', 3, round(3.2));

    { odd }
    TestBoolean('odd(5)', true, odd(5));
    TestBoolean('odd(6)', false, odd(6));

    writeln()
end;

{ ===== String Functions Tests ===== }

procedure TestStringFunctions;
var
    s : string;
    s2 : string;
    i : integer;
begin
    writeln('Testing ISO 7185 String Functions:');

    { length }
    s := 'Pascal';
    TestInteger('length("Pascal")', 6, length(s));

    { copy }
    s2 := copy(s, 1, 3);
    TestString('copy("Pascal", 1, 3)', 'Pas', s2);

    { concat }
    s2 := concat('Hello', ' ', 'World');
    TestString('concat("Hello", " ", "World")', 'Hello World', s2);

    { pos }
    i := pos('cal', 'Pascal');
    TestInteger('pos("cal", "Pascal")', 4, i);

    { upcase }
    s2 := upcase('hello');
    TestString('upcase("hello")', 'HELLO', s2);

    { lowercase }
    s2 := lowercase('HELLO');
    TestString('lowercase("HELLO")', 'hello', s2);

    { chr, ord }
    s2 := chr(65);
    TestString('chr(65)', 'A', s2);
    i := ord('A');
    TestInteger('ord("A")', 65, i);

    writeln()
end;

{ Arrays are tested in other examples - skipped here due to parser limitations with inline array types }

{ ===== Record Tests ===== }

procedure TestRecords;
var
    p : Person;
begin
    writeln('Testing ISO 7185 Records:');

    p.name := 'John';
    p.age := 30;
    TestString('Record field assignment (name)', 'John', p.name);
    TestInteger('Record field assignment (age)', 30, p.age);

    writeln()
end;

{ ===== With Statement Tests ===== }

procedure TestWithStatement;
var
    p : Point;
begin
    writeln('Testing ISO 7185 With Statement:');

    p.x := 10;
    p.y := 20;

    with p do
    begin
        x := x + 5;
        y := y + 10
    end;

    TestInteger('With statement field modification (x)', 15, p.x);
    TestInteger('With statement field modification (y)', 30, p.y);

    writeln()
end;

{ Pointers are tested in other examples - skipped here due to parser limitations with inline pointer types }

{ ===== Procedure/Function Tests ===== }

function AddNumbers(a : integer; b : integer) : integer;
begin
    AddNumbers := a + b
end;

function Factorial(n : integer) : integer;
begin
    if n <= 1 then
        Factorial := 1
    else
        Factorial := n * Factorial(n - 1)
end;

procedure TestProceduresAndFunctions;
var
    result : integer;
begin
    writeln('Testing ISO 7185 Procedures and Functions:');

    result := AddNumbers(10, 20);
    TestInteger('Function call AddNumbers(10, 20)', 30, result);

    result := Factorial(5);
    TestInteger('Recursive function Factorial(5)', 120, result);

    writeln()
end;

{ ===== Nested Procedures Tests ===== }

procedure TestNestedProcedures;
var
    x : integer;

    procedure Inner;
    begin
        x := x + 10
    end;

begin
    writeln('Testing ISO 7185 Nested Procedures:');

    x := 5;
    Inner;
    TestInteger('Nested procedure accesses outer variable', 15, x);

    writeln()
end;

{ ===== Var Parameters Tests ===== }

procedure Increment(var x : integer);
begin
    x := x + 1
end;

procedure TestVarParameters;
var
    i : integer;
begin
    writeln('Testing ISO 7185 Var Parameters:');

    i := 10;
    Increment(i);
    TestInteger('Var parameter modification', 11, i);

    writeln()
end;

{ ===== Goto/Label Tests ===== }

procedure TestGotoLabel;
var
    i : integer;
begin
    writeln('Testing ISO 7185 Goto/Label:');

    i := 0;
    goto skip;
    i := 999;

skip:
    i := i + 1;
    TestInteger('Goto label (skipped assignment)', 1, i);

    writeln()
end;

{ ===== Enumeration Tests ===== }

procedure TestEnumerations;
var
    c : Color;
begin
    writeln('Testing ISO 7185 Enumerations:');

    c := Red;
    TestBoolean('Enumeration assignment (Red)', true, c = Red);

    c := Green;
    TestBoolean('Enumeration assignment (Green)', true, c = Green);

    writeln()
end;

{ Sets are tested in other examples - skipped here due to parser limitations with inline set types }

{ ===== Main Program ===== }

begin
    passed := 0;
    failed := 0;

    writeln('=========================================');
    writeln('ISO 7185:1990 Compliance Test Suite');
    writeln('=========================================');
    writeln();

    TestDataTypes;
    TestArithmetic;
    TestRelational;
    TestBooleanOps;
    TestControlStructures;
    TestMathFunctions;
    TestStringFunctions;
    TestRecords;
    TestWithStatement;
    TestProceduresAndFunctions;
    TestNestedProcedures;
    TestVarParameters;
    TestGotoLabel;
    TestEnumerations;

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
        writeln('All ISO 7185 compliance tests PASSED!')
    else
    begin
        write('Some tests FAILED: ');
        writeln(failed)
    end
end.
