using System;
using System.Threading;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class OctaveTests
    {
        private static readonly object[] ParseData_Valid = new[]
        {
            new object[] { "5", Octave.Get(5) },
            new object[] { "3", Octave.Get(3) },
            new object[] { "3  ", Octave.Get(3) },
            new object[] { "-1", Octave.Get(-1) },
            new object[] { "+1", Octave.Get(1) },
            new object[] { "9", Octave.Get(9) },
            new object[] { "  9 ", Octave.Get(9) },
        };

        private static readonly object[] ParseData_Invalid = new[]
        {
            new object[] { "-2" },
            new object[] { "23" },
            new object[] { "10" },
            new object[] { "0 9" },
            new object[] { "a1" },
            new object[] { "1a" },
            new object[] { "abc" },
        };

        #region Test methods

        [TestCaseSource(nameof(ParseData_Valid))]
        public void Parse_Valid(string input, Octave expectedOctave)
        {
            var parsedOctave = Octave.Parse(input);
            ClassicAssert.AreEqual(expectedOctave, parsedOctave, "Parsed octave is invalid.");
        }

        [TestCaseSource(nameof(ParseData_Valid))]
        public void TryParse_Valid(string input, Octave expectedOctave)
        {
            ClassicAssert.IsTrue(Octave.TryParse(input, out var octave), "Octave was not parsed successfully.");
            ClassicAssert.AreEqual(expectedOctave, octave, "Parsed octave is invalid.");
        }

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void Parse_Invalid(string input) =>
            ClassicAssert.Throws<FormatException>(() => Octave.Parse(input), "Invalid octave parsed.");

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void TryParse_Invalid(string input) =>
            ClassicAssert.IsFalse(Octave.TryParse(input, out var octave), "Octave was parsed successfully.");

        [Test]
        public void Parse_Invalid_EmptyOrNull([Values(null, "", "  ")] string input) =>
            ClassicAssert.Throws<ArgumentException>(() => Octave.Parse(input), "Octave parsing did not throw an exception.");

        [Test]
        public void TryParse_Invalid_EmptyOrNull([Values(null, "", "  ")] string input) =>
            ClassicAssert.IsFalse(Octave.TryParse(input, out var octave), $"Parsed invalid value '{input}'.");

        [Test]
        public void GetOctavesFromDifferentThreads()
        {
            var minOctaveNumber = Octave.MinOctaveNumber;
            var maxOctaveNumber = Octave.MinOctaveNumber;

            var thread1 = new Thread(() =>
            {
                for (var octaveNumber = minOctaveNumber; octaveNumber <= maxOctaveNumber; octaveNumber++)
                {
                    var octave = Octave.Get(octaveNumber);
                }
            });

            var thread2 = new Thread(() =>
            {
                for (var octaveNumber = maxOctaveNumber; octaveNumber >= minOctaveNumber; octaveNumber--)
                {
                    var octave = Octave.Get(octaveNumber);
                }
            });

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();
        }

        #endregion
    }
}
