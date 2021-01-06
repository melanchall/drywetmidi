using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class TempoMapManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void GetTempoMap_EmptyFile()
        {
            GetTempoMap_Default(new MidiFile());
        }

        [Test]
        public void GetTempoMap_NoTempoAndTimeSignatureChanges_OneTrackChunk()
        {
            GetTempoMap_Default(new MidiFile(new TrackChunk(Enumerable.Range(0, 1000).Select(i => new NoteOnEvent { DeltaTime = 10 }))));
        }

        [Test]
        public void GetTempoMap_NoTempoAndTimeSignatureChanges_MultipleTrackChunks()
        {
            GetTempoMap_Default(new MidiFile(Enumerable.Range(0, 10).Select(i => new TrackChunk(Enumerable.Range(0, 1000).Select(j => new NoteOnEvent { DeltaTime = 10 })))));
        }

        [Test]
        public void GetTempoMap_OnlySetTempo_OneTrackChunk()
        {
            var midiFile = new MidiFile(new TrackChunk(new SetTempoEvent(10) { DeltaTime = 2 }, new SetTempoEvent(20) { DeltaTime = 2 }, new SetTempoEvent(10) { DeltaTime = 2 }));
            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<Tempo>(2, new Tempo(10)),
                    new ValueChange<Tempo>(4, new Tempo(20)),
                    new ValueChange<Tempo>(6, new Tempo(10)),
                },
                tempoMap.GetTempoChanges(),
                "Tempo changes are invalid.");
            CollectionAssert.IsEmpty(tempoMap.GetTimeSignatureChanges(), "There are time signature changes.");
            Assert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        [Test]
        public void GetTempoMap_OnlySetTempo_MultipleTrackChunks()
        {
            var midiFile = new MidiFile(Enumerable.Range(0, 2).Select(i => new TrackChunk(Enumerable.Range(0, 3).Select(j => new SetTempoEvent((i + 1) * 10) { DeltaTime = (i * 2) + 2 }))));
            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<Tempo>(2, new Tempo(10)),
                    new ValueChange<Tempo>(4, new Tempo(20)),
                    new ValueChange<Tempo>(6, new Tempo(10)),
                    new ValueChange<Tempo>(8, new Tempo(20)),
                },
                tempoMap.GetTempoChanges(),
                "Tempo changes are invalid.");
            CollectionAssert.IsEmpty(tempoMap.GetTimeSignatureChanges(), "There are time signature changes.");
            Assert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        [Test]
        public void GetTempoMap_OnlyTimeSignature_OneTrackChunk()
        {
            var midiFile = new MidiFile(new TrackChunk(new TimeSignatureEvent(2, 8) { DeltaTime = 2 }, new TimeSignatureEvent(3, 16) { DeltaTime = 2 }, new TimeSignatureEvent(2, 8) { DeltaTime = 2 }));
            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<TimeSignature>(2, new TimeSignature(2, 8)),
                    new ValueChange<TimeSignature>(4, new TimeSignature(3, 16)),
                    new ValueChange<TimeSignature>(6, new TimeSignature(2, 8)),
                },
                tempoMap.GetTimeSignatureChanges(),
                "Time signature changes are invalid.");
            CollectionAssert.IsEmpty(tempoMap.GetTempoChanges(), "There are tempo changes.");
            Assert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        [Test]
        public void GetTempoMap_OnlyTimeSignature_MultipleTrackChunks()
        {
            var midiFile = new MidiFile(Enumerable.Range(0, 2).Select(i => new TrackChunk(Enumerable.Range(0, 3).Select(j => new TimeSignatureEvent((byte)((i + 1) * 10), 8) { DeltaTime = (i * 2) + 2 }))));
            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<TimeSignature>(2, new TimeSignature(10, 8)),
                    new ValueChange<TimeSignature>(4, new TimeSignature(20, 8)),
                    new ValueChange<TimeSignature>(6, new TimeSignature(10, 8)),
                    new ValueChange<TimeSignature>(8, new TimeSignature(20, 8)),
                },
                tempoMap.GetTimeSignatureChanges(),
                "Time signature changes are invalid.");
            CollectionAssert.IsEmpty(tempoMap.GetTempoChanges(), "There are tempo changes.");
            Assert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        [Test]
        public void GetTempoMap_OneTrackChunk()
        {
            var midiFile = new MidiFile(new TrackChunk(
                new TimeSignatureEvent(2, 8) { DeltaTime = 2 },
                new SetTempoEvent(10) { DeltaTime = 3 },
                new TimeSignatureEvent(3, 16) { DeltaTime = 2 },
                new SetTempoEvent(20) { DeltaTime = 3 },
                new TimeSignatureEvent(2, 8) { DeltaTime = 2 },
                new SetTempoEvent(10) { DeltaTime = 3 }));
            
            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<TimeSignature>(2, new TimeSignature(2, 8)),
                    new ValueChange<TimeSignature>(7, new TimeSignature(3, 16)),
                    new ValueChange<TimeSignature>(12, new TimeSignature(2, 8)),
                },
                tempoMap.GetTimeSignatureChanges(),
                "Time signature changes are invalid.");
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<Tempo>(5, new Tempo(10)),
                    new ValueChange<Tempo>(10, new Tempo(20)),
                    new ValueChange<Tempo>(15, new Tempo(10)),
                },
                tempoMap.GetTempoChanges(),
                "Tempo changes are invalid.");
            Assert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        [Test]
        public void GetTempoMap_MultipleTrackChunks_TempoAndTimeSignatureChangesOnSeparateTrackChunks()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new TimeSignatureEvent(2, 8) { DeltaTime = 2 },
                    new TimeSignatureEvent(3, 16) { DeltaTime = 2 },
                    new TimeSignatureEvent(2, 8) { DeltaTime = 2 }),
                new TrackChunk(
                    new SetTempoEvent(10) { DeltaTime = 3 },
                    new SetTempoEvent(20) { DeltaTime = 3 },
                    new SetTempoEvent(10) { DeltaTime = 3 }));

            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<TimeSignature>(2, new TimeSignature(2, 8)),
                    new ValueChange<TimeSignature>(4, new TimeSignature(3, 16)),
                    new ValueChange<TimeSignature>(6, new TimeSignature(2, 8)),
                },
                tempoMap.GetTimeSignatureChanges(),
                "Time signature changes are invalid.");
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<Tempo>(3, new Tempo(10)),
                    new ValueChange<Tempo>(6, new Tempo(20)),
                    new ValueChange<Tempo>(9, new Tempo(10)),
                },
                tempoMap.GetTempoChanges(),
                "Tempo changes are invalid.");
            Assert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        [Test]
        public void GetTempoMap_MultipleTrackChunks_TempoAndTimeSignatureChangesAreMixed()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new TimeSignatureEvent(2, 8) { DeltaTime = 2 },
                    new SetTempoEvent(10) { DeltaTime = 3 },
                    new TimeSignatureEvent(3, 16) { DeltaTime = 2 }),
                new TrackChunk(
                    new SetTempoEvent(20) { DeltaTime = 3 },
                    new TimeSignatureEvent(2, 8) { DeltaTime = 2 },
                    new SetTempoEvent(10) { DeltaTime = 3 }));

            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<TimeSignature>(2, new TimeSignature(2, 8)),
                    new ValueChange<TimeSignature>(7, new TimeSignature(3, 16)),
                },
                tempoMap.GetTimeSignatureChanges(),
                "Time signature changes are invalid.");
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<Tempo>(3, new Tempo(20)),
                    new ValueChange<Tempo>(5, new Tempo(10)),
                },
                tempoMap.GetTempoChanges(),
                "Tempo changes are invalid.");
            Assert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        #endregion

        #region Private methods

        private void GetTempoMap_Default(MidiFile midiFile)
        {
            var tempoMap = midiFile.GetTempoMap();

            CollectionAssert.IsEmpty(tempoMap.GetTempoChanges(), "There are tempo changes.");
            Assert.AreEqual(Tempo.Default, tempoMap.GetTempoAtTime(new MidiTimeSpan()), "Tempo at the start is invalid.");
            Assert.AreEqual(Tempo.Default, tempoMap.GetTempoAtTime(new MidiTimeSpan(1000)), "Tempo at the middle is invalid.");

            CollectionAssert.IsEmpty(tempoMap.GetTimeSignatureChanges(), "There are time signature changes.");
            Assert.AreEqual(TimeSignature.Default, tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan()), "Time signature at the start is invalid.");
            Assert.AreEqual(TimeSignature.Default, tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(1000)), "Time signature at the middle is invalid.");
        }

        #endregion
    }
}
