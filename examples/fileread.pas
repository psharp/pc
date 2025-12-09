program FileReadExample;
var
    f : text;
    line : string;
    count : integer;
begin
    { Read from a text file }
    Assign(f, 'input.txt');
    Reset(f);

    count := 0;
    writeln('Contents of input.txt:');
    writeln('====================');

    { Read until end of file }
    while not EOF(f) do
    begin
        readln(f, line);
        count := count + 1;
        write('Line ');
        write(count);
        write(': ');
        writeln(line)
    end;

    Close(f);
    writeln('====================');
    write('Total lines read: ');
    writeln(count)
end.
