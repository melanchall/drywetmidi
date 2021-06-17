using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Common
{
    internal static class MathUtilities
    {
        #region Methods

        public static T GetLastElementBelowThreshold<T>(T[] elements, long keyThreshold, Func<T, long> keySelector)
        {
            var firstIndex = 0;
            var lastIndex = elements.Length - 1;

            while (firstIndex <= lastIndex)
            {
                var middleIndex = (firstIndex + lastIndex) / 2;
                var middle = elements[middleIndex];

                var key = keySelector(middle);
                if (key > keyThreshold)
                    lastIndex = middleIndex - 1;
                else if (key < keyThreshold)
                    firstIndex = middleIndex + 1;
                else
                    return middleIndex > 0 ? elements[middleIndex - 1] : default(T);
            }

            return firstIndex > 0 ? elements[firstIndex - 1] : default(T);
        }

        public static int EnsureInBounds(int value, int min, int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }

        public static bool IsPowerOfTwo(int value)
        {
            return value != 0 && (value & (value - 1)) == 0;
        }

        public static long LeastCommonMultiple(long a, long b)
        {
            ThrowIfArgument.IsNonpositive(nameof(a), a, "First number is zero or negative.");
            ThrowIfArgument.IsNonpositive(nameof(b), b, "Second number is zero or negative.");

            long n1, n2;

            if (a > b)
            {
                n1 = a;
                n2 = b;
            }
            else
            {
                n1 = b;
                n2 = a;
            }

            for (int i = 1; i < n2; i++)
            {
                if ((n1 * i) % n2 == 0)
                    return i * n1;
            }

            return n1 * n2;
        }

        public static long GreatestCommonDivisor(long a, long b)
        {
            long remainder;

            while (b != 0)
            {
                remainder = a % b;
                a = b;
                b = remainder;
            }

            return a;
        }

        public static Tuple<long, long> SolveDiophantineEquation(long a, long b)
        {
            var greatestCommonDivisor = GreatestCommonDivisor(a, b);
            return Tuple.Create(b / greatestCommonDivisor, -a / greatestCommonDivisor);
        }

        public static double Round(double value)
        {
            return Math.Round(value, MidpointRounding.AwayFromZero);
        }

        public static double Round(double value, int digits)
        {
            return Math.Round(value, digits, MidpointRounding.AwayFromZero);
        }

        public static long RoundToLong(double value)
        {
            return (long)Round(value);
        }

        public static IEnumerable<T[]> GetPermutations<T>(T[] objects)
        {
            return GetPermutations(objects, objects.Length);
        }

        private static IEnumerable<T[]> GetPermutations<T>(T[] objects, int k)
        {
            if (k == 1)
                yield return objects;
            else
            {
                foreach (var permutation in GetPermutations(objects, k - 1))
                {
                    yield return permutation;
                }

                for (var i = 0; i < k - 1; i++)
                {
                    var firstIndex = k % 2 == 0 ? i : 0;
                    var secondIndex = k - 1;

                    if (objects[firstIndex].Equals(objects[secondIndex]))
                        break;

                    var firstValue = objects[firstIndex];
                    objects[firstIndex] = objects[secondIndex];
                    objects[secondIndex] = firstValue;

                    foreach (var permutation in GetPermutations(objects, k - 1))
                    {
                        yield return permutation;
                    }
                }
            }
        }

        #endregion
    }
}
