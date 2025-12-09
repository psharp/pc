program FileIOAdvanced;
var
    inFile, outFile : text;
    line : string;
    num, count, total : integer;
    average : real;
begin
    writeln('=== Advanced File I/O Demo ===');
    writeln();

    { Create a file with numbers }
    writeln('Creating numbers.txt with data...');
    Assign(outFile, 'numbers.txt');
    Rewrite(outFile);

    Writeln(outFile, 'Student Scores:');
    Write(outFile, 'Score 1: ');
    Writeln(outFile, 85);
    Write(outFile, 'Score 2: ');
    Writeln(outFile, 92);
    Write(outFile, 'Score 3: ');
    Writeln(outFile, 78);
    Write(outFile, 'Score 4: ');
    Writeln(outFile, 95);
    Write(outFile, 'Score 5: ');
    Writeln(outFile, 88);

    Close(outFile);
    writeln('File created successfully!');
    writeln();

    { Read and display the file }
    writeln('Reading numbers.txt:');
    writeln('====================');
    Assign(inFile, 'numbers.txt');
    Reset(inFile);

    count := 0;
    while not EOF(inFile) do
    begin
        Readln(inFile, line);
        count := count + 1;
        writeln(line)
    end;

    Close(inFile);
    writeln('====================');
    write('Total lines: ');
    writeln(count);
    writeln();

    { Write mixed data types to file }
    writeln('Creating report.txt...');
    Assign(outFile, 'report.txt');
    Rewrite(outFile);

    Writeln(outFile, '=== Student Report ===');
    Write(outFile, 'Name: ');
    Writeln(outFile, 'Alice Smith');
    Write(outFile, 'Age: ');
    Writeln(outFile, 20);
    Write(outFile, 'GPA: ');
    Writeln(outFile, 3.85);
    Write(outFile, 'Honor Roll: ');
    Writeln(outFile, true);

    Close(outFile);
    writeln('Report created!');
    writeln();

    { Read and display the report }
    writeln('Contents of report.txt:');
    writeln('====================');
    Assign(inFile, 'report.txt');
    Reset(inFile);

    while not EOF(inFile) do
    begin
        Readln(inFile, line);
        writeln(line)
    end;

    Close(inFile);
    writeln('====================');
    writeln();

    writeln('=== Advanced File I/O test completed! ===')
end.
