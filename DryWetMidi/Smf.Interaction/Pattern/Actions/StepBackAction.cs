using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class StepBackAction : StepAction
    {
        #region Constructor

        public StepBackAction(ITimeSpan step)
            : base(step)
        {
        }

        #endregion

        #region IPatternAction

        public override PatternActionResult Invoke(long time, PatternContext context)
        {
            var tempoMap = context.TempoMap;

            context.SaveTime(time);

            var convertedTime = TimeConverter.ConvertFrom(((MidiTimeSpan)time).Subtract(Step, TimeSpanMode.TimeLength), tempoMap);
            return new PatternActionResult(Math.Max(convertedTime, 0));
        }

        #endregion
    }
}
