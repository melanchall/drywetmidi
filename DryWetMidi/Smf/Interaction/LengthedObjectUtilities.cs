using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class LengthedObjectUtilities
    {
        #region Methods

        public static IEnumerable<T> StartAtTime<T>(this IEnumerable<T> objects, long time)
            where T : ILengthedObject, ITimedObject
        {
            return AtTime(objects, time, LengthedObjectPart.Start);
        }

        public static IEnumerable<T> EndAtTime<T>(this IEnumerable<T> objects, long time)
            where T : ILengthedObject, ITimedObject
        {
            return AtTime(objects, time, LengthedObjectPart.End);
        }

        public static IEnumerable<T> StartAtTime<T>(this IEnumerable<T> objects, ITime time, TempoMap tempoMap)
            where T : ILengthedObject, ITimedObject
        {
            return AtTime(objects, time, tempoMap, LengthedObjectPart.Start);
        }

        public static IEnumerable<T> EndAtTime<T>(this IEnumerable<T> objects, ITime time, TempoMap tempoMap)
            where T : ILengthedObject, ITimedObject
        {
            return AtTime(objects, time, tempoMap, LengthedObjectPart.End);
        }

        public static IEnumerable<T> AtTime<T>(this IEnumerable<T> objects, long time, LengthedObjectPart matchBy = LengthedObjectPart.Entire)
            where T : ILengthedObject, ITimedObject
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            return objects.Where(o => IsObjectAtTime(o, time, matchBy));
        }

        public static IEnumerable<T> AtTime<T>(this IEnumerable<T> objects, ITime time, TempoMap tempoMap, LengthedObjectPart matchBy = LengthedObjectPart.Entire)
            where T : ILengthedObject, ITimedObject
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects));

            if (time == null)
                throw new ArgumentNullException(nameof(time));

            if (tempoMap == null)
                throw new ArgumentNullException(nameof(tempoMap));

            var convertedTime = TimeConverter.ConvertFrom(time, tempoMap);
            return AtTime(objects, convertedTime, matchBy);
        }

        private static bool IsObjectAtTime<T>(T obj, long time, LengthedObjectPart matchBy)
            where T : ILengthedObject, ITimedObject
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            var startTime = obj.Time;
            if (startTime == time && (matchBy == LengthedObjectPart.Start || matchBy == LengthedObjectPart.Entire))
                return true;

            var endTime = startTime + obj.Length;
            if (endTime == time && (matchBy == LengthedObjectPart.End || matchBy == LengthedObjectPart.Entire))
                return true;

            return matchBy == LengthedObjectPart.Entire && time >= startTime && time <= endTime;
        }

        #endregion
    }
}
