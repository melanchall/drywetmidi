using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction.Length
{
    [TestClass]
    public class MetricLengthTests
    {
        #region Test methods

        [TestMethod]
        [Description("Try parse metric length in form of 'Hours:Minutes:Seconds:Milliseconds'.")]
        public void TryParse_HoursMinutesSecondsMilliseconds()
        {
            LengthParsingTester.TestTryParse("2:4:30:567", new MetricLength(2, 4, 30, 567));
        }

        [TestMethod]
        [Description("Try parse metric length in form of 'Hours:Minutes:Seconds'.")]
        public void TryParse_HoursMinutesSeconds()
        {
            LengthParsingTester.TestTryParse("2:4:30", new MetricLength(2, 4, 30));
        }

        [TestMethod]
        [Description("Try parse metric length in form of 'Minutes:Seconds'.")]
        public void TryParse_MinutesSeconds()
        {
            LengthParsingTester.TestTryParse("4:30", new MetricLength(0, 4, 30));
        }

        [TestMethod]
        [Description("Parse string representation of a length.")]
        public void Parse_ToString()
        {
            LengthParsingTester.TestToString(new MetricLength(3, 6, 8, 987));
        }

        #endregion
    }
}
