using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    internal static class LengthParsingTester
    {
        #region Methods

        public static void TestTryParse(string input, ILength expectedLength)
        {
            LengthUtilities.TryParse(input, out var actualLength);
            Assert.AreEqual(expectedLength, actualLength);
        }

        public static void TestToString(ILength expectedLength)
        {
            Assert.AreEqual(expectedLength, LengthUtilities.Parse(expectedLength.ToString()));
        }

        #endregion
    }
}
