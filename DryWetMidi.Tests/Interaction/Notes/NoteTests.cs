using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class NoteTests
    {
        #region Test methods

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

            Assert.IsTrue(NoteEquality.AreEqual(note, note.Clone()),
                          "Clone of a note doesn't equal to the original one.");
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
            Assert.IsNull(parts.LeftPart,
                          "Left part is not null.");
            Assert.AreNotSame(parts.RightPart,
                              note,
                              "Right part refers to the same object as the original note.");
            Assert.IsTrue(NoteEquality.AreEqual(noteCreator(), parts.RightPart),
                          "Right part doesn't equal to the original note.");
        }

        [Test]
        [Description("Split by time below the start time of a note.")]
        public void Split_TimeBelowStartTime()
        {
            Func<Note> noteCreator = () => new Note((SevenBitNumber)100, 200, 100);

            var note = noteCreator();
            var time = 50;

            var parts = note.Split(time);
            Assert.IsNull(parts.LeftPart,
                          "Left part is not null.");
            Assert.AreNotSame(parts.RightPart,
                              note,
                              "Right part refers to the same object as the original note.");
            Assert.IsTrue(NoteEquality.AreEqual(noteCreator(), parts.RightPart),
                          "Right part doesn't equal to the original note.");
        }

        [Test]
        [Description("Split by time above the end time of a note.")]
        public void Split_TimeAboveEndTime()
        {
            Func<Note> noteCreator = () => new Note((SevenBitNumber)100, 200, 100);

            var note = noteCreator();
            var time = 500;

            var parts = note.Split(time);
            Assert.IsNull(parts.RightPart,
                          "Right part is not null.");
            Assert.AreNotSame(parts.LeftPart,
                              note,
                              "Left part refers to the same object as the original note.");
            Assert.IsTrue(NoteEquality.AreEqual(noteCreator(), parts.LeftPart),
                          "Left part doesn't equal to the original note.");
        }

        [Test]
        [Description("Split a note by time falling inside it.")]
        public void Split_TimeInsideNote()
        {
            var note = new Note((SevenBitNumber)100, 200, 100);
            var time = 120;

            var parts = note.Split(time);
            Assert.IsTrue(NoteEquality.AreEqual(new Note((SevenBitNumber)100, 20, 100), parts.LeftPart),
                          "Left part is invalid.");
            Assert.IsTrue(NoteEquality.AreEqual(new Note((SevenBitNumber)100, 180, 120), parts.RightPart),
                          "Right part is invalid.");
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
            Assert.IsNotNull(timedNoteOnEvent1, "1st event is null.");
            Assert.IsNotNull(timedNoteOnEvent2, "2nd event is null.");
            Assert.AreNotEqual(timedNoteOnEvent1, timedNoteOnEvent2, "Events have not been cloned.");
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
            Assert.IsInstanceOf(typeof(NoteOnEvent), timedNoteOnEvent.Event, "Events is not Note On.");
            Assert.AreEqual(20, timedNoteOnEvent.Time, "Time is invalid");

            var noteOnEvent = (NoteOnEvent)timedNoteOnEvent.Event;
            Assert.AreEqual((SevenBitNumber)45, noteOnEvent.NoteNumber, "Note number is invalid.");
            Assert.AreEqual((SevenBitNumber)12, noteOnEvent.Velocity, "Velocity is invalid.");
            Assert.AreEqual((FourBitNumber)9, noteOnEvent.Channel, "Channel is invalid.");
        }

        #endregion

        #region GetTimedNoteOffEvent

        [Test]
        public void GetTimedNoteOffEvent()
        {
            var note = new Note(new SevenBitNumber(1));
            var timedNoteOffEvent1 = note.GetTimedNoteOffEvent();
            var timedNoteOffEvent2 = note.GetTimedNoteOffEvent();
            Assert.IsNotNull(timedNoteOffEvent1, "1st event is null.");
            Assert.IsNotNull(timedNoteOffEvent2, "2nd event is null.");
            Assert.AreNotEqual(timedNoteOffEvent1, timedNoteOffEvent2, "Events have not been cloned.");
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
            Assert.IsInstanceOf(typeof(NoteOffEvent), timedNoteOffEvent.Event, "Events is not Note On.");
            Assert.AreEqual(220, timedNoteOffEvent.Time, "Time is invalid");

            var noteOffEvent = (NoteOffEvent)timedNoteOffEvent.Event;
            Assert.AreEqual((SevenBitNumber)45, noteOffEvent.NoteNumber, "Note number is invalid.");
            Assert.AreEqual((SevenBitNumber)122, noteOffEvent.Velocity, "Velocity is invalid.");
            Assert.AreEqual((FourBitNumber)9, noteOffEvent.Channel, "Channel is invalid.");
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

            Assert.IsFalse(lengthChangedFired, "Length changed event fired.");
            Assert.IsNull(timeChangedSender, "Sender is not null.");
            Assert.IsNull(timeChangedEventArgs, "Event args is not null.");
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

            Assert.IsFalse(lengthChangedFired, "Length changed event fired.");
            Assert.AreSame(note, timeChangedSender, "Sender is invalid.");
            Assert.IsNotNull(timeChangedEventArgs, "Event args is null.");
            Assert.AreEqual(oldTime, timeChangedEventArgs.OldTime, "Old time is invalid.");
            Assert.AreEqual(note.Time, timeChangedEventArgs.NewTime, "New time is invalid.");
            Assert.AreNotEqual(oldTime, note.Time, "New time is equal to old one.");
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

            Assert.IsFalse(timeChangedFired, "Time changed event fired.");
            Assert.IsNull(lengthChangedSender, "Sender is not null.");
            Assert.IsNull(lengthChangedEventArgs, "Event args is not null.");
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

            Assert.IsFalse(timeChangedFired, "Time changed event fired.");
            Assert.AreSame(note, lengthChangedSender, "Sender is invalid.");
            Assert.IsNotNull(lengthChangedEventArgs, "Event args is null.");
            Assert.AreEqual(oldLength, lengthChangedEventArgs.OldLength, "Old length is invalid.");
            Assert.AreEqual(note.Length, lengthChangedEventArgs.NewLength, "New length is invalid.");
            Assert.AreNotEqual(oldLength, note.Length, "New length is equal to old one.");
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
