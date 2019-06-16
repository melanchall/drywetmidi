using BenchmarkDotNet.Attributes;
using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Smf.Interaction
{
    [TestFixture]
    public sealed class MidiTimeSpanBenchmarks : BenchmarkTest
    {
        #region Nested classes

        [ClrJob]
        public class Benchmarks : TimeSpanBenchmarks<MidiTimeSpan>
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
            RunBenchmarks<Benchmarks>();
        }

        #endregion
    }
}
