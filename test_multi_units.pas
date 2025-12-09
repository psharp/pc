program TestMultipleUnits;

uses MathUtils, StringUtils;

var
    x: integer;
    msg: string;

begin
    PrintBanner('Testing Multiple Units');
    writeln;

    x := 7;
    writeln('Square(', x, ') = ', Square(x));
    writeln('Cube(', x, ') = ', Cube(x));

    writeln;
    msg := Concat('Hello', ' World');
    writeln('Concatenated: ', msg);

    writeln;
    writeln('Test completed!');
end.
