$ErrorActionPreference = 'Stop'
$projectDirectory = Split-Path -Parent $PSScriptRoot
$scratchDirectory = Join-Path $projectDirectory 'work'
New-Item -ItemType Directory -Path $scratchDirectory -Force | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
$testExecutable = Join-Path $scratchDirectory 'Verify.exe'
& $compiler /nologo /target:exe /codepage:65001 /main:Verify /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "/out:$testExecutable" (Join-Path $PSScriptRoot 'Verify.cs') (Join-Path $projectDirectory 'kaynak\TransportYollari.cs')
if ($LASTEXITCODE -ne 0) { throw 'Test derlemesi başarısız.' }
& $testExecutable (Join-Path $scratchDirectory 'test-preview.png')
if ($LASTEXITCODE -ne 0) { throw 'Kontroller başarısız.' }
