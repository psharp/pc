program FileCopyExample;
var
    infile, outfile : text;
    line : string;
    lineCount : integer;
begin
    writeln('Copying file...');

    { Open input file }
    Assign(infile, 'source.txt');
    Reset(infile);

    { Open output file }
    Assign(outfile, 'destination.txt');
    Rewrite(outfile);

    lineCount := 0;
    { Copy line by line }
    while not EOF(infile) do
    begin
        readln(infile, line);
        writeln(outfile, line);
        lineCount := lineCount + 1
    end;

    { Close both files }
    Close(infile);
    Close(outfile);

    write('Successfully copied ');
    write(lineCount);
    writeln(' lines from source.txt to destination.txt')
end.
