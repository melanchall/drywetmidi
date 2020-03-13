using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Interaction
{
    [TestFixture]
    public sealed class ReadingHandlerBenchmarks : BenchmarkTest
    {
        #region Nested classes

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_ReadFileWithTimedEventsReadingHandler
        {
            [Benchmark]
            public void ReadFileWithoutTimedEventsReadingHandler()
            {
                var midiFile = MidiFile.Read(TestFilesProvider.GetMiscFile_14000events());
                var timedEvents = midiFile.GetTimedEvents();
            }

            [Benchmark]
            public void ReadFileWithTimedEventsReadingHandler_SortEvents()
            {
                ReadFileWithTimedEventsReadingHandler(true);
            }

            [Benchmark]
            public void ReadFileWithTimedEventsReadingHandler_DontSortEvents()
            {
                ReadFileWithTimedEventsReadingHandler(false);
            }

            private void ReadFileWithTimedEventsReadingHandler(bool sortEvents)
            {
                var handler = new TimedEventsReadingHandler(sortEvents);
                var settings = new ReadingSettings();
                settings.ReadingHandlers.Add(handler);

                var midiFile = MidiFile.Read(TestFilesProvider.GetMiscFile_14000events(), settings);
                var timedEvents = handler.TimedEvents;
            }
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_ReadFileWithTempoMapReadingHandler
        {
            [Benchmark]
            public void ReadFileWithoutTempoMapReadingHandler()
            {
                var midiFile = MidiFile.Read(TestFilesProvider.GetMiscFile_14000events());
                var tempoMap = midiFile.GetTempoMap();
            }

            [Benchmark]
            public void ReadFileWithTempoMapReadingHandler()
            {
                var handler = new TempoMapReadingHandler();
                var settings = new ReadingSettings();
                settings.ReadingHandlers.Add(handler);

                var midiFile = MidiFile.Read(TestFilesProvider.GetMiscFile_14000events(), settings);
                var tempoMap = handler.TempoMap;
            }
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_ReadFileWithNotesReadingHandler
        {
            [Benchmark]
            public void ReadFileWithoutNotesReadingHandler()
            {
                var midiFile = MidiFile.Read(TestFilesProvider.GetMiscFile_14000events());
                var notes = midiFile.GetNotes();
            }

            [Benchmark]
            public void ReadFileWithNotesReadingHandler_SortNotes()
            {
                ReadFileWithNotesReadingHandler(true);
            }

            [Benchmark]
            public void ReadFileWithNotesReadingHandler_DontSortNotes()
            {
                ReadFileWithNotesReadingHandler(false);
            }

            private void ReadFileWithNotesReadingHandler(bool sortNotes)
            {
                var handler = new NotesReadingHandler(sortNotes);
                var settings = new ReadingSettings();
                settings.ReadingHandlers.Add(handler);

                var midiFile = MidiFile.Read(TestFilesProvider.GetMiscFile_14000events(), settings);
                var notes = handler.Notes;
            }
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_ReadFileWithReadingHandlers_Notes_TimedEvents_TempoMap
        {
            [Benchmark]
            public void ReadFileWithoutReadingHandlers()
            {
                var midiFile = MidiFile.Read(TestFilesProvider.GetMiscFile_14000events());
                var tempoMap = midiFile.GetTempoMap();
                var notes = midiFile.GetNotes();
                var timedEvents = midiFile.GetTimedEvents();
            }

            [Benchmark]
            public void ReadFileWithReadingHandlers_SortObjects()
            {
                ReadFileWithReadingHandlers(true);
            }

            [Benchmark]
            public void ReadFileWithReadingHandlers_DontSortObjects()
            {
                ReadFileWithReadingHandlers(false);
            }

            private void ReadFileWithReadingHandlers(bool sortObjects)
            {
                var notesReadingHandler = new NotesReadingHandler(sortObjects);
                var timedEventsReadingHandler = new TimedEventsReadingHandler(sortObjects);
                var tempoMapReadingHandler = new TempoMapReadingHandler();

                var settings = new ReadingSettings();
                settings.ReadingHandlers.Add(notesReadingHandler);
                settings.ReadingHandlers.Add(timedEventsReadingHandler);
                settings.ReadingHandlers.Add(tempoMapReadingHandler);

                var midiFile = MidiFile.Read(TestFilesProvider.GetMiscFile_14000events(), settings);
                var tempoMap = tempoMapReadingHandler.TempoMap;
                var timedEvents = timedEventsReadingHandler.TimedEvents;
                var notes = notesReadingHandler.Notes;
            }
        }

        #endregion

        #region Test methods

        [Test]
        public void ReadFileWithTimedEventsReadingHandler()
        {
            RunBenchmarks<Benchmarks_ReadFileWithTimedEventsReadingHandler>();
        }

        [Test]
        public void ReadFileWithTempoMapReadingHandler()
        {
            RunBenchmarks<Benchmarks_ReadFileWithTempoMapReadingHandler>();
        }

        [Test]
        public void ReadFileWithNotesReadingHandler()
        {
            RunBenchmarks<Benchmarks_ReadFileWithNotesReadingHandler>();
        }

        [Test]
        public void ReadFileWithReadingHandlers_Notes_TimedEvents_TempoMap()
        {
            RunBenchmarks<Benchmarks_ReadFileWithReadingHandlers_Notes_TimedEvents_TempoMap>();
        }

        #endregion
    }
}
