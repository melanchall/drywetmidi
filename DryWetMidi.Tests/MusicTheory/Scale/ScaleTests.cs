using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class ScaleTests
    {
        private static readonly object[] ParseData_Valid = new[]
        {
            new object[] { "E +4 +8 +1", new Scale(new[] { Interval.FromHalfSteps(4), Interval.FromHalfSteps(8), Interval.FromHalfSteps(1) }, NoteName.E) },
            new object[] { "E 4 -6 +1", new Scale(new[] { Interval.FromHalfSteps(4), Interval.FromHalfSteps(-6), Interval.FromHalfSteps(1) }, NoteName.E) },
            new object[] { " C   major", new Scale(ScaleIntervals.Major, NoteName.C) },
            new object[] { " D major  ", new Scale(ScaleIntervals.Major, NoteName.D) },
        }
        .Concat(
            ScaleIntervals
            .ScalesByName
            .SelectMany(f => Enum
                .GetValues(typeof(NoteName))
                .OfType<NoteName>()
                .SelectMany(n => new[]
                {
                    string.Empty,
                    Note.SharpShortString,
                    Note.SharpLongString,
                    Note.FlatShortString,
                    Note.FlatLongString,
                }.Select(a => new object[]
                {
                    $"{n}{a} {f.Key}",
                    new Scale(f.Value, (NoteName)(((int)n + Octave.OctaveSize + (string.IsNullOrWhiteSpace(a) ? 0 : (a == Note.SharpShortString || a == Note.SharpLongString ? 1 : -1))) % Octave.OctaveSize))
                }))))
        .ToArray();

        private static readonly object[] ParseData_Invalid = new[]
        {
            new object[] { "E -300" },
            new object[] { "F 500" },
            new object[] { "F yy bebop" },
            new object[] { "C majorr" },
            new object[] { "Cbx major" },
            new object[] { "major" },
            new object[] { "X major" },
            new object[] { "C +abc" },
            new object[] { "C +7 +" },
        };

        [TestCaseSource(nameof(ParseData_Valid))]
        public void Parse_Valid(string s, Scale expectedScale)
        {
            var actualScale = Scale.Parse(s);
            ClassicAssert.AreEqual(expectedScale, actualScale, "Incorrect parsed scale.");
            ClassicAssert.AreEqual(
                expectedScale,
                Scale.Parse(expectedScale.ToString()),
                "String representation was not parsed to the original scale.");
        }

        [TestCaseSource(nameof(ParseData_Valid))]
        public void TryParse_Valid(string s, Scale expectedScale)
        {
            ClassicAssert.IsTrue(Scale.TryParse(s, out var actualScale), "Failed to parse scale.");
            ClassicAssert.AreEqual(expectedScale, actualScale, "Incorrect parsed scale.");
        }

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void Parse_Invalid(string s) =>
            ClassicAssert.Throws<FormatException>(() => Scale.Parse(s), "Exception was not thrown for invalid scale.");

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void TryParse_Invalid(string s) =>
            ClassicAssert.IsFalse(Scale.TryParse(s, out var _), "Parsing succeeded for invalid scale.");

        [Test]
        public void Parse_Invalid_EmptyOrNull([Values(null, "", "  ")] string s) =>
            ClassicAssert.Throws<ArgumentException>(() => Scale.Parse(s), "Scale parsing did not throw an exception.");

        [Test]
        public void TryParse_Invalid_EmptyOrNull([Values(null, "", "  ")] string s) =>
            ClassicAssert.IsFalse(Scale.TryParse(s, out var _), $"Parsed invalid value '{s}'.");
    }
}
