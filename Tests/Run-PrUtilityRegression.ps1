$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$outputDir = Join-Path $projectRoot 'Snacks\obj\StressValidation'
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
$output = Join-Path $outputDir 'PrUtilityRegression.exe'
& $compiler /nologo /out:$output (Join-Path $PSScriptRoot 'PrUtilityRegression.cs') `
    (Join-Path $PSScriptRoot 'LocalizationDouble.cs') `
    (Join-Path $projectRoot 'Snacks\Utilities\SnacksText.cs') `
    (Join-Path $projectRoot 'Snacks\Utilities\SnacksTime.cs')
if ($LASTEXITCODE -ne 0) { throw 'Utility regression compilation failed.' }
& $output
if ($LASTEXITCODE -ne 0) { throw 'Utility regression failed.' }
