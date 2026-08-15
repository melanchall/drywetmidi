using Melanchall.DryWetMidi.Configuration;
using Melanchall.DryWetMidi.Tests.Attributes;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Tests.Configuration
{
    [TestFixture]
    public sealed class LibraryConfigurationTests
    {
        #region Test methods

        [Test]
        public void GetConfigurationSummary()
        {
            var summary = LibraryConfiguration.GetConfigurationSummary();
            ClassicAssert.IsNotNull(summary, "Summary is null.");
            ClassicAssert.IsNotEmpty(summary, "Summary is empty.");
        }

#if !NATIVELESS
        [NativeApiRequired]
        [AdvancedApiRequired]
        [WinOnly]
        [Test]
        public void CheckCapabilitiesWithWindowsMidiServicesUsage([Values] bool useWms)
        {
            MidiConfiguration.ResetHandle();

            try
            {
                LibraryConfiguration.UseWindowsMidiServices = useWms;

                ClassicAssert.AreEqual(useWms, LibraryConfiguration.IsEndpointsMultiClientAccessAvailable(), "Invalid endpoints multi-client access availability.");
                ClassicAssert.AreEqual(useWms, LibraryConfiguration.IsEndpointsWatcherApiAvailable(), "Invalid endpoints watcher API availability.");
                ClassicAssert.AreEqual(useWms, LibraryConfiguration.IsVirtualDeviceApiAvailable(), "Invalid virtual device API availability.");
            }
            finally
            {
                MidiConfiguration.ResetHandle();
                LibraryConfiguration.UseWindowsMidiServices = true;
            }
        }

        [NativeApiRequired]
        [AdvancedApiRequired]
        [Test]
        public void IsEndpointsMultiClientAccessAvailable()
        {
            ClassicAssert.IsTrue(LibraryConfiguration.IsEndpointsMultiClientAccessAvailable(), "Invalid endpoints multi-client access availability.");
        }

        [NativeApiRequired]
        [AdvancedApiRequired]
        [Test]
        public void IsEndpointsWatcherApiAvailable()
        {
            ClassicAssert.IsTrue(LibraryConfiguration.IsEndpointsWatcherApiAvailable(), "Invalid endpoints watcher API availability.");
        }

        [NativeApiRequired]
        [AdvancedApiRequired]
        [Test]
        public void IsVirtualDeviceApiAvailable()
        {
            ClassicAssert.IsTrue(LibraryConfiguration.IsVirtualDeviceApiAvailable(), "Invalid virtual device API availability.");
        }

        [NativeApiRequired]
        [Test]
        public void GetApiType()
        {
            var apiType = LibraryConfiguration.GetApiType();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                ClassicAssert.IsTrue(apiType == ApiType.WinMM || apiType == ApiType.WindowsMidiServices, "Invalid API type.");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                ClassicAssert.AreEqual(ApiType.CoreMidi, apiType, "Invalid API type.");
        }

        [NativeApiRequired]
        [AdvancedApiRequired]
        [WinOnly]
        [Test]
        public void GetApiType_WmsAvailable([Values] bool useWms)
        {
            MidiConfiguration.ResetHandle();

            try
            {
                LibraryConfiguration.UseWindowsMidiServices = useWms;

                ClassicAssert.AreEqual(
                    useWms ? ApiType.WindowsMidiServices : ApiType.WinMM,
                    LibraryConfiguration.GetApiType(),
                    "Invalid API type.");
            }
            finally
            {
                MidiConfiguration.ResetHandle();
                LibraryConfiguration.UseWindowsMidiServices = true;
            }
        }

        [NativeApiRequired]
        [Test]
        public void NativeApiMessageReceived()
        {
            var message = string.Empty;

            LibraryConfiguration.LibraryActivityMessageReceived += (_, args) => message = args.Message;

            var configurationHandle = MidiConfiguration.GetConfigurationHandle();
            MidiConfigurationApi.Api_CheckNativeApiActivityCallback(configurationHandle);

            ClassicAssert.IsNotNull(message, "Message is null.");
            ClassicAssert.IsNotEmpty(message, "Message is empty.");

            Console.WriteLine(message);
        }

        [NativeApiRequired]
        [WinOnly]
        [Test]
        public void NativeApiMessageReceivedOnWinRtError()
        {
            var message = string.Empty;

            LibraryConfiguration.LibraryActivityMessageReceived += (_, args) => message = args.Message;

            var configurationHandle = MidiConfiguration.GetConfigurationHandle();
            MidiConfigurationApi.Api_CheckWinRtErrorHandling_Win(configurationHandle);

            ClassicAssert.IsNotNull(message, "Message is null.");
            ClassicAssert.IsNotEmpty(message, "Message is empty.");

            Console.WriteLine(message);
        }

        [NativeApiRequired]
        [WinOnly]
        [Test]
        public void NativeApiMessageReceivedOnStdException()
        {
            var message = string.Empty;

            LibraryConfiguration.LibraryActivityMessageReceived += (_, args) => message = args.Message;

            var configurationHandle = MidiConfiguration.GetConfigurationHandle();
            MidiConfigurationApi.Api_CheckStdExceptionHandling_Win(configurationHandle);

            ClassicAssert.IsNotNull(message, "Message is null.");
            ClassicAssert.IsNotEmpty(message, "Message is empty.");

            Console.WriteLine(message);
        }
#endif

        #endregion
    }
}
