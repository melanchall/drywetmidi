using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public abstract class LengthedObjectsRandomizerTests<TObject, TSettings> : LengthedObjectsToolTests<TObject>
        where TObject : ILengthedObject
        where TSettings : LengthedObjectsRandomizingSettings, new()
    {
        #region Properties

        protected abstract LengthedObjectsRandomizer<TObject, TSettings> Randomizer { get; }

        #endregion

        #region Test methods

        [Test]
        public void Randomize_Start_EmptyCollection()
        {
            var tempoMap = TempoMap.Default;

            var actualObjects = Enumerable.Empty<TObject>();
            var expectedObjects = Enumerable.Empty<TObject>();

            Randomizer.Randomize(actualObjects, (MidiTimeSpan)123, tempoMap, new TSettings { RandomizingTarget = LengthedObjectTarget.Start });

            Methods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        public void Randomize_Start_Nulls()
        {
            var tempoMap = TempoMap.Default;

            var actualObjects = new[] { default(TObject), default(TObject) };
            var expectedObjects = new[] { default(TObject), default(TObject) };

            Randomizer.Randomize(actualObjects, (MidiTimeSpan)123, tempoMap, new TSettings { RandomizingTarget = LengthedObjectTarget.Start });

            Methods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        #endregion
    }
}
