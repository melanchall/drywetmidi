using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternBuilderTests
    {
        #region Test methods

        [Test]
        public void ControlChange_1()
        {
            var controlNumber = (SevenBitNumber)10;
            var controlValue = (SevenBitNumber)70;
            var eventTime = MusicalTimeSpan.Quarter;

            var pattern = new PatternBuilder()

                .Note(NoteName.A, eventTime)
                .ControlChange(controlNumber, controlValue)

                .Build();

            PatternTestUtilities.TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new ControlChangeEvent(controlNumber, controlValue) { Channel = PatternTestUtilities.Channel }, eventTime)
            });
        }

        [Test]
        public void ControlChange_2()
        {
            var controlNumber1 = (SevenBitNumber)10;
            var controlValue1 = (SevenBitNumber)70;
            var noteLength1 = MusicalTimeSpan.Quarter;

            var controlNumber2 = (SevenBitNumber)15;
            var controlValue2 = (SevenBitNumber)55;
            var noteLength2 = MusicalTimeSpan.Sixteenth;

            var pattern = new PatternBuilder()

                .Note(NoteName.A, noteLength1)
                .ControlChange(controlNumber1, controlValue1)
                .Note(NoteName.CSharp, noteLength2)
                .ControlChange(controlNumber2, controlValue2)

                .Build();

            PatternTestUtilities.TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new ControlChangeEvent(controlNumber1, controlValue1) { Channel = PatternTestUtilities.Channel }, noteLength1),
                new TimedEventInfo(new ControlChangeEvent(controlNumber2, controlValue2) { Channel = PatternTestUtilities.Channel }, noteLength1 + noteLength2),
            });
        }

        #endregion
    }
}
