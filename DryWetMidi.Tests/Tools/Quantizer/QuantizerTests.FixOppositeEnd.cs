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
        public void Quantize_FixOppositeEnd_Start()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("10", "8"),
                _factory.GetChord(
                    "11", "15",
                    "12", "60"),
                _factory.GetNote("18", "8"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizerSettings
                {
                    Target = QuantizerTarget.Start,
                    FixOppositeEnd = true
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "15", "3",
                    "15", "57",
                    "15", "11"));
        }

        [Test]
        public void Quantize_FixOppositeEnd_End()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("10", "8"),
                _factory.GetChord(
                    "11", "15",
                    "12", "60"),
                _factory.GetNote("20", "8"),
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizerSettings
                {
                    Target = QuantizerTarget.End,
                    FixOppositeEnd = true
                },
                expectedObjects: _factory.WithTimesAndLengths(objects,
                    "10", "5",
                    "11", "64",
                    "20", "10"));
        }

        #endregion
    }
}
