namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a MIDI event.
    /// </summary>
    /// <seealso cref="MidiTokensReader"/>
    public sealed class MidiEventToken : MidiToken
    {
        #region Constructor

        internal MidiEventToken(MidiEvent midiEvent)
            : base(MidiTokenType.MidiEvent)
        {
            Event = midiEvent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a MIDI event.
        /// </summary>
        public MidiEvent Event { get; }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"MIDI event token (event = {Event})";
        }

        #endregion
    }
}
