program DeeplyNestedTest;

procedure Level1;
var
    x: integer;

    procedure Level2;
    var
        y: integer;

        procedure Level3;
        var
            z: integer;
        begin
            z := 100;
            writeln('Level 3: z = ', z);
        end;

    begin
        y := 50;
        writeln('Level 2: y = ', y);
        Level3();
    end;

begin
    x := 10;
    writeln('Level 1: x = ', x);
    Level2();
end;

function Factorial(n: integer): integer;

    function FactorialHelper(num, acc: integer): integer;
    begin
        if num <= 1 then
            FactorialHelper := acc
        else
            FactorialHelper := FactorialHelper(num - 1, num * acc);
    end;

begin
    Factorial := FactorialHelper(n, 1);
end;

begin
    writeln('Testing deeply nested procedures:');
    Level1();
    writeln;
    writeln('Testing nested recursive function:');
    writeln('Factorial(5) = ', Factorial(5));
end.
