<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.NET.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.NET.dll</Reference>
  <Namespace>Mercurial</Namespace>
</Query>

// *****************************************************
// *
// * Clones the Mercurial.Net repository and shows the log.
// *
// ***********************

void Main()
{
	var repoPath = @"C:\Temp\repo";
    if (Directory.Exists(repoPath))
        Directory.Delete(repoPath, true);
    Directory.CreateDirectory(repoPath);
    var repo = new Repository(repoPath);
    repo.Clone("https://hg01.codeplex.com/mercurialnet",
        new CloneCommand()
            .WithObserver(new DebugObserver())
            .WithUpdate(false));
    repo.Log().Dump();
}