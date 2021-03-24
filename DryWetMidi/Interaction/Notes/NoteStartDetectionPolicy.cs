namespace Melanchall.DryWetMidi.Interaction
{
    /// Defines how start event of a note should be found in case of overlapping notes with
    /// the same note number and channel. The default value is <see cref="FirstNoteOn"/>.
    public enum NoteStartDetectionPolicy
    {
        /// <summary>
        /// First Note On event with corresponding note number and channel should be taken.
        /// </summary>
        FirstNoteOn = 0,

        /// <summary>
        /// Last Note On event with corresponding note number and channel should be taken.
        /// </summary>
        LastNoteOn,
    }
}
