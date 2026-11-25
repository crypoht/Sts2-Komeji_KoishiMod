param(
    [switch]$DryRun,
    [string]$RepoRoot = "D:\UINya\Godot_v4.5.1-stable_mono_win64\komeiji-koishi",
    [string]$Branch = "main",
    [string]$Remote = "origin",
    [string]$Message = "weekly auto update"
)

$ErrorActionPreference = "Stop"

function Write-Log {
    param([string]$Text)

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $line = "[$timestamp] $Text"
    Write-Host $line

    $logDir = Join-Path $env:LOCALAPPDATA "KomeijiKoishi"
    if (-not (Test-Path -LiteralPath $logDir)) {
        New-Item -ItemType Directory -Force -Path $logDir | Out-Null
    }

    Add-Content -LiteralPath (Join-Path $logDir "weekly-upload.log") -Value $line -Encoding UTF8
}

function Invoke-Git {
    param([string[]]$Arguments)

    Write-Log ("git " + ($Arguments -join " "))
    & git -C $RepoRoot @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "git command failed with exit code $LASTEXITCODE"
    }
}

if (-not (Test-Path -LiteralPath (Join-Path $RepoRoot ".git"))) {
    throw "RepoRoot is not a git repository: $RepoRoot"
}

$currentBranch = (& git -C $RepoRoot branch --show-current).Trim()
if ($LASTEXITCODE -ne 0) {
    throw "Unable to read current git branch."
}

if ($currentBranch -ne $Branch) {
    Write-Log "Skipped: current branch is '$currentBranch', expected '$Branch'."
    exit 0
}

$allowedPathspecs = @(
    ":(glob)Scripts/**/*.cs",
    ":(glob)Utils_Koishi/**/*.cs",
    ":(glob)Komeiji_Koishi/**/*.json",
    "Komeiji_Koishi.json",
    ":(glob)*.csproj"
)

Write-Log "Weekly GitHub upload started. DryRun=$DryRun"

Invoke-Git -Arguments (@("add", "--") + $allowedPathspecs)

$staged = & git -C $RepoRoot diff --cached --name-status
if ($LASTEXITCODE -ne 0) {
    throw "Unable to inspect staged changes."
}

if ([string]::IsNullOrWhiteSpace($staged)) {
    Write-Log "No allowed source/json changes to upload."
    exit 0
}

Write-Log "Staged changes:"
$staged | ForEach-Object { Write-Log "  $_" }

if ($DryRun) {
    Invoke-Git @("reset", "--quiet")
    Write-Log "Dry run complete. Staged changes were reset; no commit or push was made."
    exit 0
}

Invoke-Git @("commit", "-m", $Message)
Invoke-Git @("push", $Remote, $Branch)

Write-Log "Weekly GitHub upload completed."
