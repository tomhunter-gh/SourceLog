<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.NET.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.NET.dll</Reference>
  <Namespace>Mercurial</Namespace>
</Query>

// *****************************************************
// *
// * Dumps the log of the repo containing Mercurial.Net
// *
// ***********************

void Main()
{
	var repoPath = @"C:\Temp\repo";
    if (Directory.Exists(repoPath))
        Directory.Delete(repoPath, true);
    Directory.CreateDirectory(repoPath);
    var repo = new Repository(repoPath);
    var command = new InitCommand()
        .WithObserver(new DebugObserver());
    repo.Init(command);
}