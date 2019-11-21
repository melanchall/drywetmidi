using System;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class ScaleTests
    {
        #region Test methods

        [Test]
        [Description("Parse valid scale by name.")]
        public void Parse_Valid_ScaleName()
        {
            Parse("C# major", new Scale(ScaleIntervals.Major, NoteName.CSharp));
        }

        [Test]
        [Description("Parse valid scale by intervals.")]
        public void Parse_Valid_ScaleIntervals()
        {
            Parse("E +4 +8 +1", new Scale(new[] { Interval.FromHalfSteps(4), Interval.FromHalfSteps(8), Interval.FromHalfSteps(1) }, NoteName.E));
        }

        [Test]
        [Description("Parse valid scale by name using 'sharp' word.")]
        public void Parse_Valid_SharpWord()
        {
            Parse("F sharp bebop", new Scale(ScaleIntervals.Bebop, NoteName.FSharp));
        }

        [Test]
        [Description("Parse invalid scale where scale is unknown.")]
        public void Parse_Invalid_ScaleIsUnknown()
        {
            ParseInvalid<FormatException>("F yy bebop");
        }

        [Test]
        [Description("Parse invalid scale where a negative interval is out of range.")]
        public void Parse_Invalid_IntervalIsOutOfRange_Negative()
        {
            ParseInvalid<FormatException>("E -300");
        }

        [Test]
        [Description("Parse invalid scale where a positive interval is out of range.")]
        public void Parse_Invalid_IntervalIsOutOfRange_Positive()
        {
            ParseInvalid<FormatException>("F 500");
        }

        [Test]
        [Description("Parse invalid scale where an input string is empty.")]
        public void Parse_Invalid_EmptyInputString()
        {
            ParseInvalid<ArgumentException>(string.Empty);
        }

        #endregion

        #region Private methods

        private static void Parse(string input, Scale expectedScale)
        {
            Scale.TryParse(input, out var actualScale);
            Assert.AreEqual(expectedScale,
                            actualScale,
                            "TryParse: incorrect result.");

            actualScale = Scale.Parse(input);
            Assert.AreEqual(expectedScale,
                            actualScale,
                            "Parse: incorrect result.");

            Assert.AreEqual(expectedScale,
                            Scale.Parse(expectedScale.ToString()),
                            "Parse: string representation was not parsed to the original scale.");
        }

        private static void ParseInvalid<TException>(string input)
            where TException : Exception
        {
            Assert.Throws<TException>(() => Scale.Parse(input));
        }

        #endregion
    }
}
