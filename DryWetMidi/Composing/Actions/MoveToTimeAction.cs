using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class MoveToTimeAction : IPatternAction
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

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            if (Time != null)
                context.SaveTime(time);

            return new PatternActionResult(Time != null
                ? TimeConverter.ConvertFrom(Time, context.TempoMap)
                : context.RestoreTime());
        }

        #endregion
    }
}
