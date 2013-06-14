<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.NET.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.NET.dll</Reference>
  <Namespace>Mercurial</Namespace>
</Query>

// *****************************************************
// *
// * Clones the TortoiseHg repository, and then dumps
// * all changesets by yuja
// *
// ***********************

void Main()
{
    Directory.CreateDirectory(@"C:\Temp\TortoiseHg");
    var repo = new Repository(@"C:\Temp\TortoiseHg");
    repo.Clone("http://bitbucket.org/tortoisehg/stable",
        new CloneCommand()
            .WithUpdate(false) // don't need a working folder
            .WithTimeout(240)
            .WithObserver(new DebugObserver()));

    var log = repo.Log();
    var changesetsByYuja =
        from changeset in log
        where changeset.AuthorName == "Yuya Nishihara"
        select changeset;
        
    changesetsByYuja.Dump();
}