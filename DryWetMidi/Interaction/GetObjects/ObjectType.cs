using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// The type of objects to get with methods of <see cref="GetObjectsUtilities"/>.
    /// </summary>
    [Flags]
    public enum ObjectType
    {
        /// <summary>
        /// Represents a timed event (see <see cref="Interaction.TimedEvent"/>).
        /// </summary>
        TimedEvent = 1 << 0,

        /// <summary>
        /// Represents a note (see <see cref="Interaction.Note"/>).
        /// </summary>
        Note = 1 << 1,

        /// <summary>
        /// Represents a chord (see <see cref="Interaction.Chord"/>).
        /// </summary>
        Chord = 1 << 2,
    }
}
