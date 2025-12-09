program SimpleNestedTest;

procedure Outer;
var
    x: integer;

    procedure Inner;
    begin
        writeln('Inner procedure called');
    end;

begin
    x := 42;
    writeln('Outer: x = ', x);
    Inner();
end;

function OuterFunction: integer;

    function InnerFunction: integer;
    begin
        writeln('Inner function called');
        InnerFunction := 100;
    end;

begin
    writeln('Outer function called');
    OuterFunction := InnerFunction() + 50;
end;

begin
    writeln('Testing nested procedures:');
    Outer();
    writeln;
    writeln('Testing nested functions:');
    writeln('Result: ', OuterFunction());
end.
