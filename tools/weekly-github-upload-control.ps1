$ErrorActionPreference = "Stop"

$RepoRoot = "D:\UINya\Godot_v4.5.1-stable_mono_win64\komeiji-koishi"
$TaskName = "Komeiji Koishi Weekly GitHub Upload"
$UploadScript = Join-Path $RepoRoot "tools\weekly-github-upload.ps1"
$RegisterScript = Join-Path $RepoRoot "tools\register-weekly-github-upload-task.ps1"
$LogPath = Join-Path $env:LOCALAPPDATA "KomeijiKoishi\weekly-upload.log"

function Pause-Menu {
    Write-Host ""
    Read-Host "Press Enter to return"
}

function Invoke-MenuCommand {
    param([scriptblock]$Command)

    try {
        & $Command
    }
    catch {
        Write-Host ""
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }

    Pause-Menu
}

function Show-TaskStatus {
    $task = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
    if ($null -eq $task) {
        Write-Host "Task not found: $TaskName" -ForegroundColor Yellow
        return
    }

    $info = Get-ScheduledTaskInfo -TaskName $TaskName
    Write-Host "Task: $TaskName"
    Write-Host "State: $($task.State)"
    Write-Host "Last run: $($info.LastRunTime)"
    Write-Host "Last result: $($info.LastTaskResult)"
    Write-Host "Next run: $($info.NextRunTime)"
}

function Show-Log {
    if (-not (Test-Path -LiteralPath $LogPath)) {
        Write-Host "No log yet: $LogPath" -ForegroundColor Yellow
        return
    }

    Get-Content -LiteralPath $LogPath -Tail 100
}

while ($true) {
    Clear-Host
    Write-Host "Komeiji Koishi GitHub Upload Control" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1. Show scheduled task status"
    Write-Host "2. Dry run upload script (no commit, no push)"
    Write-Host "3. Upload now (commit + push)"
    Write-Host "4. Register/update Sunday 12:00 task"
    Write-Host "5. Disable task"
    Write-Host "6. Enable task"
    Write-Host "7. Delete task"
    Write-Host "8. Show recent log"
    Write-Host "0. Exit"
    Write-Host ""

    $choice = Read-Host "Choose"

    switch ($choice) {
        "1" {
            Invoke-MenuCommand { Show-TaskStatus }
        }
        "2" {
            Invoke-MenuCommand {
                powershell -NoProfile -ExecutionPolicy Bypass -File $UploadScript -DryRun
            }
        }
        "3" {
            Invoke-MenuCommand {
                $confirm = Read-Host "Type YES to commit and push to GitHub now"
                if ($confirm -ne "YES") {
                    Write-Host "Canceled."
                    return
                }

                powershell -NoProfile -ExecutionPolicy Bypass -File $UploadScript
            }
        }
        "4" {
            Invoke-MenuCommand {
                powershell -NoProfile -ExecutionPolicy Bypass -File $RegisterScript
            }
        }
        "5" {
            Invoke-MenuCommand {
                Disable-ScheduledTask -TaskName $TaskName
                Write-Host "Task disabled."
            }
        }
        "6" {
            Invoke-MenuCommand {
                Enable-ScheduledTask -TaskName $TaskName
                Write-Host "Task enabled."
            }
        }
        "7" {
            Invoke-MenuCommand {
                $confirm = Read-Host "Type YES to delete the scheduled task"
                if ($confirm -ne "YES") {
                    Write-Host "Canceled."
                    return
                }

                Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
                Write-Host "Task deleted."
            }
        }
        "8" {
            Invoke-MenuCommand { Show-Log }
        }
        "0" {
            break
        }
        default {
            Write-Host "Unknown option." -ForegroundColor Yellow
            Start-Sleep -Seconds 1
        }
    }
}
