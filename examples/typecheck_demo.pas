program TypeCheckingDemo;
type
    Person = record
        name : string;
        age : integer;
    end;
var
    x, y, result : integer;
    price, total : real;
    message : string;
    isValid, done : boolean;
    person : Person;
    numbers : array[1..10] of integer;
    i : integer;
begin
    writeln('=== Type Checking Demo ===');
    writeln();

    { Integer operations }
    writeln('Integer operations:');
    x := 10;
    y := 20;
    result := x + y;
    writeln('10 + 20 = ', result);
    result := y div x;
    writeln('20 div 10 = ', result);
    result := y mod 3;
    writeln('20 mod 3 = ', result);
    writeln();

    { Real operations (with implicit conversion) }
    writeln('Real operations:');
    price := 19.99;
    total := price * 2;
    writeln('19.99 * 2 = ', total);
    { Integer can be assigned to real }
    total := x;
    writeln('Integer 10 assigned to real: ', total);
    writeln();

    { String operations }
    writeln('String operations:');
    message := 'Hello, World!';
    writeln(message);
    writeln();

    { Boolean operations }
    writeln('Boolean operations:');
    isValid := true;
    done := false;
    isValid := (x > 5) and (y < 30);
    writeln('(10 > 5) and (20 < 30) = ', isValid);
    isValid := not done;
    writeln('not false = ', isValid);
    writeln();

    { Type-safe control flow }
    writeln('Control flow:');
    if x > 5 then
        writeln('x is greater than 5');

    while x > 0 do
    begin
        x := x - 1
    end;
    writeln('After while loop, x = ', x);

    for i := 1 to 5 do
    begin
        numbers[i] := i * i;
        write('numbers[', i, '] = ', numbers[i], ' ')
    end;
    writeln();
    writeln();

    { Record operations }
    writeln('Record operations:');
    person.name := 'Alice';
    person.age := 30;
    writeln('Person: ', person.name, ', Age: ', person.age);
    writeln();

    { Comparison operations }
    writeln('Comparison operations:');
    if 10 = 10 then
        writeln('10 = 10: true');
    if 10 <> 5 then
        writeln('10 <> 5: true');
    if 10 > 5 then
        writeln('10 > 5: true');
    if 5 < 10 then
        writeln('5 < 10: true');
    if 10 >= 10 then
        writeln('10 >= 10: true');
    if 5 <= 10 then
        writeln('5 <= 10: true');

    writeln();
    writeln('=== All type checks passed! ===')
end.
