param([Parameter(Mandatory = $true)][string]$ZipPath)

$ErrorActionPreference = 'Stop'
$archive = Get-Item -LiteralPath $ZipPath
if ($archive.PSIsContainer -or $archive.Name -notmatch '^TS-ETS2\.\d+\.\d+\.\d+\.\d+\.zip$') {
    throw 'Use an app ZIP named TS-ETS2.VERSION.zip, for example TS-ETS2.0.3.13.0.zip.'
}
$hash = (Get-FileHash -LiteralPath $archive.FullName -Algorithm SHA512).Hash
$metadata = "$($archive.Name)`t$hash"
$outputPath = Join-Path $archive.DirectoryName 'fileData.txt'
Set-Content -LiteralPath $outputPath -Value $metadata -NoNewline -Encoding Ascii
if ((Get-Content -LiteralPath $outputPath -Raw) -cne $metadata) {
    throw 'The update metadata did not match the ZIP filename and SHA-512 hash.'
}
Write-Output "Created $outputPath"
