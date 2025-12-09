program ParameterTest;

var
  result : integer;

procedure PrintSum(a, b : integer);
begin
  writeln('Sum: ', a + b)
end;

function Multiply(x, y : integer) : integer;
begin
  Multiply := x * y
end;

function Add(a, b : integer) : integer;
begin
  Add := a + b
end;

begin
  writeln('Testing procedure with parameters:');
  PrintSum(5, 3);
  PrintSum(10, 20);

  writeln('Testing function with parameters:');
  result := Multiply(4, 7);
  writeln('4 * 7 = ', result);

  result := Add(15, 25);
  writeln('15 + 25 = ', result);

  { Test nested calls }
  result := Add(Multiply(2, 3), Multiply(4, 5));
  writeln('(2*3) + (4*5) = ', result);

  writeln('All tests completed!')
end.
