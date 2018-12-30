using System;
using System.Linq;
using System.Reflection;

namespace Melanchall.DryWetMidi.Common
{
    internal static class ReflectionUtilities
    {
        #region Methods

        public static TValue[] GetConstantsValues<TValue>(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
                .Select(fi => (TValue)fi.GetValue(null))
                .ToArray();
        }

        #endregion
    }
}
