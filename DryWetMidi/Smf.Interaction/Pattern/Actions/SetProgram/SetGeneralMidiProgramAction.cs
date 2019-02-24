using Melanchall.DryWetMidi.Standards;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class SetGeneralMidiProgramAction : IPatternAction
    {
        #region Constructor

        public SetGeneralMidiProgramAction(GeneralMidiProgram program)
        {
            Program = program;
        }

        #endregion

        #region Properties

        public GeneralMidiProgram Program { get; }

        #endregion

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            var programEvent = Program.GetProgramEvent(context.Channel);
            var timedEvent = new TimedEvent(programEvent, time);

            return new PatternActionResult(time, new[] { timedEvent });
        }

        #endregion
    }
}
