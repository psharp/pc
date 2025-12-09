program NestedProceduresTest;

var
    x, result: integer;

procedure Outer(a: integer);
var
    y: integer;

    procedure Inner(b: integer);
    var
        z: integer;
    begin
        z := b + 10;
        writeln('Inner: b = ', b, ', z = ', z);
        result := a + b + z;
    end;

begin
    y := a * 2;
    writeln('Outer: a = ', a, ', y = ', y);
    Inner(y);
    writeln('Outer: result after Inner = ', result);
end;

function OuterFunc(n: integer): integer;
var
    temp: integer;

    function InnerFunc(m: integer): integer;
    begin
        InnerFunc := m * m;
    end;

begin
    temp := InnerFunc(n);
    writeln('OuterFunc: temp = ', temp);
    OuterFunc := temp + n;
end;

begin
    x := 5;
    writeln('Main: x = ', x);

    Outer(x);
    writeln('Main: result = ', result);

    result := OuterFunc(4);
    writeln('Main: OuterFunc(4) = ', result);
end.
