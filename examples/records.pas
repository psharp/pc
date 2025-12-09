program RecordExample;
type
    Person = record
        name : string;
        age : integer;
    end;

var
    john : Person;
begin
    { Initialize record }
    john.name := 'John Doe';
    john.age := 30;

    { Print record fields }
    write('Name: ');
    writeln(john.name);
    write('Age: ');
    writeln(john.age)
end.
