using System.Linq;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class ChordProgressionTests
    {
        #region Test methods

        [TestCase("I-II-IV", "C major", new[] { "C", "D", "F" })]
        [TestCase("Im-II7-V", "C major", new[] { "Cm", "D7", "G" })]
        [TestCase("I - bVI - III - bVII", "C major", new[] { "C", "G#", "E", "A#" })]
        public void Parse(string input, string scaleString, string[] expectedChords)
        {
            var chordProgression = ChordProgression.Parse(input, Scale.Parse(scaleString));
            CollectionAssert.AreEqual(
                expectedChords.Select(c => Chord.Parse(c)).ToArray(),
                chordProgression.Chords,
                "Chords are invalid.");
        }

        #endregion
    }
}
