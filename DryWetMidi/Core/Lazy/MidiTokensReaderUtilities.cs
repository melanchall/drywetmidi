using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Provides useful utility methods for <see cref="MidiTokensReader"/>. See
    /// <see href="xref:a_file_lazy_reading_writing">Lazy reading/writing</see> article to learn more.
    /// </summary>
    public static class MidiTokensReaderUtilities
    {
        #region Methods

        /// <summary>
        /// Returns all MIDI tokens read with the specified <see cref="MidiTokensReader"/>
        /// as a lazy collection.
        /// </summary>
        /// <param name="reader"><see cref="MidiTokensReader"/> to read MIDI tokens with.</param>
        /// <returns>Lazy collection of MIDI tokens read with the <paramref name="reader"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
        public static IEnumerable<MidiToken> EnumerateTokens(this MidiTokensReader reader)
        {
            ThrowIfArgument.IsNull(nameof(reader), reader);

            while (true)
            {
                var token = reader.ReadToken();
                if (token == null)
                    break;

                yield return token;
            }
        }

        /// <summary>
        /// Returns all consecutive MIDI events read with the specified <see cref="MidiTokensReader"/>
        /// as a lazy collection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A collection contained in the returned object will iterate over MIDI tokens with the <paramref name="reader"/>
        /// until a file's end or start of a next chunk occurred.
        /// </para>
        /// <para>
        /// For example, the reader is now at the first MIDI events of a track chunk that contains 10 events.
        /// The method will return an object that contains a lazy collection that will iterate over those 10 events
        /// only, because a next MIDI token in the file is not a MIDI event.
        /// </para>
        /// </remarks>
        /// <param name="reader"><see cref="MidiTokensReader"/> to read MIDI events with.</param>
        /// <returns>An instance of the <see cref="EnumerateEventsResult"/> containing a lazy collection of consecutive
        /// MIDI events read with the <paramref name="reader"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
        public static EnumerateEventsResult EnumerateEvents(this MidiTokensReader reader)
        {
            ThrowIfArgument.IsNull(nameof(reader), reader);

            var result = new EnumerateEventsResult();
            result.Events = EnumerateEvents(reader, result);
            return result;
        }

        private static IEnumerable<MidiEvent> EnumerateEvents(MidiTokensReader reader, EnumerateEventsResult result)
        {
            foreach (var token in EnumerateTokens(reader))
            {
                var midiEventToken = token as MidiEventToken;
                if (midiEventToken != null)
                    yield return midiEventToken.Event;
                else
                {
                    result.NextToken = token;
                    yield break;
                }
            }
        }

        #endregion
    }
}
