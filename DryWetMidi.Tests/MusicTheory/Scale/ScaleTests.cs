using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class ScaleTests
    {
        private static readonly object[] ScalesStringsToScales = ScaleIntervals
            .ScalesByName
            .SelectMany(f => Enum
                .GetValues(typeof(NoteName))
                .OfType<NoteName>()
                .Select(n => new object[]
                {
                    $"{n.ToString().Replace(Note.SharpLongString, Note.SharpShortString)} {f.Key}",
                    new Scale(f.Value, n)
                }))
            .ToArray();

        #region Test methods

        [TestCaseSource(nameof(ScalesStringsToScales))]
        public void Parse_Valid_ScaleName(string scaleString, Scale expectedScale) =>
            Parse(scaleString, expectedScale);

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

        private static void Parse(string input, Scale expectedScale, string label = null)
        {
            var labelPart = string.IsNullOrWhiteSpace(label) ? string.Empty : $"[{label}] ";

            Scale.TryParse(input, out var actualScale);
            ClassicAssert.AreEqual(expectedScale,
                            actualScale,
                            $"{labelPart}TryParse: incorrect result.");

            actualScale = Scale.Parse(input);
            ClassicAssert.AreEqual(expectedScale,
                            actualScale,
                            $"{labelPart}Parse: incorrect result.");

            ClassicAssert.AreEqual(expectedScale,
                            Scale.Parse(expectedScale.ToString()),
                            $"{labelPart}Parse: string representation was not parsed to the original scale.");
        }

        private static void ParseInvalid<TException>(string input)
            where TException : Exception
        {
            ClassicAssert.Throws<TException>(() => Scale.Parse(input));
        }

        #endregion
    }
}
