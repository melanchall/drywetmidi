using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal sealed class TimeAndLength
    {
        #region Constructor

        public TimeAndLength(ITimeSpan time, ITimeSpan length)
        {
            Time = time;
            Length = length;
        }

        #endregion

        #region Properties

        public ITimeSpan Time { get; }

        public ITimeSpan Length { get; }

        #endregion
    }
}
