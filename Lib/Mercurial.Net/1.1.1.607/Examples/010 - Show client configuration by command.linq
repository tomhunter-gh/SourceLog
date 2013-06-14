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
    var command = new ShowConfigCommand();
    Client.Execute(command);
    command.Result.Dump();
}