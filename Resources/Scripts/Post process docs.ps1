$files = Get-ChildItem -Path "Docs/_site" -Filter "*.html" -Recurse -Force
$encoding = [System.Text.Encoding]::UTF8

foreach ($file in $files)
{
    Write-Host "Searching for pattern in $file..."

    $content = [System.IO.File]::ReadAllText($file.FullName, $encoding)
    $matched = $False

    if ($content -match "<pre><code class=`"lang-image`"")
    {
        $matched = $True
        $content = $content -replace "<pre><code class=`"lang-image`"","<pre class=`"text-image`"><code class=`"lang-image`""
    }

    if ($content -match "·+")
    {
        $matched = $True
        $content = $content -replace "(·+)",'<span class="outer-block">$1</span>'
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