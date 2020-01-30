using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class NotesReadingHandlerTests
    {
        #region Test methods

        [Test]
        public void CheckNotesReadingHandler_EmptyFile()
        {
            var handler = ReadWithNotesReadingHandler(new MidiFile(), false);
            CollectionAssert.IsEmpty(handler.Notes, "Notes collection is not empty.");
        }

        [Test]
        public void CheckNotesReadingHandler_EmptyTrackChunks()
        {
            var handler = ReadWithNotesReadingHandler(new MidiFile(new TrackChunk(), new TrackChunk()), false);
            CollectionAssert.IsEmpty(handler.Notes, "Notes collection is not empty.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CheckNotesReadingHandler_SingleTrackChunk(bool sortNotes)
        {
            var handler = ReadWithNotesReadingHandler(
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(1234),
                        new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { DeltaTime = 10, Channel = (FourBitNumber)1 },
                        new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)70) { DeltaTime = 10, Channel = (FourBitNumber)1 },
                        new PitchBendEvent(123) { DeltaTime = 10 },
                        new MarkerEvent("Marker") { DeltaTime = 10 },
                        new NoteOnEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 },
                        new MarkerEvent("Marker 2") { DeltaTime = 10 },
                        new TextEvent("Text") { DeltaTime = 10 },
                        new TextEvent("Text 2") { DeltaTime = 10 },
                        new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)1) { Channel = (FourBitNumber)10 },
                        new CuePointEvent("Point") { DeltaTime = 10 },
                        new NoteOffEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 },
                        new NoteOffEvent((SevenBitNumber)1, (SevenBitNumber)0) { DeltaTime = 10, Channel = (FourBitNumber)1 },
                        new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { Channel = (FourBitNumber)10 },
                        new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { DeltaTime = 10, Channel = (FourBitNumber)1 })),
                sortNotes);

            var notes = handler.Notes;
            NoteEquality.AreEqual(
                notes,
                new[]
                {
                    new Note((SevenBitNumber)1, 80, 10) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)100 },
                    new Note((SevenBitNumber)2, 80, 20) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)70 },
                    new Note((SevenBitNumber)3, 40, 40) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)1, OffVelocity = (SevenBitNumber)1 },
                    new Note((SevenBitNumber)2, 20, 70) { Channel = (FourBitNumber)10, Velocity = (SevenBitNumber)1 }
                });

            Assert.AreSame(notes, handler.Notes, "Notes collection object is changed on second get.");
        }

        [Test]
        public void CheckNotesReadingHandler_MultipleTrackChunks_Sort()
        {
            var handler = ReadWithNotesReadingHandler(
                new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)10),
                        new NoteOffEvent((SevenBitNumber)1, (SevenBitNumber)5) { DeltaTime = 100 },
                        new NoteOnEvent((SevenBitNumber)40, (SevenBitNumber)56) { DeltaTime = 50, Channel = (FourBitNumber)10 },
                        new NoteOffEvent((SevenBitNumber)40, (SevenBitNumber)0) { DeltaTime = 100, Channel = (FourBitNumber)10 }),
                    new TrackChunk(
                        new NoteOnEvent() { DeltaTime = 45 },
                        new NoteOffEvent() { DeltaTime = 100 },
                        new TextEvent("test") { DeltaTime = 50 },
                        new ControlChangeEvent((SevenBitNumber)40, (SevenBitNumber)45) { DeltaTime = 100 })),
                true);

            var notes = handler.Notes;
            NoteEquality.AreEqual(
                notes,
                new[]
                {
                    new Note((SevenBitNumber)1, 100, 0) { Velocity = (SevenBitNumber)10, OffVelocity = (SevenBitNumber)5 },
                    new Note((SevenBitNumber)40, 100, 150) { Velocity = (SevenBitNumber)56, Channel = (FourBitNumber)10 },
                    new Note((SevenBitNumber)0, 100, 45)
                });

            Assert.AreSame(notes, handler.Notes, "Notes collection object is changed on second get.");
        }

        [Test]
        public void CheckNotesReadingHandler_MultipleTrackChunks_DontSort()
        {
            var handler = ReadWithNotesReadingHandler(
                new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)10),
                        new NoteOffEvent((SevenBitNumber)1, (SevenBitNumber)5) { DeltaTime = 100 },
                        new NoteOnEvent((SevenBitNumber)40, (SevenBitNumber)56) { DeltaTime = 50, Channel = (FourBitNumber)10 },
                        new NoteOffEvent((SevenBitNumber)40, (SevenBitNumber)0) { DeltaTime = 100, Channel = (FourBitNumber)10 }),
                    new TrackChunk(
                        new NoteOnEvent() { DeltaTime = 45 },
                        new NoteOffEvent() { DeltaTime = 100 },
                        new TextEvent("test") { DeltaTime = 50 },
                        new ControlChangeEvent((SevenBitNumber)40, (SevenBitNumber)45) { DeltaTime = 100 })),
                false);

            var notes = handler.Notes;
            NoteEquality.AreEqual(
                notes,
                new[]
                {
                    new Note((SevenBitNumber)1, 100, 0) { Velocity = (SevenBitNumber)10, OffVelocity = (SevenBitNumber)5 },
                    new Note((SevenBitNumber)0, 100, 45),
                    new Note((SevenBitNumber)40, 100, 150) { Velocity = (SevenBitNumber)56, Channel = (FourBitNumber)10 }
                });

            Assert.AreSame(notes, handler.Notes, "Notes collection object is changed on second get.");
        }

        #endregion

        #region Private methods

        private NotesReadingHandler ReadWithNotesReadingHandler(MidiFile midiFile, bool sortNotes)
        {
            var notesReadingHandler = new NotesReadingHandler(sortNotes);
            MidiFileTestUtilities.ReadUsingHandlers(midiFile, notesReadingHandler);
            return notesReadingHandler;
        }

        #endregion
    }
}
