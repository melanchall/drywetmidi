using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Callback used to process MIDI event to be played by <see cref="Playback"/>.
    /// </summary>
    /// <param name="rawEvent">MIDI event to process.</param>
    /// <param name="rawTime">Absolute time of <paramref name="rawEvent"/>.</param>
    /// <param name="playbackTime">Current time of the playback.</param>
    /// <returns>New MIDI event which is <paramref name="rawEvent"/> processed by the callback;
    /// or <c>null</c> if event should be ignored.</returns>
    /// <seealso cref="Playback"/>
    /// <seealso cref="Playback.EventCallback"/>
    public delegate MidiEvent EventCallback(MidiEvent rawEvent, long rawTime, TimeSpan playbackTime);
}
