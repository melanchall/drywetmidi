namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Layout (format) of CSV data representing a MIDI file. The default value is <see cref="DryWetMidi"/>.
    /// </summary>
    public enum MidiFileCsvLayout
    {
        /// <summary>
        /// Format used by DryWetMIDI which gives more compact and human readable CSV
        /// representation.
        /// </summary>
        DryWetMidi = 0,

        /// <summary>
        /// Format used by midicsv (http://www.fourmilab.ch/webtools/midicsv/) program.
        /// </summary>
        MidiCsv
    }
}
