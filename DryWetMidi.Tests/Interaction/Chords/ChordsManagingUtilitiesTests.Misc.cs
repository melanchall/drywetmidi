using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class ChordsManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void GetMusicTheoryChord()
        {
            var chord = new Chord(
                new Note(DryWetMidi.MusicTheory.NoteName.C, 2),
                new Note(DryWetMidi.MusicTheory.NoteName.A, 1),
                new Note(DryWetMidi.MusicTheory.NoteName.DSharp, 2));

            Assert.AreEqual(
                new DryWetMidi.MusicTheory.Chord(new[]
                {
                    DryWetMidi.MusicTheory.NoteName.A,
                    DryWetMidi.MusicTheory.NoteName.C,
                    DryWetMidi.MusicTheory.NoteName.DSharp
                }),
                chord.GetMusicTheoryChord(),
                "Chord is invalid.");
        }

        #endregion
    }
}
