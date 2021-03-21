using System;

namespace Melanchall.DryWetMidi.Interaction
{
    [Flags]
    public enum ObjectType
    {
        TimedEvent = 1 << 0,
        Note = 1 << 1,
        Chord = 1 << 2,
        Rest = 1 << 3,
    }
}
