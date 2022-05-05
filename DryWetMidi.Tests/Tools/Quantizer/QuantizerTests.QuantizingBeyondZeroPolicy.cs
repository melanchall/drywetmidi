using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class QuantizerTests
    {
        #region Test methods

        [Test]
        public void Quantize_QuantizingBeyondZeroPolicy_FixAtZero()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("2", "23"),
                _factory.GetChord(
                    "0", "21",
                    "1", "22"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)20),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondZeroPolicy = QuantizingBeyondZeroPolicy.FixAtZero,
                    Target = QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "0", "20",
                    "0", "20"));
        }

        [Test]
        public void Quantize_QuantizingBeyondZeroPolicy_FixAtZero_Metric()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0:0:2", "0:0:23"),
                _factory.GetChord(
                    "0:0:0", "0:0:21",
                    "0:0:1", "0:0:22"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(new MetricTimeSpan(0, 0, 20)),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondZeroPolicy = QuantizingBeyondZeroPolicy.FixAtZero,
                    Target = QuantizerTarget.End,
                    LengthType = TimeSpanType.Metric
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "0:0:0", "0:0:20",
                    "0:0:0", "0:0:20"));
        }

        [Test]
        public void Quantize_QuantizingBeyondZeroPolicy_FixAtZero_BarBeatTicks()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("2.0.0", "23.0.0"),
                _factory.GetChord(
                    "0.0.0", "21.0.0",
                    "1.0.0", "22.0.0"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(new BarBeatTicksTimeSpan(20)),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondZeroPolicy = QuantizingBeyondZeroPolicy.FixAtZero,
                    Target = QuantizerTarget.End,
                    LengthType = TimeSpanType.BarBeatTicks
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "0.0.0", "20.0.0",
                    "0.0.0", "20.0.0"));
        }

        [Test]
        public void Quantize_QuantizingBeyondZeroPolicy_Skip()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("2", "23"),
                _factory.GetNote("5", "20"),
                _factory.GetChord(
                    "0", "21",
                    "1", "22"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)20),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondZeroPolicy = QuantizingBeyondZeroPolicy.Skip,
                    Target = QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "2", "23",
                    "0", "20",
                    "0", "23"));
        }

        [Test]
        public void Quantize_QuantizingBeyondZeroPolicy_Abort()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("2", "23"),
                _factory.GetChord(
                    "0", "21",
                    "1", "22"),
            };

            Assert.Throws<InvalidOperationException>(() => CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)20),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondZeroPolicy = QuantizingBeyondZeroPolicy.Abort,
                    Target = QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "2", "23",
                    "0", "23")));
        }

        #endregion
    }
}
