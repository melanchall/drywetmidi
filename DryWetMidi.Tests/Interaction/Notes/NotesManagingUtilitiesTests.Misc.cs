using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class NotesManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void GetMusicTheoryNote()
        {
            var note = new Note(DryWetMidi.MusicTheory.NoteName.A, 1);

            Assert.AreEqual(
                DryWetMidi.MusicTheory.Note.Get(DryWetMidi.MusicTheory.NoteName.A, 1),
                note.GetMusicTheoryNote(),
                "Note is invalid.");
        }

        #endregion
    }
}
