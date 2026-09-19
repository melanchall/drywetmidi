using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MidiTimeSpanParser : SimpleParser<MidiTimeSpan>
    {
        internal override bool TryParseInternal(ReadOnlySpan<char> input, out MidiTimeSpan result, out string? error)
        {
            if (!long.TryParse(input, out var midiTimeSpan) || midiTimeSpan < 0)
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            result = new MidiTimeSpan(midiTimeSpan);
            error = null;
            return true;
        }
    }
}
