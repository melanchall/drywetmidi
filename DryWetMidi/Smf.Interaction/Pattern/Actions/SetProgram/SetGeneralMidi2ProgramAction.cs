using System.Linq;
using Melanchall.DryWetMidi.Standards;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class SetGeneralMidi2ProgramAction : IPatternAction
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

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            var programEvents = Program.GetProgramEvents(context.Channel);
            var timedEvents = programEvents.Select(e => new TimedEvent(e, time));

            return new PatternActionResult(time, timedEvents);
        }

        #endregion
    }
}
