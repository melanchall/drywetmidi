param (
  [string]$packageDescription = "Nativeless version of the Melanchall.DryWetMidi")

$relativeCsprojPath = "DryWetMidi\Melanchall.DryWetMidi.csproj"

if (-not (Test-Path $relativeCsprojPath)) {
    Write-Host "Looks like the script has been called not from the root of the repo. Please switch to it and call the script."
    exit 1
}

Write-Host "Modifying csproj..."

$encoding = [System.Text.Encoding]::UTF8
$filePath = Resolve-Path $relativeCsprojPath
$csproj = [xml]([System.IO.File]::ReadAllText($filePath, $encoding))

Write-Host "Setting package metadata values..."        
$packageGroup = (Select-Xml -Xml $csproj.Project -XPath "//*[@Label='Package']").Node
Write-Host "Current 'Package' group:"
Write-Host ($packageGroup | Format-Table | Out-String)
$packageGroup.PackageId = "Melanchall.DryWetMidi.Nativeless"
$packageGroup.Description = $packageDescription
$readmeNode = (Select-Xml -Xml $packageGroup -XPath "//PackageReadmeFile").Node
$packageGroup.RemoveChild($readmeNode)
Write-Host "New 'Package' group:"
Write-Host ($packageGroup | Format-Table | Out-String)
            
$nativeGroup = (Select-Xml -Xml $csproj.Project -XPath "//*[@Label='Native']").Node
Write-Host "'Native' group will be removed:"
Write-Host ($nativeGroup | Format-Table | Out-String)
$csproj.Project.RemoveChild($nativeGroup)

Write-Host "Removing README pack instruction..."
$resourcesGroup = (Select-Xml -Xml $csproj.Project -XPath "//*[@Label='Resources']").Node
Write-Host "'Resources' group:"
Write-Host ($resourcesGroup | Format-Table | Out-String)
$readmeNode = (Select-Xml -Xml $resourcesGroup -XPath "//*[contains(@Include,'README')]").Node
Write-Host "'README' node:"
Write-Host ($readmeNode | Format-Table | Out-String)
$resourcesGroup.RemoveChild($readmeNode)
            
$csproj.Save($filePath)

Write-Host "Setting icon path..."
(Get-Content $filePath) -replace 'icon.png', 'icon-nativeless.png' | Set-Content $filePath

Write-Host "Deleting sources with native dependencies..."

$filesToDelete = @(
    "DryWetMidi\Multimedia\VirtualDevice",
    "DryWetMidi\Multimedia\DevicesWatcher",
    "DryWetMidi\Multimedia\Session",
    "DryWetMidi\Multimedia\Common",
    "DryWetMidi\Multimedia\Native\NativeApi.cs",
    "DryWetMidi\Multimedia\Native\NativeApiUtilities.cs",
    "DryWetMidi\Multimedia\Native\NativeHandle.cs",
    "DryWetMidi\Multimedia\MidiDevice.cs",

    "DryWetMidi\Multimedia\InputDevice\InputDevice.cs",
    "DryWetMidi\Multimedia\InputDevice\InputDeviceHandle.cs",
    "DryWetMidi\Multimedia\InputDevice\InputDeviceApi.cs",
    "DryWetMidi\Multimedia\InputDevice\InputDeviceCheckpointsNames.cs",
    "DryWetMidi\Multimedia\InputDevice\InputDeviceProperty.cs",
    "DryWetMidi\Multimedia\InputDevice\MidiTimeCodeReceivedEventArgs.cs",

    "DryWetMidi\Multimedia\OutputDevice\OutputDevice.cs",
    "DryWetMidi\Multimedia\OutputDevice\OutputDeviceHandle.cs",
    "DryWetMidi\Multimedia\OutputDevice\OutputDeviceApi.cs",
    "DryWetMidi\Multimedia\OutputDevice\OutputDeviceCheckpointsNames.cs",
    "DryWetMidi\Multimedia\OutputDevice\OutputDeviceOption.cs",
    "DryWetMidi\Multimedia\OutputDevice\OutputDeviceProperty.cs",
    "DryWetMidi\Multimedia\OutputDevice\OutputDeviceTechnology.cs",

    "DryWetMidi\Multimedia\Clock\TickGenerator\Session",
    "DryWetMidi\Multimedia\Clock\TickGenerator\TickGeneratorApi.cs",
    "DryWetMidi\Multimedia\Clock\TickGenerator\TickGeneratorException.cs",
    "DryWetMidi\Multimedia\Clock\TickGenerator\HighPrecisionTickGenerator.cs"
)

foreach($filePath in $filesToDelete) {      
    Write-Host "Deleting '$filePath'..." -NoNewline
      
    $filePath = Resolve-Path $filePath
    Remove-Item -Path $filePath -Force -Recurse
      
    Write-Host "OK"
}
      
Write-Host "Replacing HighPrecisionTickGenerator with RegularPrecisionTickGenerator..."
      
$clockSettingsFilePath = Resolve-Path "DryWetMidi\Multimedia\Clock\MidiClockSettings.cs"
(Get-Content $clockSettingsFilePath) -replace 'HighPrecisionTickGenerator', 'RegularPrecisionTickGenerator' | Set-Content $clockSettingsFilePath