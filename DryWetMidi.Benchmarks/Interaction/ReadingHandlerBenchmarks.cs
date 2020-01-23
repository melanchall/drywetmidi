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

        #endregion

        #region Test methods

        [Test]
        public void ReadFileWithTimedEventsReadingHandler()
        {
            RunBenchmarks<Benchmarks_ReadFileWithTimedEventsReadingHandler>();
        }

        #endregion
    }
}
