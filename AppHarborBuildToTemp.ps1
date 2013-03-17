$msbuild = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
$outdir = "C:\temp\SourceLog\SourceLog" + (get-date -f yyyyMMddHHmmss) + "\"
$cmd = $msbuild + ' "' + (Split-Path $MyInvocation.MyCommand.Path) + '\SourceLog.sln" /property:Configuration=Release /property:OutDir=' + $outdir
Invoke-Expression $cmd