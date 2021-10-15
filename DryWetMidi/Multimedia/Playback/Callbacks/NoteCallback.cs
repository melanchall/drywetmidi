using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Callback used to process note to be played by <see cref="Playback"/>.
    /// </summary>
    /// <param name="rawNoteData">Data of the note to process.</param>
    /// <param name="rawTime">Absolute time of note to process.</param>
    /// <param name="rawLength">Length of note to process.</param>
    /// <param name="playbackTime">Current time of the playback.</param>
    /// <returns>Data of the new note which is <paramref name="rawNoteData"/> processed by the callback;
    /// or <c>null</c> if note should be ignored.</returns>
    /// <seealso cref="Playback"/>
    /// <seealso cref="Playback.NoteCallback"/>
    public delegate NotePlaybackData NoteCallback(NotePlaybackData rawNoteData, long rawTime, long rawLength, TimeSpan playbackTime);
}
