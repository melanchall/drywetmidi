using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class QuantizerTests
    {
        #region Test methods

        [Test]
        public void Quantize_QuantizingLevel_Start_0()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "1/1"),
                _factory.GetChord(
                    "1", "1/2",
                    "3", "3/4"),
                _factory.GetNote("1/16.", "1/2"),
                _factory.GetTimedEvent("0"),
                _factory.GetTimedEvent("33/32"),
                _factory.GetNote("33/32", "1/2")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Eighth),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingLevel = 0
                },
                expectedObjects: objects.Select(obj => _factory.Same(obj)).ToArray());
        }

        [Test]
        public void Quantize_QuantizingLevel_Start_1()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "1/1"),
                _factory.GetChord(
                    "1", "1/2",
                    "3", "3/4"),
                _factory.GetNote("1/16.", "1/2"),
                _factory.GetTimedEvent("0"),
                _factory.GetTimedEvent("33/32"),
                _factory.GetNote("33/32", "1/2"),
                _factory.GetTimedEvent("15/32"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Eighth),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingLevel = 1
                },
                expectedObjects: _factory.WithTimes(objects,
                    "0",
                    "0",
                    "1/8",
                    "0",
                    "1/1",
                    "1/1",
                    "1/2"));
        }

        [Test]
        public void Quantize_QuantizingLevel_Start_Default()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "1/1"),
                _factory.GetChord(
                    "1", "1/2",
                    "3", "3/4"),
                _factory.GetNote("1/16.", "1/2"),
                _factory.GetTimedEvent("0"),
                _factory.GetTimedEvent("33/32"),
                _factory.GetNote("33/32", "1/2"),
                _factory.GetTimedEvent("15/32"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Eighth),
                tempoMap: TempoMap.Default,
                settings: null,
                expectedObjects: _factory.WithTimes(objects,
                    "0",
                    "0",
                    "1/8",
                    "0",
                    "1/1",
                    "1/1",
                    "1/2"));
        }

        [Test]
        public void Quantize_QuantizingLevel_Start_05()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "1/1"),
                _factory.GetChord(
                    "2", "1/2",
                    "3", "3/4"),
                _factory.GetNote("1/16.", "1/2"),
                _factory.GetTimedEvent("0"),
                _factory.GetTimedEvent("33/32"),
                _factory.GetNote("33/32", "1/2"),
                _factory.GetTimedEvent("15/32"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Eighth),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingLevel = 0.5
                },
                expectedObjects: _factory.WithTimes(objects,
                    "0",
                    "1",
                    "7/64",
                    "0",
                    "65/64",
                    "65/64",
                    "31/64"));
        }

        [Test]
        public void Quantize_QuantizingLevel_End_0()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "1/1"),
                _factory.GetChord(
                    "1/32", "1/2",
                    "3/32", "27/32"),
                _factory.GetNote("1/64", "1/8"),
                _factory.GetTimedEvent("100"),
                _factory.GetTimedEvent("33/32"),
                _factory.GetNote("33/32", "1/2")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Quarter),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingLevel = 0,
                    Target = QuantizerTarget.End
                },
                expectedObjects: objects.Select(obj => _factory.Same(obj)).ToArray());
        }

        [Test]
        public void Quantize_QuantizingLevel_End_1()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "1/1"),
                _factory.GetChord(
                    "1/32", "1/2",
                    "3/32", "27/32"),
                _factory.GetNote("1/64", "1/8"),
                _factory.GetTimedEvent("100"),
                _factory.GetTimedEvent("33/32"),
                _factory.GetNote("33/32", "1/2")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Quarter),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingLevel = 1,
                    Target = QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimes(objects,
                    "0",
                    "3/32",
                    "1/8",
                    "100",
                    "33/32",
                    "1/1"));
        }

        [Test]
        public void Quantize_QuantizingLevel_End_Default()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "1/1"),
                _factory.GetChord(
                    "1/32", "1/2",
                    "3/32", "27/32"),
                _factory.GetNote("1/64", "1/8"),
                _factory.GetTimedEvent("100"),
                _factory.GetTimedEvent("33/32"),
                _factory.GetNote("33/32", "1/2")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Quarter),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    Target = QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimes(objects,
                    "0",
                    "3/32",
                    "1/8",
                    "100",
                    "33/32",
                    "1/1"));
        }

        [Test]
        public void Quantize_QuantizingLevel_End_05()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "1/1"),
                _factory.GetChord(
                    "1/32", "1/2",
                    "3/32", "27/32"),
                _factory.GetNote("1/64", "1/8"),
                _factory.GetTimedEvent("100"),
                _factory.GetTimedEvent("33/32"),
                _factory.GetNote("33/32", "1/2")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Quarter),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingLevel = 0.5,
                    Target = QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimes(objects,
                    "0",
                    "2/32",
                    "9/128",
                    "100",
                    "33/32",
                    "65/64"));
        }

        [Test]
        public void Quantize_QuantizingLevel_StartEnd_0()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "1/1"),
                _factory.GetChord(
                    "1/32", "1/2",
                    "3/32", "27/32"),
                _factory.GetNote("1/64", "1/8"),
                _factory.GetTimedEvent("100"),
                _factory.GetTimedEvent("33/32"),
                _factory.GetNote("33/32", "1/2")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Quarter),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingLevel = 0,
                    Target = QuantizerTarget.Start | QuantizerTarget.End
                },
                expectedObjects: objects.Select(obj => _factory.Same(obj)).ToArray());
        }

        [Test]
        public void Quantize_QuantizingLevel_StartEnd_1()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "1/1"),
                _factory.GetChord(
                    "1/32", "1/2",
                    "3/32", "27/32"),
                _factory.GetNote("1/64", "1/8"),
                _factory.GetTimedEvent("11/16"),
                _factory.GetTimedEvent("33/32"),
                _factory.GetNote("33/32", "1/2")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Quarter),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingLevel = 1,
                    Target = QuantizerTarget.Start | QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "0", "1/1",
                    "0", "1/1",
                    "0", "1/4",
                    "3/4", "0",
                    "1/1", "0",
                    "1/1", "1/2"));
        }

        [Test]
        public void Quantize_QuantizingLevel_StartEnd_Default()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "1/1"),
                _factory.GetChord(
                    "1/32", "1/2",
                    "3/32", "27/32"),
                _factory.GetNote("1/64", "1/8"),
                _factory.GetTimedEvent("11/16"),
                _factory.GetTimedEvent("33/32"),
                _factory.GetNote("33/32", "1/2")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Quarter),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    Target = QuantizerTarget.Start | QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "0", "1/1",
                    "0", "1/1",
                    "0", "1/4",
                    "3/4", "0",
                    "1/1", "0",
                    "1/1", "1/2"));
        }

        [Test]
        public void Quantize_QuantizingLevel_StartEnd_05()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "1/1"),
                _factory.GetChord(
                    "1/32", "1/2",
                    "3/32", "27/32"),
                _factory.GetNote("1/64", "1/8"),
                _factory.GetTimedEvent("11/16"),
                _factory.GetTimedEvent("33/32"),
                _factory.GetNote("33/32", "1/2")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Quarter),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    QuantizingLevel = 0.5,
                    Target = QuantizerTarget.Start | QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "0", "1/1",
                    "1/64", "61/64",
                    "1/128", "24/128",
                    "23/32", "0",
                    "65/64", "0",
                    "65/64", "32/64"));
        }

        #endregion
    }
}
