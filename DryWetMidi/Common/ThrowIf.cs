using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Common
{
    internal static class ThrowIf
    {
        #region Methods

        internal static void LengthIsNegative(string parameterName, long length)
        {
            ArgumentIsNegative(parameterName, length, "Length is negative.");
        }

        internal static void TimeIsNegative(string parameterName, long time)
        {
            ArgumentIsNegative(parameterName, time, "Time is negative.");
        }

        internal static void StartTimeIsNegative(string parameterName, long time)
        {
            ArgumentIsNegative(parameterName, time, "Start time is negative.");
        }

        internal static void EndTimeIsNegative(string parameterName, long time)
        {
            ArgumentIsNegative(parameterName, time, "End time is negative.");
        }

        internal static void ArgumentIsNull(string parameterName, object argument)
        {
            if (argument == null)
                throw new ArgumentNullException(parameterName);
        }

        internal static void EnumArgumentIsInvalid<TEnum>(string parameterName, int argument)
            where TEnum : struct
        {
            if (!Enum.IsDefined(typeof(TEnum), argument))
                throw new InvalidEnumArgumentException(parameterName, argument, typeof(TEnum));
        }

        private static void ArgumentIsNegative(string parameterName, long time, string message)
        {
            if (time < 0)
                throw new ArgumentOutOfRangeException(parameterName, time, message);
        }

        #endregion
    }
}
