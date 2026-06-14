param (
  [Parameter(Mandatory)]
  [string]$portsNames,
  [Parameter(Mandatory)]
  [string]$arch)

$location = Get-Location
Write-Host "Current location: $location"

####

# Set error handling preference
$ErrorActionPreference = "Stop"

$targetPath = Join-Path $location "RuntimeAndToolsInstaller.exe"
Write-Host "Downloading Windows MIDI Services SDK Runtime and Tools installer to $targetPath..."

# Source variables
Invoke-WebRequest -Uri "https://github.com/microsoft/MIDI/releases/download/rc-4/Windows.MIDI.Services.SDK.Runtime.and.Tools.1.0.17-rc.4.25-arm64.exe" -OutFile "$targetPath"
Write-Host "Downloaded."

if (-not (Test-Path $targetPath))
{
    Write-Error "Failed to download the installer."
    exit 1
}

Write-Host "Installing and waiting..."
& "$targetPath" /install /quiet /norestart /log "$location\install.log"
Start-Sleep -Seconds 120
Write-Host "Probably installed..."

$content = Get-Content -Path "$location\install.log" -Raw
Write-Host "Installation log content: $content"

$env:PATH += ";C:\Program Files\Windows MIDI Services\Tools\Console"

###########

$targetPath = Join-Path $location "BasicLoopbackInstaller.exe"
Write-Host "Downloading basic loopback plugin installer to $targetPath..."

Invoke-WebRequest -Uri "https://github.com/microsoft/MIDI/releases/download/rc-3/Windows.MIDI.Services.Basic.MIDI.1.0.Loopback.Preview.1.0.0-preview.1.2-arm64.exe" -OutFile "$targetPath"
Write-Host "Downloaded."

if (-not (Test-Path $targetPath))
{
    Write-Error "Failed to download the installer."
    exit 1
}

Write-Host "Installing and waiting..."
& "$targetPath" /install /quiet /norestart /log "$location\install2.log"
Start-Sleep -Seconds 120
Write-Host "Probably installed..."

$content = Get-Content -Path "$location\install2.log" -Raw
Write-Host "Installation log content: $content"

###########

ForEach ($port in $ports)
{
  Write-Host "Running $port port..."
  midi basic-loopback create --name "$port"
  Write-Host "$port is up."
}

####

#Write-Host "Checking Audio/MIDI service status..."
#$serviceName = "Audiosrv"
#$service = Get-Service -Name "$serviceName"
#
#if ($service.Status -ne 'Running')
#{
#  Write-Host "Audio/MIDI service is not started; starting..."
#  Start-Service "$serviceName"
#}
#
#Write-Host "Audio/MIDI service is started"
#
#Write-Host "Downloading virtualMIDI SDK..."
#$ProgressPreference = 'SilentlyContinue'
#Invoke-WebRequest -Uri "http://www.tobias-erichsen.de/wp-content/uploads/2020/01/teVirtualMIDISDKSetup_1_3_0_43.zip" -OutFile "$location\teVirtualMIDISDKSetup.zip"
#Write-Host "Downloaded."
#
#Write-Host "Extracting virtualMIDI SDK installer..."
#Expand-Archive -LiteralPath "$location\teVirtualMIDISDKSetup.zip" -DestinationPath "$location\VirtualMIDISDKSetup"
#Write-Host "Extracted."
#
#$installer = Get-ChildItem -Path "$location\VirtualMIDISDKSetup" -File -Filter "*.exe"
#
#Write-Host "Installing virtualMIDI SDK..."
#Start-Process -FilePath $installer.FullName -NoNewWindow -Wait -ArgumentList "/quiet"
#Write-Host "Installed."
#
#Write-Host "Building CreateLoopbackPort..."
#dotnet publish "$location/Resources/Utilities/CreateLoopbackPort_Windows/CreateLoopbackPort.sln" -c Release -r "win-$arch" -o "$location/CreateLoopbackPort"
#Write-Host "Built."
#
#Write-Host "Ports names string: $portsNames"
#$ports = $portsNames.Split(',')
#
#ForEach ($port in $ports)
#{
#  Write-Host "Running $port port..."
#  Start-Process "$location/CreateLoopbackPort/CreateLoopbackPort.exe" -ArgumentList """$port"""
#  Write-Host "$port is up."
#}