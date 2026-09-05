using Melanchall.DryWetMidi.Common;
using System;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MidiTimeSpanParser : SimpleParser<MidiTimeSpan>
    {
        internal override Regex[] GetRegexes()
        {
            throw new NotImplementedException();
        }

        protected override MidiTimeSpan ParseInternal(ReadOnlySpan<char> input)
        {
            if (!long.TryParse(input, out var midiTimeSpan) || midiTimeSpan < 0)
                ThrowInvalidFormatError();

            return new MidiTimeSpan(midiTimeSpan);
        }
    }
}
