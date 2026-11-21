param(
    [string]$InputName = 'game.original.sii',
    [string]$OutputName = 'game.decoded.sii'
)
$ErrorActionPreference = 'Stop'
$dllPath = Join-Path $PSScriptRoot '..\TS SE Tool\SII_Decrypt.dll'
$code = @'
using System;
using System.Runtime.InteropServices;
public static class SaveDecoder {
    [DllImport("DLL_PATH", EntryPoint="GetMemoryFormat")]
    public static extern int Format(byte[] input, uint size);
    [DllImport("DLL_PATH", EntryPoint="DecryptAndDecodeMemory")]
    public static extern int Decode(byte[] input, uint size, byte[] output, ref uint outputSize);
}
'@
Add-Type -TypeDefinition $code.Replace('DLL_PATH', $dllPath.Replace('\', '\\'))
$inputBytes = [IO.File]::ReadAllBytes((Join-Path $PSScriptRoot $InputName))
Write-Output "Process bits: $([IntPtr]::Size * 8)"
Write-Output "Save format: $([SaveDecoder]::Format($inputBytes, $inputBytes.Length))"
[uint32]$outputSize = 0
$result = [SaveDecoder]::Decode($inputBytes, $inputBytes.Length, $null, [ref]$outputSize)
Write-Output "Decode result: $result; bytes: $outputSize"
if ($result -ne 0) { exit 1 }
$outputBytes = New-Object byte[] $outputSize
$result = [SaveDecoder]::Decode($inputBytes, $inputBytes.Length, $outputBytes, [ref]$outputSize)
if ($result -ne 0) { throw "Decode failed: $result" }
[IO.File]::WriteAllBytes((Join-Path $PSScriptRoot $OutputName), $outputBytes)
