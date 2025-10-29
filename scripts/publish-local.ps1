# Publish script for local release artifacts
# Usage: .\scripts\publish-local.ps1 -Version 0.1.0
param(
    [string]$Version = "0.1.0"
)

$proj = "KeyShow.csproj"
$publishDir = Join-Path -Path $PWD -ChildPath "publish"

Write-Host "Publishing version $Version"

# win-x64
$dest = Join-Path $publishDir "KeyShow-$Version-win-x64"
Remove-Item -Recurse -Force $dest -ErrorAction SilentlyContinue
dotnet publish $proj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false -o $dest
Compress-Archive -Path (Join-Path $dest '*') -DestinationPath (Join-Path $publishDir "KeyShow-$Version-win-x64.zip") -Force

# win-arm64
$dest = Join-Path $publishDir "KeyShow-$Version-win-arm64"
Remove-Item -Recurse -Force $dest -ErrorAction SilentlyContinue
dotnet publish $proj -c Release -r win-arm64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false -o $dest
Compress-Archive -Path (Join-Path $dest '*') -DestinationPath (Join-Path $publishDir "KeyShow-$Version-win-arm64.zip") -Force

Write-Host "Published artifacts in $publishDir"
