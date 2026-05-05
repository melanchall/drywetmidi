using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Common
{
    internal static class NativeApiUtilities
    {
        #region Methods

        public static bool IsOsSupported() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        // TODO: customize message
        public static void EnsureOsIsSupported()
        {
            if (IsOsSupported())
                return;

            throw new PlatformNotSupportedException("This operation is not supported on the current operating system.");
        }

        #endregion
    }
}
