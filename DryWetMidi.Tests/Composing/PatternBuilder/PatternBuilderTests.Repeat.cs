using System;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using Melanchall.DryWetMidi.Core;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternBuilderTests
    {
        #region Constants

        private static readonly RepeatSettings TransposeNotesUpBy2Settings = new RepeatSettings
        {
            NoteTransformation = n => new NoteDescriptor(
                n.Note.Transpose(Interval.Two),
                n.Velocity,
                n.Length)
        };

        private static readonly RepeatSettings TransposeChordsUpBy2Settings = new RepeatSettings
        {
            ChordTransformation = c => new ChordDescriptor(
                c.Notes.Select(n => n.Transpose(Interval.Two)).ToArray(),
                c.Velocity,
                c.Length)
        };

        #endregion

        #region Test methods

        [Test]
        [Description("Try to repeat last action one time in case of no actions exist at the moment.")]
        public void Repeat_Last_Single_NoActions()
        {
            Assert.Throws<InvalidOperationException>(() => new PatternBuilder().Repeat());
        }

        [Test]
        [Description("Try to repeat last action several times in case of no actions exist at the moment.")]
        public void Repeat_Last_Multiple_Valid_NoActions()
        {
            Assert.Throws<InvalidOperationException>(() => new PatternBuilder().Repeat(2));
        }

        [Test]
        [Description("Try to repeat last action invalid number of times in case of no actions exist at the moment.")]
        public void Repeat_Last_Multiple_Invalid_NoActions()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PatternBuilder().Repeat(-7));
        }

        [Test]
        public void Repeat_Previous_NoActions()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PatternBuilder().Repeat(2, 2));
        }

        [Test]
        public void Repeat_Previous_NotEnoughActions()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PatternBuilder()
                    .Anchor()
                    .Repeat(2, 2));
        }

        [Test]
        [Description("Repeat some actions and insert a note.")]
        public void Repeat_Previous()
        {
            var pattern = new PatternBuilder()
                .SetStep(MusicalTimeSpan.Eighth)

                .Anchor("A")
                .StepForward()
                .Anchor("B")
                .Repeat(2, 2)
                .MoveToNthAnchor("B", 2)
                .Note(NoteName.A)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 4, 3 * MusicalTimeSpan.Eighth, MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        public void Repeat_LastOne()
        {
            var pattern = new PatternBuilder()
                .Note(NoteName.A)
                .Repeat(2)
                .Marker("A")
                .Repeat(1)
                .Build();

            PatternTestUtilities.TestTimedEventsWithExactOrder(pattern, new[]
            {
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    (MidiTimeSpan)0),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength.Multiply(2)),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength.Multiply(2)),
                new TimedEventInfo(
                    new MarkerEvent("A"),
                    PatternBuilder.DefaultNoteLength.Multiply(3)),
                new TimedEventInfo(
                    new MarkerEvent("A"),
                    PatternBuilder.DefaultNoteLength.Multiply(3)),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength.Multiply(3)),
            });
        }

        [Test]
        public void Repeat_LastOne_TransformNote()
        {
            var pattern = new PatternBuilder()
                .Note(NoteName.A)
                .Repeat(1, TransposeNotesUpBy2Settings)
                .Build();

            PatternTestUtilities.TestTimedEventsWithExactOrder(pattern, new[]
            {
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    (MidiTimeSpan)0),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.B, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.B, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength.Multiply(2)),
            });
        }

        [Test]
        public void Repeat_LastMultiple_Last()
        {
            var pattern = new PatternBuilder()
                .Note(NoteName.A)
                .Repeat(1, 2)
                .Marker("A")
                .Repeat(1)
                .Build();

            PatternTestUtilities.TestTimedEventsWithExactOrder(pattern, new[]
            {
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    (MidiTimeSpan)0),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength.Multiply(2)),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength.Multiply(2)),
                new TimedEventInfo(
                    new MarkerEvent("A"),
                    PatternBuilder.DefaultNoteLength.Multiply(3)),
                new TimedEventInfo(
                    new MarkerEvent("A"),
                    PatternBuilder.DefaultNoteLength.Multiply(3)),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength.Multiply(3)),
            });
        }

        [Test]
        public void Repeat_LastMultiple_Last_TransformChord()
        {
            var pattern = new PatternBuilder()
                .Chord(new DryWetMidi.MusicTheory.Chord(NoteName.A, NoteName.C))
                .Repeat(1, 1, TransposeChordsUpBy2Settings)
                .Build();

            PatternTestUtilities.TestTimedEventsWithExactOrder(pattern, new[]
            {
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    (MidiTimeSpan)0),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.C, PatternBuilder.DefaultOctave.Number + 1),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    (MidiTimeSpan)0),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.C, PatternBuilder.DefaultOctave.Number + 1),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.B, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.D, PatternBuilder.DefaultOctave.Number + 1),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.B, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength.Multiply(2)),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.D, PatternBuilder.DefaultOctave.Number + 1),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength.Multiply(2)),
            });
        }

        [Test]
        public void Repeat_LastMultiple_Multiple()
        {
            var pattern = new PatternBuilder()
                .MoveToStart()
                .Note(NoteName.A)
                .Repeat(2, 2)
                .Marker("A")
                .Repeat(1)
                .Build();

            PatternTestUtilities.TestTimedEventsWithExactOrder(pattern, new[]
            {
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    (MidiTimeSpan)0),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    (MidiTimeSpan)0),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    (MidiTimeSpan)0),
                new TimedEventInfo(
                    new MarkerEvent("A"),
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new MarkerEvent("A"),
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
            });
        }

        [Test]
        public void Repeat_LastMultiple_Multiple_TransformNote()
        {
            var pattern = new PatternBuilder()
                .MoveToStart()
                .Note(NoteName.A)
                .Repeat(2, 2, TransposeNotesUpBy2Settings)
                .Build();

            PatternTestUtilities.TestTimedEventsWithExactOrder(pattern, new[]
            {
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    (MidiTimeSpan)0),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.B, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    (MidiTimeSpan)0),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.B, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    (MidiTimeSpan)0),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.B, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.B, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
            });
        }

        [Test]
        public void Repeat_Last()
        {
            var pattern = new PatternBuilder()
                .Note(NoteName.A)
                .Repeat()
                .Marker("A")
                .Repeat(1)
                .Build();

            PatternTestUtilities.TestTimedEventsWithExactOrder(pattern, new[]
            {
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    (MidiTimeSpan)0),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new MarkerEvent("A"),
                    PatternBuilder.DefaultNoteLength.Multiply(2)),
                new TimedEventInfo(
                    new MarkerEvent("A"),
                    PatternBuilder.DefaultNoteLength.Multiply(2)),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength.Multiply(2)),
            });
        }

        [Test]
        public void Repeat_Last_TransformNote()
        {
            var pattern = new PatternBuilder()
                .Note(NoteName.A)
                .Repeat(TransposeNotesUpBy2Settings)
                .Build();

            PatternTestUtilities.TestTimedEventsWithExactOrder(pattern, new[]
            {
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    (MidiTimeSpan)0),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.A, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOnEvent(
                        NoteUtilities.GetNoteNumber(NoteName.B, PatternBuilder.DefaultOctave.Number),
                        PatternBuilder.DefaultVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength),
                new TimedEventInfo(
                    new NoteOffEvent(
                        NoteUtilities.GetNoteNumber(NoteName.B, PatternBuilder.DefaultOctave.Number),
                        DryWetMidi.Interaction.Note.DefaultOffVelocity) { Channel = PatternTestUtilities.Channel },
                    PatternBuilder.DefaultNoteLength.Multiply(2)),
            });
        }

        #endregion
    }
}
