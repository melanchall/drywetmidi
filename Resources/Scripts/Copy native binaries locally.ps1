param (
  [Parameter(Mandatory)]
  [string]$fromDirectory)

Write-Host "Copying x64 dll..."
$winX64Destination = "Native\win_x64"
New-Item -ItemType Directory -Force -Path $winX64Destination
Copy-Item -Path "$fromDirectory/Native_Win_x64/Melanchall_DryWetMidi_Native.dll" -Destination "$winX64Destination" -Force -Verbose

Write-Host "Copying arm64 dll..."
$winArm64Destination = "Native\win_arm64"
New-Item -ItemType Directory -Force -Path $winArm64Destination
Copy-Item -Path "$fromDirectory/Native_Win_arm64/Melanchall_DryWetMidi_Native.dll" -Destination "$winArm64Destination" -Force -Verbose

Write-Host "Copying x64/arm64 dylib..."
$macosX64Arm64Destination = "Native\macos_x64_arm64"
New-Item -ItemType Directory -Force -Path $macosX64Arm64Destination
Copy-Item -Path "$fromDirectory/Native_macOS_x64_arm64/Melanchall_DryWetMidi_Native.dylib" -Destination "$macosX64Arm64Destination" -Force -Verbose