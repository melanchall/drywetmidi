using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class SetProgramNumberAction : PatternAction
    {
        #region Constructor

        public SetProgramNumberAction(SevenBitNumber programNumber)
        {
            ProgramNumber = programNumber;
        }

        #endregion

        #region Properties

        public SevenBitNumber ProgramNumber { get; }

        #endregion

        #region Overrides

        public override PatternActionResult Invoke(long time, PatternContext context)
        {
            if (State != PatternActionState.Enabled)
                return PatternActionResult.DoNothing;

            var programChangeEvent = new ProgramChangeEvent(ProgramNumber) { Channel = context.Channel };
            var timedEvent = new TimedEvent(programChangeEvent, time);

            return new PatternActionResult(time, new[] { timedEvent });
        }

        public override PatternAction Clone()
        {
            return new SetProgramNumberAction(ProgramNumber);
        }

        #endregion
    }
}
