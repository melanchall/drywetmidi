using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class QuantizerTests
    {
        #region Nested classes

        private sealed class SkipQuantizer : Quantizer
        {
            protected override TimeProcessingInstruction OnObjectQuantizing(
                ITimedObject obj,
                QuantizedTime quantizedTime,
                IGrid grid,
                LengthedObjectTarget target,
                TempoMap tempoMap,
                QuantizingSettings settings)
            {
                return TimeProcessingInstruction.Skip;
            }
        }

        private sealed class FixedTimeQuantizer : Quantizer
        {
            #region Fields

            private readonly long _time;

            #endregion

            #region Constructor

            public FixedTimeQuantizer(long time)
            {
                _time = time;
            }

            #endregion

            #region Overrides

            protected override TimeProcessingInstruction OnObjectQuantizing(
                ITimedObject obj,
                QuantizedTime quantizedTime,
                IGrid grid,
                LengthedObjectTarget target,
                TempoMap tempoMap,
                QuantizingSettings settings)
            {
                return new TimeProcessingInstruction(_time);
            }

            #endregion
        }

        #endregion

        #region Test methods

        [Test]
        public void Quantize_Custom_Skip()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "10"),
                _factory.GetNote("20", "5"),
                _factory.GetTimedEvent("23")
            };

            CheckQuantize(
                quantizer: new SkipQuantizer(),
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: null,
                expectedObjects: _factory.WithTimes(objects,
                    "0",
                    "20",
                    "23"));
        }

        [Test]
        public void Quantize_Custom_FixedTime()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "10"),
                _factory.GetNote("20", "5"),
                _factory.GetTimedEvent("23")
            };

            CheckQuantize(
                quantizer: new FixedTimeQuantizer(100),
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: null,
                expectedObjects: _factory.WithTimes(objects,
                    "100",
                    "100",
                    "100"));
        }

        #endregion
    }
}
