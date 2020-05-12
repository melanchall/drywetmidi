using System;
using System.Linq.Expressions;
using System.Reflection;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public class ChordTests
    {
        #region Nested classes

        public abstract class NotesPropertyTester<TValue>
        {
            #region Fields

            private readonly Expression<Func<Chord, TValue>> _chordPropertySelector;
            private readonly Expression<Func<Note, TValue>> _notePropertySelector;
            private readonly TValue _value1;
            private readonly TValue _value2;
            private readonly TValue _value;

            #endregion

            #region Constructor

            public NotesPropertyTester(Expression<Func<Chord, TValue>> chordPropertySelector,
                                       Expression<Func<Note, TValue>> notePropertySelector,
                                       TValue value1,
                                       TValue value2,
                                       TValue value)
            {
                _chordPropertySelector = chordPropertySelector;
                _notePropertySelector = notePropertySelector;
                _value1 = value1;
                _value2 = value2;
                _value = value;
            }

            #endregion

            #region Test methods

            [Test]
            [Description("Get property value of a chord without notes.")]
            public void Get_NoNotes()
            {
                var chord = new Chord();

                Assert.Throws<InvalidOperationException>(() => GetPropertyValue(_chordPropertySelector, chord));
            }

            [Test]
            [Description("Get property value of a chord containing notes with the same value of the specified property.")]
            public void Get_SameValues()
            {
                var chord = GetChord(_notePropertySelector, _value1, _value1);

                Assert.AreEqual(_value1, GetPropertyValue(_chordPropertySelector, chord));
            }

            [Test]
            [Description("Get property value of a chord containing notes with different values of the specified property.")]
            public void Get_DifferentValues()
            {
                var chord = GetChord(_notePropertySelector, _value1, _value2);

                Assert.Throws<InvalidOperationException>(() => GetPropertyValue(_chordPropertySelector, chord));
            }

            [Test]
            [Description("Set a chord's property value.")]
            public void Set()
            {
                var chord = GetChord(_notePropertySelector, _value1, _value2);

                SetPropertyValue(_chordPropertySelector, chord, _value);

                Assert.AreEqual(_value, GetPropertyValue(_chordPropertySelector, chord));
            }

            #endregion

            #region Private methods

            private static Chord GetChord(Expression<Func<Note, TValue>> notePropertySelector, TValue value1, TValue value2)
            {
                var firstNote = new Note(DryWetMidi.MusicTheory.NoteName.A, 1);
                SetPropertyValue(notePropertySelector, firstNote, value1);

                var secondNote = new Note(DryWetMidi.MusicTheory.NoteName.B, 1);
                SetPropertyValue(notePropertySelector, secondNote, value2);

                return new Chord(firstNote, secondNote);
            }

            private static TValue GetPropertyValue<TObj>(Expression<Func<TObj, TValue>> propertySelector, TObj obj)
            {
                var propertyInfo = GetPropertyInfo(propertySelector);

                try
                {
                    return (TValue)propertyInfo?.GetValue(obj);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }

            private static void SetPropertyValue<TObj>(Expression<Func<TObj, TValue>> propertySelector, TObj obj, TValue value)
            {
                var propertyInfo = GetPropertyInfo(propertySelector);

                try
                {
                    propertyInfo?.SetValue(obj, value);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }

            private static PropertyInfo GetPropertyInfo<TObj>(Expression<Func<TObj, TValue>> propertySelector)
            {
                var propertySelectorExpression = propertySelector.Body as MemberExpression;
                if (propertySelectorExpression == null)
                    return null;

                return propertySelectorExpression.Member as PropertyInfo;
            }

            #endregion
        }

        [TestFixture]
        public sealed class ChannelTester : NotesPropertyTester<FourBitNumber>
        {
            public ChannelTester()
                : base(c => c.Channel,
                       n => n.Channel,
                       FourBitNumber.MaxValue,
                       FourBitNumber.MinValue,
                       (FourBitNumber)5)
            {
            }
        }

        [TestFixture]
        public sealed class VelocityTester : NotesPropertyTester<SevenBitNumber>
        {
            public VelocityTester()
                : base(c => c.Velocity,
                       n => n.Velocity,
                       SevenBitNumber.MaxValue,
                       SevenBitNumber.MinValue,
                       (SevenBitNumber)5)
            {
            }
        }

        [TestFixture]
        public sealed class OffVelocityTester : NotesPropertyTester<SevenBitNumber>
        {
            public OffVelocityTester()
                : base(c => c.OffVelocity,
                       n => n.OffVelocity,
                       SevenBitNumber.MaxValue,
                       SevenBitNumber.MinValue,
                       (SevenBitNumber)5)
            {
            }
        }

        #endregion

        #region Constants

        private static readonly Func<Chord> TwoNotesChordCreator = () => new Chord(new Note((SevenBitNumber)100, 200, 100),
                                                                                   new Note((SevenBitNumber)110, 300, 130));

        #endregion

        #region Test methods

        #region Length

        [Test]
        [Description("Get length of Chord without notes.")]
        public void Length_Get_NoNotes()
        {
            Assert.AreEqual(0, GetChord_NoNotes().Length);
        }

        [Test]
        [Description("Get length of Chord with time of zero.")]
        public void Length_Get_ZeroTime()
        {
            Assert.AreEqual(200, GetChord_ZeroTime().Length);
        }

        [Test]
        [Description("Get length of Chord with time of nonzero number.")]
        public void Length_Get_NonzeroTime()
        {
            Assert.AreEqual(200, GetChord_NonzeroTime().Length);
        }

        [Test]
        [Description("Set length of Chord without notes.")]
        public void Length_Set_NoNotes()
        {
            var chord = GetChord_NoNotes();
            chord.Length = 100;

            Assert.AreEqual(0, chord.Length);
        }

        [Test]
        [Description("Set length of Chord with time of zero.")]
        public void Length_Set_ZeroTime()
        {
            var chord = GetChord_ZeroTime();
            chord.Length = 500;

            Assert.AreEqual(500, chord.Length);
        }

        [Test]
        [Description("Set length of Chord with time of nonzero number.")]
        public void Length_Set_NonzeroTime()
        {
            var chord = GetChord_NonzeroTime();
            chord.Length = 500;

            Assert.AreEqual(500, chord.Length);
        }

        #endregion

        #region Clone

        [Test]
        [Description("Check that clone of an empty chord equals to the original one.")]
        public void Clone_1()
        {
            var chord = GetChord_NoNotes();

            Assert.IsTrue(ChordEquality.AreEqual(chord, chord.Clone()),
                          "Clone of a chord doesn't equal to the original one.");
        }

        [Test]
        [Description("Check that clone of a chord equals to the original one.")]
        public void Clone_2()
        {
            var chord = GetChord_NonzeroTime();

            Assert.IsTrue(ChordEquality.AreEqual(chord, chord.Clone()),
                          "Clone of a chord doesn't equal to the original one.");
        }

        #endregion

        #region Split

        [Test]
        [Description("Split empty chord.")]
        public void Split_Empty()
        {
            Func<Chord> chordCreator = () => new Chord();

            var chord = chordCreator();
            var time = 100;

            var parts = chord.Split(time);
            Assert.IsNull(parts.RightPart,
                          "Right part is not null.");
            Assert.AreNotSame(parts.LeftPart,
                              chord,
                              "Left part refers to the same object as the original chord.");
            Assert.IsTrue(ChordEquality.AreEqual(chordCreator(), parts.LeftPart),
                          "Left part doesn't equal to the original chord.");
        }

        [Test]
        [Description("Split a chord of zero length.")]
        public void Split_ZeroLength()
        {
            Func<Chord> chordCreator = () => new Chord(new Note((SevenBitNumber)100));

            var chord = chordCreator();
            var time = 0;

            var parts = chord.Split(time);
            Assert.IsNull(parts.LeftPart,
                          "Left part is not null.");
            Assert.AreNotSame(parts.RightPart,
                              chord,
                              "Right part refers to the same object as the original chord.");
            Assert.IsTrue(ChordEquality.AreEqual(chordCreator(), parts.RightPart),
                          "Right part doesn't equal to the original chord.");
        }

        [Test]
        [Description("Split by time below the start time of a chord.")]
        public void Split_TimeBelowStartTime()
        {
            var chordCreator = TwoNotesChordCreator;

            var chord = chordCreator();
            var time = 50;

            var parts = chord.Split(time);
            Assert.IsNull(parts.LeftPart,
                          "Left part is not null.");
            Assert.AreNotSame(parts.RightPart,
                              chord,
                              "Right part refers to the same object as the original chord.");
            Assert.IsTrue(ChordEquality.AreEqual(chordCreator(), parts.RightPart),
                          "Right part doesn't equal to the original chord.");
        }

        [Test]
        [Description("Split by time above the end time of a chord.")]
        public void Split_TimeAboveEndTime()
        {
            Func<Chord> chordCreator = TwoNotesChordCreator;

            var chord = chordCreator();
            var time = 500;

            var parts = chord.Split(time);
            Assert.IsNull(parts.RightPart,
                          "Right part is not null.");
            Assert.AreNotSame(parts.LeftPart,
                              chord,
                              "Left part refers to the same object as the original chord.");
            Assert.IsTrue(ChordEquality.AreEqual(chordCreator(), parts.LeftPart),
                          "Left part doesn't equal to the original chord.");
        }

        [Test]
        [Description("Split a chord by time falling inside it and intersecting all notes of the chord.")]
        public void Split_TimeInsideChord_IntersectingAllNotes()
        {
            Func<Chord> chordCreator = TwoNotesChordCreator;

            var note = chordCreator();
            var time = 150;

            var parts = note.Split(time);
            var expectedLeftChord = new Chord(new Note((SevenBitNumber)100, 50, 100),
                                              new Note((SevenBitNumber)110, 20, 130));
            var expectedRightChord = new Chord(new Note((SevenBitNumber)100, 150, 150),
                                               new Note((SevenBitNumber)110, 280, 150));

            Assert.IsTrue(ChordEquality.AreEqual(expectedLeftChord, parts.LeftPart),
                          "Left part is invalid.");
            Assert.IsTrue(ChordEquality.AreEqual(expectedRightChord, parts.RightPart),
                          "Right part is invalid.");
        }

        [Test]
        [Description("Split a chord by time falling inside it and intersecting not all notes of the chord.")]
        public void Split_TimeInsideChord_IntersectingNotAllNotes()
        {
            Func<Chord> chordCreator = TwoNotesChordCreator;

            var chord = chordCreator();
            var time = 120;

            var parts = chord.Split(time);
            var expectedLeftChord = new Chord(new Note((SevenBitNumber)100, 20, 100));
            var expectedRightChord = new Chord(new Note((SevenBitNumber)100, 180, 120),
                                               new Note((SevenBitNumber)110, 300, 130));

            Assert.IsTrue(ChordEquality.AreEqual(expectedLeftChord, parts.LeftPart),
                          "Left part is invalid.");
            Assert.IsTrue(ChordEquality.AreEqual(expectedRightChord, parts.RightPart),
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

        #region Time

        [Test]
        public void CheckTimeChangedEvent_ZeroTime_NoChange()
        {
            CheckTimeChangedEvent_NoChange(GetChord_ZeroTime());
        }

        [Test]
        public void CheckTimeChangedEvent_NonZeroTime_NoChange()
        {
            CheckTimeChangedEvent_NoChange(GetChord_NonzeroTime());
        }

        [Test]
        public void CheckTimeChangedEvent_ZeroTime_Changed()
        {
            CheckTimeChangedEvent_Changed(GetChord_ZeroTime());
        }

        [Test]
        public void CheckTimeChangedEvent_NonZeroTime_Changed()
        {
            CheckTimeChangedEvent_Changed(GetChord_NonzeroTime());
        }

        #endregion

        #region Length

        [Test]
        public void CheckLengthChangedEvent_ZeroTime_NoChange()
        {
            CheckLengthChangedEvent_NoChange(GetChord_ZeroTime());
        }

        [Test]
        public void CheckLengthChangedEvent_NonZeroTime_NoChange()
        {
            CheckLengthChangedEvent_NoChange(GetChord_NonzeroTime());
        }

        [Test]
        public void CheckLengthChangedEvent_ZeroTime_Changed()
        {
            CheckLengthChangedEvent_Changed(GetChord_ZeroTime());
        }

        [Test]
        public void CheckLengthChangedEvent_NonZeroTime_Changed()
        {
            CheckLengthChangedEvent_Changed(GetChord_NonzeroTime());
        }

        #endregion

        #endregion

        #region Private methods

        private static void CheckTimeChangedEvent_NoChange(Chord chord)
        {
            object timeChangedSender = null;
            TimeChangedEventArgs timeChangedEventArgs = null;

            chord.TimeChanged += (sender, eventArgs) =>
            {
                timeChangedSender = sender;
                timeChangedEventArgs = eventArgs;
            };

            var lengthChangedFired = false;
            chord.LengthChanged += (_, __) => lengthChangedFired = true;

            chord.Time = chord.Time;

            Assert.IsFalse(lengthChangedFired, "Length changed event fired.");
            Assert.IsNull(timeChangedSender, "Sender is not null.");
            Assert.IsNull(timeChangedEventArgs, "Event args is not null.");
        }

        private static void CheckTimeChangedEvent_Changed(Chord chord)
        {
            object timeChangedSender = null;
            TimeChangedEventArgs timeChangedEventArgs = null;

            chord.TimeChanged += (sender, eventArgs) =>
            {
                timeChangedSender = sender;
                timeChangedEventArgs = eventArgs;
            };

            var lengthChangedFired = false;
            chord.LengthChanged += (_, __) => lengthChangedFired = true;

            var oldTime = chord.Time;
            chord.Time += 100;

            Assert.IsFalse(lengthChangedFired, "Length changed event fired.");
            Assert.AreSame(chord, timeChangedSender, "Sender is invalid.");
            Assert.IsNotNull(timeChangedEventArgs, "Event args is null.");
            Assert.AreEqual(oldTime, timeChangedEventArgs.OldTime, "Old time is invalid.");
            Assert.AreEqual(chord.Time, timeChangedEventArgs.NewTime, "New time is invalid.");
            Assert.AreNotEqual(oldTime, chord.Time, "New time is equal to old one.");
        }

        private static void CheckLengthChangedEvent_NoChange(Chord chord)
        {
            object lengthChangedSender = null;
            LengthChangedEventArgs lengthChangedEventArgs = null;

            chord.LengthChanged += (sender, eventArgs) =>
            {
                lengthChangedSender = sender;
                lengthChangedEventArgs = eventArgs;
            };

            var timeChangedFired = false;
            chord.TimeChanged += (_, __) => timeChangedFired = true;

            chord.Length = chord.Length;

            Assert.IsFalse(timeChangedFired, "Time changed event fired.");
            Assert.IsNull(lengthChangedSender, "Sender is not null.");
            Assert.IsNull(lengthChangedEventArgs, "Event args is not null.");
        }

        private static void CheckLengthChangedEvent_Changed(Chord chord)
        {
            object lengthChangedSender = null;
            LengthChangedEventArgs lengthChangedEventArgs = null;

            chord.LengthChanged += (sender, eventArgs) =>
            {
                lengthChangedSender = sender;
                lengthChangedEventArgs = eventArgs;
            };

            var timeChangedFired = false;
            chord.TimeChanged += (_, __) => timeChangedFired = true;

            var oldLength = chord.Length;
            chord.Length += 100;

            Assert.IsFalse(timeChangedFired, "Time changed event fired.");
            Assert.AreSame(chord, lengthChangedSender, "Sender is invalid.");
            Assert.IsNotNull(lengthChangedEventArgs, "Event args is null.");
            Assert.AreEqual(oldLength, lengthChangedEventArgs.OldLength, "Old length is invalid.");
            Assert.AreEqual(chord.Length, lengthChangedEventArgs.NewLength, "New length is invalid.");
            Assert.AreNotEqual(oldLength, chord.Length, "New length is equal to old one.");
        }

        private static void CheckEndTime<TTimeSpan>(ITimeSpan time, ITimeSpan length, TTimeSpan expectedEndTime)
            where TTimeSpan : ITimeSpan
        {
            var tempoMap = TempoMap.Default;
            var chord = new ChordMethods().Create(time, length, tempoMap);
            TimeSpanTestUtilities.AreEqual(expectedEndTime, chord.EndTimeAs<TTimeSpan>(tempoMap), "End time is invalid.");
        }

        private static Chord GetChord_NoNotes()
        {
            return new Chord();
        }

        private static Chord GetChord_ZeroTime()
        {
            return new Chord(new Note(SevenBitNumber.MaxValue, 100),
                             new Note(SevenBitNumber.MaxValue, 100, 50),
                             new Note(SevenBitNumber.MaxValue, 100, 100));
        }

        private static Chord GetChord_NonzeroTime()
        {
            return new Chord(new Note(SevenBitNumber.MaxValue, 100, 50),
                             new Note(SevenBitNumber.MaxValue, 100, 100),
                             new Note(SevenBitNumber.MaxValue, 100, 150));
        }

        #endregion
    }
}
