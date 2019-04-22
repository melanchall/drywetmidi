using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Standards;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestFixture]
    public class PatternTests
    {
        #region Nested classes

        private sealed class NoteInfo
        {
            #region Constructor

            public NoteInfo(NoteName noteName, int octave, ITimeSpan time, ITimeSpan length)
                : this(noteName, octave, time, length, DryWetMidi.Smf.Interaction.Note.DefaultVelocity)
            {
            }

            public NoteInfo(NoteName noteName, int octave, ITimeSpan time, ITimeSpan length, SevenBitNumber velocity)
                : this(NoteUtilities.GetNoteNumber(noteName, octave), time, length, velocity)
            {
            }

            public NoteInfo(SevenBitNumber noteNumber, ITimeSpan time, ITimeSpan length, SevenBitNumber velocity)
            {
                NoteNumber = noteNumber;
                Time = time;
                Length = length;
                Velocity = velocity;
            }

            #endregion

            #region Properties

            public SevenBitNumber NoteNumber { get; }

            public ITimeSpan Time { get; }

            public ITimeSpan Length { get; }

            public SevenBitNumber Velocity { get; }

            #endregion
        }

        private sealed class TimedEventInfo
        {
            #region Constructor

            public TimedEventInfo(MidiEvent midiEvent, ITimeSpan time)
            {
                Event = midiEvent;
                Time = time;
            }

            #endregion

            #region Properties

            public MidiEvent Event { get; }

            public ITimeSpan Time { get; }

            #endregion
        }

        #endregion

        #region Constants

        private static readonly FourBitNumber Channel = (FourBitNumber)2;

        #endregion

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

                .Note(Octave.Get(0).A)
                .Note(Octave.Get(1).C, specifiedLength, specifiedVelocity)

                .Build();

            TestNotes(pattern, new[]
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
                .SetOctave(2)

                .Note(NoteName.G, new MetricTimeSpan(0, 0, 24))
                .Note(NoteName.A, new MetricTimeSpan(0, 1, 0))
                .Note(NoteName.B, new MetricTimeSpan(0, 0, 5))

                .Build();

            var midiFile = TestNotes(pattern, new[]
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
                .SetOctave(2)

                .Note(NoteName.G, new MetricTimeSpan(0, 0, 24))
                .Note(NoteName.A, new MetricTimeSpan(0, 1, 0))
                .Note(NoteName.B, new MetricTimeSpan(0, 0, 5))

                .Build();

            var midiFile = TestNotes(pattern, new[]
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
                .SetRootNote(DryWetMidi.MusicTheory.Note.Get(NoteName.CSharp, 5))

                .Note(Interval.Two)
                .Note(-Interval.Four)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.DSharp, 5, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.A, 4, defaultNoteLength, defaultNoteLength, defaultVelocity)
            });
        }

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

        [Test]
        [Description("Add unkeyed anchor after some time movings, jump to the anchor with MoveToFirstAnchor and add note.")]
        public void MoveToFirstAnchor_Unkeyed_OneUnkeyed()
        {
            var moveTime = new MetricTimeSpan(0, 0, 10);
            var step = new MetricTimeSpan(0, 0, 11);
            var anchorTime = moveTime + step;

            var pattern = new PatternBuilder()
                .MoveToTime(moveTime)
                .StepForward(step)
                .Anchor()
                .MoveToTime(new MetricTimeSpan(0, 0, 30))
                .StepBack(new MetricTimeSpan(0, 0, 5))
                .MoveToFirstAnchor()

                .Note(Octave.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, anchorTime, MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Add unkeyed and keyed anchors after some time movings, jump to an anchor with MoveToFirstAnchor and add note.")]
        public void MoveToFirstAnchor_Unkeyed_OneUnkeyedAndOneKeyed()
        {
            var moveTime = new MetricTimeSpan(0, 0, 10);
            var step = new MetricTimeSpan(0, 0, 11);
            var anchorTime = moveTime + step;

            var pattern = new PatternBuilder()
                .MoveToTime(moveTime)
                .StepForward(step)
                .Anchor()
                .MoveToTime(new MetricTimeSpan(0, 0, 30))
                .Anchor("Test")
                .StepBack(new MetricTimeSpan(0, 0, 5))
                .MoveToFirstAnchor()

                .Note(Octave.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, anchorTime, MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Add unkeyed and keyed anchors after some time movings, jump to an anchor with MoveToFirstAnchor(key) and add note.")]
        public void MoveToFirstAnchor_Keyed_OneUnkeyedAndOneKeyed()
        {
            var anchorTime = new MetricTimeSpan(0, 0, 30);

            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTimeSpan(0, 0, 10))
                .StepForward(new MetricTimeSpan(0, 0, 11))
                .Anchor()
                .MoveToTime(anchorTime)
                .Anchor("Test")
                .StepBack(new MetricTimeSpan(0, 0, 5))
                .MoveToFirstAnchor("Test")

                .Note(Octave.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, anchorTime, MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Add no anchors and try to jump to an anchor with MoveToFirstAnchor.")]
        public void MoveToFirstAnchor_Unkeyed_NoAnchors()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new PatternBuilder()
                    .MoveToTime(new MetricTimeSpan(0, 0, 10))
                    .StepForward(new MetricTimeSpan(0, 0, 11))
                    .MoveToTime(new MetricTimeSpan(0, 0, 30))
                    .StepBack(new MetricTimeSpan(0, 0, 5))
                    .MoveToFirstAnchor()

                    .Note(Octave.Get(0).A)

                    .Build());
        }

        [Test]
        [Description("Add unkeyed anchor and try to jump to an anchor with MoveToFirstAnchor(key).")]
        public void MoveToFirstAnchor_Keyed_NoKeyedAnchors()
        {
            Assert.Throws<ArgumentException>(() =>
                new PatternBuilder()
                    .MoveToTime(new MetricTimeSpan(0, 0, 10))
                    .StepForward(new MetricTimeSpan(0, 0, 11))
                    .MoveToTime(new MetricTimeSpan(0, 0, 30))
                    .Anchor()
                    .StepBack(new MetricTimeSpan(0, 0, 5))
                    .MoveToFirstAnchor("Test")

                    .Note(Octave.Get(0).A)

                    .Build());
        }

        [Test]
        [Description("Step back by metric step and add note.")]
        public void StepBack_Metric()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTimeSpan(0, 0, 10))
                .StepForward(new MetricTimeSpan(0, 0, 30))
                .StepBack(new MetricTimeSpan(0, 0, 37))

                .Note(Octave.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MetricTimeSpan(0, 0, 3), MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Step back by metric step beyond zero and add note.")]
        public void StepBack_Metric_BeyondZero()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTimeSpan(0, 0, 10))
                .StepForward(new MetricTimeSpan(0, 0, 30))
                .StepBack(new MetricTimeSpan(0, 1, 37))

                .Note(Octave.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MetricTimeSpan(0, 0, 0), MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Step back by musical step and add note.")]
        public void StepBack_Musical()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(MusicalTimeSpan.Eighth)
                .StepForward(MusicalTimeSpan.Whole)
                .StepBack(MusicalTimeSpan.Half)

                .Note(Octave.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MusicalTimeSpan(5, 8), MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Step back by musical step beyond zero and add note.")]
        public void StepBack_Musical_BeyondZero()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTimeSpan(0, 0, 10))
                .StepForward(new MetricTimeSpan(0, 0, 30))
                .StepBack(1000 * MusicalTimeSpan.Quarter)

                .Note(Octave.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, new MetricTimeSpan(0, 0, 0), MusicalTimeSpan.Quarter)
            });
        }

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

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A,
                             4,
                             3 * MusicalTimeSpan.Eighth,
                             MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Add single lyrics event.")]
        public void Lyrics_Single()
        {
            var pattern = new PatternBuilder()

                .Note(NoteName.A)
                .Lyrics("A")

                .Build();

            TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new LyricEvent("A"), MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Add multiple lyrics events.")]
        public void Lyrics_Multiple()
        {
            var pattern = new PatternBuilder()

                .Note(NoteName.A)
                .Lyrics("A")
                .Note(NoteName.A)
                .Lyrics("B")

                .Build();

            TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new LyricEvent("A"), MusicalTimeSpan.Quarter),
                new TimedEventInfo(new LyricEvent("B"), MusicalTimeSpan.Half)
            });
        }

        [Test]
        [Description("Add lyrics events using repeat.")]
        public void Lyrics_Repeat()
        {
            var pattern = new PatternBuilder()

                .Note(NoteName.A)
                .Lyrics("A")
                .Repeat(2, 1)

                .Build();

            TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new LyricEvent("A"), MusicalTimeSpan.Quarter),
                new TimedEventInfo(new LyricEvent("A"), MusicalTimeSpan.Half)
            });
        }

        [Test]
        [Description("Add single marker event.")]
        public void Marker_Single()
        {
            var pattern = new PatternBuilder()

                .Note(NoteName.A)
                .Marker("Marker 1")

                .Build();

            TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new MarkerEvent("Marker 1"), MusicalTimeSpan.Quarter)
            });
        }

        [Test]
        [Description("Add multiple marker events.")]
        public void Marker_Multiple()
        {
            var pattern = new PatternBuilder()

                .Note(NoteName.A)
                .Marker("Marker 1")
                .Note(NoteName.A)
                .Marker("Marker 2")

                .Build();

            TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new MarkerEvent("Marker 1"), MusicalTimeSpan.Quarter),
                new TimedEventInfo(new MarkerEvent("Marker 2"), MusicalTimeSpan.Half)
            });
        }

        [Test]
        [Description("Add marker events using repeat.")]
        public void Marker_Repeat()
        {
            const string markerName = "Marker";

            var pattern = new PatternBuilder()

                .Note(NoteName.A)
                .Marker(markerName)
                .Repeat(2, 1)

                .Build();

            TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new MarkerEvent(markerName), MusicalTimeSpan.Quarter),
                new TimedEventInfo(new MarkerEvent(markerName), MusicalTimeSpan.Half)
            });
        }

        [Test]
        [Description("Set program by number.")]
        public void SetProgram_Number()
        {
            var programNumber = (SevenBitNumber)10;
            var eventTime = MusicalTimeSpan.Quarter;

            var pattern = new PatternBuilder()

                .Note(NoteName.A, eventTime)
                .SetProgram(programNumber)

                .Build();

            TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new ProgramChangeEvent(programNumber) { Channel = Channel }, eventTime)
            });
        }

        [Test]
        [Description("Set program by General MIDI Level 1 program.")]
        public void SetProgram_GeneralMidiProgram()
        {
            var program1 = GeneralMidiProgram.Applause;
            var program2 = GeneralMidiProgram.AltoSax;
            var eventTime = MusicalTimeSpan.Quarter;

            var noteNumber = (SevenBitNumber)100;
            var note = DryWetMidi.MusicTheory.Note.Get(noteNumber);

            var pattern = new PatternBuilder()

                .SetProgram(program1)
                .Note(note, eventTime)
                .SetProgram(program2)

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
        [Description("Set program by General MIDI Level 2 program.")]
        public void SetProgram_GeneralMidi2Program()
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
                .SetProgram(generalMidi2Program)

                .Build();

            TestTimedEvents(pattern, new[]
            {
                new TimedEventInfo(new ControlChangeEvent(bankMsbControlNumber, bankMsb) { Channel = Channel }, eventsTime),
                new TimedEventInfo(new ControlChangeEvent(bankLsbControlNumber, bankLsb) { Channel = Channel }, eventsTime),
                new TimedEventInfo(new ProgramChangeEvent(generalMidiProgram.AsSevenBitNumber()) { Channel = Channel }, eventsTime),
            });
        }

        #endregion

        #region Private methods

        private static MidiFile TestNotes(Pattern pattern, ICollection<NoteInfo> expectedNotesInfos, params Tuple<long, Tempo>[] tempoChanges)
        {
            TempoMap tempoMap;
            using (var tempoMapManager = new TempoMapManager())
            {
                foreach (var tempoChange in tempoChanges)
                {
                    tempoMapManager.SetTempo(tempoChange.Item1, tempoChange.Item2);
                }

                tempoMap = tempoMapManager.TempoMap;
            }

            var midiFile = pattern.ToFile(tempoMap, Channel);

            var expectedNotes = expectedNotesInfos.Select(i =>
            {
                var expectedTime = TimeConverter.ConvertFrom(i.Time ?? new MetricTimeSpan(), tempoMap);
                var expectedLength = LengthConverter.ConvertFrom(i.Length, expectedTime, tempoMap);

                return new DryWetMidi.Smf.Interaction.Note(i.NoteNumber, expectedLength, expectedTime)
                {
                    Velocity = i.Velocity,
                    Channel = Channel
                };
            });

            Assert.IsTrue(NoteEquality.AreEqual(expectedNotes, midiFile.GetNotes()));

            return midiFile;
        }

        private static void TestTimedEvents(
            Pattern pattern,
            ICollection<TimedEventInfo> expectedTimedEventsInfos,
            params Tuple<long, Tempo>[] tempoChanges)
        {
            TempoMap tempoMap;
            using (var tempoMapManager = new TempoMapManager())
            {
                foreach (var tempoChange in tempoChanges)
                {
                    tempoMapManager.SetTempo(tempoChange.Item1, tempoChange.Item2);
                }

                tempoMap = tempoMapManager.TempoMap;
            }

            var midiFile = pattern.ToFile(tempoMap, Channel);

            var expectedTimedEvents = expectedTimedEventsInfos.Select(i =>
                new TimedEvent(i.Event,
                               TimeConverter.ConvertFrom(i.Time ?? new MidiTimeSpan(), tempoMap)));

            var actualTimedEvents = midiFile.GetTimedEvents();

            foreach (var expectedEvent in expectedTimedEvents)
            {
                Assert.IsTrue(actualTimedEvents.Any(actual => TimedEventEquality.AreEqual(expectedEvent, actual, false)),
                              $"There are no event: {expectedEvent}");
            }
        }

        private static void TestTimedEventsWithExactOrder(
            Pattern pattern,
            ICollection<TimedEventInfo> expectedTimedEventsInfos,
            params Tuple<long, Tempo>[] tempoChanges)
        {
            TempoMap tempoMap;
            using (var tempoMapManager = new TempoMapManager())
            {
                foreach (var tempoChange in tempoChanges)
                {
                    tempoMapManager.SetTempo(tempoChange.Item1, tempoChange.Item2);
                }

                tempoMap = tempoMapManager.TempoMap;
            }

            var midiFile = pattern.ToFile(tempoMap, Channel);

            var expectedTimedEvents = expectedTimedEventsInfos.Select(i =>
                new TimedEvent(i.Event, TimeConverter.ConvertFrom(i.Time ?? new MidiTimeSpan(), tempoMap)));

            var actualTimedEvents = midiFile.GetTimedEvents();

            Assert.IsTrue(TimedEventEquality.AreEqual(expectedTimedEvents, actualTimedEvents, false), "Events have invalid order.");
        }

        #endregion
    }
}
