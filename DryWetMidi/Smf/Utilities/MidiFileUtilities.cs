using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public static class MidiFileUtilities
    {
        #region Methods

        public static IEnumerable<FourBitNumber> GetChannels(this MidiFile midiFile)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            return midiFile.GetTrackChunks().GetChannels();
        }

        public static void TrimEnd(this MidiFile midiFile, Predicate<MidiEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(match), match);

            midiFile.GetTrackChunks().TrimEnd(match);
        }

        public static void TrimStart(this MidiFile midiFile, Predicate<MidiEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(match), match);

            midiFile.GetTrackChunks().TrimStart(match);
        }

        internal static IEnumerable<MidiEvent> GetEvents(this MidiFile midiFile)
        {
            return midiFile.GetTrackChunks().SelectMany(c => c.Events);
        }

        #endregion
    }
}
