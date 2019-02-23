using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestFixture]
    public sealed class NotesManagerTests
    {
        #region Test methods

        [Test]
        [Description("Check that NotesCollection is sorted when enumerated.")]
        public void Enumeration_Sorted()
        {
            using (var notesManager = new TrackChunk().ManageNotes())
            {
                var notes = notesManager.Notes;

                notes.Add(NoteTestUtilities.GetNoteByTime(123));
                notes.Add(NoteTestUtilities.GetNoteByTime(1));
                notes.Add(NoteTestUtilities.GetNoteByTime(10));
                notes.Add(NoteTestUtilities.GetNoteByTime(45));

                TimedObjectsCollectionTestUtilities.CheckTimedObjectsCollectionTimes(notes, 1, 10, 45, 123);
            }
        }

        #endregion
    }
}
