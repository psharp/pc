program FileIOShowcase;
{ This program demonstrates all file I/O capabilities }
var
    dataFile, logFile : text;
    name : string;
    score, total, count : integer;
    average : real;
begin
    writeln('=== File I/O Showcase ===');
    writeln();

    { Part 1: Create a data file with student scores }
    writeln('Part 1: Writing student data to students.txt');
    Assign(dataFile, 'students.txt');
    Rewrite(dataFile);

    { Write using both Write and Writeln }
    Write(dataFile, 'Alice');
    Write(dataFile, ' ');
    Writeln(dataFile, 95);

    Write(dataFile, 'Bob');
    Write(dataFile, ' ');
    Writeln(dataFile, 87);

    Write(dataFile, 'Charlie');
    Write(dataFile, ' ');
    Writeln(dataFile, 92);

    Close(dataFile);
    writeln('Student data written successfully!');
    writeln();

    { Part 2: Read and process the data }
    writeln('Part 2: Reading and processing students.txt');
    Assign(dataFile, 'students.txt');
    Reset(dataFile);

    total := 0;
    count := 0;

    { Also write to a log file }
    Assign(logFile, 'processing.log');
    Rewrite(logFile);
    Writeln(logFile, '=== Processing Log ===');

    writeln('Student Scores:');
    writeln('---------------');

    while not EOF(dataFile) do
    begin
        { Read the entire line }
        Readln(dataFile, name);

        count := count + 1;

        { Display to console }
        write(count);
        write('. ');
        writeln(name);

        { Log to file }
        Write(logFile, 'Processed record ');
        Writeln(logFile, count)
    end;

    Close(dataFile);

    { Finish the log }
    Write(logFile, 'Total records: ');
    Writeln(logFile, count);
    Close(logFile);

    writeln('---------------');
    write('Total students: ');
    writeln(count);
    writeln();

    { Part 3: Display the log file }
    writeln('Part 3: Displaying processing.log');
    Assign(logFile, 'processing.log');
    Reset(logFile);

    writeln('Log file contents:');
    writeln('==================');

    while not EOF(logFile) do
    begin
        Readln(logFile, name);
        writeln(name)
    end;

    Close(logFile);
    writeln('==================');
    writeln();

    writeln('=== File I/O Showcase Complete! ===');
    writeln('All file operations demonstrated successfully!')
end.
