<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.Net.dll">C:\dev\vs.net\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.Net.dll</Reference>
  <Namespace>Mercurial</Namespace>
  <Namespace>Mercurial.Extensions.Churn</Namespace>
</Query>

// *****************************************************
// *
// * This example starts the Mercurial web server
// * for the Mercurial.Net repository
// *
// * NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE
// *
// * This demo spawns a process that DOES NOT terminate
// * by itself. You will have to open Task Manager,
// * find the "hg.exe" process that is running,
// * and terminate it when you're done with the demo.
// *
// * NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE NOTE
// *
// ***********************

void Main()
{
	var repoPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), ".."));
    var repo = new Repository(repoPath);

    repo.Execute(new CustomCommand("serve")
        .WithObserver(new DebugObserver())
        .WithAdditionalArgument("--port 8123")
        .WithAdditionalArgument("--verbose"));
}