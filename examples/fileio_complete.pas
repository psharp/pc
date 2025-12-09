program FileIOComplete;
var
    f : text;
    line : string;
    count, num : integer;
begin
    writeln('=== Complete File I/O Demo ===');
    writeln();

    { Write to file using Write(f, value) syntax }
    writeln('Writing to output.txt...');
    Assign(f, 'output.txt');
    Rewrite(f);

    Write(f, 'First line');
    Writeln(f, ' with more text');
    Writeln(f, 'Second line');
    Write(f, 'Line ');
    Write(f, 3);
    Writeln(f, ' with number');

    Close(f);
    writeln('File written successfully!');
    writeln();

    { Read from file using Read(f, var) syntax }
    writeln('Reading from output.txt...');
    Assign(f, 'output.txt');
    Reset(f);

    count := 0;
    writeln('File contents:');
    writeln('====================');

    while not EOF(f) do
    begin
        Readln(f, line);
        count := count + 1;
        write('Line ');
        write(count);
        write(': ');
        writeln(line)
    end;

    Close(f);
    writeln('====================');
    write('Total lines read: ');
    writeln(count);
    writeln();
    writeln('=== File I/O test completed! ===')
end.
