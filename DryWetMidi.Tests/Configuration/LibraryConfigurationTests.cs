using Melanchall.DryWetMidi.Configuration;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Security.AccessControl;

namespace Melanchall.DryWetMidi.Tests.Configuration
{
    [TestFixture]
    public sealed class LibraryConfigurationTests
    {
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
            var oldUseWms = ResetEnvironment();

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
                LibraryConfiguration.UseWindowsMidiServices = oldUseWms;
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

        [WinOnly]
        [NativeApiRequired]
        [Test]
        public void GetApiType_Win_Default()
        {
            ResetEnvironment();

            var apiType = LibraryConfiguration.GetApiType();
            Console.WriteLine($"API type: {apiType}");

            ClassicAssert.IsTrue(apiType == ApiType.WinMM || apiType == ApiType.WindowsMidiServices, "Invalid API type.");

            if (apiType == ApiType.WindowsMidiServices)
                ClassicAssert.IsTrue(LibraryConfiguration.IsWorkerThreadUsed, "Worker thread is not used for WMS.");
            else
                ClassicAssert.IsFalse(LibraryConfiguration.IsWorkerThreadUsed, "Worker thread is used for WinMM.");
        }

        [NativeApiRequired]
        [AdvancedApiRequired]
        [WinOnly]
        [Test]
        public void GetApiType_Win_WmsAvailable([Values] bool useWms)
        {
            var oldUseWms = ResetEnvironment();

            try
            {
                LibraryConfiguration.UseWindowsMidiServices = useWms;

                ClassicAssert.AreEqual(
                    useWms ? ApiType.WindowsMidiServices : ApiType.WinMM,
                    LibraryConfiguration.GetApiType(),
                    "Invalid API type.");

                if (useWms)
                    ClassicAssert.IsTrue(LibraryConfiguration.IsWorkerThreadUsed, "Worker thread is not used for WMS.");
                else
                    ClassicAssert.IsFalse(LibraryConfiguration.IsWorkerThreadUsed, "Worker thread is used for WinMM.");
            }
            finally
            {
                MidiConfiguration.ResetHandle();
                LibraryConfiguration.UseWindowsMidiServices = oldUseWms;
            }
        }

        [MacOnly]
        [NativeApiRequired]
        [Test]
        public void GetApiType_Mac()
        {
            ResetEnvironment();

            var apiType = LibraryConfiguration.GetApiType();
            ClassicAssert.AreEqual(ApiType.CoreMidi, apiType, "Invalid API type.");
            ClassicAssert.IsFalse(MidiOperationsExecutor.Instance.IsWorkerThreadUsed, "Worker thread is used.");
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

        private bool ResetEnvironment()
        {
            var oldUseWms = LibraryConfiguration.UseWindowsMidiServices;
            MidiConfiguration.ResetHandle();
            MidiOperationsExecutor.ResetInstance();
            return oldUseWms;
        }
#endif
    }
}
