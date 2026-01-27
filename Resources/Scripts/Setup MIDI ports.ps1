param (
  [Parameter(Mandatory)]
  [string]$portsNames,
  [Parameter(Mandatory)]
  [string]$arch)

$location = Get-Location
Write-Host "Current location: $location"

Write-Host "Checking Audio/MIDI service status..."
$serviceName = "Audiosrv"
$service = Get-Service -Name "$serviceName"

if ($service.Status -ne 'Running')
{
  Write-Host "Audio/MIDI service is not started; starting..."
  Start-Service "$serviceName"
}

Write-Host "Audio/MIDI service is started"

Write-Host "Downloading virtualMIDI SDK..."
$ProgressPreference = 'SilentlyContinue'
Invoke-WebRequest -Uri "http://www.tobias-erichsen.de/wp-content/uploads/2020/01/teVirtualMIDISDKSetup_1_3_0_43.zip" -OutFile "$location\teVirtualMIDISDKSetup.zip"
Write-Host "Downloaded."

Write-Host "Extracting virtualMIDI SDK installer..."
Expand-Archive -LiteralPath "$location\teVirtualMIDISDKSetup.zip" -DestinationPath "$location\VirtualMIDISDKSetup"
Write-Host "Extracted."

$installer = Get-ChildItem -Path "$location\VirtualMIDISDKSetup" -File -Filter "*.exe"

Write-Host "Installing virtualMIDI SDK..."
Start-Process -FilePath $installer.FullName -NoNewWindow -Wait -ArgumentList "/quiet"
Write-Host "Installed."

Write-Host "Building CreateLoopbackPort..."
dotnet publish "$location/Resources/Utilities/CreateLoopbackPort_Windows/CreateLoopbackPort.sln" -c Release -r "win-$arch" -o "$location/CreateLoopbackPort"
Write-Host "Built."

Write-Host "Ports names string: $portsNames"
$ports = $portsNames.Split(',')

ForEach ($port in $ports)
{
  Write-Host "Running $port port..."
  Start-Process "$location/CreateLoopbackPort/CreateLoopbackPort.exe" -ArgumentList """$port"""
  Write-Host "$port is up."
}