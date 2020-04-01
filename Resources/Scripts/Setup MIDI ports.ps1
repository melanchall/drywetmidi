$location = Get-Location

Write-Host "Downloading virtualMIDI SDK..."
Invoke-WebRequest -Uri "http://www.tobias-erichsen.de/wp-content/uploads/2020/01/teVirtualMIDISDKSetup_1_3_0_43.zip" -OutFile "teVirtualMIDISDKSetup.zip"
Write-Host "Downloaded."

Write-Host "Extracting virtualMIDI SDK installer..."
Expand-Archive -LiteralPath 'teVirtualMIDISDKSetup.zip' -DestinationPath "VirtualMIDISDKSetup"
Write-Host "Extracted."

$installer = Get-ChildItem -Path "VirtualMIDISDKSetup" -File -Filter "*.exe"

Write-Host "Installing virtualMIDI SDK..."
Start-Process "VirtualMIDISDKSetup/$installer" -NoNewWindow -Wait -ArgumentList "/quiet"
Write-Host "Installed."

Write-Host "Building CreateLoopbackPort..."
dotnet publish "Resources/Utilities/CreateLoopbackPort/CreateLoopbackPort.sln" -c Release -r win10-x64 -o "$location/CreateLoopbackPort"
Write-Host "Built."

$ports = "MIDI A", "MIDI B", "MIDI C"

ForEach ($port in $ports)
{
  Write-Host "Running $port port..."
  Start-Process "CreateLoopbackPort/CreateLoopbackPort.exe" -ArgumentList """$port"""
  Write-Host "$port is up."
}