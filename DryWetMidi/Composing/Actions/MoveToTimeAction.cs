using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class MoveToTimeAction : PatternAction
    {
        #region Constructor

        public MoveToTimeAction()
            : this(null)
        {
        }

        public MoveToTimeAction(ITimeSpan time)
        {
            Time = time;
        }

        #endregion

        #region Properties

        public ITimeSpan Time { get; }

        #endregion

        #region Overrides

        public override PatternActionResult Invoke(long time, PatternContext context)
        {
            if (State != PatternActionState.Enabled)
                return PatternActionResult.DoNothing;

            if (Time != null)
                context.SaveTime(time);

            return new PatternActionResult(Time != null
                ? TimeConverter.ConvertFrom(Time, context.TempoMap)
                : context.RestoreTime());
        }

        public override PatternAction Clone()
        {
            return new MoveToTimeAction(Time?.Clone());
        }

        #endregion
    }
}
