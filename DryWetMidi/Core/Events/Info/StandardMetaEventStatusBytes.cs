using System.Linq;
using System.Reflection;

namespace Melanchall.DryWetMidi.Core
{
    internal static class StandardMetaEventStatusBytes
    {
        #region Fields

        private static byte[] _statusBytes;

        #endregion

        #region Methods

        public static byte[] GetStatusBytes()
        {
            return _statusBytes ?? (_statusBytes = typeof(EventStatusBytes.Meta)
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(f => (byte)f.GetValue(null))
                .ToArray());
        }

        #endregion
    }
}
