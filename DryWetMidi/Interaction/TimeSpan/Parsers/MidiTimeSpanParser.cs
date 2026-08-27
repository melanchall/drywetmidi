using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MidiTimeSpanParser : SimpleParser<MidiTimeSpan>
    {
        protected override MidiTimeSpan ParseInternal(ReadOnlySpan<char> input)
        {
            if (!long.TryParse(input, out var midiTimeSpan) || midiTimeSpan < 0)
                ThrowInvalidFormatError();

            return new MidiTimeSpan(midiTimeSpan);
        }
    }
}
