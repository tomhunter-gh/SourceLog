<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.Net.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.Net.dll</Reference>
  <Namespace>Mercurial</Namespace>
  <Namespace>Mercurial.Gui</Namespace>
</Query>

// *****************************************************
// *
// * This example shows how to open the repository explorer
// * in TortoiseHg, asynchronously.
// *
// ***********************

void Main()
{
	var repoPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), ".."));
    var repo = new Repository(repoPath);
    var evt = new ManualResetEvent(false);
    var ar = repo.BeginExecute(new GuiLogCommand(), c => evt.Set());
    Debug.WriteLine("waiting...");
    repo.EndExecute(ar);
    Debug.WriteLine("completed");
    evt.WaitOne();
    Debug.WriteLine("event signalled");
}