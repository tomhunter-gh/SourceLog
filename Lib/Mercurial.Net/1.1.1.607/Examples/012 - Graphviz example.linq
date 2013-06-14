<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.NET.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.NET.dll</Reference>
  <Namespace>Mercurial</Namespace>
</Query>

// *****************************************************
// *
// * Visualize a random bitbucket-project through Graphviz
// * The project was chosen because it had relatively few
// * changesets, and a couple of parallel branches.
// *
// ***********************

const string repoPath = @"C:\Temp\repo";
const string repoSource = @"http://bitbucket.org/ventor3000/iup.net";
const string tempFile = @"C:\Temp\test.dot";

void Main()
{
    if (Directory.Exists(repoPath))
        Directory.Delete(repoPath, true);
    Directory.CreateDirectory(repoPath);
    
    var repo = new Repository(repoPath);
    repo.Init();
    var log = repo.Incoming(repoSource, new IncomingCommand().WithTimeout(240));

    List<string> lines = new List<string>();
    lines.Add("digraph G {");
    lines.Add("   rankdir = LR;");
    
    foreach (var changeset in log)
    {
        if (changeset.LeftParentRevision >= 0)
            lines.Add("    \"" + changeset.LeftParentRevision + "\" -> \"" + changeset.RevisionNumber + "\"");
        if (changeset.RightParentRevision >= 0)
            lines.Add("    \"" + changeset.RightParentRevision + "\" -> \"" + changeset.RevisionNumber + "\"");
    }
    
    lines.Add("}");
    File.WriteAllLines(tempFile, lines.ToArray());
    
    Util.Cmd("dot " + tempFile + " -Tpng -O");
    Util.Cmd("\"" + tempFile + ".png\"");
}