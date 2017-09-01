using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    internal static class ParsingTester
    {
        #region Methods

        public static void TestTryParse(string input, ITime expectedTime)
        {
            TimeUtilities.TryParse(input, out var actualTime);
            Assert.AreEqual(expectedTime, actualTime);
        }

        public static void TestToString(ITime expectedTime)
        {
            Assert.AreEqual(expectedTime, TimeUtilities.Parse(expectedTime.ToString()));
        }

        #endregion
    }
}
