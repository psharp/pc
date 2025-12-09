program LoopsDemo;
var
    i, sum, n : integer;
begin
    { For loop demo }
    writeln('Counting from 1 to 10:');
    for i := 1 to 10 do
    begin
        write(i);
        write(' ')
    end;
    writeln();

    { Countdown demo }
    writeln('Countdown from 10 to 1:');
    for i := 10 downto 1 do
    begin
        write(i);
        write(' ')
    end;
    writeln();

    { While loop demo }
    writeln('Sum of numbers from 1 to 100:');
    sum := 0;
    i := 1;
    while i <= 100 do
    begin
        sum := sum + i;
        i := i + 1
    end;
    writeln(sum)
end.
