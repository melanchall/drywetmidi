$encoding = [System.Text.Encoding]::UTF8

Write-Host "Loading xrefmap..."

$path = Resolve-Path "Docs/_site/xrefmap.yml"
$xrefmap = [System.IO.File]::ReadAllText($path, $encoding)
$refs = [regex]::Matches($xrefmap, "\- uid: ([\w]+).+?href: ([\w\/\.-]+)", [Text.RegularExpressions.RegexOptions]'Singleline') `
    | Where-Object {$_.Groups[1].Value -match "^a_"} `

Write-Host "Processing source files..."

$files = Get-ChildItem -Path "DryWetMidi" -Filter "*.cs" -Recurse -Force

foreach ($file in $files)
{
    Write-Host "Searching for links in '$file'..."

    $content = [System.IO.File]::ReadAllText($file.FullName, $encoding)
    $matched = $False

    foreach ($ref in $refs)
    {
        $uid = $ref.Groups[1].Value
        if ($content -match "xref\:$uid")
        {
            $matched = $True
            $link = "https://melanchall.github.io/drywetmidi/$($ref.Groups[2].Value)"
            $content = $content -replace "xref\:$uid","$link"
            Write-Host "    replaced 'xref:$uid' to '$link'"
        }
    }

    if ($matched)
    {
        Write-Host "    writing updated content..." -NoNewLine
        [System.IO.File]::WriteAllText($file.FullName, $content, $encoding)
        Write-Host "OK"
    }
}