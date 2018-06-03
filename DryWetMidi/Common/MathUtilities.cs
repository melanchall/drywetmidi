using System;

namespace Melanchall.DryWetMidi.Common
{
    internal static class MathUtilities
    {
        #region Methods

        /// <summary>
        /// Ckecks if a number is a power of 2.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>true if the number is a power of 2, false - otherwise.</returns>
        internal static bool IsPowerOfTwo(int value)
        {
            return value != 0 && (value & (value - 1)) == 0;
        }

        /// <summary>
        /// Calculates least common multiple of two integer numbers.
        /// </summary>
        /// <param name="a">First number.</param>
        /// <param name="b">Second number.</param>
        /// <returns>Least common multiple of <paramref name="a"/> and <paramref name="b"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="a"/> is zero or negative. -or-
        /// <paramref name="b"/> is zero or negative.</exception>
        internal static long LeastCommonMultiple(long a, long b)
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

        internal static long GreatestCommonDivisor(long a, long b)
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

        internal static Tuple<long, long> SolveDiophantineEquation(long a, long b)
        {
            var greatestCommonDivisor = GreatestCommonDivisor(a, b);
            return Tuple.Create(b / greatestCommonDivisor, -a / greatestCommonDivisor);
        }

        internal static double Round(double value)
        {
            return Math.Round(value, MidpointRounding.AwayFromZero);
        }

        internal static double Round(double value, int digits)
        {
            return Math.Round(value, digits, MidpointRounding.AwayFromZero);
        }

        internal static long RoundToLong(double value)
        {
            return (long)Round(value);
        }

        #endregion
    }
}
