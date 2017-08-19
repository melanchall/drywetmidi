using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public class PatternTests
    {
        private sealed class NoteInfo
        {
            #region Constructor

            public NoteInfo(NoteName noteName, int octave, ITime time, ILength length)
                : this(noteName, octave, time, length, Note.DefaultVelocity)
            {
            }

            public NoteInfo(NoteName noteName, int octave, ITime time, ILength length, SevenBitNumber velocity)
                : this(NoteUtilities.GetNoteNumber(noteName, octave), time, length, velocity)
            {
            }

            public NoteInfo(SevenBitNumber noteNumber, ITime time, ILength length, SevenBitNumber velocity)
            {
                NoteNumber = noteNumber;
                Time = time;
                Length = length;
                Velocity = velocity;
            }

            #endregion

            #region Properties

            public SevenBitNumber NoteNumber { get; }

            public ITime Time { get; }

            public ILength Length { get; }

            public SevenBitNumber Velocity { get; }

            #endregion
        }

        #region Test methods

        [TestMethod]
        [Description("Add two notes where first one takes default length and velocity and the second takes specified ones.")]
        public void Note_MixedLengthAndVelocity()
        {
            var defaultNoteLength = (MusicalLength)MusicalFraction.Quarter;
            var defaultVelocity = (SevenBitNumber)90;

            var specifiedLength = new MetricLength(0, 0, 10);
            var specifiedVelocity = (SevenBitNumber)95;

            var pattern = new PatternBuilder()
                .SetNoteLength(defaultNoteLength)
                .SetVelocity(defaultVelocity)

                .Note(OctaveDefinition.Get(0).A)
                .Note(OctaveDefinition.Get(1).C, specifiedLength, specifiedVelocity)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.C, 1, (MusicalTime)MusicalFraction.Quarter, specifiedLength, specifiedVelocity)
            });
        }

        [TestMethod]
        [Description("Add several notes with metric lengths.")]
        public void Note_Multiple_MetricLengths()
        {
            var pattern = new PatternBuilder()
                .SetOctave(2)

                .Note(NoteName.G, new MetricLength(0, 0, 24))
                .Note(NoteName.A, new MetricLength(0, 1, 0))
                .Note(NoteName.B, new MetricLength(0, 0, 5))

                .Build();

            var midiFile = TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.G, 2, null, new MetricLength(0, 0, 24)),
                new NoteInfo(NoteName.A, 2, new MetricTime(0, 0, 24), new MetricLength(0, 1, 0)),
                new NoteInfo(NoteName.B, 2, new MetricTime(0, 1, 24), new MetricLength(0, 0, 5)),
            });

            var tempoMap = midiFile.GetTempoMap();
            Assert.AreEqual(new MetricLength(0, 1, 29),
                            new MetricLength(midiFile.GetTimedEvents().Last().TimeAs<MetricTime>(tempoMap)));
        }

        [TestMethod]
        [Description("Add several notes with metric lengths.")]
        public void Note_Multiple_MetricLengths_TempoChanged()
        {
            var pattern = new PatternBuilder()
                .SetOctave(2)

                .Note(NoteName.G, new MetricLength(0, 0, 24))
                .Note(NoteName.A, new MetricLength(0, 1, 0))
                .Note(NoteName.B, new MetricLength(0, 0, 5))

                .Build();

            var midiFile = TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.G, 2, null, new MetricLength(0, 0, 24)),
                new NoteInfo(NoteName.A, 2, new MetricTime(0, 0, 24), new MetricLength(0, 1, 0)),
                new NoteInfo(NoteName.B, 2, new MetricTime(0, 1, 24), new MetricLength(0, 0, 5)),
            },
            Enumerable.Range(0, 7)
                      .Select(i => Tuple.Create(i * 1000L, new Tempo(i * 100 + 10)))
                      .ToArray());

            var tempoMap = midiFile.GetTempoMap();
            Assert.AreEqual(new MetricLength(0, 1, 29).TotalMicroseconds,
                            new MetricLength(midiFile.GetTimedEvents().Last().TimeAs<MetricTime>(tempoMap)).TotalMicroseconds);
        }

        [TestMethod]
        [Description("Add several notes by intervals.")]
        public void Note_Multiple_Interval()
        {
            var defaultNoteLength = (MusicalLength)MusicalFraction.Quarter;
            var defaultVelocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(defaultNoteLength)
                .SetVelocity(defaultVelocity)
                .SetRootNote(NoteDefinition.Get(NoteName.CSharp, 5))

                .Note(IntervalDefinition.Two)
                .Note(-IntervalDefinition.Four)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.DSharp, 5, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.A, 4, new MathTime(new MusicalTime(), defaultNoteLength), defaultNoteLength, defaultVelocity)
            });
        }

        [TestMethod]
        [Description("Add chord with default velocity and octave.")]
        public void Chord_DefaultOctave()
        {
            var defaultVelocity = (SevenBitNumber)90;
            var defaultOctave = 2;

            var chordLength = (MusicalLength)MusicalFraction.SixteenthTriplet;
            var chordTime1 = new MetricTime(0, 1, 12);
            var chordTime2 = new MathTime(chordTime1, chordLength);

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

        [TestMethod]
        [Description("Add several chords by interval.")]
        public void Chord_Interval()
        {
            var defaultNoteLength = (MusicalLength)MusicalFraction.Quarter;
            var defaultVelocity = (SevenBitNumber)90;

            var pattern = new PatternBuilder()
                .SetNoteLength(defaultNoteLength)
                .SetVelocity(defaultVelocity)
                .SetRootNote(NoteDefinition.Get(NoteName.CSharp, 5))

                .Chord(new[] { IntervalDefinition.Two, IntervalDefinition.Five }, OctaveDefinition.Get(2).A)
                .Chord(new[] { IntervalDefinition.Two, -IntervalDefinition.Ten }, OctaveDefinition.Get(2).B)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 2, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.B, 2, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.D, 3, null, defaultNoteLength, defaultVelocity),

                new NoteInfo(NoteName.B, 2, new MathTime(new MusicalTime(), defaultNoteLength), defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.CSharp, 3, new MathTime(new MusicalTime(), defaultNoteLength), defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.CSharp, 2, new MathTime(new MusicalTime(), defaultNoteLength), defaultNoteLength, defaultVelocity),
            });
        }

        [TestMethod]
        [Description("Add unkeyed anchor after some time movings, jump to the anchor with MoveToFirstAnchor and add note.")]
        public void MoveToFirstAnchor_Unkeyed_OneUnkeyed()
        {
            var moveTime = new MetricTime(0, 0, 10);
            var step = new MetricLength(0, 0, 11);
            var anchorTime = moveTime + step;

            var pattern = new PatternBuilder()
                .MoveToTime(moveTime)
                .StepForward(step)
                .Anchor()
                .MoveToTime(new MetricTime(0, 0, 30))
                .StepBack(new MetricLength(0, 0, 5))
                .MoveToFirstAnchor()

                .Note(OctaveDefinition.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, anchorTime, (MusicalLength)MusicalFraction.Quarter)
            });
        }

        [TestMethod]
        [Description("Add unkeyed and keyed anchors after some time movings, jump to an anchor with MoveToFirstAnchor and add note.")]
        public void MoveToFirstAnchor_Unkeyed_OneUnkeyedAndOneKeyed()
        {
            var moveTime = new MetricTime(0, 0, 10);
            var step = new MetricLength(0, 0, 11);
            var anchorTime = moveTime + step;

            var pattern = new PatternBuilder()
                .MoveToTime(moveTime)
                .StepForward(step)
                .Anchor()
                .MoveToTime(new MetricTime(0, 0, 30))
                .Anchor("Test")
                .StepBack(new MetricLength(0, 0, 5))
                .MoveToFirstAnchor()

                .Note(OctaveDefinition.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, anchorTime, (MusicalLength)MusicalFraction.Quarter)
            });
        }

        [TestMethod]
        [Description("Add unkeyed and keyed anchors after some time movings, jump to an anchor with MoveToFirstAnchor(key) and add note.")]
        public void MoveToFirstAnchor_Keyed_OneUnkeyedAndOneKeyed()
        {
            var anchorTime = new MetricTime(0, 0, 30);

            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTime(0, 0, 10))
                .StepForward(new MetricLength(0, 0, 11))
                .Anchor()
                .MoveToTime(anchorTime)
                .Anchor("Test")
                .StepBack(new MetricLength(0, 0, 5))
                .MoveToFirstAnchor("Test")

                .Note(OctaveDefinition.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, anchorTime, (MusicalLength)MusicalFraction.Quarter)
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [Description("Add no anchors and try to jump to an anchor with MoveToFirstAnchor.")]
        public void MoveToFirstAnchor_Unkeyed_NoAnchors()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTime(0, 0, 10))
                .StepForward(new MetricLength(0, 0, 11))
                .MoveToTime(new MetricTime(0, 0, 30))
                .StepBack(new MetricLength(0, 0, 5))
                .MoveToFirstAnchor()

                .Note(OctaveDefinition.Get(0).A)

                .Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Add unkeyed anchor and try to jump to an anchor with MoveToFirstAnchor(key).")]
        public void MoveToFirstAnchor_Keyed_NoKeyedAnchors()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTime(0, 0, 10))
                .StepForward(new MetricLength(0, 0, 11))
                .MoveToTime(new MetricTime(0, 0, 30))
                .Anchor()
                .StepBack(new MetricLength(0, 0, 5))
                .MoveToFirstAnchor("Test")

                .Note(OctaveDefinition.Get(0).A)

                .Build();
        }

        [TestMethod]
        [Description("Step back by metric step and add note.")]
        public void StepBack_Metric()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTime(0, 0, 10))
                .StepForward(new MetricLength(0, 0, 30))
                .StepBack(new MetricLength(0, 0, 37))

                .Note(OctaveDefinition.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MetricTime(0, 0, 3), (MusicalLength)MusicalFraction.Quarter)
            });
        }

        [TestMethod]
        [Description("Step back by metric step beyond zero and add note.")]
        public void StepBack_Metric_BeyondZero()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTime(0, 0, 10))
                .StepForward(new MetricLength(0, 0, 30))
                .StepBack(new MetricLength(0, 1, 37))

                .Note(OctaveDefinition.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MetricTime(0, 0, 0), (MusicalLength)MusicalFraction.Quarter)
            });
        }

        [TestMethod]
        [Description("Step back by musical step and add note.")]
        public void StepBack_Musical()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MusicalTime(MusicalFraction.Eighth))
                .StepForward(new MusicalLength(MusicalFraction.Whole))
                .StepBack(new MusicalLength(MusicalFraction.Half))

                .Note(OctaveDefinition.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MusicalTime(new Fraction(5, 8)), (MusicalLength)MusicalFraction.Quarter)
            });
        }

        [TestMethod]
        [Description("Step back by musical step beyond zero and add note.")]
        public void StepBack_Musical_BeyondZero()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTime(0, 0, 10))
                .StepForward(new MetricLength(0, 0, 30))
                .StepBack(new MusicalLength(1000 * MusicalFraction.Quarter))

                .Note(OctaveDefinition.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MetricTime(0, 0, 0), (MusicalLength)MusicalFraction.Quarter)
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [Description("Try to repeat last action one time in case of no actions exist at the moment.")]
        public void Repeat_Last_Single_NoActions()
        {
            new PatternBuilder().Repeat();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [Description("Try to repeat last action several times in case of no actions exist at the moment.")]
        public void Repeat_Last_Multiple_Valid_NoActions()
        {
            new PatternBuilder().Repeat(2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Description("Try to repeat last action invalid number of times in case of no actions exist at the moment.")]
        public void Repeat_Last_Multiple_Invalid_NoActions()
        {
            new PatternBuilder().Repeat(-7);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Repeat_Previous_NoActions()
        {
            new PatternBuilder().Repeat(2, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Repeat_Previous_NotEnoughActions()
        {
            new PatternBuilder()
                .Anchor()
                .Repeat(2, 2);
        }

        [TestMethod]
        [Description("Repeat some actions and insert a note.")]
        public void Repeat_Previous()
        {
            var pattern = new PatternBuilder()
                .SetStep((MusicalLength)MusicalFraction.Eighth)

                .Anchor("A")
                .StepForward()
                .Anchor("B")
                .Repeat(2, 2)
                .MoveToNthAnchor("B", 2)
                .Note(NoteName.A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A,
                             4,
                             new MusicalTime(3 * MusicalFraction.Eighth),
                             (MusicalLength)MusicalFraction.Quarter)
            });
        }

        #endregion

        #region Private methods

        private static MidiFile TestNotes(Pattern pattern, ICollection<NoteInfo> expectedNotesInfos, params Tuple<long, Tempo>[] tempoChanges)
        {
            var channel = (FourBitNumber)2;

            TempoMap tempoMap = null;
            using (var tempoMapManager = new TempoMapManager())
            {
                foreach (var tempoChange in tempoChanges)
                {
                    tempoMapManager.SetTempo(tempoChange.Item1, tempoChange.Item2);
                }

                tempoMap = tempoMapManager.TempoMap;
            }

            var midiFile = pattern.ToFile(tempoMap, channel);

            var expectedNotes = expectedNotesInfos.Select(i =>
            {
                var expectedTime = TimeConverter.ConvertFrom(i.Time ?? new MetricTime(), tempoMap);
                var expectedLength = LengthConverter.ConvertFrom(i.Length, expectedTime, tempoMap);

                return new Note(i.NoteNumber, expectedLength, expectedTime)
                {
                    Velocity = i.Velocity,
                    Channel = channel
                };
            });

            Assert.IsTrue(NoteEquality.Equals(expectedNotes, midiFile.GetNotes()));

            return midiFile;
        }

        #endregion
    }
}
