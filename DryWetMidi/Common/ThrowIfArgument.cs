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

        internal static void IsProhibitedValue(string parameterName, char argument, char invalidValue)
        {
            if (argument == invalidValue)
                throw new ArgumentException($"'{invalidValue}' is the prohibted value for this parameter.", parameterName);
        }

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

        internal static void IsOutOfRange(string parameterName, TimeSpan value, TimeSpan min, TimeSpan max, string message)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(parameterName, value, message);
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

        internal static void IsOutOfRange(string parameterName, int value, string message, params int[] values)
        {
            if (Array.IndexOf(values, value) < 0)
                throw new ArgumentOutOfRangeException(parameterName, value, message);
        }

        internal static void DoesntSatisfyCondition<TValue>(string parameterName, TValue value, Predicate<TValue> condition, string message)
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

        internal static void IsLessThan(string parameterName, TimeSpan value, TimeSpan reference, string message)
        {
            IsOutOfRange(parameterName, value, reference, TimeSpan.MaxValue, message);
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

        internal static void ContainsInvalidEnumValue<TEnum>(string parameterName, IEnumerable<TEnum> argument)
            where TEnum : struct
        {
            foreach (var value in argument)
            {
                if (!Enum.IsDefined(typeof(TEnum), value))
                    throw new InvalidEnumArgumentException(parameterName, Convert.ToInt32(value), typeof(TEnum));
            }
        }

        internal static void StartsWithInvalidValue<T>(string parameterName, IEnumerable<T> collection, T invalidValue, string message)
        {
            if (collection != null && collection.Any() && collection.First().Equals(invalidValue))
                throw new ArgumentException(message, parameterName);
        }

        internal static void IsOfInvalidType<TInvalidType>(string parameterName, object parameterValue, string message)
        {
            if (parameterValue is TInvalidType)
                throw new ArgumentException(message, parameterName);
        }

        internal static void IsOfInvalidType<TInvalidType1, TInvalidType2>(string parameterName, object parameterValue, string message)
        {
            if (parameterValue is TInvalidType1 || parameterValue is TInvalidType2)
                throw new ArgumentException(message, parameterName);
        }

        #endregion
    }
}
