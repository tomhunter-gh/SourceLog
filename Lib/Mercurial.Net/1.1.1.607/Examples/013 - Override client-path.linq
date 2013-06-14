<Query Kind="Program">
  <Reference Relative="..\Mercurial.Net\bin\Debug\Mercurial.NET.dll">C:\Dev\VS.NET\Mercurial.Net\Mercurial.Net\bin\Debug\Mercurial.NET.dll</Reference>
  <Namespace>Mercurial</Namespace>
</Query>

// *****************************************************
// *
// * This example shows how to override the client path.
// * Note that you need to have a Mercurial copy in the
// * directory C:\Temp\Mercurial 1.6.2
// * Otherwise this example won't work.
// *
// * NOTE! If you re-run this example, note that LINQPad
// * Doesn't automatically unload appdomains, which means
// * that the overridden client will be used on subsequent
// * runs.
// *
// ***********************

void Main()
{
    Client.Version.Dump("Detected during setup");
    Client.SetClientPath(@"C:\Temp\Mercurial 1.6.2");
    Client.Version.Dump("Overridden");
}