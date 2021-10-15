using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Optional functionality supported by an output device on Windows (see <c>dwSupport</c> field
    /// description in <see href="https://docs.microsoft.com/en-us/windows/win32/api/mmeapi/ns-mmeapi-midioutcaps">
    /// MIDIOUTCAPS</see>).
    /// </summary>
    [Flags]
    public enum OutputDeviceOption
    {
        /// <summary>
        /// Unknown option.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Supports patch caching.
        /// </summary>
        PatchCaching = 1,

        /// <summary>
        /// Supports separate left and right volume control.
        /// </summary>
        LeftRightVolume = 2,

        /// <summary>
        /// Provides direct support for the <c>midiStreamOut</c> function.
        /// </summary>
        Stream = 4,

        /// <summary>
        /// Supports volume control.
        /// </summary>
        Volume = 8
    }
}
