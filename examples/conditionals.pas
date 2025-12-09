program Conditionals;
var
    age : integer;
    name : string;
begin
    writeln('Age Checker');
    writeln('Enter your name: ');
    readln(name);
    writeln('Enter your age: ');
    readln(age);

    write('Hello, ');
    writeln(name);

    if age >= 18 then
    begin
        writeln('You are an adult')
    end
    else
    begin
        writeln('You are a minor')
    end;

    if age >= 65 then
        writeln('Senior citizen discount available!')
    else if age >= 18 then
        writeln('Regular adult pricing')
    else if age >= 13 then
        writeln('Teenager discount available!')
    else
        writeln('Child discount available!')
end.
