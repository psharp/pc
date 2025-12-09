program TypeCheckErrors;
var
    x, y : integer;
    z : real;
    name : string;
    flag : boolean;
begin
    { This should cause type errors }

    { Error: assigning string to integer }
    x := 'hello';

    { Error: assigning boolean to integer }
    y := true;

    { Error: adding integer and string }
    x := y + name;

    { Error: boolean in arithmetic }
    z := flag * 5;

    { Error: non-boolean condition }
    if x then
        writeln('error');

    { Error: string in for loop }
    for name := 1 to 10 do
        writeln(name)
end.
