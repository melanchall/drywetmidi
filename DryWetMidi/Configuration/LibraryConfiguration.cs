using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Configuration
{
    public static class LibraryConfiguration
    {
        private const string UnofficialReleaseMarker = "...";

        private static readonly string Version = UnofficialReleaseMarker;
        private static readonly string CommitId = UnofficialReleaseMarker;

        public static bool IsDevicesWatcherApiAvailable() =>
            IsAdvancedApiAvailable();

        public static bool IsDevicesMultiClientAccessAvailable() =>
            IsAdvancedApiAvailable();

        public static bool IsParentDeviceApiAvailable() =>
            IsAdvancedApiAvailable();

        public static bool IsVirtualDeviceApiAvailable()
        {
#if NATIVELESS
            return false;
#else
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return MidiConfigurationApi.Api_IsVirtualDeviceApiAvailable(MidiConfiguration.GetConfigurationHandle());
#endif
        }

        public static string GetConfigurationSummary()
        {
            var resultLines = new List<string>
            {
                Version == UnofficialReleaseMarker
                    ? "DryWetMIDI (not an official release)"
                    : $"DryWetMIDI {Version} ({CommitId})"
            };

#if NATIVELESS
            resultLines.Add("This is the nativeless version");
#else
            AddNativeBackendInfo(resultLines);
#endif

            const string availableLabel = "available";
            const string notAvailableLabel = "not available";

            resultLines.AddRange(new[]
            {
                "Capabilities:",
                $"- devices watcher API: {(IsDevicesWatcherApiAvailable() ? availableLabel : notAvailableLabel)}",
                $"- parent device API: {(IsParentDeviceApiAvailable() ? availableLabel : notAvailableLabel)}",
                $"- virtual device API: {(IsVirtualDeviceApiAvailable() ? availableLabel : notAvailableLabel)}",
                $"- devices multi-client access: {(IsDevicesMultiClientAccessAvailable() ? availableLabel : notAvailableLabel)}",
            });

            return string.Join(Environment.NewLine, resultLines).Trim();
        }

        private static bool IsAdvancedApiAvailable()
        {
#if NATIVELESS
            return false;
#else
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return AreWindowsMidiServicesAvailable(out _, out _, out _, out _, out _);

            return true;
#endif
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

        private static bool AreWindowsMidiServicesAvailable(
            out bool comInitializationResult,
            out bool registryCheckResult,
            out bool comCheckResult,
            out CommonApi.WMSSERVICECHECKRESULT serviceCheckResult,
            out bool sdkCheckResult)
        {
            CommonApi.Api_GetNativeEnvironmentInfo_Win(
                out comInitializationResult,
                out registryCheckResult,
                out comCheckResult,
                out serviceCheckResult,
                out sdkCheckResult);

            return
                comInitializationResult &&
                registryCheckResult &&
                comCheckResult &&
                serviceCheckResult == CommonApi.WMSSERVICECHECKRESULT.WMSSERVICECHECKRESULT_OK &&
                sdkCheckResult;
        }

        private static void AddWindowsNativeBackendInfo(List<string> resultLines)
        {
            var wmsAvailable = AreWindowsMidiServicesAvailable(
                out var comInitializationResult,
                out var registryCheckResult,
                out var comCheckResult,
                out var serviceCheckResult,
                out var sdkCheckResult);

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
                    $"-- registry: {registryCheckResult}",
                    $"-- COM: {comCheckResult}",
                    $"-- service: {serviceCheckResult}",
                    $"-- SDK: {sdkCheckResult}",
                });
            }
        }
#endif
    }
}
