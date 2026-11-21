$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot '..\TS SE Tool'
$sources = @((Join-Path $PSScriptRoot 'InspectSave.cs'))
$sources += (Get-ChildItem -LiteralPath "$project\CustomClasses\Save\Items" -Filter '*.cs').FullName
$sources += (Get-ChildItem -LiteralPath "$project\CustomClasses\Save\DataFormat" -Filter '*.cs').FullName
$sources += "$project\CustomClasses\Utilities\NumericUtilities.cs"
$sources += "$project\CustomClasses\Utilities\TextUtilities.cs"
$sources += "$project\CustomClasses\Save\VersionToVersion.cs"
$sources += "$project\CustomClasses\Save\SaveFileProfileData.cs"
$sources += "$project\CustomClasses\ExternalData\LevelNames.cs"
$sources += "$project\CustomClasses\Global\FloatList.cs"
$sources += "$project\CustomClasses\Global\EnumerableExt.cs"
$compiler = 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Roslyn\csc.exe'
& $compiler /nologo /debug:full /optimize- /target:exe "/out:$PSScriptRoot\InspectSave.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:Microsoft.CSharp.dll $sources
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& "$PSScriptRoot\InspectSave.exe" "$PSScriptRoot\game.decoded.sii"
exit $LASTEXITCODE
