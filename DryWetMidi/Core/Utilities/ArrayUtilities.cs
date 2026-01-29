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
            return Equals(array1, 0, array2, 0);
        }

        internal static bool Equals<T>(T[] array1, int offset1, T[] array2, int offset2)
        {
            if (ReferenceEquals(array1, array2))
                return true;

            if (ReferenceEquals(array1, null) || ReferenceEquals(array2, null))
                return false;

            if ((array1.Length - offset1) != (array2.Length - offset2))
                return false;

            return array1.Skip(offset1).SequenceEqual(array2.Skip(offset2));
        }

        internal static int GetHashCode<T>(T[] array)
        {
            return (array as IStructuralEquatable)?.GetHashCode(EqualityComparer<T>.Default) ?? 0;
        }

        #endregion
    }
}
