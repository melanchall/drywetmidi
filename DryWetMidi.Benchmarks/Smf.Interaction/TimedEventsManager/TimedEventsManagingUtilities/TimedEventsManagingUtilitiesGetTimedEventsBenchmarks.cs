using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Smf.Interaction
{
    [TestFixture]
    public class TimedEventsManagingUtilitiesGetTimedEventsBenchmarks : BenchmarkTest
    {
        #region Nested classes

        [ClrJob]
        public class Benchmarks
        {
            private static readonly IEnumerable<TimedEvent> _midiFileTimedEvents = CreateTestFile().GetTimedEvents();

            [Benchmark]
            public void GetTimedEvents_MidiFile()
            {
                const int iterationsNumber = 10;

                for (int i = 0; i < iterationsNumber; i++)
                {
                    foreach (var timedEvent in _midiFileTimedEvents)
                    {
                    }
                }
            }

            private static MidiFile CreateTestFile()
            {
                const int trackChunksNumber = 100;
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
            RunBenchmarks<Benchmarks>();
        }

        #endregion
    }
}
