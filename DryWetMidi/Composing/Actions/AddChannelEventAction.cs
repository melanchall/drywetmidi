using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing.Actions
{
    internal abstract class AddChannelEventAction<TEvent> : PatternAction
        where TEvent : ChannelEvent
    {
        #region Methods

        protected abstract TEvent CreateEvent();

        #endregion

        #region Overrides

        public override PatternActionResult Invoke(long time, PatternContext context)
        {
            if (State != PatternActionState.Enabled)
                return PatternActionResult.DoNothing;

            var midiEvent = CreateEvent();
            midiEvent.Channel = context.Channel;
            var timedEvent = new TimedEvent(midiEvent, time);

            return new PatternActionResult(time, new[] { timedEvent });
        }

        #endregion
    }
}
