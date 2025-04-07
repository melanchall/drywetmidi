using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;

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
        public void PianoRoll_3()
        {
            var step = MusicalTimeSpan.Sixteenth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(step)
                .SetVelocity(velocity)
                .PianoRoll(@"
                    57  ---- ---|
                    47  --|- --|-
                    44  |--- |--|
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
        public void PianoRoll_CustomSymbols_SingleCellNoteSymbolIsSpace() => ClassicAssert.Throws<ArgumentException>(
            () => new PatternBuilder().PianoRoll(@"AH3  ----", new PianoRollSettings
            {
                SingleCellNoteSymbol = ' ',
            }));

        [Test]
        public void PianoRoll_CustomSymbols_MultiCellNoteStartSymbolIsSpace() => ClassicAssert.Throws<ArgumentException>(
            () => new PatternBuilder().PianoRoll(@"AH3  ----", new PianoRollSettings
            {
                MultiCellNoteStartSymbol = ' ',
            }));

        [Test]
        public void PianoRoll_CustomSymbols_MultiCellNoteEndSymbolIsSpace() => ClassicAssert.Throws<ArgumentException>(
            () => new PatternBuilder().PianoRoll(@"AH3  ----", new PianoRollSettings
            {
                MultiCellNoteEndSymbol = ' ',
            }));

        [Test]
        public void PianoRoll_CustomActions_1()
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
                    CustomActions = new[]
                    {
                        PianoRollAction.CreateSingleCell('/', (builder, context) => builder
                            .StepBack(MusicalTimeSpan.ThirtySecond)
                            .Note(context.Note, MusicalTimeSpan.ThirtySecond, (SevenBitNumber)(builder.Velocity * 0.5))
                            .Note(context.Note)),
                        PianoRollAction.CreateSingleCell('#', (builder, context) => builder
                            .StepBack(MusicalTimeSpan.ThirtySecond)
                            .Note(context.Note, new MusicalTimeSpan(1, 64), (SevenBitNumber)70)
                            .Note(context.Note, new MusicalTimeSpan(1, 64), (SevenBitNumber)50)
                            .Note(context.Note)),
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
        public void PianoRoll_CustomActions_2()
        {
            var step = MusicalTimeSpan.Sixteenth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(step)
                .SetVelocity(velocity)
                .PianoRoll(@"
                    B2    --/- --#- {==}
                    G#2   +--- +--- ----
                ", new PianoRollSettings
                {
                    SingleCellNoteSymbol = '+',
                    CustomActions = new[]
                    {
                        PianoRollAction.CreateSingleCell('/', (builder, context) => builder
                            .StepBack(MusicalTimeSpan.ThirtySecond)
                            .Note(context.Note, MusicalTimeSpan.ThirtySecond, (SevenBitNumber)(builder.Velocity * 0.5))
                            .Note(context.Note)),
                        PianoRollAction.CreateSingleCell('#', (builder, context) => builder
                            .StepBack(MusicalTimeSpan.ThirtySecond)
                            .Note(context.Note, new MusicalTimeSpan(1, 64), (SevenBitNumber)70)
                            .Note(context.Note, new MusicalTimeSpan(1, 64), (SevenBitNumber)50)
                            .Note(context.Note)),
                        PianoRollAction.CreateMultiCell('{', '}', (builder, context) => builder
                            .Note(context.Note, builder.NoteLength.Multiply(context.CellsNumber), (SevenBitNumber)40)),
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

                new NoteInfo(NoteName.B, 2, step * 8, step * 4, (SevenBitNumber)40),
            });
        }

        [TestCase('{', '}')]
        [TestCase('/', '/')]
        public void PianoRoll_CustomActions_MultiCell(char startSymbol, char endSymbol)
        {
            var step = MusicalTimeSpan.Sixteenth;
            var velocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(step)
                .SetVelocity(velocity)
                .PianoRoll($@"
                    B2    {startSymbol}=={endSymbol}
                    G#2   ----
                ", new PianoRollSettings
                {
                    CustomActions = new[]
                    {
                        PianoRollAction.CreateMultiCell(startSymbol, endSymbol, (builder, context) => builder
                            .Note(context.Note, context.Length, (SevenBitNumber)40)),
                    },
                })
                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.B, 2, step * 0, step * 4, (SevenBitNumber)40),
            });
        }

        [Test]
        public void PianoRoll_CustomActions_ContainsSingleCellNoteSymbol() => ClassicAssert.Throws<ArgumentOutOfRangeException>(
            () => new PatternBuilder().PianoRoll(@"AH3  ----", new PianoRollSettings
            {
                CustomActions = new[]
                {
                    PianoRollAction.CreateSingleCell('|', (_, __) => { }),
                }
            }));

        [Test]
        public void PianoRoll_CustomActions_ContainsMultiCellNoteStartSymbol() => ClassicAssert.Throws<ArgumentOutOfRangeException>(
            () => new PatternBuilder().PianoRoll(@"AH3  ----", new PianoRollSettings
            {
                CustomActions = new[]
                {
                    PianoRollAction.CreateSingleCell('[', (_, __) => { }),
                }
            }));

        [Test]
        public void PianoRoll_CustomActions_ContainsMultiCellNoteEndSymbol() => ClassicAssert.Throws<ArgumentOutOfRangeException>(
            () => new PatternBuilder().PianoRoll(@"AH3  ----", new PianoRollSettings
            {
                CustomActions = new[]
                {
                    PianoRollAction.CreateSingleCell(']', (_, __) => { }),
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
        public void PianoRoll_FailedToParseNote_1()
        {
            var exception = ClassicAssert.Throws<InvalidOperationException>(() => new PatternBuilder().PianoRoll(
                @"AH3  ----"));
            StringAssert.Contains("0", exception.Message, "No line index.");
        }

        [Test]
        public void PianoRoll_FailedToParseNote_2()
        {
            var exception = ClassicAssert.Throws<InvalidOperationException>(() => new PatternBuilder().PianoRoll(
                @"A3   ----
                  AH3  ----"));
            StringAssert.Contains("1", exception.Message, "No line index.");
        }

        [Test]
        public void PianoRoll_SingleCellNoteInMultiCellOne_1()
        {
            var exception = ClassicAssert.Throws<InvalidOperationException>(() => new PatternBuilder().PianoRoll(
                @"A3  -[-|-]"));
            StringAssert.Contains("0", exception.Message, "No line index.");
            StringAssert.Contains("7", exception.Message, "No symbol index.");
        }

        [Test]
        public void PianoRoll_SingleCellNoteInMultiCellOne_2()
        {
            var exception = ClassicAssert.Throws<InvalidOperationException>(() => new PatternBuilder().PianoRoll(
                "B3  -[---]\nA3  -[-|-]"));
            StringAssert.Contains("1", exception.Message, "No line index.");
            StringAssert.Contains("7", exception.Message, "No symbol index.");
        }

        [Test]
        public void PianoRoll_NoteStartedWithPreviousNotEnded()
        {
            var exception = ClassicAssert.Throws<InvalidOperationException>(() => new PatternBuilder().PianoRoll(
                @"A3  -[-[-]]"));
            StringAssert.Contains("0", exception.Message, "No line index.");
            StringAssert.Contains("7", exception.Message, "No symbol index.");
        }

        [Test]
        public void PianoRoll_NoteNotStarted()
        {
            var exception = ClassicAssert.Throws<InvalidOperationException>(() => new PatternBuilder().PianoRoll(
                @"A3  -[]--]--"));
            StringAssert.Contains("0", exception.Message, "No line index.");
            StringAssert.Contains("9", exception.Message, "No symbol index.");
        }

        #endregion
    }
}
