program ArrayExample;
var
    numbers : array[1..5] of integer;
    i : integer;
begin
    { Initialize array }
    numbers[1] := 10;
    numbers[2] := 20;
    numbers[3] := 30;
    numbers[4] := 40;
    numbers[5] := 50;

    { Print array elements }
    writeln('Array contents:');
    for i := 1 to 5 do
    begin
        write('numbers[');
        write(i);
        write('] = ');
        writeln(numbers[i])
    end
end.
