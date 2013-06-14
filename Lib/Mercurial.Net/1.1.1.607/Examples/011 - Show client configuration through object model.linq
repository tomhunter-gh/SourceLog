<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.NET.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.NET.dll</Reference>
  <Namespace>Mercurial</Namespace>
</Query>

// *****************************************************
// *
// * Executes the showconfig command and dumps the results
// *
// ***********************

void Main()
{
    foreach (var section in Client.Configuration.Sections)
    {
        Client.Configuration
            .Where(e => e.Section == section)
            .Select(e => new { e.Name, e.Value })
            .Dump(section);
    }
}