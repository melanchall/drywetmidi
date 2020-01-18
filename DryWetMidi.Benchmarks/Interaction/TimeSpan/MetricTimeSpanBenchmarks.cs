using System;
using BenchmarkDotNet.Engines;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Interaction
{
    [TestFixture]
    public sealed class MetricTimeSpanBenchmarks : BenchmarkTest
    {
        #region Nested classes

        [InProcessSimpleJob(RunStrategy.Monitoring, launchCount: 5, warmupCount: 5, targetCount: 5, invocationCount: 5)]
        public class Benchmarks_Metric : TimeSpanBenchmarks<MetricTimeSpan>
        {
            #region Constants

            private static readonly Tempo FirstTempo = Tempo.FromBeatsPerMinute(130);
            private static readonly Tempo SecondTempo = Tempo.FromBeatsPerMinute(90);

            #endregion

            #region Overrides

            public override TempoMap TempoMap { get; } = GetTempoMap();

            #endregion

            #region Methods

            private static TempoMap GetTempoMap()
            {
                var changeTempoOffset = 5 * TimeOffset - 1;
                var maxTime = Math.Max((TimesCount - 1) * TimeOffset, (TimesCount - 1) * TimeOffset + Length);

                bool firstTempo = true;

                using (var tempoMapManager = new TempoMapManager())
                {
                    var time = 0L;

                    while (time < maxTime)
                    {
                        tempoMapManager.SetTempo(time, firstTempo ? FirstTempo : SecondTempo);

                        firstTempo = !firstTempo;
                        time += changeTempoOffset;
                    }

                    return tempoMapManager.TempoMap;
                }
            }

            #endregion
        }

        #endregion

        #region Test methods

        [Test]
        [Description("Benchmark metric time/length conversion.")]
        public void ConvertMetricTimeSpan()
        {
            RunBenchmarks<Benchmarks_Metric>();
        }

        #endregion
    }
}
