$statusContent = Get-Content -Path "Docs\obsolete\status.json" | ConvertFrom-Json
$idsProperties = $statusContent.PSObject.Properties

Write-Host "Generating hints table..."

$hints = @{}

foreach ($idProperty in $idsProperties)
{
    $id = $idProperty.Name
    $inLibrary = $idProperty.Value.InLibrary
    if ($inLibrary -eq $false)
    {
        Write-Host "$id skipped due to API is not in library anymore"
        continue
    }

    $hint = $idProperty.Value.Hint
    $hints.Add($id, $hint)
}

Write-Host "Inserting hints into Obsolete declarations..."

$files = Get-ChildItem -Path "DryWetMidi" -Filter "*.cs" -Recurse -Force
$encoding = [System.Text.Encoding]::UTF8

foreach ($file in $files)
{
    Write-Host "Searching for declaration in $file..."

    $content = [System.IO.File]::ReadAllText($file.FullName, $encoding)
    $matched = $False

    foreach ($id in $hints.Keys)
    {
        if ($content -match "\[Obsolete\(`".+`"\)\]")
        {
            $matched = $True
            $hint = $hints[$id]
            $link = "https://melanchall.github.io/drywetmidi/obsolete/obsolete.html#$($id.ToLower())"
            $content = $content -replace "\[Obsolete\(`"$id`"\)\]","[Obsolete(`"$($id): $hint More info: $link.`")]"
        }
    }

    if ($matched)
    {
        Write-Host "    writing updated content..." -NoNewLine
        [System.IO.File]::WriteAllText($file.FullName, $content, $encoding)
        Write-Host "OK"
    }
    else
    {
        Write-Host "    skipped"
    }
}