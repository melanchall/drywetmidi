using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class QuantizerTests
    {
        #region Fields

        private readonly ObjectsFactory _factory = ObjectsFactory.Default;

        #endregion

        #region Test methods

        [Test]
        public void Quantize_EmptyCollection() => CheckQuantize(
            timedObjects: Array.Empty<ITimedObject>(),
            grid: new SteppedGrid(MusicalTimeSpan.Quarter),
            tempoMap: TempoMap.Default,
            settings: null,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void Quantize_Nulls() => CheckQuantize(
            timedObjects: new[]
            {
                default(ITimedObject),
                default(ITimedObject)
            },
            grid: new SteppedGrid(MusicalTimeSpan.Quarter),
            tempoMap: TempoMap.Default,
            settings: null,
            expectedObjects: new[]
            {
                default(ITimedObject),
                default(ITimedObject)
            });

        [Test]
        public void Quantize_Filter()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("10", "20"),
                _factory.GetTimedEvent("12"),
                _factory.GetTimedEvent("10"),
                _factory.GetNote("23", "30")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: new QuantizerSettings
                {
                    Filter = obj => obj.Time != 10
                },
                expectedObjects: _factory.WithTimes(objects,
                    "10",
                    "15",
                    "10",
                    "30"));
        }

        [Test]
        public void Quantize_AlreadyQuantized()
        {
            var objects = new ITimedObject[]
            {
                _factory.GetNote("15", "20"),
                _factory.GetTimedEvent("15"),
                _factory.GetTimedEvent("30"),
                _factory.GetNote("45", "30")
            };

            CheckQuantize(
                timedObjects: objects,
                grid: new SteppedGrid((MidiTimeSpan)15),
                tempoMap: TempoMap.Default,
                settings: null,
                expectedObjects: _factory.WithTimes(objects,
                    "15",
                    "15",
                    "30",
                    "45"));
        }

        #endregion

        #region Private methods

        private void CheckQuantize(
            ICollection<ITimedObject> timedObjects,
            IGrid grid,
            TempoMap tempoMap,
            QuantizerSettings settings,
            ICollection<ITimedObject> expectedObjects)
        {
            new Quantizer().Quantize(timedObjects, grid, tempoMap, settings);
            MidiAsserts.AreEqual(expectedObjects, timedObjects, "Invalid quantized objects.");
        }

        #endregion
    }
}
