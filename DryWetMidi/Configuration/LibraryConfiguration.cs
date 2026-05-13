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
        public static event EventHandler<NativeApiActivityRecordReceivedEventArgs> NativeApiMessageReceived;
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
            MidiConfiguration.NativeApiActivityRecord += OnNativeApiMessageReceived;
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

        public static bool IsDevicesWatcherApiAvailable()
        {
#if NATIVELESS
            return false;
#else
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return IsDevicesWatcherApiAvailable(MidiConfiguration.GetConfigurationHandle());
#endif
        }

        public static bool IsDevicesMultiClientAccessAvailable()
        {
#if NATIVELESS
            return false;
#else
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return IsDevicesWatcherApiAvailable(MidiConfiguration.GetConfigurationHandle());
#endif
        }

        public static bool IsParentDeviceApiAvailable()
        {
#if NATIVELESS
            return false;
#else
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return IsParentDeviceApiAvailable(MidiConfiguration.GetConfigurationHandle());
#endif
        }

        public static bool IsVirtualDeviceApiAvailable()
        {
#if NATIVELESS
            return false;
#else
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return IsVirtualDeviceApiAvailable(MidiConfiguration.GetConfigurationHandle());
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

            var capabilities = new Dictionary<string, bool>
            {
                ["devices watcher API"] = IsDevicesWatcherApiAvailable(),
                ["parent device API"] = IsParentDeviceApiAvailable(),
                ["virtual device API"] = IsVirtualDeviceApiAvailable(),
                ["devices multi-client access"] = IsDevicesMultiClientAccessAvailable(),
            };

            resultLines.Add("Capabilities:");
            resultLines.AddRange(capabilities.Select(kv => $"- {kv.Key}: {(kv.Value ? "available" : "not available")}"));

            return string.Join(Environment.NewLine, resultLines).Trim();
        }

#if !NATIVELESS
        internal static bool IsDevicesWatcherApiAvailable(MidiConfigurationHandle configurationHandle)
        {
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return MidiConfigurationApi.Api_IsDevicesWatcherApiAvailable(configurationHandle);
        }

        internal static bool IsDevicesMultiClientAccessAvailable(MidiConfigurationHandle configurationHandle) =>
            IsAdvancedApiAvailable();

        internal static bool IsParentDeviceApiAvailable(MidiConfigurationHandle configurationHandle) =>
            IsAdvancedApiAvailable();

        internal static bool IsVirtualDeviceApiAvailable(MidiConfigurationHandle configurationHandle)
        {
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return MidiConfigurationApi.Api_IsVirtualDeviceApiAvailable(configurationHandle);
        }

        internal static bool IsAdvancedApiAvailable()
        {
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return UseWindowsMidiServices && AreWindowsMidiServicesAvailable(out _, out _, out _, out _, out _);

            return true;
        }

        private static void OnNativeApiMessageReceived(object sender, string text)
        {
            var eventArgs = new NativeApiActivityRecordReceivedEventArgs(text);
            NativeApiMessageReceived?.Invoke(null, eventArgs);
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
