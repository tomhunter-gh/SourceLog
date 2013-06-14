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
    
	/*
    var report = repo.Churn(new ChurnCommand()
        .WithGroupTemplate("{author}")
		.WithUnit(Mercurial.Extensions.Churn.ChurnUnit.Changesets))
        .OrderBy(cg => cg.GroupName);
    report.Dump();*/
	
	ChurnCommand churn = new ChurnCommand().WithGroupTemplate("{author}")
		.WithUnit(Mercurial.Extensions.Churn.ChurnUnit.Lines);
	
	repo.Execute(churn);
	string output = churn.RawStandardOutput;
	Console.WriteLine(output);
	
}