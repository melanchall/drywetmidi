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
        public void Quantize_LengthType_Start_DefaultTempoMap_Midi()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("600", "100"),
                _factory.GetChord("600", "100"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)1000),
                tempoMap: TempoMap.Default,
                settings: new QuantizerSettings
                {
                    LengthType = TimeSpanType.Midi
                },
                expectedObjects: _factory.WithTimes(objects,
                    "1000",
                    "1000"));
        }

        [Test]
        public void Quantize_LengthType_Start_DefaultTempoMap_Metric()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("600", "0:0:10"),
                _factory.GetChord("600", "0:0:10"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)1000),
                tempoMap: TempoMap.Default,
                settings: new QuantizerSettings
                {
                    LengthType = TimeSpanType.Metric
                },
                expectedObjects: _factory.WithTimes(objects,
                    "1000",
                    "1000"));
        }

        [Test]
        public void Quantize_LengthType_Start_DefaultTempoMap_BarBeatTicks()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("600", "1.0.0"),
                _factory.GetChord("600", "1.0.0"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)1000),
                tempoMap: TempoMap.Default,
                settings: new QuantizerSettings
                {
                    LengthType = TimeSpanType.BarBeatTicks
                },
                expectedObjects: _factory.WithTimes(objects,
                    "1000",
                    "1000"));
        }

        [Test]
        public void Quantize_LengthType_Start_CustomTempoMap_Metric()
        {
            var tempoMap = CreateCustomMetricTempoMap_LengthType();
            var customFactory = new ObjectsFactory(tempoMap);

            var objects = new ITimedObject[]
            {
                customFactory.GetNote("600", "0:0:10"),
                customFactory.GetChord("600", "0:0:10"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)1000),
                tempoMap: tempoMap,
                settings: new QuantizerSettings
                {
                    LengthType = TimeSpanType.Metric
                },
                expectedObjects: customFactory.WithTimesAndLengths(objects,
                    "1000", "0:0:10",
                    "1000", "0:0:10"));
        }

        [Test]
        public void Quantize_LengthType_Start_CustomTempoMap_Metric_NoLengthTypeSpecified()
        {
            var tempoMap = CreateCustomMetricTempoMap_LengthType();
            var customFactory = new ObjectsFactory(tempoMap);

            var objects = new ITimedObject[]
            {
                customFactory.GetNote("600", "0:0:10"),
                customFactory.GetChord("600", "0:0:10"),
            };

            Assert.Throws<AssertionException>(() => CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)1000),
                tempoMap: tempoMap,
                settings: null,
                expectedObjects: customFactory.WithTimesAndLengths(objects,
                    "1000", "0:0:10",
                    "1000", "0:0:10")));
        }

        [Test]
        public void Quantize_LengthType_Start_CustomTempoMap_BarBeatTicks()
        {
            var tempoMap = CreateCustomBarBeatTicksTempoMap_LengthType();
            var customFactory = new ObjectsFactory(tempoMap);

            var objects = new ITimedObject[]
            {
                customFactory.GetNote("4.0.0", "3.0.0"),
                customFactory.GetChord("4.0.0", "3.0.0"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(new BarBeatTicksTimeSpan(7, 0, 0)),
                tempoMap: tempoMap,
                settings: new QuantizerSettings
                {
                    LengthType = TimeSpanType.BarBeatTicks,
                    DistanceCalculationType = TimeSpanType.BarBeatTicks
                },
                expectedObjects: customFactory.WithTimesAndLengths(objects,
                    "7.0.0", "3.0.0",
                    "7.0.0", "3.0.0"));
        }

        [Test]
        public void Quantize_LengthType_Start_CustomTempoMap_BarBeatTicks_NoLengthTypeSpecified()
        {
            var tempoMap = CreateCustomBarBeatTicksTempoMap_LengthType();
            var customFactory = new ObjectsFactory(tempoMap);

            var objects = new ITimedObject[]
            {
                customFactory.GetNote("4.0.0", "3.0.0"),
                customFactory.GetChord("4.0.0", "3.0.0"),
            };

            Assert.Throws<AssertionException>(() => CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid(new BarBeatTicksTimeSpan(7, 0, 0)),
                tempoMap: tempoMap,
                settings: new QuantizerSettings
                {
                    DistanceCalculationType = TimeSpanType.BarBeatTicks
                },
                expectedObjects: customFactory.WithTimesAndLengths(objects,
                    "7.0.0", "3.0.0",
                    "7.0.0", "3.0.0")));
        }

        #endregion

        #region Private methods

        private static TempoMap CreateCustomMetricTempoMap_LengthType()
        {
            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(200, Tempo.FromBeatsPerMinute(200));
                tempoMapManager.SetTempo(700, Tempo.FromBeatsPerMinute(80));

                return tempoMapManager.TempoMap;
            }
        }

        private static TempoMap CreateCustomBarBeatTicksTempoMap_LengthType()
        {
            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(new BarBeatTicksTimeSpan(2, 0, 0), new TimeSignature(8, 4));
                tempoMapManager.SetTimeSignature(new BarBeatTicksTimeSpan(5, 0, 0), new TimeSignature(5, 8));

                return tempoMapManager.TempoMap;
            }
        }

        #endregion
    }
}
