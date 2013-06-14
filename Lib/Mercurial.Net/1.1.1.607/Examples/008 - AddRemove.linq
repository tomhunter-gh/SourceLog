<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.NET.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.NET.dll</Reference>
  <Namespace>Mercurial</Namespace>
</Query>

// *****************************************************
// *
// * AddRemove example.
// *
// ***********************

void Main()
{
	var repoPath = @"C:\Temp\repo";
    if (Directory.Exists(repoPath))
        Directory.Delete(repoPath, true);
    Directory.CreateDirectory(repoPath);
    var repo = new Repository(repoPath);
    repo.Init();
    
    File.WriteAllText(Path.Combine(repo.Path, "test1.txt"), "test1.txt contents");
    File.WriteAllText(Path.Combine(repo.Path, "test2.txt"), "test2.txt contents");
    
    repo.AddRemove(new AddRemoveCommand()
        .WithIncludePattern("test1.*"));
    
    repo.Commit("dummy");
    repo.Log(new LogCommand()
        .WithIncludePathActions()).Dump();
}