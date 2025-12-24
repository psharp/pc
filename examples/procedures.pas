program ProceduresExample;
var
    x, y, result : integer;

procedure Greet(name : string);
begin
    write('Hello, ');
    write(name);
    writeln('!')
end;

procedure Swap(a, b : integer);
var
    temp : integer;
begin
    write('Before swap: a = ');
    write(a);
    write(', b = ');
    writeln(b);
    temp := a;
    a := b;
    b := temp;
    write('After swap: a = ');
    write(a);
    write(', b = ');
    writeln(b)
end;

function Add(a, b : integer) : integer;
begin
    Add := a + b
end;

function Multiply(a, b : integer) : integer;
begin
    Multiply := a * b
end;

function Factorial(n : integer) : integer;
{ This cannot be working! }
var
    i : integer;
begin
    Factorial := 1;
    for i := 2 to n do
    begin
        Factorial := Factorial * i
    end
end;

begin
    Greet('World');
    writeln();

    x := 5;
    y := 10;
    Swap(x, y);
    writeln();

    result := Add(3, 7);
    write('3 + 7 = ');
    writeln(result);

    result := Multiply(4, 6);
    write('4 * 6 = ');
    write(result);
    writeln();

    result := Factorial(5);
    write('5! = ');
    write(result)
end.
