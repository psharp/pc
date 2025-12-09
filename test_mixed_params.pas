program MixedParametersTest;

var
    a, b, c: integer;

procedure MixedParams(var x: integer; y: integer; var z: integer);
begin
    writeln('Inside MixedParams - x = ', x, ', y = ', y, ', z = ', z);
    x := x + 100;
    y := y + 100;
    z := z + 100;
    writeln('After modifications - x = ', x, ', y = ', y, ', z = ', z);
end;

begin
    a := 1;
    b := 2;
    c := 3;

    writeln('Before call: a = ', a, ', b = ', b, ', c = ', c);
    MixedParams(a, b, c);
    writeln('After call: a = ', a, ', b = ', b, ', c = ', c);
    writeln('Expected: a = 101, b = 2 (unchanged), c = 103');
end.
