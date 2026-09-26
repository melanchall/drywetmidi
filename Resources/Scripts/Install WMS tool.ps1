param (
  [Parameter(Mandatory)]
  [string]$workingDirectory,
  [Parameter(Mandatory)]
  [string]$installerUrl)

Write-Host "Working directory: $workingDirectory"

$installerLocalPath = Join-Path "$workingDirectory" "Installer.exe"

Write-Host "----------------------------"

Write-Host "Downloading tool from '$installerUrl' to '$installerLocalPath'..."
Invoke-WebRequest -Uri "$installerUrl" -OutFile "$installerLocalPath"

if (-not (Test-Path $installerLocalPath))
{
    Write-Error "Failed to download the installer."
    exit 1
}

Write-Host "Downloaded."

Write-Host "----------------------------"

$installationLogPath = Join-Path "$workingDirectory" "install.log"

Write-Host "Installing..."

$proc = Start-Process -FilePath "$installerLocalPath" -ArgumentList  "/install", "/quiet", "/norestart", "/log", "$installationLogPath" -Wait -PassThru
Write-Host "Installed ($($proc.ExitCode))."

Write-Host "----------------------------"

Write-Host "Printing installation log..."
$content = Get-Content -Path "$installationLogPath" -Raw
$content