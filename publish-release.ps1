$ErrorActionPreference = "Stop"

$root = $PSScriptRoot
$project = Join-Path $root "EasyShell\EasyShell.csproj"
$dist = Join-Path $root "dist"
$runtimeRequiredDir = Join-Path $dist "runtime-required"
$standaloneDir = Join-Path $dist "standalone"

[xml]$projectXml = Get-Content $project
$version = $projectXml.Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($version)) {
    $version = "0.0.0"
}

if (Test-Path $dist) {
    $resolvedDist = (Resolve-Path $dist).Path
    $rootWithSeparator = $root.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    if (-not $resolvedDist.StartsWith($rootWithSeparator, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to delete outside workspace: $resolvedDist"
    }

    Remove-Item -LiteralPath $resolvedDist -Recurse -Force
}

dotnet publish $project -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true /p:PublishDir="..\dist\runtime-required\"
Rename-Item -LiteralPath (Join-Path $runtimeRequiredDir "EasyShell.exe") -NewName "EasyShell-v$version-runtime-required.exe"
Remove-Item -LiteralPath (Join-Path $runtimeRequiredDir "EasyShell.pdb") -Force -ErrorAction SilentlyContinue

dotnet publish $project -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishDir="..\dist\standalone\"
Rename-Item -LiteralPath (Join-Path $standaloneDir "EasyShell.exe") -NewName "EasyShell-v$version-standalone.exe"
Remove-Item -LiteralPath (Join-Path $standaloneDir "EasyShell.pdb") -Force -ErrorAction SilentlyContinue

Get-ChildItem -LiteralPath $dist -Recurse -File | Select-Object FullName, Length
