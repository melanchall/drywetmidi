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
        public void Quantize_QuantizingBeyondFixedEndPolicy_Start_CollapseAndFix()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("15", "3"),
                _factory.GetChord(
                    "13", "5",
                    "13", "5"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)20),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.CollapseAndFix,
                    Target = QuantizerTarget.Start,
                    FixOppositeEnd = true
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "18", "0",
                    "18", "0"));
        }

        [Test]
        public void Quantize_QuantizingBeyondFixedEndPolicy_Start_CollapseAndMove()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("15", "3"),
                _factory.GetChord(
                    "13", "5",
                    "13", "5"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)20),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.CollapseAndMove,
                    Target = QuantizerTarget.Start,
                    FixOppositeEnd = true
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "20", "0",
                    "20", "0"));
        }

        [Test]
        public void Quantize_QuantizingBeyondFixedEndPolicy_Start_SwapEnds()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("15", "3"),
                _factory.GetChord(
                    "13", "5",
                    "13", "5"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)20),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.SwapEnds,
                    Target = QuantizerTarget.Start,
                    FixOppositeEnd = true
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "18", "2",
                    "18", "2"));
        }

        [Test]
        public void Quantize_QuantizingBeyondFixedEndPolicy_Start_Skip()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("15", "3"),
                _factory.GetNote("15", "10"),
                _factory.GetChord(
                    "13", "5",
                    "13", "5"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)20),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.Skip,
                    Target = QuantizerTarget.Start,
                    FixOppositeEnd = true
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "15", "3",
                    "20", "5",
                    "13", "5"));
        }

        [Test]
        public void Quantize_QuantizingBeyondFixedEndPolicy_Start_Abort()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("15", "3"),
                _factory.GetChord(
                    "13", "5",
                    "13", "5"),
            };

            Assert.Throws<InvalidOperationException>(() => CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)20),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.Abort,
                    Target = QuantizerTarget.Start,
                    FixOppositeEnd = true
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "15", "3",
                    "13", "5")));
        }

        [Test]
        public void Quantize_QuantizingBeyondFixedEndPolicy_End_CollapseAndFix()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("16", "3"),
                _factory.GetChord(
                    "18", "2",
                    "18", "2"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.CollapseAndFix,
                    Target = QuantizerTarget.End,
                    FixOppositeEnd = true
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "16", "0",
                    "18", "0"));
        }

        [Test]
        public void Quantize_QuantizingBeyondFixedEndPolicy_End_CollapseAndMove()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("16", "3"),
                _factory.GetChord(
                    "18", "2",
                    "18", "2"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.CollapseAndMove,
                    Target = QuantizerTarget.End,
                    FixOppositeEnd = true
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "15", "0",
                    "15", "0"));
        }

        [Test]
        public void Quantize_QuantizingBeyondFixedEndPolicy_End_SwapEnds()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("16", "3"),
                _factory.GetChord(
                    "18", "2",
                    "18", "2"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.SwapEnds,
                    Target = QuantizerTarget.End,
                    FixOppositeEnd = true
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "15", "1",
                    "15", "3"));
        }

        [Test]
        public void Quantize_QuantizingBeyondFixedEndPolicy_End_Skip()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("16", "3"),
                _factory.GetNote("13", "6"),
                _factory.GetChord(
                    "18", "2",
                    "18", "2"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.Skip,
                    Target = QuantizerTarget.End,
                    FixOppositeEnd = true
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "16", "3",
                    "13", "2",
                    "18", "2"));
        }

        [Test]
        public void Quantize_QuantizingBeyondFixedEndPolicy_End_Abort()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("16", "3"),
                _factory.GetChord(
                    "18", "2",
                    "18", "2"),
            };

            Assert.Throws<InvalidOperationException>(() => CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingBeyondFixedEndPolicy = QuantizingBeyondFixedEndPolicy.Abort,
                    Target = QuantizerTarget.End,
                    FixOppositeEnd = true
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "0", "0",
                    "0", "0")));
        }

        #endregion
    }
}
