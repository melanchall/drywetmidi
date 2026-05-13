using Melanchall.DryWetMidi.Configuration;
using Melanchall.DryWetMidi.Tests.Attributes;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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

        [AdvancedApiRequired]
        [WinOnly]
        [Test]
        public void CheckCapabilitiesWithWindowsMidiServicesUsage([Values] bool useWms)
        {
            MidiConfiguration.ResetHandle();
            LibraryConfiguration.UseWindowsMidiServices = useWms;

            ClassicAssert.AreEqual(useWms, LibraryConfiguration.IsDevicesMultiClientAccessAvailable(), "Invalid devices multi-client access availability.");
            ClassicAssert.AreEqual(useWms, LibraryConfiguration.IsDevicesWatcherApiAvailable(), "Invalid devices watcher API availability.");
            ClassicAssert.AreEqual(useWms, LibraryConfiguration.IsParentDeviceApiAvailable(), "Invalid parent device API availability.");
            ClassicAssert.AreEqual(useWms, LibraryConfiguration.IsVirtualDeviceApiAvailable(), "Invalid virtual device API availability.");
        }

        #endregion
    }
}
