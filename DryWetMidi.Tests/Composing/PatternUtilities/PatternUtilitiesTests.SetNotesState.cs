using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternUtilitiesTests
    {
        #region Test methods

        [Test]
        public void SetNotesState_SelectAll_Enabled()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => true, PatternActionState.Enabled);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectAll_Disabled()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => true, PatternActionState.Disabled);

            PatternTestUtilities.TestNotes(pattern, new NoteInfo[] { });
        }

        [Test]
        public void SetNotesState_SelectAll_Excluded()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => true, PatternActionState.Excluded);

            PatternTestUtilities.TestNotes(pattern, new NoteInfo[] { });
        }

        [Test]
        public void SetNotesState_SelectSome_Enabled()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => d.Note.Octave == 2, PatternActionState.Enabled);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectSome_Disabled()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => i == 0, PatternActionState.Disabled);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectSome_Excluded()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => d.Note == Notes.A2, PatternActionState.Excluded);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectNone([Values] PatternActionState state)
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.D3)
                .Build();

            pattern.SetNotesState((i, d) => false, PatternActionState.Excluded);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectSome_Enabled_Recursive()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.C3)
                .Pattern(new PatternBuilder()
                    .Note(Notes.DSharp3)
                    .Note(Notes.B1)
                    .Build())
                .Build();

            pattern.SetNotesState((i, d) => i == 0 || i == 2, PatternActionState.Enabled, true);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.C, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),

                new NoteInfo(NoteName.DSharp, 3, PatternBuilder.DefaultNoteLength.Multiply(2), PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.B, 1, PatternBuilder.DefaultNoteLength.Multiply(3), PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectSome_Disabled_Recursive()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.C3)
                .Pattern(new PatternBuilder()
                    .Note(Notes.DSharp3)
                    .Note(Notes.B1)
                    .Build())
                .Build();

            pattern.SetNotesState((i, d) => i == 0 || i == 2, PatternActionState.Disabled, true);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.C, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),

                new NoteInfo(NoteName.B, 1, PatternBuilder.DefaultNoteLength.Multiply(3), PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetNotesState_SelectSome_Excluded_Recursive()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Note(Notes.C3)
                .Pattern(new PatternBuilder()
                    .Note(Notes.DSharp3)
                    .Note(Notes.B1)
                    .Build())
                .Build();

            pattern.SetNotesState((i, d) => i == 0 || i == 2, PatternActionState.Excluded, true);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.C, 3, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),

                new NoteInfo(NoteName.B, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        #endregion
    }
}
