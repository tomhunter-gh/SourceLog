<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.Net.dll">C:\dev\vs.net\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.Net.dll</Reference>
  <Namespace>Mercurial</Namespace>
  <Namespace>Mercurial.Extensions.Churn</Namespace>
</Query>

// *****************************************************
// *
// * This example shows how to do a bisection of a
// * repository, to subdivide looking for the first
// * good revision according to some test criteria.
// *
// ***********************

void Main()
{
	var repoPath = @"C:\Temp\SomeOtherRepo";
    var repo = new Repository(repoPath);
    
    repo.Bisect(BisectState.Reset);
    
    repo.Update(0);
    repo.Bisect(GoodToBisectState(IsCurrentChangesetGood(repo)));
    
    repo.Update();
    BisectResult result = repo.Bisect(GoodToBisectState(IsCurrentChangesetGood(repo)));
    while (!result.Done)
    {
        Debug.WriteLine("at: " + result.Revision);
        result = repo.Bisect(GoodToBisectState(IsCurrentChangesetGood(repo)));
    }
    Debug.WriteLine("found: " + result.Revision);
    
    repo.Log(result.Revision).Dump();
}

public BisectState GoodToBisectState(bool isGood)
{
    if (isGood)
        return BisectState.Good;
    else
        return BisectState.Bad;
}

public bool IsCurrentChangesetGood(Repository repo)
{
    var revno = repo.Log(RevSpec.WorkingDirectoryParent).First().RevisionNumber;
    if (revno == 0)
        return false;
    else
        return true;
}