program EnumSetDemo;

type
  Color = (Red, Green, Blue, Yellow);
  Day = (Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday);

var
  favoriteColor : Color;
  workDays : set of Day;
  primaryColors : set of Color;
  today : Day;
  isWorkDay : boolean;

begin
  { Demonstrate enum values }
  favoriteColor := Red;
  writeln('Favorite color is Red');

  today := Wednesday;
  writeln('Today is Wednesday');

  { Demonstrate set literals }
  primaryColors := [Red, Blue, Yellow];
  writeln('Primary colors set created');

  workDays := [Monday, Tuesday, Wednesday, Thursday, Friday];
  writeln('Work days set created');

  { Demonstrate set membership testing with 'in' operator }
  if Red in primaryColors then
    writeln('Red is a primary color')
  else
    writeln('Red is not a primary color');

  if Green in primaryColors then
    writeln('Green is a primary color')
  else
    writeln('Green is not a primary color');

  { Test if today is a work day }
  isWorkDay := today in workDays;
  if isWorkDay then
    writeln('Today is a work day')
  else
    writeln('Today is not a work day');

  { Test weekend }
  if Saturday in workDays then
    writeln('Saturday is a work day')
  else
    writeln('Saturday is a weekend day');

  writeln('Enum and set demo completed!')
end.
