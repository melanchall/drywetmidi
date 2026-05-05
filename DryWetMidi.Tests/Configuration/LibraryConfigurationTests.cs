using Melanchall.DryWetMidi.Configuration;
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

        #endregion
    }
}
