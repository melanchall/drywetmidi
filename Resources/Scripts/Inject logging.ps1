$files = Get-ChildItem -Path "DryWetMidi" -Filter "*.cs" -Recurse -Force
$encoding = [System.Text.Encoding]::UTF8

foreach ($file in $files)
{
	Write-Host "Inject logging in $file..."
  
	$content = [System.IO.File]::ReadAllText($file.FullName, $encoding)
	$matched = $content -match "// LOGREAD:"

	if ($matched)
	{
		$content = $content -Replace "// LOGREAD: ([\w \-\+_<\{\.\}>\(\)]+)","Melanchall.DryWetMidi.Common.Logger.Instance.Write(`"readlog.log`", $`"## [{reader.Position}] `$1 ##`");"
		$content = $content -Replace "// LOGREADEND","Melanchall.DryWetMidi.Common.Logger.Instance.Dispose();"
		Write-Host "    writing updated content..." -NoNewLine
		[System.IO.File]::WriteAllText($file.FullName, $content, $encoding)
		Write-Host "OK"
	}
	else
	{
		Write-Host "    skipped"
	}
}