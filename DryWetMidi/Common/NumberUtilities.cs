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
    }
}
