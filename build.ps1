param([string]$GameDirectory = 'C:\SteamLibrary\steamapps\common\Pummel Party')

$ErrorActionPreference = 'Stop'
$projectPath = Join-Path $PSScriptRoot 'SchummelPartie\SchummelPartie.csproj'
& dotnet build $projectPath -c Release "-p:GameDirectory=$GameDirectory" --nologo -v minimal
if ($LASTEXITCODE -ne 0) { throw 'Mod build failed.' }

& dotnet run --project (Join-Path $PSScriptRoot 'tests\RuntimeChecks\RuntimeChecks.csproj') -c Release --nologo
if ($LASTEXITCODE -ne 0) { throw 'Runtime checks failed.' }
& (Join-Path $PSScriptRoot 'tests\Verify-Artifact.ps1') -GameDirectory $GameDirectory

$artifactDirectory = Join-Path $PSScriptRoot 'artifacts'
New-Item -ItemType Directory -Path $artifactDirectory -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'SchummelPartie\bin\Release\net48\SchummelPartie.dll') -Destination (Join-Path $artifactDirectory 'SchummelPartie.dll') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.zh-CN.md') -Destination (Join-Path $artifactDirectory 'README.zh-CN.md') -Force
Get-FileHash -LiteralPath (Join-Path $artifactDirectory 'SchummelPartie.dll') -Algorithm SHA256 | Format-List
