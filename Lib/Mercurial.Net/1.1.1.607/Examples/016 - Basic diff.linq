<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.Net.dll">C:\Users\Wysie\Desktop\MercurialNet\Mercurial.Net\bin\Debug\Mercurial.Net.dll</Reference>
  <Namespace>Mercurial</Namespace>
  <Namespace>Mercurial.Extensions.Churn</Namespace>
</Query>

// *****************************************************
// *
// * This example shows how to produce a churn
// * report for the repository.
// *
// ***********************

void Main()
{
	var repoPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), ".."));
    var repo = new Repository(repoPath);
    
    DiffCommand diff = new DiffCommand().WithRevision(RevSpec.Single(267)).WithGitDiffFormat();
	repo.Execute(diff);
	string output = diff.RawStandardOutput;
	Console.WriteLine(output);
}