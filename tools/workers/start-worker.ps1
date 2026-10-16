# Starts one After Effects worker on this Windows host and keeps it running: when it quits, idle or asked to, it comes back fresh.
# The worker names itself from SNAIL_BRIDGE_WORKER, dials every server announced under the state folder (the server in Docker among them)
# and the server adopts it. Run one copy per worker, as a scheduled task that starts at logon:
#   Register-ScheduledTask -TaskName "Snail AE worker w1" -Trigger (New-ScheduledTaskTrigger -AtLogOn) `
#       -Action (New-ScheduledTaskAction -Execute powershell.exe -Argument "-NoProfile -ExecutionPolicy Bypass -File C:\path\to\start-worker.ps1 w1")
param(
    [string] $Name = 'w1',
    [int] $IdleMinutes = 15,
    [string] $State = $(if ($env:SNAIL_BRIDGE_DATA_ROOT) { $env:SNAIL_BRIDGE_DATA_ROOT } elseif (Test-Path (Join-Path $env:USERPROFILE '.s3l-mcp-aftereffects')) { Join-Path $env:USERPROFILE '.s3l-mcp-aftereffects' } else { Join-Path $env:USERPROFILE '.snail-mcp-aftereffects' }),
    [string] $AfterEffects = $env:AFTER_EFFECTS,
    [switch] $WithInterface
)
$ErrorActionPreference = 'Stop'

if (-not $AfterEffects) {
    $install = Get-ChildItem (Join-Path $env:ProgramFiles 'Adobe') -Directory -Filter 'Adobe After Effects *' -ErrorAction SilentlyContinue | Sort-Object Name -Descending | Select-Object -First 1
    if ($install) { $AfterEffects = Join-Path $install.FullName 'Support Files\AfterFX.exe' }
}

if (-not $AfterEffects -or -not (Test-Path $AfterEffects)) {
    throw "No AfterFX.exe at '$AfterEffects'; pass -AfterEffects or set AFTER_EFFECTS."
}

$arguments = @('-m')
if (-not $WithInterface) { $arguments += '-noui' }

Write-Host "worker $Name`: $AfterEffects $($arguments -join ' '), state $State, idle $IdleMinutes min"

while ($true) {
    $env:SNAIL_BRIDGE_WORKER = $Name
    $env:SNAIL_BRIDGE_IDLE_MINUTES = "$IdleMinutes"
    $env:SNAIL_BRIDGE_DATA_ROOT = $State
    $process = Start-Process -FilePath $AfterEffects -ArgumentList $arguments -PassThru
    $process.WaitForExit()
    Write-Host "worker $Name exited with $($process.ExitCode); starting again in 5 s"
    Start-Sleep -Seconds 5
}
