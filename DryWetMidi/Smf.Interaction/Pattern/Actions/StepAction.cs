namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal abstract class StepAction : IPatternAction
    {
        #region Constructor

        public StepAction(ITimeSpan step)
        {
            Step = step;
        }

        #endregion

        #region Properties

        public ITimeSpan Step { get; }

        #endregion

        #region IPatternAction

        public abstract PatternActionResult Invoke(long time, PatternContext context);

        #endregion
    }
}
