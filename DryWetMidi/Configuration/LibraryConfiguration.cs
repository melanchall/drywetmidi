using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Configuration
{
    public static class LibraryConfiguration
    {
        #region Events

#if !NATIVELESS
        public static event EventHandler<LibraryActivityMessageReceivedEventArgs> LibraryActivityMessageReceived;
#endif

        #endregion

        #region Constants

        private const string UnofficialReleaseMarker = "...";

        private static readonly string Version = UnofficialReleaseMarker;
        private static readonly string CommitId = UnofficialReleaseMarker;

        #endregion

        #region Constructor

        static LibraryConfiguration()
        {
#if !NATIVELESS
            MidiConfiguration.NativeApiMessageReceived += OnLibraryActivityMessageReceived;
#endif
        }

        #endregion

        #region Properties

#if !NATIVELESS
        public static bool UseWindowsMidiServices
        {
            get => MidiConfiguration.UseWindowsMidiServices;
            set => MidiConfiguration.UseWindowsMidiServices = value;
        }
#endif

        #endregion

        #region Methods

#if !NATIVELESS
        public static bool IsEndpointsWatcherApiAvailable()
        {
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return MidiConfigurationApi.Api_IsDevicesWatcherApiAvailable(MidiConfiguration.GetConfigurationHandle());
        }

        public static bool IsEndpointsMultiClientAccessAvailable()
        {
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return IsAdvancedApiAvailable();
        }

        public static bool IsDeviceInformationApiAvailable()
        {
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return IsAdvancedApiAvailable();
        }

        public static bool IsVirtualDeviceApiAvailable()
        {
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return MidiConfigurationApi.Api_IsVirtualDeviceApiAvailable(MidiConfiguration.GetConfigurationHandle());
        }
#endif

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

#if !NATIVELESS
            var capabilities = new Dictionary<string, bool>
            {
                ["endpoints watcher API"] = IsEndpointsWatcherApiAvailable(),
                ["parent device API"] = IsDeviceInformationApiAvailable(),
                ["virtual device API"] = IsVirtualDeviceApiAvailable(),
                ["endpoints multi-client access"] = IsEndpointsMultiClientAccessAvailable(),
            };

            resultLines.Add("Capabilities:");
            resultLines.AddRange(capabilities.Select(kv => $"- {kv.Key}: {(kv.Value ? "available" : "not available")}"));
#endif

            return string.Join(Environment.NewLine, resultLines).Trim();
        }

#if !NATIVELESS
        internal static bool IsAdvancedApiAvailable()
        {
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return UseWindowsMidiServices && AreWindowsMidiServicesAvailable(out _, out _, out _, out _, out _);

            return true;
        }

        private static void OnLibraryActivityMessageReceived(object sender, string text)
        {
            var eventArgs = new LibraryActivityMessageReceivedEventArgs(text);
            LibraryActivityMessageReceived?.Invoke(null, eventArgs);
        }

        private static void AddNativeBackendInfo(List<string> resultLines)
        {
            if (!NativeApiUtilities.IsOsSupported())
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

#endregion
    }
}
