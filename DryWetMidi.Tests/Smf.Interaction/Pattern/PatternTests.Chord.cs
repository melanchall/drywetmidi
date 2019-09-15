using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestFixture]
    public sealed partial class PatternTests
    {
        #region Test methods

        [Test]
        [Description("Add chord with default velocity and octave.")]
        public void Chord_DefaultOctave()
        {
            var defaultVelocity = (SevenBitNumber)90;
            var defaultOctave = 2;

            var chordLength = MusicalTimeSpan.Sixteenth.Triplet();
            var chordTime1 = new MetricTimeSpan(0, 1, 12);
            var chordTime2 = chordTime1.Add(chordLength, TimeSpanMode.TimeLength);

            var pattern = new PatternBuilder()
                .SetVelocity(defaultVelocity)
                .SetOctave(defaultOctave)

                .MoveToTime(chordTime1)
                .Chord(new[]
                {
                    NoteName.C,
                    NoteName.G
                }, chordLength)
                .Repeat()

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.C, defaultOctave, chordTime1, chordLength, defaultVelocity),
                new NoteInfo(NoteName.G, defaultOctave, chordTime1, chordLength, defaultVelocity),
                new NoteInfo(NoteName.C, defaultOctave, chordTime2, chordLength, defaultVelocity),
                new NoteInfo(NoteName.G, defaultOctave, chordTime2, chordLength, defaultVelocity)
            });
        }

        [Test]
        [Description("Add several chords by interval.")]
        public void Chord_Interval()
        {
            var defaultNoteLength = MusicalTimeSpan.Quarter;
            var defaultVelocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(defaultNoteLength)
                .SetVelocity(defaultVelocity)
                .SetRootNote(DryWetMidi.MusicTheory.Note.Get(NoteName.CSharp, 5))

                .Chord(new[] { Interval.Two, Interval.Five }, Octave.Get(2).A)
                .Chord(new[] { Interval.Two, -Interval.Ten }, Octave.Get(2).B)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.B, 2, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.D, 3, null, defaultNoteLength, defaultVelocity),

                new NoteInfo(NoteName.B, 2, defaultNoteLength, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.CSharp, 3, defaultNoteLength, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.CSharp, 2, defaultNoteLength, defaultNoteLength, defaultVelocity),
            });
        }

        #endregion
    }
}
