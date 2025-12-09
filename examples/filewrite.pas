program FileWriteExample;
var
    f : text;
    i : integer;
begin
    { Create and write to a text file }
    Assign(f, 'output.txt');
    Rewrite(f);

    writeln(f, 'Hello from Pascal!');
    writeln(f, 'This is a file I/O example.');

    { Write some numbers }
    for i := 1 to 5 do
    begin
        write(f, 'Number: ');
        writeln(f, i)
    end;

    Close(f);
    writeln('File written successfully!')
end.
