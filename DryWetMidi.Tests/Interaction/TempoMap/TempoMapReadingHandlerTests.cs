using System;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class TempoMapReadingHandlerTests
    {
        #region Test methods

        [Test]
        public void CheckTempoMapReadingHandler_EmptyFile()
        {
            var handler = ReadWithTempoMapReadingHandler(new MidiFile());
            CollectionAssert.IsEmpty(handler.TempoMap.GetTempoChanges(), "Tempo changes collection is not empty.");
            CollectionAssert.IsEmpty(handler.TempoMap.GetTimeSignatureChanges(), "Time signature changes collection is not empty.");
        }

        [Test]
        public void CheckTempoMapReadingHandler_EmptyTrackChunks()
        {
            var handler = ReadWithTempoMapReadingHandler(new MidiFile(new TrackChunk(), new TrackChunk()));
            CollectionAssert.IsEmpty(handler.TempoMap.GetTempoChanges(), "Tempo changes collection is not empty.");
            CollectionAssert.IsEmpty(handler.TempoMap.GetTimeSignatureChanges(), "Time signature changes collection is not empty.");
        }

        [Test]
        public void CheckTempoMapReadingHandler_SingleTrackChunk()
        {
            var timeDivision = new TicksPerQuarterNoteTimeDivision(100);
            var handler = ReadWithTempoMapReadingHandler(
                new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new SetTempoEvent(100000),
                        new SetTempoEvent(150000) { DeltaTime = 50 },
                        new SetTempoEvent(200000),
                        new NoteOffEvent() { DeltaTime = 100 },
                        new TimeSignatureEvent(3, 4))) { TimeDivision = timeDivision });

            var tempoMap = handler.TempoMap;
            Assert.AreEqual(timeDivision, tempoMap.TimeDivision, "Time division is invalid.");
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<Tempo>(0, new Tempo(100000)),
                    new ValueChange<Tempo>(50, new Tempo(200000))
                },
                tempoMap.GetTempoChanges(),
                "Tempo changes collection contains invalid values.");
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<TimeSignature>(150, new TimeSignature(3, 4))
                },
                tempoMap.GetTimeSignatureChanges(),
                "Time signature changes collection contains invalid values.");

            Assert.AreSame(tempoMap, handler.TempoMap, "Tempo map object is changed on second get.");
        }

        [Test]
        public void CheckTempoMapReadingHandler_SingleTrackChunk_DontAllowTempoMapUsageDuringReading_AccessDuringReading()
        {
            var timeDivision = new TicksPerQuarterNoteTimeDivision(100);
            var handler = new TempoMapReadingHandler();

            var exceptionThrown = false;

            try
            {
                var tempoMap = handler.TempoMap;
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }

            MidiFileTestUtilities.ReadUsingHandlers(
                new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new SetTempoEvent(100000),
                        new SetTempoEvent(150000) { DeltaTime = 50 },
                        new SetTempoEvent(200000),
                        new NoteOffEvent() { DeltaTime = 100 },
                        new TimeSignatureEvent(3, 4))) { TimeDivision = timeDivision },
                handler);

            Assert.IsTrue(exceptionThrown, "Exception was not thrown on get during read.");
            Assert.DoesNotThrow(() => { var tempoMap = handler.TempoMap; }, "Exception thrown on get after read.");
        }

        [Test]
        public void CheckTempoMapReadingHandler_MultipleTrackChunks()
        {
            var timeDivision = new TicksPerQuarterNoteTimeDivision(100);
            var handler = ReadWithTempoMapReadingHandler(
                new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new SetTempoEvent(100000),
                        new SetTempoEvent(150000) { DeltaTime = 50 },
                        new SetTempoEvent(200000),
                        new NoteOffEvent() { DeltaTime = 100 },
                        new TimeSignatureEvent(3, 4)),
                    new TrackChunk(
                        new SetTempoEvent(300000) { DeltaTime = 50 },
                        new TimeSignatureEvent(5, 8) { DeltaTime = 1000 })) { TimeDivision = timeDivision });

            var tempoMap = handler.TempoMap;
            Assert.AreEqual(timeDivision, tempoMap.TimeDivision, "Time division is invalid.");
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<Tempo>(0, new Tempo(100000)),
                    new ValueChange<Tempo>(50, new Tempo(300000))
                },
                tempoMap.GetTempoChanges(),
                "Tempo changes collection contains invalid values.");
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<TimeSignature>(150, new TimeSignature(3, 4)),
                    new ValueChange<TimeSignature>(1050, new TimeSignature(5, 8))
                },
                tempoMap.GetTimeSignatureChanges(),
                "Time signature changes collection contains invalid values.");

            Assert.AreSame(tempoMap, handler.TempoMap, "Tempo map object is changed on second get.");
        }

        [Test]
        public void CheckTempoMapReadingHandler_MultipleTrackChunks_DontAllowTempoMapUsageDuringReading_AccessDuringReading()
        {
            var timeDivision = new TicksPerQuarterNoteTimeDivision(100);
            var handler = new TempoMapReadingHandler();

            var exceptionThrown = false;

            try
            {
                var tempoMap = handler.TempoMap;
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }

            MidiFileTestUtilities.ReadUsingHandlers(
                new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new SetTempoEvent(100000),
                        new SetTempoEvent(150000) { DeltaTime = 50 },
                        new SetTempoEvent(200000),
                        new NoteOffEvent() { DeltaTime = 100 },
                        new TimeSignatureEvent(3, 4)),
                    new TrackChunk(
                        new SetTempoEvent(300000) { DeltaTime = 50 },
                        new TimeSignatureEvent(5, 8) { DeltaTime = 1000 })) { TimeDivision = timeDivision },
                handler);

            Assert.IsTrue(exceptionThrown, "Exception was not thrown on get during read.");
            Assert.DoesNotThrow(() => { var tempoMap = handler.TempoMap; }, "Exception thrown on get after read.");
        }

        #endregion

        #region Private methods

        private TempoMapReadingHandler ReadWithTempoMapReadingHandler(MidiFile midiFile)
        {
            var tempoMapReadingHandler = new TempoMapReadingHandler();
            MidiFileTestUtilities.ReadUsingHandlers(midiFile, tempoMapReadingHandler);
            return tempoMapReadingHandler;
        }

        #endregion
    }
}
