param(
    [string]$TaskName = "Komeiji Koishi Weekly GitHub Upload",
    [string]$ScriptPath = "D:\UINya\Godot_v4.5.1-stable_mono_win64\komeiji-koishi\tools\weekly-github-upload.ps1"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $ScriptPath)) {
    throw "Upload script not found: $ScriptPath"
}

$action = New-ScheduledTaskAction `
    -Execute "powershell.exe" `
    -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$ScriptPath`""

$trigger = New-ScheduledTaskTrigger -Weekly -DaysOfWeek Sunday -At 12:00
$settings = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -ExecutionTimeLimit (New-TimeSpan -Minutes 30)

Register-ScheduledTask `
    -TaskName $TaskName `
    -Action $action `
    -Trigger $trigger `
    -Settings $settings `
    -Description "Uploads Komeiji Koishi source and JSON changes to GitHub every Sunday at 12:00." `
    -Force

Write-Host "Registered scheduled task: $TaskName"
