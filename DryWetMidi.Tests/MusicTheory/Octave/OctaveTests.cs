using System;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class OctaveTests
    {
        #region Test methods

        [TestCase("-2")]
        [TestCase("23")]
        public void Parse_Invalid(string input)
        {
            Assert.Throws<FormatException>(() => Octave.Parse(input), "Invalid octave parsed.");
        }

        [TestCase("5", 5)]
        [TestCase("3", 3)]
        [TestCase("-1", -1)]
        [TestCase("9", 9)]
        public void Parse(string input, int expectedOctaveNumber)
        {
            var parsedOctave = Octave.Parse(input);
            var expectedOctave = Octave.Get(expectedOctaveNumber);
            Assert.AreEqual(expectedOctave, parsedOctave, "Parsed octave is invalid.");
        }

        #endregion
    }
}
