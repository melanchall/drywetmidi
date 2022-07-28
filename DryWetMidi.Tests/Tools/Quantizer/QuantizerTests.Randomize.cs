using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class QuantizerTests
    {
        #region Constants

        private const int RetriesCount = 2;

        #endregion

        #region Test methods

        [Retry(RetriesCount)]
        [Test]
        public void Quantize_Randomize_NoQuantize_Start()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "20"),
                _factory.GetNote("50", "20"),
                _factory.GetTimedEvent("93")
            };

            CheckRandomize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    RandomizingSettings = new RandomizingSettings
                    {
                        Bounds = new ConstantBounds((MidiTimeSpan)10)
                    },
                    QuantizingLevel = 0
                },
                expectedTimeRanges: new (long Min, long Max)[]
                {
                    (0, 10),
                    (10, 30),
                    (40, 60),
                    (60, 80),
                    (83, 103),
                });
        }

        [Retry(RetriesCount)]
        [Test]
        public void Quantize_Randomize_NoQuantize_Start_Filter()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "20"),
                _factory.GetNote("50", "20"),
                _factory.GetTimedEvent("93")
            };

            CheckRandomize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    RandomizingSettings = new RandomizingSettings
                    {
                        Bounds = new ConstantBounds((MidiTimeSpan)10),
                        Filter = obj => obj.Time != 50
                    },
                    QuantizingLevel = 0
                },
                expectedTimeRanges: new (long Min, long Max)[]
                {
                    (0, 10),
                    (10, 30),
                    (50, 50),
                    (70, 70),
                    (83, 103),
                });
        }

        [Retry(RetriesCount)]
        [Test]
        public void Quantize_Randomize_NoQuantize_Start_FixOppositeEnd()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "20"),
                _factory.GetNote("50", "20"),
                _factory.GetTimedEvent("93")
            };

            CheckRandomize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    RandomizingSettings = new RandomizingSettings
                    {
                        Bounds = new ConstantBounds((MidiTimeSpan)10)
                    },
                    QuantizingLevel = 0,
                    FixOppositeEnd = true
                },
                expectedTimeRanges: new (long Min, long Max)[]
                {
                    (0, 10),
                    (20, 20),
                    (40, 60),
                    (70, 70),
                    (83, 103),
                });
        }

        [Retry(RetriesCount)]
        [Test]
        public void Quantize_Randomize_NoQuantize_End()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "20"),
                _factory.GetNote("50", "20"),
                _factory.GetTimedEvent("93")
            };

            CheckRandomize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    RandomizingSettings = new RandomizingSettings
                    {
                        Bounds = new ConstantBounds((MidiTimeSpan)10)
                    },
                    QuantizingLevel = 0,
                    Target = QuantizerTarget.End
                },
                expectedTimeRanges: new (long Min, long Max)[]
                {
                    (0, 10),
                    (10, 30),
                    (40, 60),
                    (60, 80),
                    (93, 93),
                });
        }

        [Retry(RetriesCount)]
        [Test]
        public void Quantize_Randomize_NoQuantize_End_FixOppositeEnd()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "20"),
                _factory.GetNote("50", "20"),
                _factory.GetTimedEvent("93")
            };

            CheckRandomize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    RandomizingSettings = new RandomizingSettings
                    {
                        Bounds = new ConstantBounds((MidiTimeSpan)10)
                    },
                    QuantizingLevel = 0,
                    Target = QuantizerTarget.End,
                    FixOppositeEnd = true
                },
                expectedTimeRanges: new (long Min, long Max)[]
                {
                    (0, 0),
                    (10, 30),
                    (50, 50),
                    (60, 80),
                    (93, 93),
                });
        }

        [Retry(RetriesCount)]
        [Test]
        public void Quantize_Randomize_NoQuantize_StartEnd()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("0", "10"),
                _factory.GetNote("50", "20"),
                _factory.GetTimedEvent("93")
            };

            CheckRandomize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizingSettings
                {
                    RandomizingSettings = new RandomizingSettings
                    {
                        Bounds = new ConstantBounds((MidiTimeSpan)10)
                    },
                    QuantizingLevel = 0,
                    Target = QuantizerTarget.Start | QuantizerTarget.End
                },
                expectedTimeRanges: new (long Min, long Max)[]
                {
                    (0, 10),
                    (0, 20),
                    (40, 60),
                    (60, 80),
                    (83, 103),
                });
        }

        #endregion

        #region Private methods

        private void CheckRandomize(
            ICollection<ITimedObject> timedObjects,
            IGrid grid,
            TempoMap tempoMap,
            QuantizingSettings settings,
            (long Min, long Max)[] expectedTimeRanges,
            double unchangedMaxPercent = 0.7)
        {
            var originalTimedEvents = timedObjects.GetObjects(ObjectType.TimedEvent).Select(obj => obj.Clone()).ToArray();

            new Quantizer().Quantize(timedObjects, grid, tempoMap, settings);
            var actualTimedEvents = timedObjects.GetObjects(ObjectType.TimedEvent).ToArray();

            var timesForChangeCount = 0;
            var unchangedTimesCount = 0;

            for (var i = 0; i < expectedTimeRanges.Length; i++)
            {
                var actualTime = actualTimedEvents[i].Time;
                var timeRange = expectedTimeRanges[i];

                Assert.IsTrue(
                    actualTime >= timeRange.Min && actualTime <= timeRange.Max,
                    $"Time [{i}] is not in [{timeRange.Min}; {timeRange.Max}] range.");

                if (timeRange.Min != timeRange.Max)
                {
                    timesForChangeCount++;

                    if (actualTime == originalTimedEvents[i].Time)
                        unchangedTimesCount++;
                }
            }

            Assert.LessOrEqual(
                unchangedTimesCount / (double)timesForChangeCount,
                unchangedMaxPercent,
                "Too high percent of unchanged times.");
        }

        #endregion
    }
}
