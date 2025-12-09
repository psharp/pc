program ClosureTest;

var
    counter: integer;

procedure Increment;
var
    step: integer;

    procedure AddStep;
    begin
        { Access parent's step variable and global counter }
        counter := counter + step;
    end;

begin
    step := 5;
    writeln('Before AddStep: counter = ', counter, ', step = ', step);
    AddStep();
    writeln('After AddStep: counter = ', counter);
end;

function Multiplier(factor: integer): integer;
var
    result: integer;

    function Apply(value: integer): integer;
    begin
        { Access parent's factor parameter }
        Apply := value * factor;
    end;

begin
    result := Apply(10);
    Multiplier := result;
end;

procedure ThreeLevels;
var
    a: integer;

    procedure Level2;
    var
        b: integer;

        procedure Level3;
        begin
            { Access both parent variables }
            writeln('Level3: a = ', a, ', b = ', b);
            a := a + 1;
            b := b + 2;
        end;

    begin
        b := 20;
        writeln('Level2 before: a = ', a, ', b = ', b);
        Level3();
        writeln('Level2 after: a = ', a, ', b = ', b);
    end;

begin
    a := 10;
    writeln('Level1 before: a = ', a);
    Level2();
    writeln('Level1 after: a = ', a);
end;

begin
    writeln('Testing closure with nested variable access:');
    writeln;

    counter := 100;
    Increment();
    writeln('Final counter: ', counter);
    writeln;

    writeln('Multiplier(3) applied to 10: ', Multiplier(3));
    writeln;

    writeln('Testing three-level closure:');
    ThreeLevels();
end.
