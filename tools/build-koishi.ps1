$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ProjectFile = Join-Path $ProjectRoot "Komeiji_Koishi.csproj"
$LoaderProjectFile = Join-Path $ProjectRoot "Komeiji_Koishi.Loader.csproj"
$LibDir = Join-Path $ProjectRoot "lib"
$StableDir = Join-Path $LibDir "build_stable"
$BetaDir = Join-Path $LibDir "build_beta"

function Assert-FileExists {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Missing required file: $Path"
    }
}

function Build-Koishi {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Name,
        [Parameter(Mandatory = $true)]
        [bool] $IsBeta,
        [Parameter(Mandatory = $true)]
        [string] $Sts2DataDir
    )

    Write-Host ""
    Write-Host "=== Building $Name ===" -ForegroundColor Cyan

    $betaValue = if ($IsBeta) { "true" } else { "false" }
    dotnet build $ProjectFile `
        "-p:Sts2Beta=$betaValue" `
        "-p:Sts2DataDir=$Sts2DataDir" `
        "-p:BaseLibDir=$LibDir" `
        -v:minimal

    if ($LASTEXITCODE -ne 0) {
        throw "$Name build failed with exit code $LASTEXITCODE"
    }
}

function Build-Loader {
    Write-Host ""
    Write-Host "=== Building Loader ===" -ForegroundColor Cyan

    dotnet build $LoaderProjectFile `
        "-p:Sts2DataDir=$StableDir" `
        -v:minimal

    if ($LASTEXITCODE -ne 0) {
        throw "Loader build failed with exit code $LASTEXITCODE"
    }
}

Assert-FileExists $ProjectFile
Assert-FileExists $LoaderProjectFile
Assert-FileExists (Join-Path $LibDir "BaseLib.dll")
Assert-FileExists (Join-Path $StableDir "sts2.dll")
Assert-FileExists (Join-Path $StableDir "0Harmony.dll")
Assert-FileExists (Join-Path $BetaDir "sts2.dll")
Assert-FileExists (Join-Path $BetaDir "0Harmony.dll")

Build-Koishi -Name "Stable" -IsBeta $false -Sts2DataDir $StableDir
Build-Koishi -Name "Beta" -IsBeta $true -Sts2DataDir $BetaDir
Build-Loader

Write-Host ""
Write-Host "All builds completed." -ForegroundColor Green
