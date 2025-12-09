program VarParameterTest;

var
    x, y: integer;
    a, b: real;

procedure Swap(var p, q: integer);
var
    temp: integer;
begin
    temp := p;
    p := q;
    q := temp;
end;

procedure Increment(var n: integer);
begin
    n := n + 1;
end;

procedure TestValueParam(m: integer);
begin
    m := m + 100;
end;

function AddAndModify(var x: integer; y: integer): integer;
begin
    x := x + 10;
    AddAndModify := x + y;
end;

begin
    x := 5;
    y := 10;

    writeln('Before Swap: x = ', x, ', y = ', y);
    Swap(x, y);
    writeln('After Swap: x = ', x, ', y = ', y);

    writeln('Before Increment: x = ', x);
    Increment(x);
    writeln('After Increment: x = ', x);

    writeln('Before TestValueParam: y = ', y);
    TestValueParam(y);
    writeln('After TestValueParam (should be unchanged): y = ', y);

    x := 20;
    y := 30;
    writeln('Before AddAndModify: x = ', x, ', y = ', y);
    writeln('Result of AddAndModify: ', AddAndModify(x, y));
    writeln('After AddAndModify: x = ', x, ', y = ', y);
end.
