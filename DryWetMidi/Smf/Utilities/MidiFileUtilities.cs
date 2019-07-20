using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Provides useful methods to manipulate an instance of the <see cref="MidiFile"/>.
    /// </summary>
    public static class MidiFileUtilities
    {
        #region Methods

        /// <summary>
        /// Gets all channel numbers presented in the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to get channels of.</param>
        /// <returns>Collection of channel numbers presented in the <paramref name="midiFile"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null.</exception>
        public static IEnumerable<FourBitNumber> GetChannels(this MidiFile midiFile)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            return midiFile.GetTrackChunks().GetChannels();
        }

        /// <summary>
        /// Removes all trailing MIDI events of <see cref="MidiFile"/> satisfying to the specified predicate.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to trim.</param>
        /// <param name="match">Predicate to check MIDI events.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null. -or-
        /// <paramref name="match"/> is null.</exception>
        public static void TrimEnd(this MidiFile midiFile, Predicate<MidiEvent> match)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(match), match);

            midiFile.GetTrackChunks().TrimEnd(match);
        }

        /// <summary>
        /// Removes all leading MIDI events of <see cref="MidiFile"/> satisfying to the specified predicate.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to trim.</param>
        /// <param name="match">Predicate to check MIDI events.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null. -or-
        /// <paramref name="match"/> is null.</exception>
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
