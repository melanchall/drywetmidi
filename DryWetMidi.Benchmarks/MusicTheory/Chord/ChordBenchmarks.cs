using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.MusicTheory
{
    [TestFixture]
    public sealed class ChordBenchmarks : BenchmarkTest
    {
        #region Nested classes

        [InProcessSimpleJob(RunStrategy.Throughput, warmupCount: 10, targetCount: 10, launchCount: 10, invocationCount: 16)]
        public class Benchmarks_GetNames
        {
            [Benchmark]
            public void GetNames_CMajor()
            {
                for (int i = 0; i < 100; i++)
                {
                    var chord = new Chord(NoteName.C, NoteName.E, NoteName.G);
                    var names = chord.GetNames();
                }
            }

            [Benchmark]
            public void GetNames_CMajorExtended()
            {
                for (int i = 0; i < 100; i++)
                {
                    var chord = new Chord(NoteName.C, NoteName.E, NoteName.G, NoteName.C, NoteName.C);
                    var names = chord.GetNames();
                }
            }
        }

        #endregion

        #region Test methods

        [Test]
        public void GetNames()
        {
            RunBenchmarks<Benchmarks_GetNames>();
        }

        #endregion
    }
}
