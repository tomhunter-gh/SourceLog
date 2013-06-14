<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.Net.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.Net.dll</Reference>
  <Namespace>Mercurial</Namespace>
  <Namespace>Mercurial.Gui</Namespace>
</Query>

// *****************************************************
// *
// * This example shows how to open the repository explorer
// * in TortoiseHg.
// *
// ***********************

void Main()
{
	var repoPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), ".."));
    var repo = new Repository(repoPath);
    repo.LogGui();
    Debug.WriteLine("completed");
}