namespace Melanchall.DryWetMidi.Core
{
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

        public MidiEvent Event { get; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"MIDI event token (event = {Event})";
        }

        #endregion
    }
}
