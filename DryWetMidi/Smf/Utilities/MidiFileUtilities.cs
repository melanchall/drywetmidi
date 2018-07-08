using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf
{
    internal static class MidiFileUtilities
    {
        #region Methods

        public static IEnumerable<MidiEvent> GetEvents(this MidiFile midiFile)
        {
            return midiFile.GetTrackChunks().SelectMany(c => c.Events);
        }

        #endregion
    }
}
