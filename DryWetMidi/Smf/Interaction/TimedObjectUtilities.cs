using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class TimedObjectUtilities
    {
        #region Methods

        public static TTime TimeAs<TTime>(this ITimedObject obj, TempoMap tempoMap)
            where TTime : ITime
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            return TimeConverter.ConvertTo<TTime>(obj.Time, tempoMap);
        }

        public static IEnumerable<TObject> AtTime<TObject>(this IEnumerable<TObject> objects, long time)
            where TObject : ITimedObject
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            return objects.Where(o => o.Time == time);
        }

        public static IEnumerable<TObject> AtTime<TObject>(this IEnumerable<TObject> objects, ITime time, TempoMap tempoMap)
            where TObject : ITimedObject
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            var convertedTime = TimeConverter.ConvertFrom(time, tempoMap);
            return AtTime(objects, convertedTime);
        }

        #endregion
    }
}
