using BenchmarkDotNet.Engines;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Interaction
{
    [TestFixture]
    public sealed class MidiTimeSpanBenchmarks : BenchmarkTest
    {
        #region Nested classes

        [InProcessSimpleJob(RunStrategy.Monitoring, launchCount: 5, warmupCount: 5, targetCount: 5, invocationCount: 5)]
        public class Benchmarks_Midi : TimeSpanBenchmarks<MidiTimeSpan>
        {
            #region Overrides

            public override TempoMap TempoMap { get; } = TempoMap.Default;

            #endregion
        }

        #endregion

        #region Test methods

        [Test]
        [Description("Benchmark metric time/length conversion.")]
        public void ConvertMidiTimeSpan()
        {
            RunBenchmarks<Benchmarks_Midi>();
        }

        #endregion
    }
}
