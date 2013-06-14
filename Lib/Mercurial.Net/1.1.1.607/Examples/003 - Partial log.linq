<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.NET.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.NET.dll</Reference>
  <Namespace>Mercurial</Namespace>
</Query>

// *****************************************************
// *
// * Dumps a partial log of the repo containing
// * Mercurial.Net
// *
// ***********************

void Main()
{
	var repoPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), ".."));
    var repo = new Repository(repoPath);
    var command = new LogCommand()
        .WithIncludePathActions()
        .WithRevision(RevSpec.Range(2, 4));
    
    var log = repo.Log(command);
    
    log.Dump();
}