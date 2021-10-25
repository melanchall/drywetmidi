Write-Host "Copying README.md as index.md to Docs folder..."
Copy-Item README.md -Destination Docs\index.md

Write-Host "Writing index.md title..."
$indexContent = Get-Content -Path "Docs\index.md" -Raw
$indexContent = $indexContent -replace '\<\!--OVERVIEW--\>',"# Overview"
Set-Content -Path "Docs\index.md" -Value $indexContent