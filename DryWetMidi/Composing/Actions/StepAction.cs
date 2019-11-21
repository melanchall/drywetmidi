using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
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
