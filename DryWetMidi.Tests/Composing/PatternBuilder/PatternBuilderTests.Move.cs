using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternBuilderTests
    {
        #region Test methods

        #region StepBack

        [Test]
        public void StepBack()
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
        public void StepBack_BeyondZero()
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
        public void StepBack_DefaultStep()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(MusicalTimeSpan.Whole)
                .StepBack()
                .Note(Notes.A0)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, MusicalTimeSpan.Half.SingleDotted(), MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        public void StepBack_CustomStep()
        {
            var pattern = new PatternBuilder()
                .SetStep(MusicalTimeSpan.Sixteenth)
                .MoveToTime(MusicalTimeSpan.Whole)
                .StepBack()
                .Note(Notes.A0)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, 15 * MusicalTimeSpan.Sixteenth, MusicalTimeSpan.Quarter)
            });
        }

        #endregion

        #region StepBack

        [Test]
        public void StepForward()
        {
            var pattern = new PatternBuilder()
                .StepForward(new MetricTimeSpan(0, 0, 30))
                .Note(Notes.A0)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MetricTimeSpan(0, 0, 30), PatternBuilder.DefaultNoteLength)
            });
        }

        [Test]
        public void StepForward_DefaultStep()
        {
            var pattern = new PatternBuilder()
                .StepForward()
                .Note(Notes.A0)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, PatternBuilder.DefaultStep, PatternBuilder.DefaultNoteLength)
            });
        }

        [Test]
        public void StepForward_CustomStep()
        {
            var pattern = new PatternBuilder()
                .SetStep(MusicalTimeSpan.Sixteenth)
                .StepForward()
                .Note(Notes.A0)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, MusicalTimeSpan.Sixteenth, PatternBuilder.DefaultNoteLength)
            });
        }

        #endregion

        #region MoveToTime

        [Test]
        public void MoveToTime()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTimeSpan(1, 0, 0))
                .Note(Notes.A0)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MetricTimeSpan(1, 0, 0), PatternBuilder.DefaultNoteLength)
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
