using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Configuration
{
    /// <summary>
    /// Provides access to library-wide configuration and native MIDI API capabilities.
    /// </summary>
    public static class LibraryConfiguration
    {
        #region Events

#if !NATIVELESS
        /// <summary>
        /// Occurs when an activity message is received from the native MIDI API.
        /// </summary>
        public static event EventHandler<LibraryActivityMessageReceivedEventArgs>? LibraryActivityMessageReceived;
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
        /// <summary>
        /// Gets or sets whether Windows MIDI Services is preferred as the native MIDI API on Windows.
        /// The default value is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Setting this property to <c>true</c> does not guarantee that Windows MIDI Services will be used.
        /// It is used only when the API is available on the current system; see <see cref="GetApiType"/>.
        /// </remarks>
        public static bool UseWindowsMidiServices
        {
            get => MidiConfiguration.UseWindowsMidiServices;
            set => MidiConfiguration.UseWindowsMidiServices = value;
        }

        /// <summary>
        /// Gets or sets whether native MIDI API operations are executed on a worker thread.
        /// The default value is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// When Windows MIDI Services is used, enabling this property runs native API operations on a dedicated
        /// background thread initialized with a Windows multithreaded apartment. This keeps those calls off the
        /// calling application's threads and provides the threading context expected by Windows MIDI Services.
        /// See <see href="https://microsoft.github.io/MIDI/kb/moving-from-winmm-to-wms">Windows MIDI Services threading guidance</see>.
        /// The worker thread is not used when another native MIDI API is selected.
        /// </remarks>
        public static bool UseWorkerThread { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether native MIDI API operations are currently executed on a worker thread.
        /// </summary>
        /// <exception cref="FeatureNotAvailableException">The current operating system is not supported.</exception>
        public static bool IsWorkerThreadUsed
        {
            get
            {
                NativeApiUtilities.EnsureOsIsSupported();
                MidiConfiguration.GetConfigurationHandle();
                return MidiOperationsExecutor.Instance.IsWorkerThreadUsed;
            }
        }
#endif

        #endregion

        #region Methods

#if !NATIVELESS
        /// <summary>
        /// Determines whether the endpoints watcher API is available on the current system.
        /// </summary>
        /// <returns><c>true</c> if the endpoints watcher API is available; otherwise, <c>false</c>.</returns>
        /// <remarks>A <c>true</c> result indicates that <see cref="EndpointsWatcher"/> can be used on this system.</remarks>
        public static bool IsEndpointsWatcherApiAvailable()
        {
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return MidiConfigurationApi.Api_IsDevicesWatcherApiAvailable(MidiConfiguration.GetConfigurationHandle());
        }

        /// <summary>
        /// Determines whether the current system supports access to an endpoint by multiple clients.
        /// </summary>
        /// <returns><c>true</c> if multi-client endpoint access is available; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// A <c>true</c> result indicates that multiple instances of an <see cref="InputEndpoint"/> or
        /// <see cref="OutputEndpoint"/> can access the same endpoint at the same time.
        /// </remarks>
        public static bool IsEndpointsMultiClientAccessAvailable()
        {
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return IsAdvancedApiAvailable();
        }

        /// <summary>
        /// Determines whether the virtual device API is available on the current system.
        /// </summary>
        /// <returns><c>true</c> if the virtual device API is available; otherwise, <c>false</c>.</returns>
        /// <remarks>A <c>true</c> result indicates that <see cref="VirtualDevice"/> can be used on this system.</remarks>
        public static bool IsVirtualDeviceApiAvailable()
        {
            if (!NativeApiUtilities.IsOsSupported())
                return false;

            return MidiConfigurationApi.Api_IsVirtualDeviceApiAvailable(MidiConfiguration.GetConfigurationHandle());
        }

        /// <summary>
        /// Gets the native MIDI API used by DryWetMIDI on the current system.
        /// </summary>
        /// <returns>The type of native MIDI API in use.</returns>
        /// <exception cref="FeatureNotAvailableException">The current operating system is not supported.</exception>
        public static ApiType GetApiType()
        {
            NativeApiUtilities.EnsureOsIsSupported();
            return MidiConfigurationApi.Api_GetApiType(MidiConfiguration.GetConfigurationHandle());
        }
#endif

        /// <summary>
        /// Gets a summary of the DryWetMIDI version, native backend, and available capabilities.
        /// </summary>
        /// <returns>A multiline summary of the current library configuration.</returns>
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
                return UseWindowsMidiServices && AreWindowsMidiServicesAvailable();

            return true;
        }

        private static void OnLibraryActivityMessageReceived(object? sender, string text)
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

            var osType = CommonApi.Api_GetOsType();
            resultLines.Add($"- OS type: {osType}");

            if (NativeApiUtilities.IsOsSupported())
            {
                var apiType = MidiConfigurationApi.Api_GetApiType(MidiConfiguration.GetConfigurationHandle());
                resultLines.Add($"- API type: {apiType}");
            }

            if (osType == CommonApi.OsType.Windows)
                AddWindowsNativeBackendInfo(resultLines);
        }

        private static bool AreWindowsMidiServicesAvailable()
        {
            var apiType = MidiConfigurationApi.Api_GetApiType(MidiConfiguration.GetConfigurationHandle());
            return apiType == ApiType.WindowsMidiServices;
        }

        private static void AddWindowsNativeBackendInfo(List<string> resultLines)
        {
            var wmsAvailable = AreWindowsMidiServicesAvailable();
            if (wmsAvailable)
            {
                resultLines.Add("- Windows MIDI Services are available and fully functional");
                return;
            }

            resultLines.Add("- Windows MIDI Services are not available");
        }
#endif

#endregion
    }
}
