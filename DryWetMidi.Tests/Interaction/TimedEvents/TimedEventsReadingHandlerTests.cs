using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class TimedEventsReadingHandlerTests
    {
        #region Test methods

        [Test]
        public void CheckTimedEventsReadingHandler_EmptyFile()
        {
            var handler = ReadWithTimedEventsReadingHandler(new MidiFile(), false);
            CollectionAssert.IsEmpty(handler.TimedEvents, "Timed events collection is not empty.");
        }

        [Test]
        public void CheckTimedEventsReadingHandler_EmptyTrackChunks()
        {
            var handler = ReadWithTimedEventsReadingHandler(new MidiFile(new TrackChunk(), new TrackChunk()), false);
            CollectionAssert.IsEmpty(handler.TimedEvents, "Timed events collection is not empty.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CheckTimedEventsReadingHandler_SingleTrackChunk(bool sortEvents)
        {
            var handler = ReadWithTimedEventsReadingHandler(
                new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new NoteOffEvent() { DeltaTime = 100 },
                        new NoteOnEvent((SevenBitNumber)40, (SevenBitNumber)56) { DeltaTime = 50 },
                        new NoteOffEvent((SevenBitNumber)40, (SevenBitNumber)0) { DeltaTime = 100 })),
                sortEvents);

            var timedEvents = handler.TimedEvents;
            TimedEventEquality.AreEqual(
                timedEvents,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)40, (SevenBitNumber)56), 150),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)40, (SevenBitNumber)0), 200),
                },
                true);

            Assert.AreSame(timedEvents, handler.TimedEvents, "Timed events collection object is changed on second get.");
        }

        [Test]
        public void CheckTimedEventsReadingHandler_MultipleTrackChunks_Sort()
        {
            var handler = ReadWithTimedEventsReadingHandler(
                new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new NoteOffEvent() { DeltaTime = 100 },
                        new NoteOnEvent((SevenBitNumber)40, (SevenBitNumber)56) { DeltaTime = 50 },
                        new NoteOffEvent((SevenBitNumber)40, (SevenBitNumber)0) { DeltaTime = 100 }),
                    new TrackChunk(
                        new NoteOnEvent() { DeltaTime = 45 },
                        new NoteOffEvent() { DeltaTime = 100 },
                        new TextEvent("test") { DeltaTime = 50 },
                        new ControlChangeEvent((SevenBitNumber)40, (SevenBitNumber)45) { DeltaTime = 100 })),
                true);

            var timedEvents = handler.TimedEvents;
            TimedEventEquality.AreEqual(
                timedEvents,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOnEvent(), 45),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new NoteOffEvent(), 145),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)40, (SevenBitNumber)56), 150),
                    new TimedEvent(new TextEvent("test"), 195),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)40, (SevenBitNumber)0), 200),
                    new TimedEvent(new ControlChangeEvent((SevenBitNumber)40, (SevenBitNumber)45), 295),
                },
                true);

            Assert.AreSame(timedEvents, handler.TimedEvents, "Timed events collection object is changed on second get.");
        }

        [Test]
        public void CheckTimedEventsReadingHandler_MultipleTrackChunks_DontSort()
        {
            var handler = ReadWithTimedEventsReadingHandler(
                new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new NoteOffEvent() { DeltaTime = 100 },
                        new NoteOnEvent((SevenBitNumber)40, (SevenBitNumber)56) { DeltaTime = 50 },
                        new NoteOffEvent((SevenBitNumber)40, (SevenBitNumber)0) { DeltaTime = 100 }),
                    new TrackChunk(
                        new NoteOnEvent() { DeltaTime = 45 },
                        new NoteOffEvent() { DeltaTime = 100 },
                        new TextEvent("test") { DeltaTime = 50 },
                        new ControlChangeEvent((SevenBitNumber)40, (SevenBitNumber)45) { DeltaTime = 100 })),
                false);

            var timedEvents = handler.TimedEvents;
            TimedEventEquality.AreEqual(
                timedEvents,
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 100),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)40, (SevenBitNumber)56), 150),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)40, (SevenBitNumber)0), 200),
                    new TimedEvent(new NoteOnEvent(), 45),
                    new TimedEvent(new NoteOffEvent(), 145),
                    new TimedEvent(new TextEvent("test"), 195),
                    new TimedEvent(new ControlChangeEvent((SevenBitNumber)40, (SevenBitNumber)45), 295),
                },
                true);

            Assert.AreSame(timedEvents, handler.TimedEvents, "Timed events collection object is changed on second get.");
        }

        #endregion

        #region Private methods

        private TimedEventsReadingHandler ReadWithTimedEventsReadingHandler(MidiFile midiFile, bool sortEvents)
        {
            var timedEventsReadingHandler = new TimedEventsReadingHandler(sortEvents);
            MidiFileTestUtilities.ReadUsingHandlers(midiFile, timedEventsReadingHandler);
            return timedEventsReadingHandler;
        }

        #endregion
    }
}
