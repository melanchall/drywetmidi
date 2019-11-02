using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternBuilderTests
    {
        #region Test methods

        [Test]
        [Description("Add single lyrics event.")]
        public void Lyrics_Single()
        {
            var pattern = new PatternBuilder()

                .Note(NoteName.A)
                .Lyrics("A")

                .Build();

            PatternTestUtilities.TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new LyricEvent("A"), MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Add multiple lyrics events.")]
        public void Lyrics_Multiple()
        {
            var pattern = new PatternBuilder()

                .Note(NoteName.A)
                .Lyrics("A")
                .Note(NoteName.A)
                .Lyrics("B")

                .Build();

            PatternTestUtilities.TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new LyricEvent("A"), MusicalTimeSpan.Quarter),
                new TimedEventInfo(new LyricEvent("B"), MusicalTimeSpan.Half)
            });
        }

        [Test]
        [Description("Add lyrics events using repeat.")]
        public void Lyrics_Repeat()
        {
            var pattern = new PatternBuilder()

                .Note(NoteName.A)
                .Lyrics("A")
                .Repeat(2, 1)

                .Build();

            PatternTestUtilities.TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new LyricEvent("A"), MusicalTimeSpan.Quarter),
                new TimedEventInfo(new LyricEvent("A"), MusicalTimeSpan.Half)
            });
        }

        [Test]
        [Description("Add single marker event.")]
        public void Marker_Single()
        {
            var pattern = new PatternBuilder()

                .Note(NoteName.A)
                .Marker("Marker 1")

                .Build();

            PatternTestUtilities.TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new MarkerEvent("Marker 1"), MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Add multiple marker events.")]
        public void Marker_Multiple()
        {
            var pattern = new PatternBuilder()

                .Note(NoteName.A)
                .Marker("Marker 1")
                .Note(NoteName.A)
                .Marker("Marker 2")

                .Build();

            PatternTestUtilities.TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new MarkerEvent("Marker 1"), MusicalTimeSpan.Quarter),
                new TimedEventInfo(new MarkerEvent("Marker 2"), MusicalTimeSpan.Half)
            });
        }

        [Test]
        [Description("Add marker events using repeat.")]
        public void Marker_Repeat()
        {
            const string markerName = "Marker";

            var pattern = new PatternBuilder()

                .Note(NoteName.A)
                .Marker(markerName)
                .Repeat(2, 1)

                .Build();

            PatternTestUtilities.TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new MarkerEvent(markerName), MusicalTimeSpan.Quarter),
                new TimedEventInfo(new MarkerEvent(markerName), MusicalTimeSpan.Half)
            });
        }

        #endregion
    }
}
