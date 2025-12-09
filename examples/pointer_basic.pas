program PointerBasic;
var
    ptr : ^integer;
    value : integer;
begin
    writeln('=== Basic Pointer Demo ===');
    writeln();

    { Allocate memory for pointer }
    writeln('Allocating memory with New(ptr)...');
    New(ptr);
    writeln('Memory allocated!');
    writeln();

    { Assign value through pointer }
    writeln('Assigning 42 to ptr^...');
    ptr^ := 42;
    writeln('Value assigned!');
    writeln();

    { Read value through pointer }
    writeln('Reading value from ptr^:');
    value := ptr^;
    write('Value is: ');
    writeln(value);
    writeln();

    { Test nil }
    writeln('Testing nil pointer...');
    Dispose(ptr);
    writeln('Memory disposed, ptr is now nil');
    writeln();

    writeln('=== Pointer test completed! ===')
end.
