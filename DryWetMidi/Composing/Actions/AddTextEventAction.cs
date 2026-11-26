using System;
using System.Diagnostics.CodeAnalysis;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class AddTextEventAction<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEvent> : PatternAction
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

        #region Overrides

        public override PatternActionResult Invoke(long time, PatternContext context)
        {
            if (State != PatternActionState.Enabled)
                return PatternActionResult.DoNothing;

            var textEventObject = Activator.CreateInstance(typeof(TEvent), Text);
            if (textEventObject == null)
                throw new InvalidOperationException($"Cannot create an instance of the '{typeof(TEvent).FullName}' type.");

            var textEvent = (BaseTextEvent)textEventObject;
            var timedEvent = new TimedEvent(textEvent, time);

            return new PatternActionResult(time, new[] { timedEvent });
        }

        public override PatternAction Clone()
        {
            return new AddTextEventAction<TEvent>(Text);
        }

        #endregion
    }
}
