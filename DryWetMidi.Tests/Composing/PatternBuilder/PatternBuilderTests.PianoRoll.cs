using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternBuilderTests
    {
        #region Test methods

        [Test]
        public void PianoRoll_1()
        {
            var step = MusicalTimeSpan.Sixteenth;
            var velocity = (SevenBitNumber)70;

            var pattern = new PatternBuilder()
                .SetNoteLength(step)
                .SetVelocity(velocity)
                .PianoRoll(@"A#5 --|--|--[==]--[....]")
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.ASharp, 5, step * 2, step, velocity),
                new NoteInfo(NoteName.ASharp, 5, step * 5, step, velocity),
                new NoteInfo(NoteName.ASharp, 5, step * 8, step * 4, velocity),
                new NoteInfo(NoteName.ASharp, 5, step * 14, step * 6, velocity),
            });
        }

        [Test]
        public void PianoRoll_2()
        {
            var step = MusicalTimeSpan.Sixteenth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(step)
                .SetVelocity(velocity)
                .PianoRoll(@"
                    A3  ---- ---|
                    B2  --|- --|-
                    G#2 |--- |--|
                ")
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.GSharp, 2, step * 0, step, velocity),
                new NoteInfo(NoteName.B, 2, step * 2, step, velocity),
                new NoteInfo(NoteName.GSharp, 2, step * 4, step, velocity),
                new NoteInfo(NoteName.B, 2, step * 6, step, velocity),
                new NoteInfo(NoteName.A, 3, step * 7, step, velocity),
                new NoteInfo(NoteName.GSharp, 2, step * 7, step, velocity),
            });
        }

        [Test]
        public void PianoRoll_CustomSymbols()
        {
            var step = MusicalTimeSpan.Sixteenth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(step)
                .SetVelocity(velocity)
                .PianoRoll(@"
                    A3  ---- ---+ <--->---
                    B2  --+- --+- --<--->-
                    G#2 +--- +--+ ---<--->
                ", new PianoRollSettings
                {
                    SingleCellNoteSymbol = '+',
                    MultiCellNoteStartSymbol = '<',
                    MultiCellNoteEndSymbol = '>',
                })
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.GSharp, 2, step * 0, step, velocity),
                new NoteInfo(NoteName.B, 2, step * 2, step, velocity),
                new NoteInfo(NoteName.GSharp, 2, step * 4, step, velocity),
                new NoteInfo(NoteName.B, 2, step * 6, step, velocity),
                new NoteInfo(NoteName.A, 3, step * 7, step, velocity),
                new NoteInfo(NoteName.GSharp, 2, step * 7, step, velocity),

                new NoteInfo(NoteName.A, 3, step * 8, step * 5, velocity),
                new NoteInfo(NoteName.B, 2, step * 10, step * 5, velocity),
                new NoteInfo(NoteName.GSharp, 2, step * 11, step * 5, velocity),
            });
        }

        [Test]
        public void PianoRoll_CustomSymbols_SingleCellNoteSymbolIsSpace() => Assert.Throws<ArgumentException>(
            () => new PatternBuilder().PianoRoll(@"AH3  ----", new PianoRollSettings
            {
                SingleCellNoteSymbol = ' ',
            }));

        [Test]
        public void PianoRoll_CustomSymbols_MultiCellNoteStartSymbolIsSpace() => Assert.Throws<ArgumentException>(
            () => new PatternBuilder().PianoRoll(@"AH3  ----", new PianoRollSettings
            {
                MultiCellNoteStartSymbol = ' ',
            }));

        [Test]
        public void PianoRoll_CustomSymbols_MultiCellNoteEndSymbolIsSpace() => Assert.Throws<ArgumentException>(
            () => new PatternBuilder().PianoRoll(@"AH3  ----", new PianoRollSettings
            {
                MultiCellNoteEndSymbol = ' ',
            }));

        [Test]
        public void PianoRoll_CustomActions()
        {
            var step = MusicalTimeSpan.Sixteenth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(step)
                .SetVelocity(velocity)
                .PianoRoll(@"
                    B2    --/- --#-
                    G#2   +--- +---
                ", new PianoRollSettings
                {
                    SingleCellNoteSymbol = '+',
                    CustomActions = new Dictionary<char, Action<DryWetMidi.MusicTheory.Note, PatternBuilder>>
                    {
                        ['/'] = (note, builder) => builder
                            .StepBack(MusicalTimeSpan.ThirtySecond)
                            .Note(note, MusicalTimeSpan.ThirtySecond, (SevenBitNumber)(builder.Velocity * 0.5))
                            .Note(note),
                        ['#'] = (note, builder) => builder
                            .StepBack(MusicalTimeSpan.ThirtySecond)
                            .Note(note, new MusicalTimeSpan(1, 64), (SevenBitNumber)70)
                            .Note(note, new MusicalTimeSpan(1, 64), (SevenBitNumber)50)
                            .Note(note),
                    },
                })
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.GSharp, 2, step * 0, step, velocity),

                new NoteInfo(NoteName.B, 2, step * 2 - MusicalTimeSpan.ThirtySecond, MusicalTimeSpan.ThirtySecond, (SevenBitNumber)45),
                new NoteInfo(NoteName.B, 2, step * 2, step, velocity),

                new NoteInfo(NoteName.GSharp, 2, step * 4, step, velocity),

                new NoteInfo(NoteName.B, 2, step * 6 - MusicalTimeSpan.ThirtySecond, new MusicalTimeSpan(1, 64), (SevenBitNumber)70),
                new NoteInfo(NoteName.B, 2, step * 6 - new MusicalTimeSpan(1, 64), new MusicalTimeSpan(1, 64), (SevenBitNumber)50),
                new NoteInfo(NoteName.B, 2, step * 6, step, velocity),
            });
        }

        [Test]
        public void PianoRoll_CustomActions_ContainsSingleCellNoteSymbol() => Assert.Throws<ArgumentOutOfRangeException>(
            () => new PatternBuilder().PianoRoll(@"AH3  ----", new PianoRollSettings
            {
                CustomActions = new Dictionary<char, Action<DryWetMidi.MusicTheory.Note, PatternBuilder>>
                {
                    ['|'] = (note, builder) => { },
                }
            }));

        [Test]
        public void PianoRoll_CustomActions_ContainsMultiCellNoteStartSymbol() => Assert.Throws<ArgumentOutOfRangeException>(
            () => new PatternBuilder().PianoRoll(@"AH3  ----", new PianoRollSettings
            {
                CustomActions = new Dictionary<char, Action<DryWetMidi.MusicTheory.Note, PatternBuilder>>
                {
                    ['['] = (note, builder) => { },
                }
            }));

        [Test]
        public void PianoRoll_CustomActions_ContainsMultiCellNoteEndSymbol() => Assert.Throws<ArgumentOutOfRangeException>(
            () => new PatternBuilder().PianoRoll(@"AH3  ----", new PianoRollSettings
            {
                CustomActions = new Dictionary<char, Action<DryWetMidi.MusicTheory.Note, PatternBuilder>>
                {
                    [']'] = (note, builder) => { },
                }
            }));

        [Test]
        public void PianoRoll_Repeat()
        {
            var step = MusicalTimeSpan.Sixteenth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(step)
                .SetVelocity(velocity)
                .PianoRoll(@"
                    A3  ---- ---|
                    B2  --|- --|-
                    G#2 |--- |--|
                ")
                .Repeat(1)
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.GSharp, 2, step * 0, step, velocity),
                new NoteInfo(NoteName.B, 2, step * 2, step, velocity),
                new NoteInfo(NoteName.GSharp, 2, step * 4, step, velocity),
                new NoteInfo(NoteName.B, 2, step * 6, step, velocity),
                new NoteInfo(NoteName.A, 3, step * 7, step, velocity),
                new NoteInfo(NoteName.GSharp, 2, step * 7, step, velocity),

                new NoteInfo(NoteName.GSharp, 2, step * 8, step, velocity),
                new NoteInfo(NoteName.B, 2, step * 10, step, velocity),
                new NoteInfo(NoteName.GSharp, 2, step * 12, step, velocity),
                new NoteInfo(NoteName.B, 2, step * 14, step, velocity),
                new NoteInfo(NoteName.A, 3, step * 15, step, velocity),
                new NoteInfo(NoteName.GSharp, 2, step * 15, step, velocity),
            });
        }

        [Test]
        public void PianoRoll_FailedToParseNote() => Assert.Throws<InvalidOperationException>(
            () => new PatternBuilder().PianoRoll(@"AH3  ----"));

        [Test]
        public void PianoRoll_SingleCellNoteInMultiCellOne() => Assert.Throws<InvalidOperationException>(
            () => new PatternBuilder().PianoRoll(@"A3  -[-|-]"));

        [Test]
        public void PianoRoll_NoteStartedWithPreviousNotEnded() => Assert.Throws<InvalidOperationException>(
            () => new PatternBuilder().PianoRoll(@"A3  -[-[-]]"));

        [Test]
        public void PianoRoll_NoteNotStarted() => Assert.Throws<InvalidOperationException>(
            () => new PatternBuilder().PianoRoll(@"A3  -[]--]--"));

        #endregion
    }
}
