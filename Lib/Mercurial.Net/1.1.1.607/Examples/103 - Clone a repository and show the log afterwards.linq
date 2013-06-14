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
    var repoPath = @"C:\Temp\repo";
    if (Directory.Exists(repoPath))
        Directory.Delete(repoPath, true);
    Directory.CreateDirectory(repoPath);
    var repo = new Repository(repoPath);
    repo.CloneGui();
    repo.LogGui();
}