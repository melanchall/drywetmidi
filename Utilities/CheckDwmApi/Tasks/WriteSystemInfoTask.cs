using System;
using System.Diagnostics;

namespace Melanchall.CheckDwmApi
{
    internal sealed class WriteSystemInfoTask : ITask
    {
        private enum InfoType
        {
            CpuArchitecture,
            CpuName,
            Os,
        }

        private record CommandExecutor(
            string Command,
            string Arguments,
            Func<string, string> ProcessOutput);

        private record InfoProvider(
            InfoType InfoType,
            params CommandExecutor[] CommandExecutors);

        private record LscpuField(
            string Field,
            string Data);

        private record LscpuResult(
            LscpuField[] Lscpu);

        public string GetTitle() =>
            "Write system information";

        public string GetDescription() =>
            "Writes system information: CPU architecture, CPU name and operating system.";

        public void Execute(
            ToolOptions toolOptions,
            ReportWriter reportWriter)
        {
            reportWriter.WriteOperationTitle($"Retrieving system information for {GetBasicOsInfo()}...");

            InfoProvider[] infoProviders = null;

            try
            {
                infoProviders = GetInfoProviders();
                reportWriter.WriteOperationSubTitle("info providers obtained for the current OS");
            }
            catch (Exception ex)
            {
                throw new TaskFailedException(
                    "Failed to get info providers for the current operating system.",
                    ex);
            }

            foreach (var (infoType, commandExecutors) in infoProviders)
            {
                reportWriter.WriteOperationTitle($"Retrieving {infoType}...");

                foreach (var (command, arguments, processOutput) in commandExecutors)
                {
                    try
                    {
                        var commandOutput = ExecuteCommand(command, arguments, reportWriter).Trim();
                        var processedOutput = processOutput(commandOutput.Trim()).Trim();

                        switch (infoType)
                        {
                            case InfoType.CpuArchitecture:
                                reportWriter.WriteOperationSubTitle($"CPU arch: {processedOutput}");
                                break;
                            case InfoType.CpuName:
                                reportWriter.WriteOperationSubTitle($"CPU name: {processedOutput}");
                                break;
                            case InfoType.Os:
                                reportWriter.WriteOperationSubTitle($"OS: {processedOutput}");
                                break;
                        }

                        break;
                    }
                    catch (Exception ex)
                    {
                        reportWriter.WriteOperationSubTitle($"Failed to execute command: {ex.Message}.");
                    }

                    throw new TaskFailedException($"Failed to retrieve {infoType} information.");
                }
            }
        }

        private static string GetBasicOsInfo()
        {
            var versionString = Environment.OSVersion.VersionString;
            var osDescription = OperatingSystem.IsWindows() ? "Windows" :
                                OperatingSystem.IsLinux() ? "Linux" :
                                OperatingSystem.IsMacOS() ? "macOS" :
                                "Unknown OS";

            return $"{osDescription} (){versionString})";
        }

        private static InfoProvider[] GetInfoProviders()
        {
            if (OperatingSystem.IsWindows())
                return GetWindowsInfoProviders();
            else if (OperatingSystem.IsLinux())
                return GetLinuxInfoProviders();
            else if (OperatingSystem.IsMacOS())
                return GetMacOsInfoProviders();
            else
                throw new NotSupportedException($"Unsupported operating system.");
        }

        private static InfoProvider[] GetWindowsInfoProviders() =>
        [
            new InfoProvider(
                InfoType.CpuArchitecture,
                new CommandExecutor(
                    "cmd",
                    "/C echo %PROCESSOR_ARCHITECTURE%",
                    output => output)),
            new InfoProvider(
                InfoType.CpuName,
                new CommandExecutor(
                    "cmd",
                    "/C powershell -NoProfile -NonInteractive -Command \"Get-CimInstance Win32_Processor | Select-Object -ExpandProperty Name\"",
                    output => output)),
            new InfoProvider(
                InfoType.Os,
                new CommandExecutor(
                    "cmd",
                    "/C powershell -NoProfile -NonInteractive -Command \"$o=Get-CimInstance Win32_OperatingSystem; Write-Output \\\"$($o.Caption) $($o.Version) (Build $($o.BuildNumber))\\\"\"",
                    output => output))
        ];

        private static InfoProvider[] GetLinuxInfoProviders() =>
        [
            new InfoProvider(
                InfoType.CpuArchitecture,
                new CommandExecutor(
                    "uname",
                    "-m",
                    output => output)),
            new InfoProvider(
                InfoType.CpuName,
                new CommandExecutor(
                    "sh",
                    @"-c ""awk -F: '/model name/ {print $2; exit}' /proc/cpuinfo || true""",
                    output => string.IsNullOrWhiteSpace(output) ? "Unknown" : output)),
            new InfoProvider(
                InfoType.Os,
                new CommandExecutor(
                    "lsb_release",
                    "-d",
                    output => output.Split(':')[1]),
                new CommandExecutor(
                    "uname",
                    "-a",
                    output => output))
        ];

        private static InfoProvider[] GetMacOsInfoProviders() =>
        [
            new InfoProvider(
                InfoType.CpuArchitecture,
                new CommandExecutor(
                    "uname",
                    "-m",
                    output => output)),
            new InfoProvider(
                InfoType.CpuName,
                new CommandExecutor(
                    "sysctl",
                    "-n machdep.cpu.brand_string",
                    output => output)),
            new InfoProvider(
                InfoType.Os,
                new CommandExecutor(
                    "sw_vers",
                    "-productVersion",
                    output => $"macOS {output}"))
        ];

        private static string ExecuteCommand(
            string command,
            string arguments,
            ReportWriter reportWriter)
        {
            reportWriter.WriteOperationTitle($"Executing command: {command} {arguments}");

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            var timeout = TimeSpan.FromSeconds(10);
            var exited = process.WaitForExit(timeout);
            if (!exited)
            {
                process.Kill();
                throw new TimeoutException($"Command timed out after {timeout}.");
            }

            reportWriter.WriteOperationSubTitle("executed");
            return process.StandardOutput.ReadToEnd();
        }
    }
}
