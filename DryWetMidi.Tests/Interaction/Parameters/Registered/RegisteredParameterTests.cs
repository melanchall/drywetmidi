using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    public abstract class RegisteredParameterTests<TParameter>
        where TParameter : RegisteredParameter, new()
    {
        #region Methods

        protected TParameter GetDefaultParameter() => new TParameter();

        protected TParameter GetParameter(Func<TParameter> createParameter) => createParameter();

        protected void CheckTimedEvents(TParameter registeredParameter, params (byte ControlNumber, byte ControlValue)[] expectedEvents)
        {
            var timedEvents = registeredParameter.GetTimedEvents();
            Assert.AreEqual(1, timedEvents.Select(e => e.Time).Distinct().Count(), "Time is different for some timed events.");
            Assert.IsTrue(timedEvents.All(e => e.Time == registeredParameter.Time), "Time is invalid.");

            var midiEvents = timedEvents.Select(e => e.Event).ToArray();
            Assert.IsTrue(midiEvents.All(e => e.EventType == MidiEventType.ControlChange), "Some events have not Control Change type.");
            Assert.IsTrue(midiEvents.All(e => e is ControlChangeEvent), "Some events are not Control Change ones.");

            Assert.That(
                midiEvents,
                Is.EqualTo(expectedEvents.Select(e => new ControlChangeEvent((SevenBitNumber)e.ControlNumber, (SevenBitNumber)e.ControlValue) { Channel = registeredParameter.Channel })).Using(new MidiEventEqualityComparer()),
                "Events are invalid.");
        }

        #endregion
    }
}
