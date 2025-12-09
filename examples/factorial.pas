program Factorial;
var
    n, i, result : integer;
begin
    writeln('Factorial Calculator');
    writeln('Enter a number: ');
    readln(n);

    result := 1;
    for i := 1 to n do
    begin
        result := result * i
    end;

    write('Factorial of ');
    write(n);
    write(' is ');
    writeln(result)
end.
