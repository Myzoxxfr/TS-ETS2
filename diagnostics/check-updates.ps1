$ErrorActionPreference = 'Stop'
$sourcePath = Join-Path $PSScriptRoot '..\TS SE Tool\CustomClasses\Utilities\Web_Utilities.cs'
$compiler = 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Roslyn\csc.exe'
& $compiler /nologo /target:exe "/out:$PSScriptRoot\CheckUpdates.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll $sourcePath "$PSScriptRoot\CheckUpdates.cs"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& "$PSScriptRoot\CheckUpdates.exe"
exit $LASTEXITCODE
