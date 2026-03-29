using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Configuration
{
    public static class LibraryInformation
    {
        private const string UnofficialReleaseMarker = "...";

        private static readonly string Version = UnofficialReleaseMarker;
        private static readonly string CommitId = UnofficialReleaseMarker;

        public static string GetInformation()
        {
            var resultLines = new List<string>();

            resultLines.Add(Version == UnofficialReleaseMarker
                ? "DryWetMIDI (not an official release)"
                : $"DryWetMIDI {Version} ({CommitId})");

#if NATIVELESS
            resultLines.Add("This is the nativeless version");
#else
            AddNativeBackendInfo(resultLines);
#endif

            return string.Join(Environment.NewLine, resultLines).Trim();
        }

#if !NATIVELESS
        private static void AddNativeBackendInfo(List<string> resultLines)
        {
            var nativeBackendAvailable =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

            if (!nativeBackendAvailable)
            {
                resultLines.Add($"Native backend is not available for the current OS ({RuntimeInformation.OSDescription})");
                return;
            }

            resultLines.Add("Native backend info:");

            var apiType = CommonApi.Api_GetApiType();
            resultLines.Add($"- API type: {apiType}");

            if (apiType == CommonApi.API_TYPE.API_TYPE_WIN)
                AddWindowsNativeBackendInfo(resultLines);
        }

        private static void AddWindowsNativeBackendInfo(List<string> resultLines)
        {
            CommonApi.Api_GetNativeEnvironmentInfo_Win(
                out var comInitializationResult,
                out var registryCheckResult,
                out var comCheckResult,
                out var serviceCheckResult,
                out var sdkCheckResult);

            var wmsAvailable =
                comInitializationResult &&
                registryCheckResult &&
                comCheckResult &&
                serviceCheckResult == CommonApi.WMSSERVICECHECKRESULT.WMSSERVICECHECKRESULT_OK &&
                sdkCheckResult;

            if (wmsAvailable)
            {
                resultLines.Add("- Windows MIDI Services are available and fully functional");
                return;
            }

            resultLines.Add("- Windows MIDI Services are not available; components status:");

            if (!comInitializationResult)
            {
                resultLines.Add($"-- COM failed to initialize");
            }
            else
            {
                resultLines.AddRange(new[]
                {
                    $"-- registry check result: {registryCheckResult}",
                    $"-- COM check result: {comCheckResult}",
                    $"-- service check result: {serviceCheckResult}",
                    $"-- SDK check result: {sdkCheckResult}"
                });
            }
        }
#endif
    }
}
