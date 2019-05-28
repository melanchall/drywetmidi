using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public static class MidiFileUtilities
    {
        #region Methods

        internal static IEnumerable<MidiEvent> GetEvents(this MidiFile midiFile)
        {
            return midiFile.GetTrackChunks().SelectMany(c => c.Events);
        }

        public static IEnumerable<FourBitNumber> GetChannels(this MidiFile midiFile)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            return midiFile.GetTrackChunks().GetChannels();
        }

        #endregion
    }
}
