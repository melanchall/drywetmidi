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

        internal static void IsOutOfRange(string parameterName, TimeSpan value, TimeSpan min, TimeSpan max, string message, params object[] messageArguments)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(parameterName, value, string.Format(message, messageArguments));
        }

        internal static void IsOutOfRange(string parameterName, int value, int min, int max, string message, params object[] messageArguments)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(parameterName, value, string.Format(message, messageArguments));
        }

        internal static void IsOutOfRange(string parameterName, long value, long min, long max, string message, params object[] messageArguments)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(parameterName, value, string.Format(message, messageArguments));
        }

        internal static void IsOutOfRange(string parameterName, double value, double min, double max, string message, params object[] messageArguments)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(parameterName, value, string.Format(message, messageArguments));
        }

        internal static void DoesntSatisfyCondition(string parameterName, int value, Predicate<int> condition, string message, params object[] messageArguments)
        {
            if (!condition(value))
                throw new ArgumentOutOfRangeException(parameterName, value, string.Format(message, messageArguments));
        }

        internal static void IsGreaterThan(string parameterName, int value, int reference, string message, params object[] messageArguments)
        {
            IsOutOfRange(parameterName, value, int.MinValue, reference, message, messageArguments);
        }

        internal static void IsGreaterThan(string parameterName, long value, long reference, string message, params object[] messageArguments)
        {
            IsOutOfRange(parameterName, value, long.MinValue, reference, message, messageArguments);
        }

        internal static void IsLessThan(string parameterName, TimeSpan value, TimeSpan reference, string message, params object[] messageArguments)
        {
            IsOutOfRange(parameterName, value, reference, TimeSpan.MaxValue, message, messageArguments);
        }

        internal static void IsLessThan(string parameterName, int value, int reference, string message, params object[] messageArguments)
        {
            IsOutOfRange(parameterName, value, reference, int.MaxValue, message, messageArguments);
        }

        internal static void IsLessThan(string parameterName, long value, long reference, string message, params object[] messageArguments)
        {
            IsOutOfRange(parameterName, value, reference, long.MaxValue, message, messageArguments);
        }

        internal static void IsLessThan(string parameterName, double value, double reference, string message, params object[] messageArguments)
        {
            IsOutOfRange(parameterName, value, reference, double.MaxValue, message, messageArguments);
        }

        internal static void IsNegative(string parameterName, int value, string message, params object[] messageArguments)
        {
            IsLessThan(parameterName, value, MinNonnegativeValue, message, messageArguments);
        }

        internal static void IsNegative(string parameterName, long value, string message, params object[] messageArguments)
        {
            IsLessThan(parameterName, value, MinNonnegativeValue, message, messageArguments);
        }

        internal static void IsNegative(string parameterName, double value, string message, params object[] messageArguments)
        {
            IsLessThan(parameterName, value, MinNonnegativeValue, message, messageArguments);
        }

        internal static void IsNonpositive(string parameterName, int value, string message, params object[] messageArguments)
        {
            IsLessThan(parameterName, value, MinPositiveValue, message, messageArguments);
        }

        internal static void IsNonpositive(string parameterName, long value, string message, params object[] messageArguments)
        {
            IsLessThan(parameterName, value, MinPositiveValue, message, messageArguments);
        }

        internal static void IsNonpositive(string parameterName, double value, string message, params object[] messageArguments)
        {
            IsLessThan(parameterName, value, double.Epsilon, message, messageArguments);
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

        internal static void IsEmptyCollection<T>(string parameterName, IEnumerable<T> collection, string message, params object[] messageArguments)
        {
            if (!collection.Any())
                throw new ArgumentException(string.Format(message, messageArguments), parameterName);
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

        internal static void StartsWithInvalidValue<T>(string parameterName, IEnumerable<T> collection, T invalidValue, string message, params object[] messageArguments)
        {
            if (collection != null && collection.First().Equals(invalidValue))
                throw new ArgumentException(string.Format(message, messageArguments), parameterName);
        }

        internal static void IsOfInvalidType<TInvalidType>(string parameterName, object parameterValue, string message, params object[] messageArguments)
        {
            if (parameterValue is TInvalidType)
                throw new ArgumentException(string.Format(message, messageArguments), parameterName);
        }

        internal static void IsOfInvalidType<TInvalidType1, TInvalidType2>(string parameterName, object parameterValue, string message, params object[] messageArguments)
        {
            if (parameterValue is TInvalidType1 || parameterValue is TInvalidType2)
                throw new ArgumentException(string.Format(message, messageArguments), parameterName);
        }

        #endregion
    }
}
