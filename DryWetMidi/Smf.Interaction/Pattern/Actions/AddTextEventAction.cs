using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class AddTextEventAction<TEvent> : IPatternAction
        where TEvent : BaseTextEvent
    {
        #region Constructor

        public AddTextEventAction(string text)
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; }

        #endregion

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            var textEvent = (BaseTextEvent)Activator.CreateInstance(typeof(TEvent), Text);
            var timedEvent = new TimedEvent(textEvent, time);

            return new PatternActionResult(time, new[] { timedEvent });
        }

        #endregion
    }
}
