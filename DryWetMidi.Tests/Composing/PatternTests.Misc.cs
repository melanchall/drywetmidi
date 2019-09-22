using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternTests
    {
        #region Test methods

        [Test]
        public void ProgramChange_Number()
        {
            var programNumber = (SevenBitNumber)10;
            var eventTime = MusicalTimeSpan.Quarter;

            var pattern = new PatternBuilder()

                .Note(NoteName.A, eventTime)
                .ProgramChange(programNumber)

                .Build();

            TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new ProgramChangeEvent(programNumber) { Channel = Channel }, eventTime)
            });
        }

        [Test]
        public void ProgramChange_GeneralMidiProgram()
        {
            var program1 = GeneralMidiProgram.Applause;
            var program2 = GeneralMidiProgram.AltoSax;
            var eventTime = MusicalTimeSpan.Quarter;

            var noteNumber = (SevenBitNumber)100;
            var note = DryWetMidi.MusicTheory.Note.Get(noteNumber);

            var pattern = new PatternBuilder()

                .ProgramChange(program1)
                .Note(note, eventTime)
                .ProgramChange(program2)

                .Build();

            TestTimedEventsWithExactOrder(pattern, new[]
            {
                new TimedEventInfo(new ProgramChangeEvent(program1.AsSevenBitNumber()) { Channel = Channel }, new MidiTimeSpan()),
                new TimedEventInfo(new NoteOnEvent(noteNumber, DryWetMidi.Smf.Interaction.Note.DefaultVelocity) { Channel = Channel }, new MidiTimeSpan()),
                new TimedEventInfo(new ProgramChangeEvent(program2.AsSevenBitNumber()) { Channel = Channel }, eventTime),
                new TimedEventInfo(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = Channel }, eventTime)
            });
        }

        [Test]
        public void ProgramChange_GeneralMidi2Program()
        {
            var eventsTime = MusicalTimeSpan.Quarter;

            var bankMsbControlNumber = ControlName.BankSelect.AsSevenBitNumber();
            var bankMsb = (SevenBitNumber)0x79;

            var bankLsbControlNumber = ControlName.LsbForBankSelect.AsSevenBitNumber();
            var bankLsb = (SevenBitNumber)0x03;

            var generalMidiProgram = GeneralMidiProgram.BirdTweet;
            var generalMidi2Program = GeneralMidi2Program.BirdTweet2;

            var pattern = new PatternBuilder()

                .Note(NoteName.A, eventsTime)
                .ProgramChange(generalMidi2Program)

                .Build();

            TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new ControlChangeEvent(bankMsbControlNumber, bankMsb) { Channel = Channel }, eventsTime),
                new TimedEventInfo(new ControlChangeEvent(bankLsbControlNumber, bankLsb) { Channel = Channel }, eventsTime),
                new TimedEventInfo(new ProgramChangeEvent(generalMidiProgram.AsSevenBitNumber()) { Channel = Channel }, eventsTime),
            });
        }

        [Test]
        public void Clone_Empty()
        {
            var pattern = new PatternBuilder()
                .Build();

            var patternClone = pattern.Clone();

            CollectionAssert.IsEmpty(patternClone.Actions, "Actions count is invalid.");
            CollectionAssert.AreEqual(pattern.Actions, patternClone.Actions, "Pattern clone is invalid.");
        }

        [Test]
        public void Clone()
        {
            var pattern = new PatternBuilder()
                .Note(Notes.A3)
                .Repeat(9)
                .Build();

            var patternClone = pattern.Clone();

            Assert.AreEqual(10, patternClone.Actions.Count(), "Actions count is invalid.");
            CollectionAssert.AreEqual(pattern.Actions, patternClone.Actions, "Pattern clone is invalid.");
        }

        [Test]
        public void ReplayPattern_Empty()
        {
            var pattern1 = new PatternBuilder().Build();

            var pattern2 = new PatternBuilder()
                .ReplayPattern(pattern1)
                .Build();

            CollectionAssert.IsEmpty(pattern2.Actions, "Pattern is not empty.");
        }

        [Test]
        public void ReplayPattern_Notes()
        {
            var pattern1 = new PatternBuilder()
                .Note(Notes.A4)
                .Note(Notes.ASharp4)
                .Build();

            var pattern2 = new PatternBuilder()
                .ReplayPattern(pattern1)
                .Build();

            TestNotes(pattern2, new[]
            {
                new NoteInfo(NoteName.A, 4, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 4, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void ReplayPattern_Anchor()
        {
            var pattern1 = new PatternBuilder()
                .Note(Notes.A4)
                .Anchor("X")
                .Note(Notes.ASharp4)
                .MoveToFirstAnchor("X")
                .Note(Notes.DSharp3)
                .Build();

            var pattern2 = new PatternBuilder()
                .ReplayPattern(pattern1)
                .Build();

            TestNotes(pattern2, new[]
            {
                new NoteInfo(NoteName.A, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.ASharp, 4, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength)
            });
        }

        [Test]
        public void BuildFromPattern_Empty()
        {
            var pattern1 = new PatternBuilder().Build();
            var pattern2 = new PatternBuilder(pattern1).Build();

            CollectionAssert.IsEmpty(pattern2.Actions, "Pattern is not empty.");
        }

        [Test]
        public void BuildFromPattern_Notes()
        {
            var pattern1 = new PatternBuilder()
                .Note(Notes.A4)
                .Note(Notes.ASharp4)
                .Build();

            var pattern2 = new PatternBuilder(pattern1).Build();

            TestNotes(pattern2, new[]
            {
                new NoteInfo(NoteName.A, 4, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 4, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void BuildFromPattern_Anchor()
        {
            var pattern1 = new PatternBuilder()
                .Note(Notes.A4)
                .Anchor("X")
                .Note(Notes.ASharp4)
                .MoveToFirstAnchor("X")
                .Note(Notes.DSharp3)
                .Build();

            var pattern2 = new PatternBuilder(pattern1).Build();

            TestNotes(pattern2, new[]
            {
                new NoteInfo(NoteName.A, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.ASharp, 4, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength)
            });
        }

        [Test]
        public void SplitAtAnchor_Empty()
        {
            var pattern = new PatternBuilder().Build();
            var patterns = pattern.SplitAtAnchor(new object());
            CollectionAssert.IsEmpty(patterns, "Pattern splitted incorrectly.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtAnchor_NoActionsBetweenAnchors(bool removeEmptyPatterns)
        {
            var anchor = "A";
            var pattern = new PatternBuilder()
                .Anchor(anchor)
                .Anchor(anchor)
                .Build();

            var patterns = pattern.SplitAtAnchor(anchor, removeEmptyPatterns).ToList();

            if (removeEmptyPatterns)
                CollectionAssert.IsEmpty(patterns, "Pattern splitted incorrectly.");
            else
            {
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                CollectionAssert.IsEmpty(patterns[1].Actions, "Second sub-pattern is not empty.");
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtAnchor_SingleAnchor(bool removeEmptyPatterns)
        {
            var anchor = "A";
            var pattern = new PatternBuilder()
                .Anchor(anchor)
                .Note(Notes.FSharp4)
                .Note(Notes.DSharp1)
                .Anchor(anchor)
                .Note(Notes.DSharp0)
                .Anchor(anchor)
                .Build();

            var patterns = pattern.SplitAtAnchor(anchor, removeEmptyPatterns).ToList();

            var firstPatternIndex = 0;
            var secondPatternIndex = 1;

            if (removeEmptyPatterns)
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
            else
            {
                Assert.AreEqual(3, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                firstPatternIndex++;
                secondPatternIndex++;
            }

            TestNotes(patterns[firstPatternIndex], new[]
            {
                new NoteInfo(NoteName.FSharp, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength)
            });

            TestNotes(patterns[secondPatternIndex], new[]
            {
                new NoteInfo(NoteName.DSharp, 0, null, PatternBuilder.DefaultNoteLength)
            });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SplitAtAnchor_MultipleAnchors(bool removeEmptyPatterns)
        {
            var anchor1 = "A";
            var anchor2 = "B";

            var pattern = new PatternBuilder()
                .Anchor(anchor1)
                .Note(Notes.FSharp4)
                .Note(Notes.DSharp1)
                .Anchor(anchor2)
                .Note(Notes.DSharp0)
                .Anchor(anchor1)
                .Build();

            var patterns = pattern.SplitAtAnchor(anchor1, removeEmptyPatterns).ToList();

            var subPatternIndex = 0;

            if (removeEmptyPatterns)
                Assert.AreEqual(1, patterns.Count, "Sub-patterns count is invalid.");
            else
            {
                Assert.AreEqual(2, patterns.Count, "Sub-patterns count is invalid.");
                CollectionAssert.IsEmpty(patterns[0].Actions, "First sub-pattern is not empty.");
                subPatternIndex++;
            }

            TestNotes(patterns[subPatternIndex], new[]
            {
                new NoteInfo(NoteName.FSharp, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 1, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 0, PatternBuilder.DefaultNoteLength.Multiply(2), PatternBuilder.DefaultNoteLength)
            });
        }

        #endregion
    }
}
