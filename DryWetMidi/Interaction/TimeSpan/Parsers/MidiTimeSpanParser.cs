using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MidiTimeSpanParser : SimpleParser<MidiTimeSpan>
    {
        protected override MidiTimeSpan ParseInternal(string input)
        {
            if (!long.TryParse(input, out var midiTimeSpan))
                ThrowInvalidFormatError();

            return new MidiTimeSpan(midiTimeSpan);
        }
    }
}
