program StudentDatabase;
type
    Student = record
        name : string;
        age : integer;
        grade : integer;
    end;

var
    students : array[1..3] of Student;
    i : integer;
    avgGrade : integer;
    sum : integer;
begin
    { Initialize student records in array }
    students[1].name := 'Alice';
    students[1].age := 20;
    students[1].grade := 85;

    students[2].name := 'Bob';
    students[2].age := 22;
    students[2].grade := 92;

    students[3].name := 'Charlie';
    students[3].age := 21;
    students[3].grade := 78;

    { Display all students }
    writeln('Student Database:');
    writeln('================');
    for i := 1 to 3 do
    begin
        write('Name: ');
        writeln(students[i].name);
        write('Age: ');
        writeln(students[i].age);
        write('Grade: ');
        writeln(students[i].grade);
        writeln('---')
    end;

    { Calculate average grade }
    sum := 0;
    for i := 1 to 3 do
    begin
        sum := sum + students[i].grade
    end;
    avgGrade := sum div 3;

    writeln('Average Grade: ');
    writeln(avgGrade)
end.
