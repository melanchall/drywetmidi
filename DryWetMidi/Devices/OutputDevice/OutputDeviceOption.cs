using System;

namespace Melanchall.DryWetMidi.Devices
{
    [Flags]
    public enum OutputDeviceOption
    {
        Unknown = 0,
        PatchCaching = 1,
        LeftRightVolume = 2,
        Stream = 4,
        Volume = 8
    }
}
