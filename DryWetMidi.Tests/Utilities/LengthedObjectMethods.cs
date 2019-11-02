using System.Collections.Generic;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    public abstract class LengthedObjectMethods<TObject> : TimedObjectMethods<TObject>
        where TObject : ILengthedObject
    {
        #region Methods

        public void SetLength(TObject obj, ITimeSpan length, ITimeSpan time, TempoMap tempoMap)
        {
            var convertedTime = TimeConverter.ConvertFrom(time, tempoMap);
            SetLength(obj, LengthConverter.ConvertFrom(length, convertedTime, tempoMap));
        }

        public TObject Create(ITimeSpan time, ITimeSpan length, TempoMap tempoMap)
        {
            var convertedTime = TimeConverter.ConvertFrom(time, tempoMap);
            return Create(convertedTime, LengthConverter.ConvertFrom(length, convertedTime, tempoMap));
        }

        public IEnumerable<TObject> CreateCollection(TempoMap tempoMap, params string[] timeAndLengthStrings)
        {
            var result = new List<TObject>();

            foreach (var timeAndLengthString in timeAndLengthStrings)
            {
                var parts = timeAndLengthString.Split(';');
                var time = TimeSpanUtilities.Parse(parts[0]);
                var length = TimeSpanUtilities.Parse(parts[1]);

                result.Add(Create(time, length, tempoMap));
            }

            return result;
        }

        public void AssertCollectionsAreEqual(IEnumerable<TObject> expected, IEnumerable<TObject> actual, string message = null)
        {
            CollectionAssert.AreEqual(expected, actual, Comparer, message);
        }

        public abstract TObject Create(long time, long length);

        public abstract void SetLength(TObject obj, long length);

        #endregion
    }
}
