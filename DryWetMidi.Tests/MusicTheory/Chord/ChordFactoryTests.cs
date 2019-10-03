using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class ChordFactoryTests
    {
        #region Test methods

        [TestCase(NoteName.C, ChordQuality.Major, new[] { NoteName.C, NoteName.E, NoteName.G })]
        [TestCase(NoteName.F, ChordQuality.Major, new[] { NoteName.F, NoteName.A, NoteName.C })]
        [TestCase(NoteName.C, ChordQuality.Minor, new[] { NoteName.C, NoteName.DSharp, NoteName.G })]
        [TestCase(NoteName.F, ChordQuality.Minor, new[] { NoteName.F, NoteName.GSharp, NoteName.C })]
        [TestCase(NoteName.C, ChordQuality.Augmented, new[] { NoteName.C, NoteName.E, NoteName.GSharp })]
        [TestCase(NoteName.F, ChordQuality.Augmented, new[] { NoteName.F, NoteName.A, NoteName.CSharp })]
        [TestCase(NoteName.C, ChordQuality.Diminished, new[] { NoteName.C, NoteName.DSharp, NoteName.FSharp })]
        [TestCase(NoteName.F, ChordQuality.Diminished, new[] { NoteName.F, NoteName.GSharp, NoteName.B })]
        public void GetChord_Quality(NoteName rootNoteName, ChordQuality chordQuality, NoteName[] expectedNotesNames)
        {
            var chord = ChordFactory.GetChord(rootNoteName, chordQuality);
            CollectionAssert.AreEqual(expectedNotesNames, chord.NotesNames, "Notes names are invalid.");
        }

        #endregion
    }
}
