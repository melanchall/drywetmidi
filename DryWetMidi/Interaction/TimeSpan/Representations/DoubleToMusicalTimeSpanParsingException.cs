using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// The exception that is thrown when a number cannot be represented as a <see cref="MusicalTimeSpan"/>.
    /// </summary>
    public sealed class DoubleToMusicalTimeSpanParsingException : MidiException
    {
        internal DoubleToMusicalTimeSpanParsingException(string message)
            : base(message)
        {
        }
    }
}
