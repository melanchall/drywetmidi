using System;

namespace Melanchall.DryWetMidi.Common
{
    internal static class SystemTimeSpanUtilities
    {
        public static TimeSpan MultiplyBy(this TimeSpan timeSpan, double factor)
        {
            return TimeSpan.FromTicks(MathUtilities.RoundToLong(timeSpan.Ticks * factor));
        }

        public static TimeSpan DivideBy(this TimeSpan timeSpan, double divider)
        {
            return TimeSpan.FromTicks(MathUtilities.RoundToLong(timeSpan.Ticks / divider));
        }
    }
}
