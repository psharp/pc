program StringFunctionsTest;

{ Comprehensive test of all standard Pascal string functions }

var
    strVal, strResult : string;
    intVal, intResult : integer;
    passed, failed : integer;

procedure TestString(name : string; expected, actual : string);
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
    writeln('Pascal String Functions Verification Test');
    writeln('=========================================');
    writeln();

    { ===== LENGTH Function ===== }
    writeln('Testing LENGTH (String Length):');
    TestInteger('length("Hello")', 5, length('Hello'));
    TestInteger('length("Pascal")', 6, length('Pascal'));
    TestInteger('length("")', 0, length(''));
    TestInteger('length("A")', 1, length('A'));
    TestInteger('length("Hello World!")', 12, length('Hello World!'));
    writeln();

    { ===== COPY Function ===== }
    writeln('Testing COPY (Copy Substring):');
    TestString('copy("Hello", 1, 3)', 'Hel', copy('Hello', 1, 3));
    TestString('copy("Pascal", 1, 3)', 'Pas', copy('Pascal', 1, 3));
    TestString('copy("Programming", 4, 4)', 'gram', copy('Programming', 4, 4));
    TestString('copy("Test", 2, 2)', 'es', copy('Test', 2, 2));
    TestString('copy("Hello World", 7, 5)', 'World', copy('Hello World', 7, 5));
    TestString('copy("ABC", 1, 1)', 'A', copy('ABC', 1, 1));
    TestString('copy("Full", 1, 4)', 'Full', copy('Full', 1, 4));
    writeln();

    { ===== CONCAT Function ===== }
    writeln('Testing CONCAT (Concatenate Strings):');
    TestString('concat("Hello", " ", "World")', 'Hello World', concat('Hello', ' ', 'World'));
    TestString('concat("Pas", "cal")', 'Pascal', concat('Pas', 'cal'));
    TestString('concat("A", "B", "C")', 'ABC', concat('A', 'B', 'C'));
    TestString('concat("One", "Two")', 'OneTwo', concat('One', 'Two'));
    TestString('concat("", "Test")', 'Test', concat('', 'Test'));
    TestString('concat("Test", "")', 'Test', concat('Test', ''));
    TestString('concat("1", "2", "3", "4")', '1234', concat('1', '2', '3', '4'));
    writeln();

    { ===== POS Function ===== }
    writeln('Testing POS (Find Substring Position):');
    TestInteger('pos("ll", "Hello")', 3, pos('ll', 'Hello'));
    TestInteger('pos("Pas", "Pascal")', 1, pos('Pas', 'Pascal'));
    TestInteger('pos("cal", "Pascal")', 4, pos('cal', 'Pascal'));
    TestInteger('pos("o", "Hello")', 5, pos('o', 'Hello'));
    TestInteger('pos("xyz", "Hello")', 0, pos('xyz', 'Hello'));
    TestInteger('pos("", "Hello")', 1, pos('', 'Hello'));
    TestInteger('pos("World", "Hello World")', 7, pos('World', 'Hello World'));
    TestInteger('pos("A", "ABC")', 1, pos('A', 'ABC'));
    writeln();

    { ===== UPCASE Function ===== }
    writeln('Testing UPCASE (Convert to Uppercase):');
    TestString('upcase("hello")', 'HELLO', upcase('hello'));
    TestString('upcase("Pascal")', 'PASCAL', upcase('Pascal'));
    TestString('upcase("abc123")', 'ABC123', upcase('abc123'));
    TestString('upcase("Test")', 'TEST', upcase('Test'));
    TestString('upcase("ALREADY")', 'ALREADY', upcase('ALREADY'));
    TestString('upcase("")', '', upcase(''));
    TestString('upcase("a")', 'A', upcase('a'));
    writeln();

    { ===== LOWERCASE Function ===== }
    writeln('Testing LOWERCASE (Convert to Lowercase):');
    TestString('lowercase("HELLO")', 'hello', lowercase('HELLO'));
    TestString('lowercase("Pascal")', 'pascal', lowercase('Pascal'));
    TestString('lowercase("ABC123")', 'abc123', lowercase('ABC123'));
    TestString('lowercase("Test")', 'test', lowercase('Test'));
    TestString('lowercase("already")', 'already', lowercase('already'));
    TestString('lowercase("")', '', lowercase(''));
    TestString('lowercase("A")', 'a', lowercase('A'));
    writeln();

    { ===== CHR Function ===== }
    writeln('Testing CHR (Convert Integer to Character):');
    TestString('chr(65)', 'A', chr(65));
    TestString('chr(97)', 'a', chr(97));
    TestString('chr(48)', '0', chr(48));
    TestString('chr(32)', ' ', chr(32));
    TestString('chr(90)', 'Z', chr(90));
    TestString('chr(122)', 'z', chr(122));
    writeln();

    { ===== ORD Function ===== }
    writeln('Testing ORD (Convert Character to Integer):');
    TestInteger('ord("A")', 65, ord('A'));
    TestInteger('ord("a")', 97, ord('a'));
    TestInteger('ord("0")', 48, ord('0'));
    TestInteger('ord(" ")', 32, ord(' '));
    TestInteger('ord("Z")', 90, ord('Z'));
    TestInteger('ord("z")', 122, ord('z'));
    writeln();

    { ===== Combined Operations ===== }
    writeln('Testing Combined Operations:');
    strResult := concat(upcase('hello'), ' ', upcase('world'));
    TestString('concat(upcase("hello"), " ", upcase("world"))', 'HELLO WORLD', strResult);

    strResult := lowercase(concat('HEL', 'LO'));
    TestString('lowercase(concat("HEL", "LO"))', 'hello', strResult);

    intResult := length(concat('Pas', 'cal'));
    TestInteger('length(concat("Pas", "cal"))', 6, intResult);

    intResult := pos('cal', concat('Pas', 'cal'));
    TestInteger('pos("cal", concat("Pas", "cal"))', 4, intResult);

    strResult := copy(upcase('hello'), 2, 3);
    TestString('copy(upcase("hello"), 2, 3)', 'ELL', strResult);

    strResult := chr(ord('A'));
    TestString('chr(ord("A"))', 'A', strResult);

    intResult := ord(chr(88));
    TestInteger('ord(chr(88))', 88, intResult);

    strResult := concat(chr(72), chr(105));
    TestString('concat(chr(72), chr(105))', 'Hi', strResult);
    writeln();

    { ===== Test with Variables ===== }
    writeln('Testing with Variables:');
    strVal := 'Pascal';
    TestInteger('length(strVal) where strVal="Pascal"', 6, length(strVal));

    strVal := 'Hello World';
    TestString('copy(strVal, 1, 5) where strVal="Hello World"', 'Hello', copy(strVal, 1, 5));

    strVal := 'test';
    TestString('upcase(strVal) where strVal="test"', 'TEST', upcase(strVal));

    strVal := 'TEST';
    TestString('lowercase(strVal) where strVal="TEST"', 'test', lowercase(strVal));

    intVal := 65;
    TestString('chr(intVal) where intVal=65', 'A', chr(intVal));
    writeln();

    { ===== Edge Cases ===== }
    writeln('Testing Edge Cases:');
    TestInteger('length("")', 0, length(''));
    TestString('copy("A", 1, 1)', 'A', copy('A', 1, 1));
    TestString('concat("", "")', '', concat('', ''));
    TestInteger('pos("", "Test")', 1, pos('', 'Test'));
    TestString('upcase("")', '', upcase(''));
    TestString('lowercase("")', '', lowercase(''));

    { Test copy with out of bounds - should return empty or partial }
    TestString('copy("Hi", 5, 2)', '', copy('Hi', 5, 2));
    TestString('copy("Test", 3, 10)', 'st', copy('Test', 3, 10));
    writeln();

    { ===== Practical Examples ===== }
    writeln('Testing Practical Examples:');

    { Extract file extension }
    strVal := 'program.pas';
    strResult := copy(strVal, pos('.', strVal) + 1, length(strVal));
    TestString('Extract extension from "program.pas"', 'pas', strResult);

    { Build full name }
    strResult := concat('Mr. ', 'John', ' ', 'Doe');
    TestString('Build full name', 'Mr. John Doe', strResult);

    { Convert case }
    strVal := 'PaScAl';
    strResult := upcase(strVal);
    TestString('Normalize to uppercase', 'PASCAL', strResult);

    { Check if string contains substring }
    intResult := pos('gram', 'Programming');
    TestInteger('Find "gram" in "Programming"', 4, intResult);

    { ASCII value manipulation }
    intResult := ord('B') - ord('A');
    TestInteger('ord("B") - ord("A")', 1, intResult);
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
