program TestWithUnit;

uses MathUtils;

var
    x, result: integer;

begin
    writeln('Testing program with unit support');
    writeln;

    x := 5;
    result := Square(x);
    writeln('Square(', x, ') = ', result);

    result := Cube(x);
    writeln('Cube(', x, ') = ', result);

    writeln;
    PrintSquare(7);

    writeln;
    writeln('Program completed successfully!');
end.
