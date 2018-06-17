using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class ArrayEquality
    {
        #region Methods

        public static bool AreEqual<T>(T[] array1, T[] array2)
        {
            if (ReferenceEquals(array1, array2))
                return true;

            if (ReferenceEquals(array1, null) || ReferenceEquals(array2, null))
                return false;

            if (array1.Length != array2.Length)
                return false;

            return array1.SequenceEqual(array2);
        }

        #endregion
    }
}
