using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
    {
        #region Nested classes

        private abstract class BaseReadingHandler : ReadingHandler
        {
            #region Constructor

            public BaseReadingHandler(TargetScope scope)
                : base(scope)
            {
            }

            #endregion

            #region Properties

            public int BadHandledCount { get; private set; }

            #endregion

            #region Overrides

            public override void OnFinishEventReading(MidiEvent midiEvent, long absoluteTime)
            {
                BadHandledCount++;
            }

            public override void OnFinishFileReading(MidiFile midiFile)
            {
                BadHandledCount++;
            }

            public override void OnFinishHeaderChunkReading(TimeDivision timeDivision)
            {
                BadHandledCount++;
            }

            public override void OnFinishTrackChunkReading(TrackChunk trackChunk)
            {
                BadHandledCount++;
            }

            public override void OnStartFileReading()
            {
                BadHandledCount++;
            }

            public override void OnStartTrackChunkContentReading(TrackChunk trackChunk)
            {
                BadHandledCount++;
            }

            public override void OnStartTrackChunkReading()
            {
                BadHandledCount++;
            }

            #endregion
        }

        private sealed class FileReadingHandler : BaseReadingHandler
        {
            #region Constructor

            public FileReadingHandler()
                : base(TargetScope.File)
            {
            }

            #endregion

            #region Properties

            public int StartHandledCount { get; private set; }

            public int EndHandledCount { get; private set; }

            public TimeDivision TimeDivision { get; private set; }

            #endregion

            #region Overrides

            public override void OnStartFileReading()
            {
                StartHandledCount++;
            }

            public override void OnFinishFileReading(MidiFile midiFile)
            {
                EndHandledCount++;
            }

            public override void OnFinishHeaderChunkReading(TimeDivision timeDivision)
            {
                TimeDivision = timeDivision;
            }

            #endregion
        }

        private sealed class TrackChunkReadingHandler : BaseReadingHandler
        {
            #region Constructor

            public TrackChunkReadingHandler()
                : base(TargetScope.TrackChunk)
            {
            }

            #endregion

            #region Properties

            public int StartHandledCount { get; private set; }

            public int ContentStartHandledCount { get; private set; }

            public int EndHandledCount { get; private set; }

            #endregion

            #region Overrides

            public override void OnStartTrackChunkReading()
            {
                StartHandledCount++;
            }

            public override void OnStartTrackChunkContentReading(TrackChunk trackChunk)
            {
                ContentStartHandledCount++;
            }

            public override void OnFinishTrackChunkReading(TrackChunk trackChunk)
            {
                EndHandledCount++;
            }

            #endregion
        }

        private sealed class EventReadingHandler : BaseReadingHandler
        {
            #region Constructor

            public EventReadingHandler()
                : base(TargetScope.Event)
            {
            }

            #endregion

            #region Properties

            public int HandledCount { get; private set; }

            #endregion

            #region Overrides

            public override void OnFinishEventReading(MidiEvent midiEvent, long absoluteTime)
            {
                HandledCount++;
            }

            #endregion
        }

        private sealed class MixedReadingHandler : ReadingHandler
        {
            #region Constructor

            public MixedReadingHandler()
                : base(TargetScope.File | TargetScope.Event | TargetScope.TrackChunk)
            {
            }

            #endregion

            #region Properties

            public int FileStartHandledCount { get; private set; }

            public int FileEndHandledCount { get; private set; }

            public TimeDivision FileTimeDivision { get; private set; }

            public int TrackChunkStartHandledCount { get; private set; }

            public int TrackChunkContentStartHandledCount { get; private set; }

            public int TrackChunkEndHandledCount { get; private set; }

            public int EventHandledCount { get; private set; }

            #endregion

            #region Overrides

            public override void OnFinishEventReading(MidiEvent midiEvent, long absoluteTime)
            {
                EventHandledCount++;
            }

            public override void OnFinishFileReading(MidiFile midiFile)
            {
                FileEndHandledCount++;
            }

            public override void OnFinishHeaderChunkReading(TimeDivision timeDivision)
            {
                FileTimeDivision = timeDivision;
            }

            public override void OnFinishTrackChunkReading(TrackChunk trackChunk)
            {
                TrackChunkEndHandledCount++;
            }

            public override void OnStartFileReading()
            {
                FileStartHandledCount++;
            }

            public override void OnStartTrackChunkContentReading(TrackChunk trackChunk)
            {
                TrackChunkContentStartHandledCount++;
            }

            public override void OnStartTrackChunkReading()
            {
                TrackChunkStartHandledCount++;
            }

            #endregion
        }

        #endregion

        #region Test methods

        [Test]
        public void Read_Handler_File()
        {
            var timeDivision = new TicksPerQuarterNoteTimeDivision(1000);
            var handler = new FileReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(new MidiFile { TimeDivision = timeDivision }, handler);
            Assert.AreEqual(1, handler.StartHandledCount, "Start Handled Count is invalid.");
            Assert.AreEqual(1, handler.EndHandledCount, "End Handled Count is invalid.");
            Assert.AreEqual(timeDivision, handler.TimeDivision, "Time division is invalid.");
            Assert.AreEqual(0, handler.BadHandledCount, "Scope wasn't used correctly.");
        }

        [Test]
        public void Read_Handler_TrackChunk()
        {
            var handler = new TrackChunkReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(new MidiFile(new TrackChunk(), new TrackChunk()), handler);
            Assert.AreEqual(2, handler.StartHandledCount, "Start Handled Count is invalid.");
            Assert.AreEqual(2, handler.ContentStartHandledCount, "Content Start Handled Count is invalid.");
            Assert.AreEqual(2, handler.EndHandledCount, "End Handled Count is invalid.");
            Assert.AreEqual(0, handler.BadHandledCount, "Scope wasn't used correctly.");
        }

        [Test]
        public void Read_Handler_Event_EmptyFile()
        {
            var handler = new EventReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(new MidiFile(), handler);
            Assert.AreEqual(0, handler.HandledCount, "Handled Count is invalid.");
            Assert.AreEqual(0, handler.BadHandledCount, "Scope wasn't used correctly.");
        }

        [Test]
        public void Read_Handler_Event_EmptyTrackChunk()
        {
            var handler = new EventReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(new MidiFile(new TrackChunk()), handler);
            Assert.AreEqual(0, handler.HandledCount, "Handled Count is invalid.");
            Assert.AreEqual(0, handler.BadHandledCount, "Scope wasn't used correctly.");
        }

        [Test]
        public void Read_Handler_Event()
        {
            var handler = new EventReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(new MidiFile(new TrackChunk(new TextEvent("test"), new TextEvent("test 2")), new TrackChunk(), new TrackChunk(new SetTempoEvent(100000))), handler);
            Assert.AreEqual(3, handler.HandledCount, "Handled Count is invalid.");
            Assert.AreEqual(0, handler.BadHandledCount, "Scope wasn't used correctly.");
        }

        [Test]
        public void Read_Handler_AllHandlers()
        {
            var timeDivision = new TicksPerQuarterNoteTimeDivision(1000);

            var fileReadingHandler = new FileReadingHandler();
            var trackChunkReadingHandler = new TrackChunkReadingHandler();
            var eventReadingHandler = new EventReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(
                new MidiFile(
                    new TrackChunk(new TextEvent("test"), new TextEvent("test 2")),
                    new TrackChunk(),
                    new TrackChunk(new SetTempoEvent(100000)),
                    new TrackChunk())
                { TimeDivision = timeDivision },
                fileReadingHandler,
                trackChunkReadingHandler,
                eventReadingHandler);

            Assert.AreEqual(1, fileReadingHandler.StartHandledCount, "File: Start Handled Count is invalid.");
            Assert.AreEqual(1, fileReadingHandler.EndHandledCount, "File: End Handled Count is invalid.");
            Assert.AreEqual(timeDivision, fileReadingHandler.TimeDivision, "File: Time division is invalid.");
            Assert.AreEqual(0, fileReadingHandler.BadHandledCount, "File: Scope wasn't used correctly.");

            Assert.AreEqual(4, trackChunkReadingHandler.StartHandledCount, "Track chunk: Start Handled Count is invalid.");
            Assert.AreEqual(4, trackChunkReadingHandler.ContentStartHandledCount, "Track chunk: Content Start Handled Count is invalid.");
            Assert.AreEqual(4, trackChunkReadingHandler.EndHandledCount, "Track chunk: End Handled Count is invalid.");
            Assert.AreEqual(0, trackChunkReadingHandler.BadHandledCount, "Track chunk: Scope wasn't used correctly.");

            Assert.AreEqual(3, eventReadingHandler.HandledCount, "Event: Handled Count is invalid.");
            Assert.AreEqual(0, eventReadingHandler.BadHandledCount, "Event: Scope wasn't used correctly.");
        }

        [Test]
        public void Read_Handler_Mixed()
        {
            var timeDivision = new TicksPerQuarterNoteTimeDivision(1000);

            var handler = new MixedReadingHandler();

            MidiFileTestUtilities.ReadUsingHandlers(
                new MidiFile(
                    new TrackChunk(new TextEvent("test"), new TextEvent("test 2")),
                    new TrackChunk(),
                    new TrackChunk(new SetTempoEvent(100000)),
                    new TrackChunk())
                { TimeDivision = timeDivision },
                handler);

            Assert.AreEqual(1, handler.FileStartHandledCount, "File: Start Handled Count is invalid.");
            Assert.AreEqual(1, handler.FileEndHandledCount, "File: End Handled Count is invalid.");
            Assert.AreEqual(timeDivision, handler.FileTimeDivision, "File: Time division is invalid.");

            Assert.AreEqual(4, handler.TrackChunkStartHandledCount, "Track chunk: Start Handled Count is invalid.");
            Assert.AreEqual(4, handler.TrackChunkContentStartHandledCount, "Track chunk: Content Start Handled Count is invalid.");
            Assert.AreEqual(4, handler.TrackChunkEndHandledCount, "Track chunk: End Handled Count is invalid.");

            Assert.AreEqual(3, handler.EventHandledCount, "Event: Handled Count is invalid.");
        }

        #endregion
    }
}
