using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Interaction
{
    [TestFixture]
    public class TimedEventsManagingUtilitiesGetTimedEventsBenchmarks : BenchmarkTest
    {
        #region Nested classes

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_GetTimedEvents
        {
            private static readonly MidiFile _midiFile = CreateTestFile();

            [Benchmark]
            public void GetTimedEvents_MidiFile()
            {
                var timedEvents = _midiFile.GetTimedEvents();
            }

            private static MidiFile CreateTestFile()
            {
                const int trackChunksNumber = 10;
                const int eventsPerTrackChunk = 10000;

                var midiFile = new MidiFile();

                for (int i = 0; i < trackChunksNumber; i++)
                {
                    var trackChunk = new TrackChunk();

                    using (var timedEventsManager = trackChunk.ManageTimedEvents())
                    {
                        for (int j = 0; j < eventsPerTrackChunk; j++)
                        {
                            timedEventsManager.Events.Add(new TimedEvent(new SetTempoEvent(j + 1), j));
                        }
                    }

                    midiFile.Chunks.Add(trackChunk);
                }

                return midiFile;
            }
        }

        #endregion

        #region Test methods

        [Test]
        [Description("Benchmark TimedEventsManagingUtilities.GetTimedEvents method.")]
        public void GetTimedEvents()
        {
            RunBenchmarks<Benchmarks_GetTimedEvents>();
        }

        #endregion
    }
}
