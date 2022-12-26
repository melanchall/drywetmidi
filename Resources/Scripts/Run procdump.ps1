$location = Get-Location

Write-Host "Building RunProcdump..."
dotnet publish "Resources/Utilities/RunProcdump/RunProcdump.sln" -c Release -r win10-x64 -o "$location/RunProcdump"
Write-Host "Built."

Write-Host "Running RunProcdump..."
Start-Process "RunProcdump/RunProcdump.exe"