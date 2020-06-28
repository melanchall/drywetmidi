using System.Linq;
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
        public void SetChordsState_SelectAll_Enabled()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A2)
                .Chord(new[] { Notes.D3, Notes.A1 })
                .Build();

            pattern.SetChordsState((i, d) => true, PatternActionState.Enabled);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.A, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectAll_Disabled()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Chord(new[] { Notes.D3, Notes.ASharp5 })
                .Build();

            pattern.SetChordsState((i, d) => true, PatternActionState.Disabled);

            PatternTestUtilities.TestNotes(pattern, new NoteInfo[] { });
        }

        [Test]
        public void SetChordsState_SelectAll_Excluded()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Chord(new[] { Notes.D3, Notes.ASharp5 })
                .Build();

            pattern.SetChordsState((i, d) => true, PatternActionState.Excluded);

            PatternTestUtilities.TestNotes(pattern, new NoteInfo[] { });
        }

        [Test]
        public void SetChordsState_SelectSome_Enabled()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Chord(new[] { Notes.D3, Notes.ASharp5 })
                .Build();

            pattern.SetChordsState((i, d) => d.Notes.Any(n => n.Octave == 2), PatternActionState.Enabled);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 5, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectSome_Disabled()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Chord(new[] { Notes.D3, Notes.ASharp5 })
                .Build();

            pattern.SetChordsState((i, d) => i == 0, PatternActionState.Disabled);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 5, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectSome_Excluded()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Chord(new[] { Notes.D3, Notes.ASharp5 })
                .Build();

            pattern.SetChordsState((i, d) => d.Notes.Contains(Notes.A2), PatternActionState.Excluded);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 3, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 5, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectNone([Values] PatternActionState state)
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Chord(new[] { Notes.D3, Notes.ASharp5 })
                .Build();

            pattern.SetChordsState((i, d) => false, PatternActionState.Excluded);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 5, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectSome_Enabled_Recursive()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Pattern(new PatternBuilder()
                    .Chord(new[] { Notes.D3, Notes.ASharp5 })
                    .Build())
                .Build();

            pattern.SetChordsState((i, d) => i == 0, PatternActionState.Enabled, true);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 5, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectSome_Disabled_Recursive()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Pattern(new PatternBuilder()
                    .Chord(new[] { Notes.D3, Notes.ASharp5 })
                    .Chord(new[] { Notes.D6 })
                    .Build())
                .Build();

            pattern.SetChordsState((i, d) => i == 1, PatternActionState.Disabled, true);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.D, 6, PatternBuilder.DefaultNoteLength.Multiply(2), PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void SetChordsState_SelectSome_Excluded_Recursive()
        {
            var pattern = new PatternBuilder()
                .Chord(new[] { Notes.A2 })
                .Pattern(new PatternBuilder()
                    .Chord(new[] { Notes.D3, Notes.ASharp5 })
                    .Chord(new[] { Notes.D6 })
                    .Build())
                .Build();

            pattern.SetChordsState((i, d) => i == 0 || i == 1, PatternActionState.Excluded, true);

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.D, 6, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        #endregion
    }
}
