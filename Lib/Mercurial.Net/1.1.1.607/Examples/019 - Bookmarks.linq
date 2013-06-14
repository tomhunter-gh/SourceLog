<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.Net.dll">C:\dev\vs.net\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.Net.dll</Reference>
  <Namespace>Mercurial</Namespace>
  <Namespace>Mercurial.Extensions.Churn</Namespace>
</Query>

// *****************************************************
// *
// * This example shows how to list all the bookmarks
// * in a repository.
// *
// ***********************

void Main()
{
	var repoPath = @"C:\Temp\SomeOtherRepo";
    var repo = new Repository(repoPath);

    repo.Bookmarks().Dump();
}