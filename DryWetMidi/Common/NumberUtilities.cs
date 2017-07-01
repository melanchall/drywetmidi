using System;

namespace Melanchall.DryWetMidi.Common
{
    internal static class NumberUtilities
    {
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
        internal static int LeastCommonMultiple(int a, int b)
        {
            if (a <= 0)
                throw new ArgumentOutOfRangeException(nameof(a), a, "First number is zero or negative.");

            if (b <= 0)
                throw new ArgumentOutOfRangeException(nameof(b), b, "Second number is zero or negative.");

            int n1, n2;

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
    }
}
