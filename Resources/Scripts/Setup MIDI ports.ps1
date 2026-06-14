param (
  [Parameter(Mandatory)]
  [string]$portsNames,
  [Parameter(Mandatory)]
  [string]$arch)

$location = Get-Location
Write-Host "Current location: $location"

####

#
$ErrorActionPreference = "Stop"

#
$url = "https://aka.ms/MidiServicesLatestSdkRuntimeInstaller_DirectArm64"
$tempDir = [System.IO.Path]::GetTempPath()

Write-Host "1..." -ForegroundColor Cyan

try {
    #
    $request = [System.Net.WebRequest]::Create($url)
    $request.Method = "HEAD"
    $response = $request.GetResponse()
    $realUri = $response.ResponseUri.AbsoluteUri
    $response.Close()
    
    #
    $fileName = [System.IO.Path]::GetFileName($realUri)
    
    #
    if ([string]::IsNullOrWhiteSpace($fileName) -or $fileName -notlike "*.exe*") {
        $fileName = "MidiServicesSdkRuntimeInstaller_Arm64.exe"
    }
}
catch {
    Write-Warning "ERR 1"
    $fileName = "MidiServicesSdkRuntimeInstaller_Arm64.exe"
}

$destinationPath = Join-Path $tempDir $fileName
Write-Host "File will be saved as: $fileName" -ForegroundColor Gray

Write-Host "2..." -ForegroundColor Cyan
#
Invoke-WebRequest -Uri $url -OutFile $destinationPath -UseBasicParsing

Write-Host "3..." -ForegroundColor Cyan
$installArgs = "/install /quiet /norestart"

#
$process = Start-Process -FilePath $destinationPath -ArgumentList $installArgs -NoNewWindow -PassThru -Wait

#
if ($process.ExitCode -eq 0 -or $process.ExitCode -eq 3010) {
    Write-Host "Success: $($process.ExitCode)" -ForegroundColor Green
} else {
    Write-Error "Failed: $($process.ExitCode)"
}

#
if (Test-Path $destinationPath) {
    Remove-Item $destinationPath -Force
    Write-Host "4..." -ForegroundColor Gray
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