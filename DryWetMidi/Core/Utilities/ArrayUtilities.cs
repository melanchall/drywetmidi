using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Core
{
    internal static class ArrayUtilities
    {
        #region Methods

        internal static bool Equals<T>(T[] array1, T[] array2)
        {
            if (ReferenceEquals(array1, array2))
                return true;

            if (ReferenceEquals(array1, null) || ReferenceEquals(array2, null))
                return false;

            if (array1.Length != array2.Length)
                return false;

            return array1.SequenceEqual(array2);
        }

        internal static int GetHashCode<T>(T[] array)
        {
            return (array as IStructuralEquatable)?.GetHashCode(EqualityComparer<T>.Default) ?? 0;
        }

        #endregion
    }
}
