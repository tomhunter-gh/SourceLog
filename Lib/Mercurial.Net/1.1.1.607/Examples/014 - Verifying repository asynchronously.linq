<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.NET.dll">C:\dev\vs.net\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.NET.dll</Reference>
  <Namespace>Mercurial</Namespace>
</Query>

// *****************************************************
// *
// * This example shows how to verify the integrity
// * of the repository, asynchronously.
// *
// ***********************

void Main()
{
	var repoPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), ".."));
    var repo = new Repository(repoPath);
    var evt = new ManualResetEvent(false);
    var ar = repo.BeginExecute(new VerifyCommand(), c =>
    {
        Debug.WriteLine(((VerifyCommand)c.AsyncState).RawStandardOutput);
        evt.Set();
    });
    Debug.WriteLine("waiting...");
    repo.EndExecute(ar);
    Debug.WriteLine("finished executing");
    evt.WaitOne();
    Debug.WriteLine("event signalled");
}