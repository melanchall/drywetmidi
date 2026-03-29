using Melanchall.DryWetMidi.Configuration;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Configuration
{
    [TestFixture]
    public sealed class LibraryInformationTests
    {
        #region Test methods

        [Test]
        public void GetInformation()
        {
            var info = LibraryInformation.GetInformation();
            ClassicAssert.IsNotNull(info, "Info is null.");
            ClassicAssert.IsNotEmpty(info, "Info is empty.");
        }

        #endregion
    }
}
