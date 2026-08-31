using Melanchall.DryWetMidi.Common;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MidiTimeSpanParser : SimpleParser<MidiTimeSpan>
    {
        internal override IEnumerable<string> GetPatterns()
        {
            throw new System.NotImplementedException();
        }

        protected override MidiTimeSpan ParseInternal(string input)
        {
            if (!long.TryParse(input, out var midiTimeSpan))
                ThrowInvalidFormatError();

            return new MidiTimeSpan(midiTimeSpan);
        }
    }
}
