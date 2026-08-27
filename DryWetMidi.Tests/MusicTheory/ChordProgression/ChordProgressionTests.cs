using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class ChordProgressionTests
    {
        private static readonly object[] ParseData_Valid = new[]
        {
            new object[] { "I-II-IV", "C major", new[] { "C", "D", "F" } },
            new object[] { "I", "C major", new[] { "C" } },
            new object[] { "  I ", "C major", new[] { "C" } },
            new object[] { "Im-II7-V", "C major", new[] { "Cm", "D7", "G" } },
            new object[] { "I - bVI - III - bVII", "C major", new[] { "C", "G#", "E", "A#" } },
        };

        private static readonly object[] ParseData_Invalid = new[]
        {
            new object[] { "I--IV", "C major" },
            new object[] { "I V", "C major" },
            new object[] { "abc", "C major" },
            new object[] { "-IV-", "C major" },
            new object[] { "I-bbIV-I", "C major" },
            new object[] { "I-IV-", "C major" },
            new object[] { "-I-IV", "C major" },
            new object[] { "Im-IIabc-V", "C major" },
            new object[] { "XYZ", "C major" },
        };

        [TestCaseSource(nameof(ParseData_Valid))]
        public void Parse_Valid(string input, string scaleString, string[] expectedChords)
        {
            var chordProgression = ChordProgression.Parse(input, Scale.Parse(scaleString));
            CollectionAssert.AreEqual(
                expectedChords.Select(c => Chord.Parse(c)).ToArray(),
                chordProgression.Chords,
                "Chords are invalid.");
        }

        [TestCaseSource(nameof(ParseData_Valid))]
        public void TryParse_Valid(string input, string scaleString, string[] expectedChords)
        {
            ClassicAssert.IsTrue(
                ChordProgression.TryParse(input, Scale.Parse(scaleString), out var actualChordProgression),
                "Chord progression could not be parsed.");
            CollectionAssert.AreEqual(
                expectedChords.Select(c => Chord.Parse(c)).ToArray(),
                actualChordProgression.Chords,
                "Chords are invalid.");
        }

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void Parse_Invalid(string input, string scaleString) =>
            ClassicAssert.Throws<FormatException>(() => ChordProgression.Parse(input, Scale.Parse(scaleString)));

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void TryParse_Invalid(string input, string scaleString)
        {
            ClassicAssert.IsFalse(
                ChordProgression.TryParse(input, Scale.Parse(scaleString), out var actualChordProgression),
                "Chord progression could be parsed.");
        }

        [Test]
        public void Parse_Invalid_EmptyOrNull([Values(null, "", "  ")] string input, [Values("C major")] string scaleString) =>
            ClassicAssert.Throws<ArgumentException>(() => ChordProgression.Parse(input, Scale.Parse(scaleString)));

        [Test]
        public void TryParse_Invalid_EmptyOrNull([Values(null, "", "  ")] string input, [Values("C major")] string scaleString) =>
            ClassicAssert.IsFalse(ChordProgression.TryParse(input, Scale.Parse(scaleString), out var actualChordProgression), $"Parsed invalid value '{input}'.");
    }
}
