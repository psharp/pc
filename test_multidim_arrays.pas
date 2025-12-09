program MultidimensionalArrayTest;

var
    matrix: array[1..3, 1..4] of integer;
    cube: array[0..2, 0..2, 0..2] of integer;
    i, j, k: integer;
    sum: integer;

begin
    writeln('Testing 2D Array (3x4 matrix):');

    { Initialize 2D array }
    for i := 1 to 3 do
    begin
        for j := 1 to 4 do
        begin
            matrix[i, j] := i * 10 + j;
        end;
    end;

    { Display 2D array }
    for i := 1 to 3 do
    begin
        for j := 1 to 4 do
        begin
            write('matrix[', i, ',', j, '] = ', matrix[i, j], '  ');
        end;
        writeln;
    end;

    writeln;
    writeln('Testing 3D Array (3x3x3 cube):');

    { Initialize 3D array }
    for i := 0 to 2 do
    begin
        for j := 0 to 2 do
        begin
            for k := 0 to 2 do
            begin
                cube[i, j, k] := i * 100 + j * 10 + k;
            end;
        end;
    end;

    { Display some values from 3D array }
    writeln('cube[0,0,0] = ', cube[0, 0, 0]);
    writeln('cube[1,1,1] = ', cube[1, 1, 1]);
    writeln('cube[2,2,2] = ', cube[2, 2, 2]);
    writeln('cube[1,2,0] = ', cube[1, 2, 0]);

    { Calculate sum of all elements in matrix }
    sum := 0;
    for i := 1 to 3 do
    begin
        for j := 1 to 4 do
        begin
            sum := sum + matrix[i, j];
        end;
    end;

    writeln;
    writeln('Sum of all matrix elements: ', sum);
end.
