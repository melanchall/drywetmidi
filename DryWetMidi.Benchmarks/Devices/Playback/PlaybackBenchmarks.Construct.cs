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

        public abstract class Benchmarks_ConstructPlayback
        {
            private IEnumerable<ITimedObject> _timedObjects;

            protected abstract int NotesCount { get; }

            [GlobalSetup]
            public void GlobalSetup()
            {
                _timedObjects = GetTimedObjects(NotesCount);
            }

            [Benchmark]
            public void Construct()
            {
                using (var playback = new Playback(_timedObjects, TempoMap.Default)) { }
            }
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 2, targetCount: 3, launchCount: 4, invocationCount: 5)]
        public class Benchmarks_ConstructPlayback_Small : Benchmarks_ConstructPlayback
        {
            protected override int NotesCount => 10;
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 2, targetCount: 3, launchCount: 4, invocationCount: 5)]
        public class Benchmarks_ConstructPlayback_Middle : Benchmarks_ConstructPlayback
        {
            protected override int NotesCount => 100;
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 2, targetCount: 3, launchCount: 4, invocationCount: 5)]
        public class Benchmarks_ConstructPlayback_Large : Benchmarks_ConstructPlayback
        {
            protected override int NotesCount => 1000;
        }

        #endregion

        #region Test methods

        [Test]
        public void Construct_Small()
        {
            RunBenchmarks<Benchmarks_ConstructPlayback_Small>();
        }

        [Test]
        public void Construct_Middle()
        {
            RunBenchmarks<Benchmarks_ConstructPlayback_Middle>();
        }

        [Test]
        public void Construct_Large()
        {
            RunBenchmarks<Benchmarks_ConstructPlayback_Large>();
        }

        #endregion
    }
}
