using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Devices
{
    [TestFixture]
    public sealed partial class PlaybackBenchmarks : BenchmarkTest
    {
        #region Nested classes

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput)]
        public class Benchmarks_StartPlayback_AfterConstruct
        {
            private static readonly IEnumerable<ITimedObject> TimedObjects = GetTimedObjects(1000);

            private Playback _playback;

            [IterationSetup]
            public void IterationSetup()
            {
                _playback = new Playback(TimedObjects, TempoMap.Default);
            }

            [IterationCleanup]
            public void IterationCleanup()
            {
                _playback.Stop();
                _playback.Dispose();
            }

            [Benchmark]
            public void StartAfterConstruct()
            {
                _playback.Start();
            }
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 2, targetCount: 3, launchCount: 4, invocationCount: 5)]
        public class Benchmarks_StartPlayback_AfterMove
        {
            private static readonly IEnumerable<ITimedObject> TimedObjects = GetTimedObjects(1000);

            private Playback _playback;

            [IterationSetup]
            public void IterationSetup()
            {
                _playback = new Playback(TimedObjects, TempoMap.Default);
                _playback.MoveToTime(_playback.GetDuration(TimeSpanType.Metric));
            }

            [IterationCleanup]
            public void IterationCleanup()
            {
                _playback.Stop();
                _playback.Dispose();
            }

            [Benchmark]
            public void StartAfterMove()
            {
                _playback.Start();
            }
        }

        #endregion

        #region Test methods

        [Test]
        public void Start_AfterConstruct()
        {
            RunBenchmarks<Benchmarks_StartPlayback_AfterConstruct>();
        }

        [Test]
        public void Start_AfterMove()
        {
            RunBenchmarks<Benchmarks_StartPlayback_AfterMove>();
        }

        #endregion
    }
}
