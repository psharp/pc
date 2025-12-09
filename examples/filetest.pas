program FileTest;
var
    f : text;
    msg : string;
begin
    { Test Assign operation }
    writeln('Testing file operations...');
    msg := 'Test message';

    Assign(f, 'test.txt');
    writeln('File assigned');

    Rewrite(f);
    writeln('File opened for writing');

    Close(f);
    writeln('File closed');

    writeln('File operations test completed!')
end.
