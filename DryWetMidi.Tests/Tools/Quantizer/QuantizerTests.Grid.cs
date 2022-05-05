using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class QuantizerTests
    {
        #region Test methods

        [Test]
        public void Quantize_Grid_Stepped_FromZero_Start()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "10"),
                _factory.GetNote("20", "5"),
                _factory.GetTimedEvent("23")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: null,
                expectedObjects: _factory.WithTimes(objects,
                    "0",
                    "15",
                    "30"));
        }

        [Test]
        public void Quantize_Grid_Stepped_FromZero_Start_NearGridEnd()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("3/4", "1/16"),
                _factory.GetTimedEvent("5/6")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Whole),
                tempoMap: TempoMap.Default,
                settings: null,
                expectedObjects: _factory.WithTimes(objects,
                    "1/1",
                    "1/1"));
        }

        [Test]
        public void Quantize_Grid_Stepped_FromZero_End()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "10"),
                _factory.GetNote("20", "5"),
                _factory.GetTimedEvent("23"),
                _factory.GetNote("10", "7"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    Target = QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimes(objects,
                    "5",
                    "25",
                    "23",
                    "8"));
        }

        [Test]
        public void Quantize_Grid_Stepped_FromZero_End_NearGridEnd()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("25", "3"),
                _factory.GetNote("20", "10"),
                _factory.GetTimedEvent("23"),
                _factory.GetNote("27", "2"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    Target = QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimes(objects,
                    "27",
                    "20",
                    "23",
                    "28"));
        }

        [Test]
        public void Quantize_Grid_Stepped_FromZero_StartEnd()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "10"),
                _factory.GetNote("20", "5"),
                _factory.GetTimedEvent("23"),
                _factory.GetNote("10", "7"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    Target = QuantizerTarget.Start | QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "0", "15",
                    "15", "15",
                    "30", "0",
                    "15", "0"));
        }

        [Test]
        public void Quantize_Grid_Stepped_AwayFromZero_Start()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "10"),
                _factory.GetNote("20", "5"),
                _factory.GetTimedEvent("23")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)10, (MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: null,
                expectedObjects: _factory.WithTimes(objects,
                    "10",
                    "25",
                    "25"));
        }

        [Test]
        public void Quantize_Grid_Stepped_AwayFromZero_Start_NearGridEnd()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("1/1", "3/4"),
                _factory.GetNote("6/7", "1/1"),
                _factory.GetTimedEvent("5/4")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(MusicalTimeSpan.Quarter, MusicalTimeSpan.Whole),
                tempoMap: TempoMap.Default,
                settings: null,
                expectedObjects: _factory.WithTimes(objects,
                    "5/4",
                    "5/4",
                    "5/4"));
        }

        [Test]
        public void Quantize_Grid_Stepped_AwayFromZero_End()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "8"),
                _factory.GetNote("20", "8"),
                _factory.GetTimedEvent("23")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)10, (MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    Target = QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimes(objects,
                    "2",
                    "17",
                    "23"));
        }

        [Test]
        public void Quantize_Grid_Stepped_AwayFromZero_StartEnd()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "8"),
                _factory.GetNote("20", "8"),
                _factory.GetTimedEvent("23"),
                _factory.GetNote("20", "18")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)10, (MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    Target = QuantizerTarget.Start | QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "10", "0",
                    "25", "0",
                    "25", "0",
                    "25", "15"));
        }

        [Test]
        public void Quantize_Grid_Arbitrary_Start()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "10"),
                _factory.GetNote("20", "5"),
                _factory.GetTimedEvent("23")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new ArbitraryGrid(
                    (MidiTimeSpan)10,
                    (MidiTimeSpan)18,
                    (MidiTimeSpan)24),
                tempoMap: TempoMap.Default,
                settings: null,
                expectedObjects: _factory.WithTimes(objects,
                    "10",
                    "18",
                    "24"));
        }

        [Test]
        public void Quantize_Grid_Arbitrary_Start_NearGridEnd()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("16", "1"),
                _factory.GetNote("15", "2"),
                _factory.GetTimedEvent("15")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new ArbitraryGrid(
                    (MidiTimeSpan)10,
                    (MidiTimeSpan)18),
                tempoMap: TempoMap.Default,
                settings: null,
                expectedObjects: _factory.WithTimes(objects,
                    "18",
                    "18",
                    "18"));
        }

        [Test]
        public void Quantize_Grid_Arbitrary_End()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "10"),
                _factory.GetNote("16", "3"),
                _factory.GetTimedEvent("23"),
                _factory.GetNote("0", "8"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new ArbitraryGrid(
                    (MidiTimeSpan)10,
                    (MidiTimeSpan)18,
                    (MidiTimeSpan)24),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    Target = QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimes(objects,
                    "0",
                    "15",
                    "23",
                    "2"));
        }

        [Test]
        public void Quantize_Grid_Arbitrary_StartEnd()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "10"),
                _factory.GetNote("16", "3"),
                _factory.GetTimedEvent("23"),
                _factory.GetNote("0", "8"),
                _factory.GetNote("12", "20"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new ArbitraryGrid(
                    (MidiTimeSpan)10,
                    (MidiTimeSpan)18,
                    (MidiTimeSpan)24),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    Target = QuantizerTarget.Start | QuantizerTarget.End
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "10", "0",
                    "18", "0",
                    "24", "0",
                    "10", "0",
                    "10", "14"));
        }

        #endregion
    }
}
