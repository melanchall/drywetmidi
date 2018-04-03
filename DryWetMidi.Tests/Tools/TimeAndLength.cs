using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    internal sealed class TimeAndLength
    {
        #region Constrcutor

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
