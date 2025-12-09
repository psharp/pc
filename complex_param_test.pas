program ComplexParameterTest;

var
  x, y, z : integer;

function Max(a, b : integer) : integer;
begin
  if a > b then
    Max := a
  else
    Max := b
end;

function Min(a, b : integer) : integer;
begin
  if a < b then
    Min := a
  else
    Min := b
end;

function Clamp(value, minVal, maxVal : integer) : integer;
begin
  if value < minVal then
    Clamp := minVal
  else if value > maxVal then
    Clamp := maxVal
  else
    Clamp := value
end;

procedure PrintValues(a, b, c : integer);
begin
  writeln('Values: a=', a, ', b=', b, ', c=', c)
end;

begin
  writeln('Testing max and min functions:');
  writeln('Max(5, 10) = ', Max(5, 10));
  writeln('Min(5, 10) = ', Min(5, 10));
  writeln('Max(20, 15) = ', Max(20, 15));
  writeln('Min(20, 15) = ', Min(20, 15));

  writeln('');
  writeln('Testing nested function calls:');
  writeln('Max(Min(8, 12), Max(3, 7)) = ', Max(Min(8, 12), Max(3, 7)));

  writeln('');
  writeln('Testing function with 3 parameters:');
  writeln('Clamp(5, 1, 10) = ', Clamp(5, 1, 10));
  writeln('Clamp(-5, 1, 10) = ', Clamp(-5, 1, 10));
  writeln('Clamp(15, 1, 10) = ', Clamp(15, 1, 10));

  writeln('');
  writeln('Testing procedure with 3 parameters:');
  PrintValues(100, 200, 300);
  PrintValues(Max(10, 20), Min(30, 25), Clamp(50, 1, 40));

  writeln('');
  writeln('All complex tests completed!')
end.
