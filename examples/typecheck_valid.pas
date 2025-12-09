program TypeCheckValid;
var
    x, y : integer;
    z : real;
    name : string;
    flag : boolean;
    numbers : array[1..5] of integer;
begin
    { All type-safe operations }

    { Integer assignments }
    x := 10;
    y := x + 5;

    { Real assignments (integer can be assigned to real) }
    z := 3.14;
    z := x;  { implicit conversion }
    z := x + 2.5;  { mixed arithmetic gives real }

    { String operations }
    name := 'John';

    { Boolean operations }
    flag := true;
    flag := (x > 5) and (y < 20);

    { Correct conditions }
    if flag then
        writeln('Flag is true');

    if x > 0 then
        writeln('x is positive');

    while x > 0 do
    begin
        x := x - 1
    end;

    { Correct for loop }
    for x := 1 to 10 do
        writeln(x);

    { Array operations }
    numbers[1] := 42;
    y := numbers[1];

    writeln('Type checking passed!')
end.
