Write-Host "Creating virtual MIDI ports to run devices tests..."
$ErrorActionPreference = "Stop"
$currentDirectory = Get-Location
Write-Host "Current directory is '$currentDirectory', switching to '$PSScriptRoot'..."
cd $PSScriptRoot
$wshell = New-Object -ComObject wscript.shell

Write-Host "Installing loopMIDI..."
Start-Process ../Tools/loopMIDISetup.exe -NoNewWindow -Wait -ArgumentList "/quiet"
$runPath = (Get-ItemProperty HKCU:\Software\Microsoft\Windows\CurrentVersion\Run -Name "loopMIDI").loopMIDI

Write-Host "Running loopMIDI..."
& $runPath
sleep 1

Write-Host "Destroying existing MIDI ports..."
$wshell.AppActivate('loopMIDI') | Out-Null
sleep 1
$wshell.SendKeys('+{TAB}+{TAB}{ENTER}{ENTER}')
Stop-Process -Name "loopMIDI"

Write-Host "Rerunning loopMIDI..."
& $runPath
sleep 1

Write-Host "Creating MIDI ports..."
$wshell.AppActivate('loopMIDI') | Out-Null
sleep 1
$wshell.SendKeys('{TAB}MIDI A{TAB}{TAB}{TAB}{ENTER}{TAB}{TAB}MIDI B{TAB}{TAB}{TAB}{ENTER}{TAB}{TAB}MIDI C{TAB}{TAB}{TAB}{ENTER}')

Write-Host "Switching back to '$currentDirectory'..."
cd $currentDirectory
Write-Host "MIDI ports are successfully created." -ForegroundColor Green