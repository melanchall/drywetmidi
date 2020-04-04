param ($Folder, $Filter)

$files = Get-ChildItem -Path $Folder -Filter $Filter -Recurse -Force

foreach ($file in $files)
{
  Write-Host "Searching for links in $file..."
  $matches = Select-String -Path $file -Pattern 'https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9@:%_\+.~#?&\/=]*)' -AllMatches | Foreach-Object { $_.Matches }
  
  Write-Host "    Checking links..."
  foreach ($match in $matches)
  {
    if (-Not $match.Success)
    {
      continue
    }
    
    try
    {    
      $linkUrl = $match.Value
      if ($linkUrl -Match "melanchall\/drywetmidi\/wiki")
      {
        Write-Host "##vso[task.logissue type=error]    FAILED since Wiki link is prohibited"
        Write-Host "##vso[task.complete result=Failed;]    FAILED"
      }
      
      Write-Host "    Checking '$linkUrl'"
      
      $HTTP_Request = [System.Net.WebRequest]::Create($linkUrl)
      $HTTP_Response = $HTTP_Request.GetResponse()
      $HTTP_Status = [int]$HTTP_Response.StatusCode
      
      If ($HTTP_Status -eq 200) {
        Write-Host "    OK"
      }
      Else {
        Write-Host "##vso[task.logissue type=error]    FAILED with status $HTTP_Status"
        Write-Host "##vso[task.complete result=Failed;]    FAILED"
      }
      
      $HTTP_Response.Close()
    }
    catch
    {
      $catchedException = $_.Exception
      Write-Host "##vso[task.logissue type=error]    FAILED by exception $catchedException"
      Write-Host "##vso[task.complete result=Failed;]    FAILED"
    }
  }
}