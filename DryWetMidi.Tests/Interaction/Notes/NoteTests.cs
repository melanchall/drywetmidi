using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class NoteTests
    {
        #region Nested classes

        private sealed class TaggedNote : Note
        {
            public TaggedNote(SevenBitNumber sevenBitNumber, object tag)
                : base(sevenBitNumber)
            {
                Tag = tag;
            }

            public object Tag { get; }

            public override ITimedObject Clone()
            {
                return new TaggedNote(NoteNumber, Tag)
                {
                    Time = Time,
                    Length = Length,
                    Channel = Channel,
                    Velocity = Velocity,
                    OffVelocity = OffVelocity
                };
            }
        }

        #endregion

        #region Test methods

        #region Constructor

        [Test]
        public void CreateNote_ByTimedEvents()
        {
            var note = new Note(
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, 100));

            ClassicAssert.AreEqual((SevenBitNumber)70, note.NoteNumber, "Invalid note number.");
            ClassicAssert.AreEqual((SevenBitNumber)20, note.Velocity, "Invalid velocity.");
            ClassicAssert.AreEqual((SevenBitNumber)10, note.OffVelocity, "Invalid off velocity.");
            ClassicAssert.AreEqual((FourBitNumber)5, note.Channel, "Invalid channel.");
            ClassicAssert.AreEqual(10, note.Time, "Invalid time.");
            ClassicAssert.AreEqual(90, note.Length, "Invalid length.");
        }

        [Test]
        public void CreateNote_ByTimedEvents_NotNoteOn()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => new Note(
                new TimedEvent(new TextEvent("A"), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, 100)));
        }

        [Test]
        public void CreateNote_ByTimedEvents_NotNoteOff()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => new Note(
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new TextEvent("A"), 100)));
        }

        [Test]
        public void CreateNote_ByTimedEvents_NoteOffBeforeNoteOn()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => new Note(
                new TimedEvent(new NoteOnEvent(), 10),
                new TimedEvent(new NoteOffEvent(), 5)));
        }

        #endregion

        #region Clone

        [Test]
        [Description("Check that clone of a note equals to the original one.")]
        public void Clone()
        {
            var note = new Note((SevenBitNumber)100, 500, 123)
            {
                Channel = (FourBitNumber)10,
                Velocity = (SevenBitNumber)45,
                OffVelocity = (SevenBitNumber)54
            };

            MidiAsserts.AreEqual(note, note.Clone(), "Clone of a note doesn't equal to the original one.");
        }

        [Test]
        public void CloneCustomNote()
        {
            const string tag = "Tag";
            
            var noteNumber = (SevenBitNumber)70;
            var taggedNote = new TaggedNote(noteNumber, tag);

            var clone = (TaggedNote)taggedNote.Clone();

            ClassicAssert.AreEqual(noteNumber, clone.NoteNumber, "Note number is invalid.");
            ClassicAssert.AreEqual(tag, clone.Tag, "Tag is invalid.");
        }

        #endregion

        #region Split

        [Test]
        [Description("Split a note of zero length.")]
        public void Split_ZeroLength()
        {
            Func<Note> noteCreator = () => new Note((SevenBitNumber)100);

            var note = noteCreator();
            var time = 0;

            var parts = note.Split(time);
            ClassicAssert.IsNull(parts.LeftPart,
                          "Left part is not null.");
            ClassicAssert.AreNotSame(parts.RightPart,
                              note,
                              "Right part refers to the same object as the original note.");
            MidiAsserts.AreEqual(noteCreator(), parts.RightPart, "Right part doesn't equal to the original note.");
        }

        [Test]
        [Description("Split by time below the start time of a note.")]
        public void Split_TimeBelowStartTime()
        {
            Func<Note> noteCreator = () => new Note((SevenBitNumber)100, 200, 100);

            var note = noteCreator();
            var time = 50;

            var parts = note.Split(time);
            ClassicAssert.IsNull(parts.LeftPart,
                          "Left part is not null.");
            ClassicAssert.AreNotSame(parts.RightPart,
                              note,
                              "Right part refers to the same object as the original note.");
            MidiAsserts.AreEqual(noteCreator(), parts.RightPart, "Right part doesn't equal to the original note.");
        }

        [Test]
        [Description("Split by time above the end time of a note.")]
        public void Split_TimeAboveEndTime()
        {
            Func<Note> noteCreator = () => new Note((SevenBitNumber)100, 200, 100);

            var note = noteCreator();
            var time = 500;

            var parts = note.Split(time);
            ClassicAssert.IsNull(parts.RightPart,
                          "Right part is not null.");
            ClassicAssert.AreNotSame(parts.LeftPart,
                              note,
                              "Left part refers to the same object as the original note.");
            MidiAsserts.AreEqual(noteCreator(), parts.LeftPart, "Left part doesn't equal to the original note.");
        }

        [Test]
        [Description("Split a note by time falling inside it.")]
        public void Split_TimeInsideNote()
        {
            var note = new Note((SevenBitNumber)100, 200, 100);
            var time = 120;

            var parts = note.Split(time);
            MidiAsserts.AreEqual(new Note((SevenBitNumber)100, 20, 100), parts.LeftPart, "Left part is invalid.");
            MidiAsserts.AreEqual(new Note((SevenBitNumber)100, 180, 120), parts.RightPart, "Right part is invalid.");
        }

        #endregion

        #region EndTimeAs

        [Test]
        public void EndTimeAs_ZeroTime_ZeroLength()
        {
            CheckEndTime(new MetricTimeSpan(), new MetricTimeSpan(), new MetricTimeSpan());
        }

        [Test]
        public void EndTimeAs_ZeroLength()
        {
            CheckEndTime(MusicalTimeSpan.Eighth, new MetricTimeSpan(), MusicalTimeSpan.Eighth);
        }

        [Test]
        public void EndTimeAs()
        {
            CheckEndTime(MusicalTimeSpan.Eighth, MusicalTimeSpan.Eighth, MusicalTimeSpan.Quarter);
        }

        #endregion

        #region GetTimedNoteOnEvent

        [Test]
        public void GetTimedNoteOnEvent()
        {
            var note = new Note(new SevenBitNumber(1));
            var timedNoteOnEvent1 = note.GetTimedNoteOnEvent();
            var timedNoteOnEvent2 = note.GetTimedNoteOnEvent();
            ClassicAssert.IsNotNull(timedNoteOnEvent1, "1st event is null.");
            ClassicAssert.IsNotNull(timedNoteOnEvent2, "2nd event is null.");
            ClassicAssert.AreNotEqual(timedNoteOnEvent1, timedNoteOnEvent2, "Events have not been cloned.");
        }

        [Test]
        public void GetTimedNoteOnEvent_NoteChanged()
        {
            var note = new Note(new SevenBitNumber(1));

            note.NoteNumber = (SevenBitNumber)45;
            note.Velocity = (SevenBitNumber)12;
            note.OffVelocity = (SevenBitNumber)122;
            note.Channel = (FourBitNumber)9;

            note.Time = 20;
            note.Length = 200;

            var timedNoteOnEvent = note.GetTimedNoteOnEvent();
            ClassicAssert.IsInstanceOf(typeof(NoteOnEvent), timedNoteOnEvent.Event, "Events is not Note On.");
            ClassicAssert.AreEqual(20, timedNoteOnEvent.Time, "Time is invalid");

            var noteOnEvent = (NoteOnEvent)timedNoteOnEvent.Event;
            ClassicAssert.AreEqual((SevenBitNumber)45, noteOnEvent.NoteNumber, "Note number is invalid.");
            ClassicAssert.AreEqual((SevenBitNumber)12, noteOnEvent.Velocity, "Velocity is invalid.");
            ClassicAssert.AreEqual((FourBitNumber)9, noteOnEvent.Channel, "Channel is invalid.");
        }

        #endregion

        #region GetTimedNoteOffEvent

        [Test]
        public void GetTimedNoteOffEvent()
        {
            var note = new Note(new SevenBitNumber(1));
            var timedNoteOffEvent1 = note.GetTimedNoteOffEvent();
            var timedNoteOffEvent2 = note.GetTimedNoteOffEvent();
            ClassicAssert.IsNotNull(timedNoteOffEvent1, "1st event is null.");
            ClassicAssert.IsNotNull(timedNoteOffEvent2, "2nd event is null.");
            ClassicAssert.AreNotEqual(timedNoteOffEvent1, timedNoteOffEvent2, "Events have not been cloned.");
        }

        [Test]
        public void GetTimedNoteOffEvent_NoteChanged()
        {
            var note = new Note(new SevenBitNumber(1));

            note.NoteNumber = (SevenBitNumber)45;
            note.Velocity = (SevenBitNumber)12;
            note.OffVelocity = (SevenBitNumber)122;
            note.Channel = (FourBitNumber)9;

            note.Time = 20;
            note.Length = 200;

            var timedNoteOffEvent = note.GetTimedNoteOffEvent();
            ClassicAssert.IsInstanceOf(typeof(NoteOffEvent), timedNoteOffEvent.Event, "Events is not Note On.");
            ClassicAssert.AreEqual(220, timedNoteOffEvent.Time, "Time is invalid");

            var noteOffEvent = (NoteOffEvent)timedNoteOffEvent.Event;
            ClassicAssert.AreEqual((SevenBitNumber)45, noteOffEvent.NoteNumber, "Note number is invalid.");
            ClassicAssert.AreEqual((SevenBitNumber)122, noteOffEvent.Velocity, "Velocity is invalid.");
            ClassicAssert.AreEqual((FourBitNumber)9, noteOffEvent.Channel, "Channel is invalid.");
        }

        #endregion

        #region Time

        [Test]
        public void CheckTimeChangedEvent_ZeroTime_NoChange()
        {
            CheckTimeChangedEvent_NoChange(GetNote_ZeroTime());
        }

        [Test]
        public void CheckTimeChangedEvent_NonZeroTime_NoChange()
        {
            CheckTimeChangedEvent_NoChange(GetNote_NonzeroTime());
        }

        [Test]
        public void CheckTimeChangedEvent_ZeroTime_Changed()
        {
            CheckTimeChangedEvent_Changed(GetNote_ZeroTime());
        }

        [Test]
        public void CheckTimeChangedEvent_NonZeroTime_Changed()
        {
            CheckTimeChangedEvent_Changed(GetNote_NonzeroTime());
        }

        #endregion

        #region Length

        [Test]
        public void CheckLengthChangedEvent_ZeroTime_NoChange()
        {
            CheckLengthChangedEvent_NoChange(GetNote_ZeroTime());
        }

        [Test]
        public void CheckLengthChangedEvent_NonZeroTime_NoChange()
        {
            CheckLengthChangedEvent_NoChange(GetNote_NonzeroTime());
        }

        [Test]
        public void CheckLengthChangedEvent_ZeroTime_Changed()
        {
            CheckLengthChangedEvent_Changed(GetNote_ZeroTime());
        }

        [Test]
        public void CheckLengthChangedEvent_NonZeroTime_Changed()
        {
            CheckLengthChangedEvent_Changed(GetNote_NonzeroTime());
        }

        #endregion

        #region Properties

        [Test]
        public void CheckNoteChannel()
        {
            var initialChannel = (FourBitNumber)0;

            var note = new Note((SevenBitNumber)70);
            ClassicAssert.AreEqual(initialChannel, note.Channel, "Invalid channel after note created.");
            ClassicAssert.AreEqual(initialChannel, ((NoteOnEvent)note.TimedNoteOnEvent.Event).Channel, "Invalid channel of Note On timed event after note created.");
            ClassicAssert.AreEqual(initialChannel, ((NoteOffEvent)note.TimedNoteOffEvent.Event).Channel, "Invalid channel of Note Off timed event after note created.");
            MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after note created.");
            MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after note created.");

            var newChannel = (FourBitNumber)6;
            note.Channel = newChannel;

            ClassicAssert.AreEqual(newChannel, note.Channel, "Invalid channel after update.");
            ClassicAssert.AreEqual(newChannel, ((NoteOnEvent)note.TimedNoteOnEvent.Event).Channel, "Invalid channel of Note On timed event after update.");
            ClassicAssert.AreEqual(newChannel, ((NoteOffEvent)note.TimedNoteOffEvent.Event).Channel, "Invalid channel of Note Off timed event after update.");
            MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after update.");
            MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after update.");
        }

        [Test]
        public void CheckNoteVelocity()
        {
            var initialVelocity = Note.DefaultVelocity;

            var note = new Note((SevenBitNumber)70);
            ClassicAssert.AreEqual(initialVelocity, note.Velocity, "Invalid velocity after note created.");
            ClassicAssert.AreEqual(initialVelocity, ((NoteOnEvent)note.TimedNoteOnEvent.Event).Velocity, "Invalid velocity of Note On timed event after note created.");
            ClassicAssert.AreEqual(SevenBitNumber.MinValue, ((NoteOffEvent)note.TimedNoteOffEvent.Event).Velocity, "Invalid velocity of Note Off timed event after note created.");
            MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after note created.");
            MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after note created.");

            var newVelocity = (SevenBitNumber)60;
            note.Velocity = newVelocity;

            ClassicAssert.AreEqual(newVelocity, note.Velocity, "Invalid velocity after update.");
            ClassicAssert.AreEqual(newVelocity, ((NoteOnEvent)note.TimedNoteOnEvent.Event).Velocity, "Invalid velocity of Note On timed event after update.");
            ClassicAssert.AreEqual(SevenBitNumber.MinValue, ((NoteOffEvent)note.TimedNoteOffEvent.Event).Velocity, "Invalid velocity of Note Off timed event after update.");
            MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after update.");
            MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after update.");
        }

        [Test]
        public void CheckNoteOffVelocity()
        {
            var initialOffVelocity = SevenBitNumber.MinValue;

            var note = new Note((SevenBitNumber)70);
            ClassicAssert.AreEqual(initialOffVelocity, note.OffVelocity, "Invalid off velocity after note created.");
            ClassicAssert.AreEqual(Note.DefaultVelocity, ((NoteOnEvent)note.TimedNoteOnEvent.Event).Velocity, "Invalid off velocity of Note On timed event after note created.");
            ClassicAssert.AreEqual(initialOffVelocity, ((NoteOffEvent)note.TimedNoteOffEvent.Event).Velocity, "Invalid off velocity of Note Off timed event after note created.");
            MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after note created.");
            MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after note created.");

            var newOffVelocity = (SevenBitNumber)60;
            note.OffVelocity = newOffVelocity;

            ClassicAssert.AreEqual(newOffVelocity, note.OffVelocity, "Invalid off velocity after update.");
            ClassicAssert.AreEqual(Note.DefaultVelocity, ((NoteOnEvent)note.TimedNoteOnEvent.Event).Velocity, "Invalid off velocity of Note On timed event after update.");
            ClassicAssert.AreEqual(newOffVelocity, ((NoteOffEvent)note.TimedNoteOffEvent.Event).Velocity, "Invalid off velocity of Note Off timed event after update.");
            MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after update.");
            MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after update.");
        }

        [Test]
        public void CheckNoteNumber()
        {
            var initialNoteNumber = (SevenBitNumber)80;

            var note = new Note(initialNoteNumber);
            ClassicAssert.AreEqual(initialNoteNumber, note.NoteNumber, "Invalid note number after note created.");
            ClassicAssert.AreEqual(initialNoteNumber, ((NoteOnEvent)note.TimedNoteOnEvent.Event).NoteNumber, "Invalid note number of Note On timed event after note created.");
            ClassicAssert.AreEqual(initialNoteNumber, ((NoteOffEvent)note.TimedNoteOffEvent.Event).NoteNumber, "Invalid note number of Note Off timed event after note created.");
            MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after note created.");
            MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after note created.");

            var newNoteNumber = (SevenBitNumber)60;
            note.NoteNumber = newNoteNumber;

            ClassicAssert.AreEqual(newNoteNumber, note.NoteNumber, "Invalid note number after update.");
            ClassicAssert.AreEqual(newNoteNumber, ((NoteOnEvent)note.TimedNoteOnEvent.Event).NoteNumber, "Invalid note number of Note On timed event after update.");
            ClassicAssert.AreEqual(newNoteNumber, ((NoteOffEvent)note.TimedNoteOffEvent.Event).NoteNumber, "Invalid note number of Note Off timed event after update.");
            MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after update.");
            MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after update.");
        }

        [Test]
        public void CheckNoteNameAndOctave()
        {
            var initialNoteName = DryWetMidi.MusicTheory.NoteName.A;
            var initialOctave = 4;

            var note = new Note(initialNoteName, initialOctave);
            ClassicAssert.AreEqual(initialNoteName, note.NoteName, "Invalid note name after note created.");
            ClassicAssert.AreEqual(initialOctave, note.Octave, "Invalid octave after note created.");
            ClassicAssert.AreEqual(initialNoteName, ((NoteOnEvent)note.TimedNoteOnEvent.Event).GetNoteName(), "Invalid note name of Note On timed event after note created.");
            ClassicAssert.AreEqual(initialOctave, ((NoteOnEvent)note.TimedNoteOnEvent.Event).GetNoteOctave(), "Invalid octave of Note On timed event after note created.");
            ClassicAssert.AreEqual(initialNoteName, ((NoteOffEvent)note.TimedNoteOffEvent.Event).GetNoteName(), "Invalid note name of Note Off timed event after note created.");
            ClassicAssert.AreEqual(initialOctave, ((NoteOffEvent)note.TimedNoteOffEvent.Event).GetNoteOctave(), "Invalid octave of Note Off timed event after note created.");
            MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after note created.");
            MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after note created.");

            var newNoteName = DryWetMidi.MusicTheory.NoteName.B;
            var newOctave = 3;
            note.SetNoteNameAndOctave(newNoteName, newOctave);

            ClassicAssert.AreEqual(newNoteName, note.NoteName, "Invalid note name after update.");
            ClassicAssert.AreEqual(newOctave, note.Octave, "Invalid octave after update.");
            ClassicAssert.AreEqual(newNoteName, ((NoteOnEvent)note.TimedNoteOnEvent.Event).GetNoteName(), "Invalid note name of Note On timed event after update.");
            ClassicAssert.AreEqual(newOctave, ((NoteOnEvent)note.TimedNoteOnEvent.Event).GetNoteOctave(), "Invalid octave of Note On timed event after update.");
            ClassicAssert.AreEqual(newNoteName, ((NoteOffEvent)note.TimedNoteOffEvent.Event).GetNoteName(), "Invalid note name of Note Off timed event after update.");
            ClassicAssert.AreEqual(newOctave, ((NoteOffEvent)note.TimedNoteOffEvent.Event).GetNoteOctave(), "Invalid octave of Note Off timed event after update.");
            MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after update.");
            MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after update.");
        }

        #endregion

        #region GetMusicTheoryNote

        [Test]
        public void GetMusicTheoryNote()
        {
            var note = new Note(DryWetMidi.MusicTheory.NoteName.A, 1);

            ClassicAssert.AreEqual(
                DryWetMidi.MusicTheory.Note.Get(DryWetMidi.MusicTheory.NoteName.A, 1),
                note.GetMusicTheoryNote(),
                "Note is invalid.");
        }

        #endregion

        #endregion

        #region Private methods

        private static void CheckTimeChangedEvent_NoChange(Note note)
        {
            object timeChangedSender = null;
            TimeChangedEventArgs timeChangedEventArgs = null;

            note.TimeChanged += (sender, eventArgs) =>
            {
                timeChangedSender = sender;
                timeChangedEventArgs = eventArgs;
            };

            var lengthChangedFired = false;
            note.LengthChanged += (_, __) => lengthChangedFired = true;

            note.Time = note.Time;

            ClassicAssert.IsFalse(lengthChangedFired, "Length changed event fired.");
            ClassicAssert.IsNull(timeChangedSender, "Sender is not null.");
            ClassicAssert.IsNull(timeChangedEventArgs, "Event args is not null.");
        }

        private static void CheckTimeChangedEvent_Changed(Note note)
        {
            object timeChangedSender = null;
            TimeChangedEventArgs timeChangedEventArgs = null;

            note.TimeChanged += (sender, eventArgs) =>
            {
                timeChangedSender = sender;
                timeChangedEventArgs = eventArgs;
            };

            var lengthChangedFired = false;
            note.LengthChanged += (_, __) => lengthChangedFired = true;

            var oldTime = note.Time;
            note.Time += 100;

            ClassicAssert.IsFalse(lengthChangedFired, "Length changed event fired.");
            ClassicAssert.AreSame(note, timeChangedSender, "Sender is invalid.");
            ClassicAssert.IsNotNull(timeChangedEventArgs, "Event args is null.");
            ClassicAssert.AreEqual(oldTime, timeChangedEventArgs.OldTime, "Old time is invalid.");
            ClassicAssert.AreEqual(note.Time, timeChangedEventArgs.NewTime, "New time is invalid.");
            ClassicAssert.AreNotEqual(oldTime, note.Time, "New time is equal to old one.");
        }

        private static void CheckLengthChangedEvent_NoChange(Note note)
        {
            object lengthChangedSender = null;
            LengthChangedEventArgs lengthChangedEventArgs = null;

            note.LengthChanged += (sender, eventArgs) =>
            {
                lengthChangedSender = sender;
                lengthChangedEventArgs = eventArgs;
            };

            var timeChangedFired = false;
            note.TimeChanged += (_, __) => timeChangedFired = true;

            note.Length = note.Length;

            ClassicAssert.IsFalse(timeChangedFired, "Time changed event fired.");
            ClassicAssert.IsNull(lengthChangedSender, "Sender is not null.");
            ClassicAssert.IsNull(lengthChangedEventArgs, "Event args is not null.");
        }

        private static void CheckLengthChangedEvent_Changed(Note note)
        {
            object lengthChangedSender = null;
            LengthChangedEventArgs lengthChangedEventArgs = null;

            note.LengthChanged += (sender, eventArgs) =>
            {
                lengthChangedSender = sender;
                lengthChangedEventArgs = eventArgs;
            };

            var timeChangedFired = false;
            note.TimeChanged += (_, __) => timeChangedFired = true;

            var oldLength = note.Length;
            note.Length += 100;

            ClassicAssert.IsFalse(timeChangedFired, "Time changed event fired.");
            ClassicAssert.AreSame(note, lengthChangedSender, "Sender is invalid.");
            ClassicAssert.IsNotNull(lengthChangedEventArgs, "Event args is null.");
            ClassicAssert.AreEqual(oldLength, lengthChangedEventArgs.OldLength, "Old length is invalid.");
            ClassicAssert.AreEqual(note.Length, lengthChangedEventArgs.NewLength, "New length is invalid.");
            ClassicAssert.AreNotEqual(oldLength, note.Length, "New length is equal to old one.");
        }

        private static void CheckEndTime<TTimeSpan>(ITimeSpan time, ITimeSpan length, TTimeSpan expectedEndTime)
            where TTimeSpan : ITimeSpan
        {
            var tempoMap = TempoMap.Default;
            var note = new NoteMethods().Create(time, length, tempoMap);
            TimeSpanTestUtilities.AreEqual(expectedEndTime, note.EndTimeAs<TTimeSpan>(tempoMap), "End time is invalid.");
        }

        private static Note GetNote_ZeroTime()
        {
            return new Note((SevenBitNumber)10, 170, 0);
        }

        private static Note GetNote_NonzeroTime()
        {
            return new Note((SevenBitNumber)10, 150, 100);
        }

        #endregion
    }
}
