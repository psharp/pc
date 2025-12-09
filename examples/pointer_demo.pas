program PointerDemo;
var
    p1, p2 : ^integer;
    x, y : integer;
begin
    writeln('=== Pointer Demo ===');
    writeln();

    { Test 1: Basic pointer operations }
    writeln('Test 1: Basic pointer operations');
    New(p1);
    p1^ := 10;
    write('p1^ = ');
    writeln(p1^);
    writeln();

    { Test 2: Multiple pointers }
    writeln('Test 2: Multiple pointers');
    New(p2);
    p2^ := 20;
    write('p1^ = ');
    write(p1^);
    write(', p2^ = ');
    writeln(p2^);
    writeln();

    { Test 3: Pointer assignment }
    writeln('Test 3: Assigning pointer values');
    p1^ := p2^;
    write('After p1^ := p2^, p1^ = ');
    writeln(p1^);
    writeln();

    { Test 4: Using pointer values in expressions }
    writeln('Test 4: Pointer arithmetic');
    x := p1^ + p2^;
    write('p1^ + p2^ = ');
    writeln(x);
    writeln();

    { Test 5: nil pointer }
    writeln('Test 5: Nil pointer handling');
    Dispose(p1);
    Dispose(p2);
    p1 := nil;
    p2 := nil;
    writeln('Both pointers set to nil');
    writeln();

    { Test 6: Reassign after nil }
    writeln('Test 6: Reallocate after dispose');
    New(p1);
    p1^ := 100;
    write('New p1^ = ');
    writeln(p1^);
    writeln();

    Dispose(p1);
    writeln('=== All pointer tests passed! ===')
end.
