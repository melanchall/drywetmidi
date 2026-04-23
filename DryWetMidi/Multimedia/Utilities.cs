using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class Utilities
    {
        public static bool IsOsSupported() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static void EnsureOsIsSupported()
        {
            if (IsOsSupported())
                return;

            throw new PlatformNotSupportedException("This operation is not supported on the current operating system.");
        }
    }
}
