using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.Interaction.Note;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class NoteIdUtilitiesTests
    {
        #region Test methods

        #region GetNoteId

        [Test]
        public void GetNoteId_Note_Null()
        {
            Note note = null;
            Assert.Throws<ArgumentNullException>(
                () => NoteIdUtilities.GetNoteId(note),
                "The method did not throw a NullReferenceException when passed a null reference.");
        }

        [Test]
        public void GetNoteId_Note_ValidEmpty()
        {
            var note = new Note(new NoteName(), 1);
            var noteId = note.GetNoteId();
            Assert.NotNull(noteId, "The NoteId is null.");
            Assert.AreEqual(new FourBitNumber(0), noteId.Channel, "The NoteId Channel is not the expected value.");
            Assert.AreEqual(new SevenBitNumber(24), noteId.NoteNumber, "The NoteId NoteNumber is not the expected value.");
        }

        [Test]
        public void GetNoteId_Note_ValidSet()
        {
            var note = new Note(new SevenBitNumber(2))
            {
                Channel = new FourBitNumber(1)
            };
            var noteId = note.GetNoteId();
            Assert.NotNull(noteId, "The NoteId is null.");
            Assert.AreEqual(new FourBitNumber(1), noteId.Channel, "The NoteId Channel is not the expected value.");
            Assert.AreEqual(new SevenBitNumber(2), noteId.NoteNumber, "The NoteId NoteNumber is not the expected value.");
        }

        [Test]
        public void GetNoteId_NoteEvent_Null()
        {
            NoteOnEvent noteEvent = null;
            Assert.Throws<ArgumentNullException>(
                () => NoteIdUtilities.GetNoteId(noteEvent),
                "The method did not throw a NullReferenceException when passed a null reference.");
        }

        [Test]
        public void GetNoteId_NoteEvent_ValidEmpty()
        {
            var noteEvent = new NoteOnEvent();
            var noteId = noteEvent.GetNoteId();
            Assert.NotNull(noteId, "The NoteId is null.");
            Assert.AreEqual(new FourBitNumber(0), noteId.Channel, "The NoteId Channel is not the expected value.");
            Assert.AreEqual(new SevenBitNumber(0), noteId.NoteNumber, "The NoteId NoteNumber is not the expected value.");
        }

        [Test]
        public void GetNoteId_NoteEvent_ValidSet()
        {
            var noteEvent = new NoteOnEvent
            {
                Channel = new FourBitNumber(1),
                NoteNumber = new SevenBitNumber(2)
            };
            var noteId = noteEvent.GetNoteId();
            Assert.NotNull(noteId, "The NoteId is null.");
            Assert.AreEqual(new FourBitNumber(1), noteId.Channel, "The NoteId Channel is not the expected value.");
            Assert.AreEqual(new SevenBitNumber(2), noteId.NoteNumber, "The NoteId NoteNumber is not the expected value.");
        }

        #endregion

        #endregion
    }
}
