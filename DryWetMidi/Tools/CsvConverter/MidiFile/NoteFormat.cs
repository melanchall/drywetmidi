namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// The format which should be used to write notes to or read them from CSV.
    /// </summary>
    public enum NoteFormat
    {
        /// <summary>
        /// Notes are presented in CSV as note objects.
        /// </summary>
        Note,

        /// <summary>
        /// Notes are presented in CSV as Note On/Note Off events.
        /// </summary>
        Events
    }
}
