$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$outputDir = Join-Path $projectRoot 'Snacks\obj\StressValidation'
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
$output = Join-Path $outputDir 'StressRegression.exe'
& $compiler /nologo /out:$output (Join-Path $PSScriptRoot 'StressRegression.cs') `
    (Join-Path $projectRoot 'Snacks\Processors\StressProcessor.cs') `
    (Join-Path $projectRoot 'Snacks\EventSystem\Outcomes\OnStrikePenalty.cs') `
    (Join-Path $projectRoot 'Snacks\ResourceClasses\SnacksRosterResource.cs') `
    (Join-Path $projectRoot 'Snacks\Utilities\SnacksText.cs') `
    (Join-Path $PSScriptRoot 'LocalizationDouble.cs')
if ($LASTEXITCODE -ne 0) { throw 'Regression harness compilation failed.' }
& $output
if ($LASTEXITCODE -ne 0) { throw 'Stress regression failed.' }
