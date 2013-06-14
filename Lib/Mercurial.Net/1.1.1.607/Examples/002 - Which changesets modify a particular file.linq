<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.NET.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.NET.dll</Reference>
  <Namespace>Mercurial</Namespace>
</Query>

// *****************************************************
// *
// * Dumps all the changesets where Repository.cs is
// * modified.
// *
// ***********************

void Main()
{
	var repoPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), ".."));
    var repo = new Repository(repoPath);
    var command = new LogCommand().WithIncludePathActions();
    var log = repo.Log(command);
    
    var changesets =
        from changeset in log
        where changeset.PathActions.Any(pa =>
            pa.Path == "Mercurial.Net/Repository.cs" &&
            (pa.Action == ChangesetPathActionType.Modify || pa.Action == ChangesetPathActionType.Add))
        select changeset;
            
    changesets.Dump();
}