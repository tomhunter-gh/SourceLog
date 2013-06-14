<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.Net.dll">C:\dev\vs.net\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.Net.dll</Reference>
  <Namespace>Mercurial</Namespace>
  <Namespace>Mercurial.Extensions.Churn</Namespace>
</Query>

// *****************************************************
// *
// * This example just shows the syntax of RevSpec
// * usage. If you check the "hg help revsets" you'll
// * see that the examples have been lifted from there.
// *
// ***********************

void Main()
{
    RevSpec.ByBranch("default").ToString().Dump("Changesets on the default branch");
    (RevSpec.ByBranch("default") & RevSpec.DescendantsOf("1.5") & !RevSpec.Merges).ToString().Dump("Cahngesets on the default branch since tag 1.5 (excluding merges)");
    (RevSpec.Heads & RevSpec.Closed.Not).ToString().Dump("Open branch heads");
    (RevSpec.Bracketed("1.3", "1.5") && RevSpec.Keyword("bug") && RevSpec.Affects("hgext/*")).ToString().Dump("Changesets between tags 1.3 and 1.5 mentioning \"bug\" that affect \"hgext/*\"");
    ((RevSpec.Keyword("bug") || RevSpec.Keyword("issue")) && !RevSpec.Tagged().Ancestors).ToString().Dump("Changesets mentioning \"bug\" or \"issue\" that are not in a tagged release");
}