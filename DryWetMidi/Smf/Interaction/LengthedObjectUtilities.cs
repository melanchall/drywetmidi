using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class LengthedObjectUtilities
    {
        public static IEnumerable<T> AtTime<T>(this IEnumerable<T> objects, long time, LengthedObjectPart matchBy)
            where T : ILengthedObject, ITimedObject
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(nameof(time), time, "Time is negative.");

            return objects.Where(o => IsObjectAtTime(o, time, matchBy));
        }

        private static bool IsObjectAtTime<T>(T obj, long time, LengthedObjectPart matchBy)
            where T : ILengthedObject, ITimedObject
        {
            var startTime = obj.Time;
            if (startTime == time && (matchBy == LengthedObjectPart.Start || matchBy == LengthedObjectPart.Entire))
                return true;

            var endTime = startTime + obj.Length;
            if (endTime == time && (matchBy == LengthedObjectPart.End || matchBy == LengthedObjectPart.Entire))
                return true;

            return matchBy == LengthedObjectPart.Entire && time >= startTime && time <= endTime;
        }
    }
}
