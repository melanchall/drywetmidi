using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Common
{
    public abstract class LengthedObjectMethods<TObject> : TimedObjectMethods<TObject>
        where TObject : ILengthedObject
    {
        #region Methods

        public void SetLength(TObject obj, ITimeSpan length, ITimeSpan time, TempoMap tempoMap)
        {
            var convertedTime = TimeConverter.ConvertFrom(time, tempoMap);
            obj.Length = LengthConverter.ConvertFrom(length, convertedTime, tempoMap);
        }

        public TObject Create(ITimeSpan time, ITimeSpan length, TempoMap tempoMap)
        {
            var convertedTime = TimeConverter.ConvertFrom(time, tempoMap);
            return Create(convertedTime, LengthConverter.ConvertFrom(length, convertedTime, tempoMap));
        }

        public void AssertCollectionsAreEqual(IEnumerable<TObject> expected, IEnumerable<TObject> actual)
        {
            CollectionAssert.AreEqual(expected, actual, Comparer);
        }

        #endregion
    }
}
