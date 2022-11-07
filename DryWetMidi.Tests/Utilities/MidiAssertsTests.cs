using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    [TestFixture]
    public sealed class MidiAssertsTests
    {
        #region Test methods

        [Test]
        public void AreEqual_NotesCollections_Equal() =>
            MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) },
                new[] { new Note(SevenBitNumber.MaxValue) },
                "Notes aren't equal.");

        [Test]
        public void AreEqual_NotesCollections_NotEqual_NoteNumber() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) },
                new[] { new Note(SevenBitNumber.MinValue) },
                "Notes are equal."));

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Channel() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { Channel = (FourBitNumber)7 } },
                "Notes are equal."));

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Velocity() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)7 } },
                "Notes are equal."));

        [Test]
        public void AreEqual_NotesCollections_NotEqual_OffVelocity() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { OffVelocity = (SevenBitNumber)4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { OffVelocity = (SevenBitNumber)7 } },
                "Notes are equal."));

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Time() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { Time = 4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { Time = 7 } },
                "Notes are equal."));

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Length() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue) { Length = 4 } },
                new[] { new Note(SevenBitNumber.MaxValue) { Length = 7 } },
                "Notes are equal."));

        [Test]
        public void AreEqual_NotesCollections_NotEqual_Order() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Note(SevenBitNumber.MaxValue), new Note(SevenBitNumber.MinValue) },
                new[] { new Note(SevenBitNumber.MinValue), new Note(SevenBitNumber.MaxValue) },
                "Notes are equal."));

        [Test]
        public void AreEqual_ChordsCollections_Equal() =>
            MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue)) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue)) },
                "Chords aren't equal.");

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_NoteNumber() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue)) },
                new[] { new Chord(new Note(SevenBitNumber.MinValue)) },
                "Chords are equal."));

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_Channel() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 }) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Channel = (FourBitNumber)7 }) },
                "Chords are equal."));

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_Velocity() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)4 }) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)7 }) },
                "Chords are equal."));

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_OffVelocity() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { OffVelocity = (SevenBitNumber)4 }) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { OffVelocity = (SevenBitNumber)7 }) },
                "Chords are equal."));

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_Time() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Time = 4 }) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Time = 7 }) },
                "Chords are equal."));

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_Length() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Length = 4 }) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue) { Length = 7 }) },
                "Chords are equal."));

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_NotesCount() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue), new Note(SevenBitNumber.MinValue)) },
                new[] { new Chord(new Note(SevenBitNumber.MaxValue)) },
                "Chords are equal."));

        [Test]
        public void AreEqual_ChordsCollections_NotEqual_Order() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new[] { new Chord(new Note(SevenBitNumber.MaxValue)), new Chord(new Note(SevenBitNumber.MinValue)) },
                new[] { new Chord(new Note(SevenBitNumber.MinValue)), new Chord(new Note(SevenBitNumber.MaxValue)) },
                "Chords are equal."));

        [Test]
        public void AreEqual_Files_Equal_1() =>
            MidiAsserts.AreEqual(
                new MidiFile(),
                new MidiFile(),
                false,
                "Files aren't equal.");

        [Test]
        public void AreEqual_Files_Equal_2() =>
            MidiAsserts.AreEqual(
                new MidiFile(new TrackChunk()),
                new MidiFile(new TrackChunk()),
                false,
                "Files aren't equal.");

        [Test]
        public void AreEqual_Files_Equal_3() =>
            MidiAsserts.AreEqual(
                new MidiFile(new TrackChunk(new TextEvent("A"))),
                new MidiFile(new TrackChunk(new TextEvent("A"))),
                true,
                "Files aren't equal.");

        [Test]
        public void AreEqual_Files_Equal_4() =>
            MidiAsserts.AreEqual(
                new MidiFile(new TrackChunk(new TextEvent("A")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(180)
                },
                new MidiFile(new TrackChunk(new TextEvent("A")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(180)
                },
                true,
                "Files aren't equal.");

        [Test]
        public void AreEqual_Files_NotEqual_1() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new MidiFile(),
                new MidiFile(new TrackChunk()),
                false,
                "Files are equal."));

        [Test]
        public void AreEqual_Files_NotEqual_2() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new MidiFile(new TrackChunk()),
                new MidiFile(new TrackChunk(new TextEvent("A"))),
                false,
                "Files are equal."));

        [Test]
        public void AreEqual_Files_NotEqual_3() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new MidiFile(new TrackChunk(new TextEvent("A"))),
                new MidiFile(new TrackChunk(new TextEvent("B"))),
                true,
                "Files are equal."));

        [Test]
        public void AreEqual_Files_NotEqual_4() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new MidiFile(new TrackChunk(new TextEvent("A"))),
                new MidiFile(new TrackChunk(new ProgramChangeEvent((SevenBitNumber)90))),
                true,
                "Files are equal."));

        [Test]
        public void AreEqual_Files_NotEqual_5() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                new MidiFile(new TrackChunk(new TextEvent("A")))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(180)
                },
                new MidiFile(new TrackChunk(new TextEvent("A"))),
                true,
                "Files are equal."));

        [Test]
        public void AreEqual_TempoMap_Equal_1() =>
            MidiAsserts.AreEqual(
                TempoMap.Default,
                TempoMap.Default,
                "Tempo maps aren't equal.");

        [Test]
        public void AreEqual_TempoMap_Equal_2() =>
            MidiAsserts.AreEqual(
                TempoMap.Create(new Tempo(100000), new TimeSignature(2, 4)),
                TempoMap.Create(new Tempo(100000), new TimeSignature(2, 4)),
                "Tempo maps aren't equal.");

        [Test]
        public void AreEqual_TempoMap_Equal_3() =>
            MidiAsserts.AreEqual(
                TempoMap.Create(new TimeSignature(2, 4)),
                TempoMap.Create(new TimeSignature(2, 4)),
                "Tempo maps aren't equal.");

        [Test]
        public void AreEqual_TempoMap_Equal_4() =>
            MidiAsserts.AreEqual(
                TempoMap.Create(new Tempo(100000)),
                TempoMap.Create(new Tempo(100000)),
                "Tempo maps aren't equal.");

        [Test]
        public void AreEqual_TempoMap_Equal_5() =>
            MidiAsserts.AreEqual(
                TempoMap.Create(new TicksPerQuarterNoteTimeDivision(10), new Tempo(100000)),
                TempoMap.Create(new TicksPerQuarterNoteTimeDivision(10), new Tempo(100000)),
                "Tempo maps aren't equal.");

        [Test]
        public void AreEqual_TempoMap_Equal_6() =>
            MidiAsserts.AreEqual(
                TempoMap.Create(new TicksPerQuarterNoteTimeDivision(10), new TimeSignature(3, 4)),
                TempoMap.Create(new TicksPerQuarterNoteTimeDivision(10), new TimeSignature(3, 4)),
                "Tempo maps aren't equal.");

        [Test]
        public void AreEqual_TempoMap_NotEqual_1() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                TempoMap.Default,
                TempoMap.Create(new Tempo(100000)),
                "Tempo maps aren't equal."));

        [Test]
        public void AreEqual_TempoMap_NotEqual_2() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                TempoMap.Create(new Tempo(100000), new TimeSignature(2, 4)),
                TempoMap.Create(new Tempo(50000), new TimeSignature(2, 4)),
                "Tempo maps aren't equal."));

        [Test]
        public void AreEqual_TempoMap_NotEqual_3() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                TempoMap.Create(new TimeSignature(2, 4)),
                TempoMap.Create(new TimeSignature(2, 8)),
                "Tempo maps aren't equal."));

        [Test]
        public void AreEqual_TempoMap_NotEqual_4() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                TempoMap.Create(new Tempo(100000)),
                TempoMap.Create(new Tempo(10000)),
                "Tempo maps aren't equal."));

        [Test]
        public void AreEqual_TempoMap_NotEqual_5() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                TempoMap.Create(new TicksPerQuarterNoteTimeDivision(10), new Tempo(100000)),
                TempoMap.Create(new TicksPerQuarterNoteTimeDivision(50), new Tempo(100000)),
                "Tempo maps aren't equal."));

        [Test]
        public void AreEqual_TempoMap_NotEqual_6() => Assert.Throws<AssertionException>(() =>
            MidiAsserts.AreEqual(
                TempoMap.Create(new TicksPerQuarterNoteTimeDivision(10), new TimeSignature(3, 4)),
                TempoMap.Create(new TicksPerQuarterNoteTimeDivision(10), new TimeSignature(2, 4)),
                "Tempo maps aren't equal."));

        #endregion
    }
}
