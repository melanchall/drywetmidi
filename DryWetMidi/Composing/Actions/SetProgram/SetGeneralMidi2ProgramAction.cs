using System.Linq;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Standards;

namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class SetGeneralMidi2ProgramAction : PatternAction
    {
        #region Constructor

        public SetGeneralMidi2ProgramAction(GeneralMidi2Program program)
        {
            Program = program;
        }

        #endregion

        #region Properties

        public GeneralMidi2Program Program { get; }

        #endregion

        #region Overrides

        public override PatternActionResult Invoke(long time, PatternContext context)
        {
            if (State != PatternActionState.Enabled)
                return PatternActionResult.DoNothing;

            var programEvents = Program.GetProgramEvents(context.Channel);
            var timedEvents = programEvents.Select(e => new TimedEvent(e, time));

            return new PatternActionResult(time, timedEvents);
        }

        public override PatternAction Clone()
        {
            return new SetGeneralMidi2ProgramAction(Program);
        }

        #endregion
    }
}
