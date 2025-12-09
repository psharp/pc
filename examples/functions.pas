program FunctionsExample;
var
    num, square, cube : integer;
    x, y : real;

function Square(n : integer) : integer;
begin
    Square := n * n
end;

function Cube(n : integer) : integer;
begin
    Cube := n * n * n
end;

function Max(a, b : integer) : integer;
begin
    if a > b then
        Max := a
    else
        Max := b
end;

function IsEven(n : integer) : boolean;
begin
    IsEven := (n mod 2) = 0
end;

procedure PrintSquares(start, finish : integer);
var
    i : integer;
begin
    for i := start to finish do
    begin
        write(i);
        write(' squared is ');
        writeln(Square(i))
    end
end;

begin
    num := 7;
    square := Square(num);
    cube := Cube(num);

    write(num);
    write(' squared is ');
    writeln(square);

    write(num);
    write(' cubed is ');
    writeln(cube);
    writeln();

    writeln('Max of 15 and 23 is:');
    writeln(Max(15, 23));
    writeln();

    write('Is 10 even? ');
    writeln(IsEven(10));

    write('Is 7 even? ');
    writeln(IsEven(7));
    writeln();

    writeln('Squares from 1 to 5:');
    PrintSquares(1, 5)
end.
