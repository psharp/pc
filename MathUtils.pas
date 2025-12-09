unit MathUtils;

interface

function Square(x: integer): integer;
function Cube(x: integer): integer;
procedure PrintSquare(x: integer);

implementation

function Square(x: integer): integer;
begin
    Square := x * x;
end;

function Cube(x: integer): integer;
begin
    Cube := x * x * x;
end;

procedure PrintSquare(x: integer);
begin
    writeln('Square of ', x, ' is ', Square(x));
end;

end.
