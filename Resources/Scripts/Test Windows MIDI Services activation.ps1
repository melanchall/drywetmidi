param(
  [Parameter(Mandatory)]
  [string]$testOutputPath
)

function Get-PeMachine([string]$Path)
{
  if (-not (Test-Path $Path))
  {
    return "missing"
  }

  $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
  $reader = $null
  try
  {
    $reader = [System.IO.BinaryReader]::new($stream)
    if ($reader.ReadUInt16() -ne 0x5A4D)
    {
      return "not-pe"
    }

    $stream.Seek(0x3C, [System.IO.SeekOrigin]::Begin) | Out-Null
    $peOffset = $reader.ReadInt32()
    $stream.Seek($peOffset + 4, [System.IO.SeekOrigin]::Begin) | Out-Null

    $stream.Seek($peOffset, [System.IO.SeekOrigin]::Begin) | Out-Null
    if ($reader.ReadUInt32() -ne 0x00004550)
    {
      return "not-pe"
    }

    $machine = $reader.ReadUInt16()

    switch ($machine)
    {
      0x8664 { return "x64" }
      0xAA64 { return "arm64" }
      0xA641 { return "arm64ec" }
      0x014C { return "x86" }
      default { return ('0x{0:X4}' -f $machine) }
    }
  }
  finally
  {
    if ($reader -ne $null)
    {
      $reader.Dispose()
    }

    $stream.Dispose()
  }
}

function Format-HResult([int]$Value)
{
  $unsigned = [uint32]$Value
  return ('0x{0:X8}' -f $unsigned)
}

function Add-CandidatePath([System.Collections.Generic.List[string]]$Candidates, [string]$Path)
{
  if (-not [string]::IsNullOrWhiteSpace($Path) -and -not $Candidates.Contains($Path))
  {
    $Candidates.Add($Path)
  }
}

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public static class WmsActivationProbe
{
    public const uint LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100;
    public const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;
    public const uint RO_INIT_MULTITHREADED = 1;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadLibraryExW(string fileName, IntPtr fileHandle, uint flags);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr GetProcAddress(IntPtr module, string procName);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool FreeLibrary(IntPtr module);

    [DllImport("combase.dll")]
    public static extern int RoInitialize(uint initType);

    [DllImport("combase.dll")]
    public static extern void RoUninitialize();

    [DllImport("combase.dll")]
    public static extern int RoGetActivationFactory(IntPtr activatableClassId, ref Guid iid, ref IntPtr factory);

    [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CharSet = CharSet.Unicode)]
    public static extern int WindowsCreateString(string sourceString, int length, out IntPtr hstring);

    [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll")]
    public static extern int WindowsDeleteString(IntPtr hstring);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int DllGetActivationFactoryDelegate(IntPtr activatableClassId, ref IntPtr factory);

    public static readonly Guid IActivationFactoryIid = new Guid("00000035-0000-0000-C000-000000000046");
}
"@

function New-HString([string]$Value)
{
  $handle = [IntPtr]::Zero
  $utf16CodeUnitCount = [System.Text.Encoding]::Unicode.GetByteCount($Value) / 2
  $result = [WmsActivationProbe]::WindowsCreateString($Value, $utf16CodeUnitCount, [ref]$handle)
  if ($result -lt 0)
  {
    throw "WindowsCreateString failed for '$Value' with $(Format-HResult $result)."
  }

  return $handle
}

function Invoke-RoGetActivationFactory([string]$ClassName)
{
  $hstring = [IntPtr]::Zero
  $factory = [IntPtr]::Zero

  try
  {
    $hstring = New-HString $ClassName
    $iid = [WmsActivationProbe]::IActivationFactoryIid
    $result = [WmsActivationProbe]::RoGetActivationFactory($hstring, [ref]$iid, [ref]$factory)
    return $result
  }
  finally
  {
    if ($factory -ne [IntPtr]::Zero)
    {
      [System.Runtime.InteropServices.Marshal]::Release($factory) | Out-Null
    }

    if ($hstring -ne [IntPtr]::Zero)
    {
      [WmsActivationProbe]::WindowsDeleteString($hstring) | Out-Null
    }
  }
}

function Invoke-DllGetActivationFactory([IntPtr]$Module, [string]$ClassName)
{
  $export = [WmsActivationProbe]::GetProcAddress($Module, "DllGetActivationFactory")
  if ($export -eq [IntPtr]::Zero)
  {
    return $null
  }

  $delegateType = [WmsActivationProbe+DllGetActivationFactoryDelegate]
  $delegate = [System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer($export, $delegateType)
  $hstring = [IntPtr]::Zero
  $factory = [IntPtr]::Zero

  try
  {
    $hstring = New-HString $ClassName
    return $delegate.Invoke($hstring, [ref]$factory)
  }
  finally
  {
    if ($factory -ne [IntPtr]::Zero)
    {
      [System.Runtime.InteropServices.Marshal]::Release($factory) | Out-Null
    }

    if ($hstring -ne [IntPtr]::Zero)
    {
      [WmsActivationProbe]::WindowsDeleteString($hstring) | Out-Null
    }
  }
}

$dotnetPath = (Get-Command dotnet).Source
$workspacePath = $env:GITHUB_WORKSPACE
$candidatePaths = [System.Collections.Generic.List[string]]::new()

Add-CandidatePath $candidatePaths (Join-Path (Get-Location) "Windows.Devices.Midi2.dll")
Add-CandidatePath $candidatePaths (Join-Path $testOutputPath "Windows.Devices.Midi2.dll")

if (-not [string]::IsNullOrWhiteSpace($workspacePath))
{
  Add-CandidatePath $candidatePaths (Join-Path $workspacePath "Windows.Devices.Midi2.dll")
}

if (-not [string]::IsNullOrWhiteSpace($env:ProgramFiles))
{
  Add-CandidatePath $candidatePaths (Join-Path $env:ProgramFiles "Windows MIDI Services\Tools\Console\Windows.Devices.Midi2.dll")
}

$selectedMidi2RuntimePath = $candidatePaths | Where-Object { Test-Path $_ } | Select-Object -First 1

Write-Host "Current directory: $(Get-Location)"
Write-Host "PROCESSOR_ARCHITECTURE: $env:PROCESSOR_ARCHITECTURE"
Write-Host "PROCESSOR_ARCHITEW6432: $env:PROCESSOR_ARCHITEW6432"
Write-Host "dotnet path: $dotnetPath"
Write-Host "dotnet machine: $(Get-PeMachine $dotnetPath)"
dotnet --info

$diagnosticFiles = @(
  $dotnetPath,
  (Join-Path $testOutputPath "Melanchall_DryWetMidi_Native.dll"),
  (Join-Path $testOutputPath "Windows.Devices.Midi2.dll"),
  (Join-Path $testOutputPath "Windows.Devices.Midi2.pri"),
  (Join-Path $testOutputPath "Windows.Devices.Midi2.winmd"),
  (Join-Path $testOutputPath "testhost.exe")
)

if (-not [string]::IsNullOrWhiteSpace($workspacePath))
{
  $diagnosticFiles += @(
    (Join-Path $workspacePath "Melanchall_DryWetMidi_Native.dll"),
    (Join-Path $workspacePath "Windows.Devices.Midi2.dll"),
    (Join-Path $workspacePath "Windows.Devices.Midi2.pri"),
    (Join-Path $workspacePath "Windows.Devices.Midi2.winmd")
  )
}

foreach ($filePath in $diagnosticFiles | Select-Object -Unique)
{
  if (Test-Path $filePath)
  {
    $fileInfo = Get-Item $filePath
    Write-Host "$filePath | size=$($fileInfo.Length) | machine=$(Get-PeMachine $filePath)"
  }
  else
  {
    Write-Host "$filePath | missing"
  }
}

if (Test-Path $testOutputPath)
{
  Get-ChildItem -Path $testOutputPath -File | Select-Object Name, Length
}
else
{
  Write-Host "$testOutputPath | missing directory"
}

Write-Host "Windows.Devices.Midi2.dll candidates:"
foreach ($candidatePath in $candidatePaths)
{
  Write-Host " - $candidatePath"
}

$roInitializationResult = [WmsActivationProbe]::RoInitialize([WmsActivationProbe]::RO_INIT_MULTITHREADED)
Write-Host "RoInitialize result: $(Format-HResult $roInitializationResult)"

$classesToProbe = @(
  "Windows.Devices.Midi2.MidiApi",
  "Windows.Devices.Midi2.Transports.BasicLoopback.MidiBasicLoopbackManager"
)

try
{
  if ($roInitializationResult -lt 0)
  {
    Write-Host "Skipping activation probes because RoInitialize failed."
  }
  else
  {
    foreach ($className in $classesToProbe)
    {
      $result = Invoke-RoGetActivationFactory $className
      Write-Host "RoGetActivationFactory($className): $(Format-HResult $result)"
    }

    if (-not [string]::IsNullOrWhiteSpace($selectedMidi2RuntimePath))
    {
      $module = [WmsActivationProbe]::LoadLibraryExW(
        $selectedMidi2RuntimePath,
        [IntPtr]::Zero,
        [WmsActivationProbe]::LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR -bor [WmsActivationProbe]::LOAD_LIBRARY_SEARCH_DEFAULT_DIRS)

      if ($module -eq [IntPtr]::Zero)
      {
        $lastError = [System.Runtime.InteropServices.Marshal]::GetLastWin32Error()
        Write-Host "LoadLibraryExW($selectedMidi2RuntimePath) failed with Win32 error $lastError."
      }
      else
      {
        Write-Host "Loaded Windows.Devices.Midi2.dll from $selectedMidi2RuntimePath"

        $dllGetActivationFactoryExport = [WmsActivationProbe]::GetProcAddress($module, "DllGetActivationFactory")
        if ($dllGetActivationFactoryExport -eq [IntPtr]::Zero)
        {
          Write-Host "DllGetActivationFactory export is missing."
        }
        else
        {
          Write-Host "DllGetActivationFactory export found."

          foreach ($className in $classesToProbe)
          {
            $result = Invoke-DllGetActivationFactory $module $className
            if ($null -eq $result)
            {
              Write-Host "DllGetActivationFactory($className): export was not found."
            }
            else
            {
              Write-Host "DllGetActivationFactory($className): $(Format-HResult $result)"
            }
          }
        }

        [WmsActivationProbe]::FreeLibrary($module) | Out-Null
      }
    }
    else
    {
      Write-Host "No Windows.Devices.Midi2.dll candidate was found."
    }
  }
}
finally
{
  if ($roInitializationResult -ge 0)
  {
    [WmsActivationProbe]::RoUninitialize()
  }
}
