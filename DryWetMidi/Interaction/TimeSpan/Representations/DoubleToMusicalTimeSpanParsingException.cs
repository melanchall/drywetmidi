using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// The exception that is thrown by <see cref="MusicalTimeSpan.FromDouble(double, double)"/> when a number
    /// cannot be represented as a <see cref="MusicalTimeSpan"/> within the specified tolerance.
    /// </summary>
    public sealed class DoubleToMusicalTimeSpanParsingException : MidiException
    {
        internal DoubleToMusicalTimeSpanParsingException(string message)
            : base(message)
        {
        }
    }
}
