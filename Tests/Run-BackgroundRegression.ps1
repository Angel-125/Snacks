param([string]$KspManagedDirectory = 'C:\KSPDev\KSP_x64_Data\Managed', [ValidateSet('Debug','Release')][string]$Configuration = 'Debug')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$msbuild = 'C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$outputDir = Join-Path $projectRoot 'Snacks\obj\StressValidation\bin'
& $msbuild (Join-Path $projectRoot 'Snacks\Snacks.csproj') /t:Build /p:Configuration=$Configuration /p:PostBuildEvent= /p:OutputPath=obj\StressValidation\bin\ /p:IntermediateOutputPath=obj\StressValidation\obj\ /p:DocumentationFile= /verbosity:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
$output = Join-Path $outputDir 'BackgroundRegression.exe'
$references = @('SnacksUtils.dll', 'Assembly-CSharp.dll', 'UnityEngine.CoreModule.dll')
$referenceArguments = @(
    ('/r:' + (Join-Path $outputDir $references[0]))
    ('/r:' + (Join-Path $KspManagedDirectory $references[1]))
    ('/r:' + (Join-Path $KspManagedDirectory $references[2]))
    ('/r:' + (Join-Path $KspManagedDirectory 'UnityEngine.UI.dll'))
)
& $compiler /nologo /out:$output @referenceArguments (Join-Path $PSScriptRoot 'BackgroundRegression.cs')
if ($LASTEXITCODE -ne 0) { throw 'Regression compilation failed.' }
& $output $KspManagedDirectory
if ($LASTEXITCODE -ne 0) { throw 'Background regression failed.' }
