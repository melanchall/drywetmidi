using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class DoubleToMusicalTimeSpanParsingException : MidiException
    {
        internal DoubleToMusicalTimeSpanParsingException(string message)
            : base(message)
        {
        }
    }
}
