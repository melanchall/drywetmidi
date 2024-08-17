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
        public void PitchBend_1()
        {
            var pitchValue = (ushort)8000;
            var eventTime = MusicalTimeSpan.Quarter;

            var pattern = new PatternBuilder()

                .Note(NoteName.A, eventTime)
                .PitchBend(pitchValue)

                .Build();

            PatternTestUtilities.TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new PitchBendEvent(pitchValue) { Channel = PatternTestUtilities.Channel }, eventTime)
            });
        }

        [Test]
        public void PitchBend_2()
        {
            var pitchValue1 = (ushort)8000;
            var noteLength1 = MusicalTimeSpan.Quarter;

            var pitchValue2 = (ushort)5000;
            var noteLength2 = MusicalTimeSpan.Sixteenth;

            var pattern = new PatternBuilder()

                .Note(NoteName.A, noteLength1)
                .PitchBend(pitchValue1)
                .Note(NoteName.CSharp, noteLength2)
                .PitchBend(pitchValue2)

                .Build();

            PatternTestUtilities.TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new PitchBendEvent(pitchValue1) { Channel = PatternTestUtilities.Channel }, noteLength1),
                new TimedEventInfo(new PitchBendEvent(pitchValue2) { Channel = PatternTestUtilities.Channel }, noteLength1 + noteLength2),
            });
        }

        #endregion
    }
}
