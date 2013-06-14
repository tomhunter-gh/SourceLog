<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.NET.dll">C:\Users\Wysie\Desktop\MercurialNet\Mercurial.Net\bin\Debug\Mercurial.NET.dll</Reference>
  <Namespace>Mercurial</Namespace>
</Query>

// *****************************************************
// *
// * Dumps a partial log of the repo containing
// * Mercurial.Net, excluding some changesets.
// *
// ***********************

void Main()
{
	var repoPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), ".."));
    var repo = new Repository(repoPath);
    var log = repo.Log(new LogCommand()
        .WithIncludePathActions()
		//.WithUser("Lasse")
		.WithUser("Wysie")
        .WithRevision(!RevSpec.Range(2, 3)));
    
    log.Dump();
}