$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
$destination = Join-Path (Split-Path -Parent $PSScriptRoot) 'SAP-Transport-Yollari.exe'
& $compiler /nologo /target:winexe /platform:anycpu /optimize+ /codepage:65001 /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "/out:$destination" (Join-Path $PSScriptRoot 'TransportYollari.cs')
if ($LASTEXITCODE -ne 0) { throw 'Derleme başarısız.' }
Write-Output $destination
