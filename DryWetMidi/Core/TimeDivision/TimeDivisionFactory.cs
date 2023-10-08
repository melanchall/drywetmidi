using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    internal static class TimeDivisionFactory
    {
        #region Methods

        internal static TimeDivision GetTimeDivision(short division)
        {
            if (division < 0)
            {
                return new SmpteTimeDivision((SmpteFormat)(-division.GetHead()), division.GetTail());
            }

            return new TicksPerQuarterNoteTimeDivision(division);
        }

        #endregion
    }
}
