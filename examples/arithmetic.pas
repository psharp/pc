program ArithmeticOperations;
var
    a, b, result : integer;
begin
    writeln('Arithmetic Calculator');
    writeln('Enter first number: ');
    readln(a);
    writeln('Enter second number: ');
    readln(b);

    result := a + b;
    write('Addition: ');
    write(a);
    write(' + ');
    write(b);
    write(' = ');
    writeln(result);

    result := a - b;
    write('Subtraction: ');
    write(a);
    write(' - ');
    write(b);
    write(' = ');
    writeln(result);

    result := a * b;
    write('Multiplication: ');
    write(a);
    write(' * ');
    write(b);
    write(' = ');
    writeln(result);

    if b <> 0 then
    begin
        result := a div b;
        write('Division: ');
        write(a);
        write(' div ');
        write(b);
        write(' = ');
        writeln(result);

        result := a mod b;
        write('Modulo: ');
        write(a);
        write(' mod ');
        write(b);
        write(' = ');
        writeln(result)
    end
    else
    begin
        writeln('Cannot divide by zero!')
    end
end.
