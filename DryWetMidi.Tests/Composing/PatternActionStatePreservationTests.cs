using System.Collections.Generic;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed class PatternActionStatePreservationTests
    {
        [Test]
        public void Clone_PreservesNoteActionState([Values(
            PatternActionState.Enabled,
            PatternActionState.Disabled,
            PatternActionState.Excluded)] PatternActionState state)
        {
            var pattern = CreatePatternWithNote(state);

            var clonedPattern = pattern.Clone();

            AssertNotes(clonedPattern, state, 1, false);
        }

        [Test]
        public void Clone_PreservesChordActionState([Values(
            PatternActionState.Enabled,
            PatternActionState.Disabled,
            PatternActionState.Excluded)] PatternActionState state)
        {
            var pattern = CreatePatternWithChord(state);

            var clonedPattern = pattern.Clone();

            AssertNotes(clonedPattern, state, 1, true);
        }

        [Test]
        public void TransformNotes_PreservesNoteActionState([Values(
            PatternActionState.Enabled,
            PatternActionState.Disabled,
            PatternActionState.Excluded)] PatternActionState state)
        {
            var pattern = CreatePatternWithNote(state);

            var transformedPattern = pattern.TransformNotes(noteDescriptor => noteDescriptor);

            AssertNotes(transformedPattern, state, 1, false);
        }

        [Test]
        public void TransformChords_PreservesChordActionState([Values(
            PatternActionState.Enabled,
            PatternActionState.Disabled,
            PatternActionState.Excluded)] PatternActionState state)
        {
            var pattern = CreatePatternWithChord(state);

            var transformedPattern = pattern.TransformChords(chordDescriptor => chordDescriptor);

            AssertNotes(transformedPattern, state, 1, true);
        }

        [Test]
        public void Repeat_PreservesNoteActionState([Values(
            PatternActionState.Enabled,
            PatternActionState.Disabled,
            PatternActionState.Excluded)] PatternActionState state)
        {
            var pattern = CreatePatternWithNote(state);

            var repeatedPattern = new PatternBuilder(pattern)
                .Repeat()
                .Build();

            AssertNotes(repeatedPattern, state, 2, false);
        }

        [Test]
        public void Repeat_PreservesChordActionState([Values(
            PatternActionState.Enabled,
            PatternActionState.Disabled,
            PatternActionState.Excluded)] PatternActionState state)
        {
            var pattern = CreatePatternWithChord(state);

            var repeatedPattern = new PatternBuilder(pattern)
                .Repeat()
                .Build();

            AssertNotes(repeatedPattern, state, 2, true);
        }

        private static Pattern CreatePatternWithNote(PatternActionState state)
        {
            var pattern = new PatternBuilder()
                .Pattern(new PatternBuilder()
                    .Note(Notes.A3)
                    .Note(Notes.C4)
                    .Build())
                .Build();

            pattern.SetNotesState((index, descriptor) => index == 0, state);

            return pattern;
        }

        private static Pattern CreatePatternWithChord(PatternActionState state)
        {
            var pattern = new PatternBuilder()
                .Pattern(new PatternBuilder()
                    .Chord(new[] { Notes.A3, Notes.D4 })
                    .Note(Notes.C4)
                    .Build())
                .Build();

            pattern.SetChordsState((index, descriptor) => index == 0, state);

            return pattern;
        }

        private static void AssertNotes(Pattern pattern, PatternActionState state, int repetitions, bool isChord)
        {
            var expectedNotes = new List<NoteInfo>();
            var currentTime = 0;

            for (var repetitionIndex = 0; repetitionIndex < repetitions; repetitionIndex++)
            {
                if (state == PatternActionState.Enabled)
                {
                    expectedNotes.Add(new NoteInfo(
                        NoteName.A,
                        3,
                        GetTime(currentTime),
                        PatternBuilder.DefaultNoteLength,
                        PatternBuilder.DefaultVelocity));

                    if (isChord)
                    {
                        expectedNotes.Add(new NoteInfo(
                            NoteName.D,
                            4,
                            GetTime(currentTime),
                            PatternBuilder.DefaultNoteLength,
                            PatternBuilder.DefaultVelocity));
                    }
                }

                if (state != PatternActionState.Excluded)
                    currentTime++;

                expectedNotes.Add(new NoteInfo(
                    NoteName.C,
                    4,
                    GetTime(currentTime),
                    PatternBuilder.DefaultNoteLength,
                    PatternBuilder.DefaultVelocity));
                currentTime++;
            }

            PatternTestUtilities.TestNotes(pattern, expectedNotes);
        }

        private static ITimeSpan GetTime(int quarterNotes)
        {
            return quarterNotes == 0
                ? null
                : PatternBuilder.DefaultNoteLength.Multiply(quarterNotes);
        }
    }
}
