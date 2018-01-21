using Melanchall.DryWetMidi.Standards;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class AddProgramChangeEventAction : IPatternAction
    {
        #region Constructor

        public AddProgramChangeEventAction(SevenBitNumber programNumber)
        {
            ProgramNumber = programNumber;
        }

        public AddProgramChangeEventAction(GeneralMidi.Program program)
        {
            Program = program;
        }

        #endregion

        #region Properties

        public SevenBitNumber? ProgramNumber { get; }

        public GeneralMidi.Program Program { get; }

        #endregion

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            var programChangeEvent = ProgramNumber != null
                ? new ProgramChangeEvent(ProgramNumber.Value)
                : Program.GetProgramChangeEvent();
            var timedEvent = new TimedEvent(programChangeEvent, time);

            return new PatternActionResult(time, new[] { timedEvent });
        }

        #endregion
    }
}
