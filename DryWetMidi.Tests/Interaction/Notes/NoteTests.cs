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

        #endregion

        #region Private methods

        private static void CheckEndTime<TTimeSpan>(ITimeSpan time, ITimeSpan length, TTimeSpan expectedEndTime)
            where TTimeSpan : ITimeSpan
        {
            var tempoMap = TempoMap.Default;
            var note = new NoteMethods().Create(time, length, tempoMap);
            TimeSpanTestUtilities.AreEqual(expectedEndTime, note.EndTimeAs<TTimeSpan>(tempoMap), "End time is invalid.");
        }

        #endregion
    }
}
