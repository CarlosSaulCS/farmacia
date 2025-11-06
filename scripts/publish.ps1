param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [switch]$SingleFile,
    [switch]$Package
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionRoot = Resolve-Path (Join-Path $root "..")
$projectPath = Join-Path $solutionRoot "Farmacia.UI.Wpf/Farmacia.UI.Wpf.csproj"
$publishRoot = Join-Path $solutionRoot "artifacts/publish/$Runtime"

if (-not (Test-Path $publishRoot)) {
    New-Item -ItemType Directory -Path $publishRoot | Out-Null
}

$singleFileValue = if ($SingleFile.IsPresent) { "true" } else { "false" }
$publishArgs = @(
    "publish",
    $projectPath,
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", "true",
    "-o", $publishRoot,
    "/p:PublishSingleFile=$singleFileValue",
    "/p:IncludeNativeLibrariesForSelfExtract=true",
    "/p:UseAppHost=true",
    "/p:PublishTrimmed=false"
)

Write-Host "dotnet $($publishArgs -join ' ')" -ForegroundColor Cyan
$publishResult = dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed."
}

if ($Package.IsPresent) {
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $artifactName = "Farmacia-$Runtime-$Configuration-$timestamp.zip"
    $artifactPath = Join-Path $publishRoot "..\$artifactName"

    if (Test-Path $artifactPath) {
        Remove-Item $artifactPath
    }

    Write-Host "Creating package $artifactPath" -ForegroundColor Cyan
    Compress-Archive -Path (Join-Path $publishRoot '*') -DestinationPath $artifactPath -Force
}

Write-Host "Publicación completa. Archivos disponibles en $publishRoot" -ForegroundColor Green
