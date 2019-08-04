using System;

namespace Melanchall.DryWetMidi.Devices
{
    public delegate NotePlaybackData NoteCallback(NotePlaybackData rawNoteData, long rawTime, long rawLength, TimeSpan playbackTime);
}
