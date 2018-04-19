using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public abstract class LengthedObjectsRandomizerTests<TObject, TSettings> : LengthedObjectsToolTests<TObject>
        where TObject : ILengthedObject
        where TSettings : LengthedObjectsRandomizingSettings, new()
    {
        #region Constructor

        public LengthedObjectsRandomizerTests(LengthedObjectMethods<TObject> methods, LengthedObjectsRandomizer<TObject, TSettings> randomizer)
            : base(methods)
        {
            Randomizer = randomizer;
        }

        #endregion

        #region Properties

        protected LengthedObjectsRandomizer<TObject, TSettings> Randomizer { get; }

        #endregion

        #region Test methods

        [Test]
        public void Randomize_Start_EmptyCollection()
        {
            var tempoMap = TempoMap.Default;

            var actualObjects = Enumerable.Empty<TObject>();
            var expectedObjects = Enumerable.Empty<TObject>();

            Randomizer.Randomize(actualObjects, new ConstantBounds((MidiTimeSpan)123), tempoMap, new TSettings { RandomizingTarget = LengthedObjectTarget.Start });

            Methods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        public void Randomize_Start_Nulls()
        {
            var tempoMap = TempoMap.Default;

            var actualObjects = new[] { default(TObject), default(TObject) };
            var expectedObjects = new[] { default(TObject), default(TObject) };

            Randomizer.Randomize(actualObjects, new ConstantBounds((MidiTimeSpan)123), tempoMap, new TSettings { RandomizingTarget = LengthedObjectTarget.Start });

            Methods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        [Test]
        public void Randomize_Start_Constant_Zero()
        {
            var tempoMap = TempoMap.Default;

            var actualObjects = new[]
            {
                Methods.Create(1000, 1000),
                Methods.Create(0, 10000),
            };
            var expectedObjects = actualObjects.Select(o => Methods.Clone(o)).ToList();

            Randomizer.Randomize(actualObjects, new ConstantBounds((MidiTimeSpan)0), tempoMap, new TSettings { RandomizingTarget = LengthedObjectTarget.Start });

            Methods.AssertCollectionsAreEqual(expectedObjects, actualObjects);
        }

        #endregion
    }
}
