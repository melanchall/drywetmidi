using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Common
{
    internal static class ThrowIfArgument
    {
        #region Constants

        private const int MinNonnegativeValue = 0;
        private const int MinPositiveValue = 1;

        #endregion

        #region Methods

        internal static void IsNull(string parameterName, object argument)
        {
            if (argument == null)
                throw new ArgumentNullException(parameterName);
        }

        internal static void ContainsNull<T>(string parameterName, IEnumerable<T> argument)
        {
            if (argument.Any(e => e == null))
                throw new ArgumentException("Collection contains null.", parameterName);
        }

        internal static void IsInvalidEnumValue<TEnum>(string parameterName, TEnum argument)
            where TEnum : struct
        {
            if (!Enum.IsDefined(typeof(TEnum), argument))
                throw new InvalidEnumArgumentException(parameterName, Convert.ToInt32(argument), typeof(TEnum));
        }

        internal static void IsOutOfRange(string parameterName, int value, int min, int max, string message)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(parameterName, value, message);
        }

        internal static void IsOutOfRange(string parameterName, long value, long min, long max, string message)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(parameterName, value, message);
        }

        internal static void IsOutOfRange(string parameterName, double value, double min, double max, string message)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(parameterName, value, message);
        }

        internal static void DoesntSatisfyCondition(string parameterName, int value, Predicate<int> condition, string message)
        {
            if (!condition(value))
                throw new ArgumentOutOfRangeException(parameterName, value, message);
        }

        internal static void IsGreaterThan(string parameterName, int value, int reference, string message)
        {
            IsOutOfRange(parameterName, value, int.MinValue, reference, message);
        }

        internal static void IsGreaterThan(string parameterName, long value, long reference, string message)
        {
            IsOutOfRange(parameterName, value, long.MinValue, reference, message);
        }

        internal static void IsLessThan(string parameterName, int value, int reference, string message)
        {
            IsOutOfRange(parameterName, value, reference, int.MaxValue, message);
        }

        internal static void IsLessThan(string parameterName, long value, long reference, string message)
        {
            IsOutOfRange(parameterName, value, reference, long.MaxValue, message);
        }

        internal static void IsLessThan(string parameterName, double value, double reference, string message)
        {
            IsOutOfRange(parameterName, value, reference, double.MaxValue, message);
        }

        internal static void IsNegative(string parameterName, int value, string message)
        {
            IsLessThan(parameterName, value, MinNonnegativeValue, message);
        }

        internal static void IsNegative(string parameterName, long value, string message)
        {
            IsLessThan(parameterName, value, MinNonnegativeValue, message);
        }

        internal static void IsNegative(string parameterName, double value, string message)
        {
            IsLessThan(parameterName, value, MinNonnegativeValue, message);
        }

        internal static void IsNonpositive(string parameterName, int value, string message)
        {
            IsLessThan(parameterName, value, MinPositiveValue, message);
        }

        internal static void IsNonpositive(string parameterName, long value, string message)
        {
            IsLessThan(parameterName, value, MinPositiveValue, message);
        }

        internal static void IsNonpositive(string parameterName, double value, string message)
        {
            IsLessThan(parameterName, value, double.Epsilon, message);
        }

        internal static void IsNullOrWhiteSpaceString(string parameterName, string value, string stringDescription)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{stringDescription} is null or contains white-spaces only.", parameterName);
        }

        internal static void IsNullOrEmptyString(string parameterName, string value, string stringDescription)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"{stringDescription} is null or empty.", parameterName);
        }

        internal static void IsInvalidIndex(string parameterName, int index, int collectionSize)
        {
            IsOutOfRange(parameterName, index, MinNonnegativeValue, collectionSize, "Index is out of range.");
        }

        internal static void IsEmptyCollection<T>(string parameterName, IEnumerable<T> collection, string message)
        {
            if (!collection.Any())
                throw new ArgumentException(message, parameterName);
        }

        #endregion
    }
}
