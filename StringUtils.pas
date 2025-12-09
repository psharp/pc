unit StringUtils;

interface

function Concat(s1, s2: string): string;
procedure PrintBanner(msg: string);

implementation

function Concat(s1, s2: string): string;
begin
    Concat := s1 + s2;
end;

procedure PrintBanner(msg: string);
begin
    writeln('====================');
    writeln(msg);
    writeln('====================');
end;

end.
