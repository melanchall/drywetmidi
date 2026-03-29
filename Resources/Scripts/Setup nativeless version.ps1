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