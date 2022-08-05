using Melanchall.DryWetMidi.Common;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Core
{
    public static class MidiTokensReaderUtilities
    {
        #region Methods

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

        public static IEnumerable<MidiEvent> EnumerateEvents(this MidiTokensReader reader)
        {
            ThrowIfArgument.IsNull(nameof(reader), reader);

            foreach (var token in EnumerateTokens(reader))
            {
                var midiEventToken = token as MidiEventToken;
                if (midiEventToken != null)
                    yield return midiEventToken.Event;
            }
        }

        #endregion
    }
}
