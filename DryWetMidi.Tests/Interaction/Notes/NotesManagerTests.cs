using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
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

        [Test]
        public void Notes_Overlapped()
        {
            var trackChunk = new TrackChunk(
                new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)70),
                new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)50),
                new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0) { DeltaTime = 100 },
                new NoteOnEvent((SevenBitNumber)40, (SevenBitNumber)70),
                new NoteOffEvent((SevenBitNumber)40, (SevenBitNumber)0) { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)0));

            var expectedNotes = new[]
            {
                new Note((SevenBitNumber)30, 200, 0) { Velocity = (SevenBitNumber)70 },
                new Note((SevenBitNumber)60, 100, 0) { Velocity = (SevenBitNumber)50 },
                new Note((SevenBitNumber)40, 100, 100) { Velocity = (SevenBitNumber)70 }
            };

            using (var notesManager = trackChunk.ManageNotes())
            {
                var notes = notesManager.Notes;
                Assert.IsTrue(TimedObjectEquality.AreEqual(expectedNotes, notes, true));
            }
        }

        #endregion
    }
}
