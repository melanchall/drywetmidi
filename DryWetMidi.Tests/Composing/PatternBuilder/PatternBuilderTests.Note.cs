using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;
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

        [Test]
        [Description("Add two notes where first one takes default length and velocity and the second takes specified ones.")]
        public void Note_MixedLengthAndVelocity()
        {
            var defaultNoteLength = MusicalTimeSpan.Quarter;
            var defaultVelocity = (SevenBitNumber)90;

            var specifiedLength = new MetricTimeSpan(0, 0, 10);
            var specifiedVelocity = (SevenBitNumber)95;

            var pattern = new PatternBuilder()
                .SetNoteLength(defaultNoteLength)
                .SetVelocity(defaultVelocity)

                .Note(Notes.A0)
                .Note(Notes.C1, specifiedLength, specifiedVelocity)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.C, 1, MusicalTimeSpan.Quarter, specifiedLength, specifiedVelocity)
            });
        }

        [Test]
        [Description("Add several notes with metric lengths.")]
        public void Note_Multiple_MetricLengths()
        {
            var pattern = new PatternBuilder()
                .SetOctave(Octave.Get(2))

                .Note(NoteName.G, new MetricTimeSpan(0, 0, 24))
                .Note(NoteName.A, new MetricTimeSpan(0, 1, 0))
                .Note(NoteName.B, new MetricTimeSpan(0, 0, 5))

                .Build();

            var midiFile = PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.G, 2, null, new MetricTimeSpan(0, 0, 24)),
                new NoteInfo(NoteName.A, 2, new MetricTimeSpan(0, 0, 24), new MetricTimeSpan(0, 1, 0)),
                new NoteInfo(NoteName.B, 2, new MetricTimeSpan(0, 1, 24), new MetricTimeSpan(0, 0, 5)),
            });

            Assert.AreEqual(new MetricTimeSpan(0, 1, 29), midiFile.GetDuration<MetricTimeSpan>());
        }

        [Test]
        [Description("Add several notes with metric lengths.")]
        public void Note_Multiple_MetricLengths_TempoChanged()
        {
            var pattern = new PatternBuilder()
                .SetOctave(Octave.Get(2))

                .Note(NoteName.G, new MetricTimeSpan(0, 0, 24))
                .Note(NoteName.A, new MetricTimeSpan(0, 1, 0))
                .Note(NoteName.B, new MetricTimeSpan(0, 0, 5))

                .Build();

            var midiFile = PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.G, 2, null, new MetricTimeSpan(0, 0, 24)),
                new NoteInfo(NoteName.A, 2, new MetricTimeSpan(0, 0, 24), new MetricTimeSpan(0, 1, 0)),
                new NoteInfo(NoteName.B, 2, new MetricTimeSpan(0, 1, 24), new MetricTimeSpan(0, 0, 5)),
            },
            Enumerable.Range(0, 7)
                      .Select(i => Tuple.Create(i * 1000L, new Tempo(i * 100 + 10)))
                      .ToArray());

            Assert.AreEqual(new MetricTimeSpan(0, 1, 29).TotalMicroseconds,
                            midiFile.GetDuration<MetricTimeSpan>().TotalMicroseconds);
        }

        [Test]
        [Description("Add several notes by intervals.")]
        public void Note_Multiple_Interval()
        {
            var defaultNoteLength = MusicalTimeSpan.Quarter;
            var defaultVelocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(defaultNoteLength)
                .SetVelocity(defaultVelocity)
                .SetRootNote(Notes.CSharp5)

                .Note(Interval.Two)
                .Note(-Interval.Four)

                .Build();

            PatternTestUtilities.TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.DSharp, 5, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.A, 4, defaultNoteLength, defaultNoteLength, defaultVelocity)
            });
        }

        #endregion
    }
}
