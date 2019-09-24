using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternBuilderTests
    {
        #region Test methods

        #region StepBack

        [Test]
        [Description("Step back by metric step and add note.")]
        public void StepBack_Metric()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTimeSpan(0, 0, 10))
                .StepForward(new MetricTimeSpan(0, 0, 30))
                .StepBack(new MetricTimeSpan(0, 0, 37))

                .Note(Notes.A0)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MetricTimeSpan(0, 0, 3), MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Step back by metric step beyond zero and add note.")]
        public void StepBack_Metric_BeyondZero()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTimeSpan(0, 0, 10))
                .StepForward(new MetricTimeSpan(0, 0, 30))
                .StepBack(new MetricTimeSpan(0, 1, 37))

                .Note(Notes.A0)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MetricTimeSpan(0, 0, 0), MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Step back by musical step and add note.")]
        public void StepBack_Musical()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(MusicalTimeSpan.Eighth)
                .StepForward(MusicalTimeSpan.Whole)
                .StepBack(MusicalTimeSpan.Half)

                .Note(Notes.A0)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MusicalTimeSpan(5, 8), MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Step back by musical step beyond zero and add note.")]
        public void StepBack_Musical_BeyondZero()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTimeSpan(0, 0, 10))
                .StepForward(new MetricTimeSpan(0, 0, 30))
                .StepBack(1000 * MusicalTimeSpan.Quarter)

                .Note(Notes.A0)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MetricTimeSpan(0, 0, 0), MusicalTimeSpan.Quarter)
            });
        }

        #endregion

        #region MoveToPreviousTime

        [Test]
        public void MoveToPreviousTime()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A0)
                .MoveToPreviousTime()
                .Note(Notes.B0)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.B, 0, null, PatternBuilder.DefaultNoteLength)
            });
        }

        [Test]
        public void MoveToPreviousTime_Pattern()
        {
            var subPattern = new PatternBuilder()
                .Note(Notes.A0)
                .Build();

            var pattern = new PatternBuilder()
                .Pattern(subPattern)
                .MoveToPreviousTime()
                .Note(Notes.B0)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.B, 0, null, PatternBuilder.DefaultNoteLength)
            });
        }

        #endregion

        #endregion
    }
}
