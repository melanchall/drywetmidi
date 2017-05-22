using System.Linq;

namespace Melanchall.DryWetMidi.Smf
{
    internal static class ArrayUtilities
    {
        #region Methods

        internal static bool Equals<T>(T[] array1, T[] array2)
        {
            if (ReferenceEquals(array1, array2))
                return true;

            if (array1 == null || array2 == null)
                return false;

            if (array1.Length != array2.Length)
                return false;

            return array1.SequenceEqual(array2);
        }

        #endregion
    }
}
