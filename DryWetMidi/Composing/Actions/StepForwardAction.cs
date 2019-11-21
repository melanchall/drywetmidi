using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class StepForwardAction : StepAction
    {
        #region Constructor

        public StepForwardAction(ITimeSpan step)
            : base(step)
        {
        }

        #endregion

        #region IPatternAction

        public override PatternActionResult Invoke(long time, PatternContext context)
        {
            context.SaveTime(time);
            return new PatternActionResult(time + LengthConverter.ConvertFrom(Step, time, context.TempoMap));
        }

        #endregion
    }
}
