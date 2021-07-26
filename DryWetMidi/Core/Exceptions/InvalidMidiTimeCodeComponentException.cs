using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when the reading engine has encountered an invalid MIDI time
    /// code component (i.e. a value that doesn't belong to values of <see cref="Core.MidiTimeCodeComponent"/>)
    /// during reading <see cref="MidiTimeCodeEvent"/>.
    /// </summary>
    public sealed class InvalidMidiTimeCodeComponentException : MidiException
    {
        #region Constructors

        internal InvalidMidiTimeCodeComponentException(byte midiTimeCodeComponent)
            : base($"Invalid MIDI Time Code component ({midiTimeCodeComponent}).")
        {
            MidiTimeCodeComponent = midiTimeCodeComponent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the value representing MIDI time code component that caused this exception.
        /// </summary>
        public byte MidiTimeCodeComponent { get; }

        #endregion
    }
}
