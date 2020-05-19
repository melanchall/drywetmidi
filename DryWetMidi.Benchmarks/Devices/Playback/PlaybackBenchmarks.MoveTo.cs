using System.Collections.Generic;
using System.Threading;
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

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 2, targetCount: 3, launchCount: 4, invocationCount: 5)]
        public class Benchmarks_Playback_MoveForward_NeverRun
        {
            private static readonly IEnumerable<ITimedObject> TimedObjects = GetTimedObjects(1000);

            private Playback _playback;
            private ITimeSpan _playbackDuration;

            [IterationSetup]
            public void IterationSetup()
            {
                _playback = new Playback(TimedObjects, TempoMap.Default);
                _playbackDuration = _playback.GetDuration(TimeSpanType.Metric);
            }

            [IterationCleanup]
            public void IterationCleanup()
            {
                _playback.Dispose();
            }

            [Benchmark]
            public void MoveToTime()
            {
                _playback.MoveToTime(_playbackDuration);
            }

            [Benchmark]
            public void MoveForward()
            {
                _playback.MoveForward(_playbackDuration);
            }
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 2, targetCount: 3, launchCount: 4, invocationCount: 5)]
        public class Benchmarks_Playback_MoveBack_NeverRun
        {
            private static readonly IEnumerable<ITimedObject> TimedObjects = GetTimedObjects(1000);
            private static readonly ITimeSpan Offset = new MetricTimeSpan(0, 0, 0, 1);

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
                _playback.Dispose();
            }

            [Benchmark]
            public void MoveBack()
            {
                _playback.MoveBack(Offset);
            }
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 2, targetCount: 3, launchCount: 4, invocationCount: 5)]
        public class Benchmarks_Playback_MoveForward_InProgress
        {
            private static readonly ITimeSpan InitialOffset = new MetricTimeSpan(0, 0, 1);
            private static readonly IEnumerable<ITimedObject> TimedObjects = GetTimedObjects(1000);

            private Playback _playback;
            private ITimeSpan _playbackDuration;

            [IterationSetup]
            public void IterationSetup()
            {
                _playback = new Playback(TimedObjects, TempoMap.Default);
                _playback.MoveToTime(InitialOffset);
                _playbackDuration = _playback.GetDuration(TimeSpanType.Metric);
                _playback.Start();
            }

            [IterationCleanup]
            public void IterationCleanup()
            {
                _playback.Stop();
                _playback.Dispose();
            }

            [Benchmark]
            public void MoveToTime()
            {
                _playback.MoveToTime(_playbackDuration);
            }

            [Benchmark]
            public void MoveForward()
            {
                _playback.MoveForward(_playbackDuration);
            }
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 2, targetCount: 3, launchCount: 4, invocationCount: 5)]
        public class Benchmarks_Playback_MoveBack_InProgress
        {
            private static readonly IEnumerable<ITimedObject> TimedObjects = GetTimedObjects(1000);
            private static readonly ITimeSpan InitialOffset = new MetricTimeSpan(0, 0, 10);
            private static readonly ITimeSpan Offset = new MetricTimeSpan(0, 0, 0, 1);

            private Playback _playback;

            [IterationSetup]
            public void IterationSetup()
            {
                _playback = new Playback(TimedObjects, TempoMap.Default);
                _playback.MoveToTime(_playback.GetDuration(TimeSpanType.Metric).Subtract(InitialOffset, TimeSpanMode.TimeLength));
                _playback.Start();
            }

            [IterationCleanup]
            public void IterationCleanup()
            {
                _playback.Dispose();
            }

            [Benchmark]
            public void MoveBack()
            {
                _playback.MoveBack(Offset);
            }
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 2, targetCount: 3, launchCount: 4, invocationCount: 5)]
        public class Benchmarks_Playback_MoveForward_AfterStop
        {
            private static readonly ITimeSpan InitialOffset = new MetricTimeSpan(0, 0, 1);
            private static readonly IEnumerable<ITimedObject> TimedObjects = GetTimedObjects(1000);

            private Playback _playback;
            private ITimeSpan _playbackDuration;

            [IterationSetup]
            public void IterationSetup()
            {
                _playback = new Playback(TimedObjects, TempoMap.Default);
                _playback.MoveToTime(InitialOffset);
                _playbackDuration = _playback.GetDuration(TimeSpanType.Metric);
                _playback.Start();
                Thread.Sleep(1000);
                _playback.Stop();
            }

            [IterationCleanup]
            public void IterationCleanup()
            {
                _playback.Stop();
                _playback.Dispose();
            }

            [Benchmark]
            public void MoveToTime()
            {
                _playback.MoveToTime(_playbackDuration);
            }

            [Benchmark]
            public void MoveForward()
            {
                _playback.MoveForward(_playbackDuration);
            }
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 2, targetCount: 3, launchCount: 4, invocationCount: 5)]
        public class Benchmarks_Playback_MoveBack_AfterStop
        {
            private static readonly IEnumerable<ITimedObject> TimedObjects = GetTimedObjects(1000);
            private static readonly ITimeSpan InitialOffset = new MetricTimeSpan(0, 0, 10);
            private static readonly ITimeSpan Offset = new MetricTimeSpan(0, 0, 0, 1);

            private Playback _playback;

            [IterationSetup]
            public void IterationSetup()
            {
                _playback = new Playback(TimedObjects, TempoMap.Default);
                _playback.MoveToTime(_playback.GetDuration(TimeSpanType.Metric).Subtract(InitialOffset, TimeSpanMode.TimeLength));
                _playback.Start();
                Thread.Sleep(1000);
                _playback.Stop();
            }

            [IterationCleanup]
            public void IterationCleanup()
            {
                _playback.Dispose();
            }

            [Benchmark]
            public void MoveBack()
            {
                _playback.MoveBack(Offset);
            }
        }

        #endregion

        #region Test methods

        [Test]
        public void MoveForward_NeverRun()
        {
            RunBenchmarks<Benchmarks_Playback_MoveForward_NeverRun>();
        }

        [Test]
        public void MoveBack_NeverRun()
        {
            RunBenchmarks<Benchmarks_Playback_MoveBack_NeverRun>();
        }

        [Test]
        public void MoveForward_InProgress()
        {
            RunBenchmarks<Benchmarks_Playback_MoveForward_InProgress>();
        }

        [Test]
        public void MoveBack_InProgress()
        {
            RunBenchmarks<Benchmarks_Playback_MoveBack_InProgress>();
        }

        [Test]
        public void MoveForward_AfterStop()
        {
            RunBenchmarks<Benchmarks_Playback_MoveForward_AfterStop>();
        }

        [Test]
        public void MoveBack_AfterStop()
        {
            RunBenchmarks<Benchmarks_Playback_MoveBack_AfterStop>();
        }

        #endregion
    }
}
