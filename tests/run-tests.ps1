$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repositoryRoot 'src\portable\WeatherToolsPortable\WeatherToolsPortable.vbproj'
$appOutput = Join-Path $repositoryRoot 'src\portable\WeatherToolsPortable\bin\Release\WeatherToolsV6.exe'
$msbuild = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$testOutputDirectory = Join-Path $env:TEMP ("WeatherToolsSmokeTests_{0}" -f $PID)
$testExecutable = Join-Path $testOutputDirectory 'WeatherToolsSmokeTests.exe'

if (-not (Test-Path -LiteralPath $msbuild)) { throw "MSBuild not found: $msbuild" }
if (-not (Test-Path -LiteralPath $csc)) { throw "C# compiler not found: $csc" }

& $msbuild $project /p:Configuration=Release /verbosity:minimal
if ($LASTEXITCODE -ne 0) { throw "WeatherToolsPortable build failed with exit code $LASTEXITCODE" }

New-Item -ItemType Directory -Path $testOutputDirectory -Force | Out-Null
Copy-Item -LiteralPath $appOutput -Destination $testOutputDirectory
& $csc /nologo /target:exe "/out:$testExecutable" "/reference:$appOutput" `
    /reference:System.Xml.dll /reference:System.Xml.Linq.dll `
    /reference:System.Windows.Forms.dll /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.DataVisualization.dll `
    (Join-Path $PSScriptRoot 'WeatherToolsSmokeTests.cs')
if ($LASTEXITCODE -ne 0) { throw "Smoke test compilation failed with exit code $LASTEXITCODE" }

Push-Location $repositoryRoot
try {
    & $testExecutable $repositoryRoot
    if ($LASTEXITCODE -ne 0) { throw "Smoke tests failed with exit code $LASTEXITCODE" }
}
finally {
    Pop-Location
}
