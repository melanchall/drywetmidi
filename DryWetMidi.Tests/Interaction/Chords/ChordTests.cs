using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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

                ClassicAssert.Throws<InvalidOperationException>(() => GetPropertyValue(_chordPropertySelector, chord));
            }

            [Test]
            [Description("Get property value of a chord containing notes with the same value of the specified property.")]
            public void Get_SameValues()
            {
                var chord = GetChord(_notePropertySelector, _value1, _value1);

                ClassicAssert.AreEqual(_value1, GetPropertyValue(_chordPropertySelector, chord));
            }

            [Test]
            [Description("Get property value of a chord containing notes with different values of the specified property.")]
            public void Get_DifferentValues()
            {
                var chord = GetChord(_notePropertySelector, _value1, _value2);

                ClassicAssert.Throws<InvalidOperationException>(() => GetPropertyValue(_chordPropertySelector, chord));
            }

            [Test]
            [Description("Set a chord's property value.")]
            public void Set()
            {
                var chord = GetChord(_notePropertySelector, _value1, _value2);

                SetPropertyValue(_chordPropertySelector, chord, _value);

                ClassicAssert.AreEqual(_value, GetPropertyValue(_chordPropertySelector, chord));
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

        private sealed class TaggedChord : Chord
        {
            public TaggedChord(IEnumerable<Note> notes, object tag)
                : base(notes)
            {
                Tag = tag;
            }

            public object Tag { get; }

            public override ITimedObject Clone()
            {
                return new TaggedChord(Notes, Tag)
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
            ClassicAssert.AreEqual(0, GetChord_NoNotes().Length);
        }

        [Test]
        [Description("Get length of Chord with time of zero.")]
        public void Length_Get_ZeroTime()
        {
            ClassicAssert.AreEqual(200, GetChord_ZeroTime().Length);
        }

        [Test]
        [Description("Get length of Chord with time of nonzero number.")]
        public void Length_Get_NonzeroTime()
        {
            ClassicAssert.AreEqual(200, GetChord_NonzeroTime().Length);
        }

        [Test]
        [Description("Set length of Chord without notes.")]
        public void Length_Set_NoNotes()
        {
            var chord = GetChord_NoNotes();
            chord.Length = 100;

            ClassicAssert.AreEqual(0, chord.Length);
        }

        [Test]
        [Description("Set length of Chord with time of zero.")]
        public void Length_Set_ZeroTime()
        {
            var chord = GetChord_ZeroTime();
            chord.Length = 500;

            ClassicAssert.AreEqual(500, chord.Length);
        }

        [Test]
        [Description("Set length of Chord with time of nonzero number.")]
        public void Length_Set_NonzeroTime()
        {
            var chord = GetChord_NonzeroTime();
            chord.Length = 500;

            ClassicAssert.AreEqual(500, chord.Length);
        }

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

        [Test]
        public void Length_Set_BelowLastNoteDistance_1([Values(0, 20)] long time, [Values(0, 10)] long secondNoteShift)
        {
            var chord = new Chord(
                new Note((SevenBitNumber)70, 100, time),
                new Note((SevenBitNumber)80, 100, time + secondNoteShift));

            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => chord.Length = secondNoteShift - 1, "No exception thrown.");
        }

        [Test]
        public void Length_Set_LastNoteDistance([Values(0, 20)] long time, [Values(0, 10)] long secondNoteShift)
        {
            var chord = new Chord(
                new Note((SevenBitNumber)70, 100, time),
                new Note((SevenBitNumber)80, 100, time + secondNoteShift));

            ClassicAssert.DoesNotThrow(() => chord.Length = secondNoteShift, "Exception thrown.");
        }

        [Test]
        public void Length_Set_AboveLastNoteDistance([Values(0, 20)] long time, [Values(0, 10)] long secondNoteShift)
        {
            var chord = new Chord(
                new Note((SevenBitNumber)70, 100, time),
                new Note((SevenBitNumber)80, 100, time + secondNoteShift));

            ClassicAssert.DoesNotThrow(() => chord.Length = secondNoteShift + 1, "Exception thrown.");
        }

        #endregion

        #region Clone

        [Test]
        [Description("Check that clone of an empty chord equals to the original one.")]
        public void Clone_1()
        {
            var chord = GetChord_NoNotes();

            MidiAsserts.AreEqual(chord, chord.Clone(), "Clone of a chord doesn't equal to the original one.");
        }

        [Test]
        [Description("Check that clone of a chord equals to the original one.")]
        public void Clone_2()
        {
            var chord = GetChord_NonzeroTime();

            MidiAsserts.AreEqual(chord, chord.Clone(), "Clone of a chord doesn't equal to the original one.");
        }

        [Test]
        public void CloneCustomChord()
        {
            const string tag = "Tag";

            var notes = new[] { new Note((SevenBitNumber)70) };
            var taggedChord = new TaggedChord(notes, tag);

            var clone = (TaggedChord)taggedChord.Clone();

            MidiAsserts.AreEqual(notes, clone.Notes, "Notes are invalid.");
            ClassicAssert.AreEqual(tag, clone.Tag, "Tag is invalid.");
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
            ClassicAssert.IsNull(parts.RightPart,
                          "Right part is not null.");
            ClassicAssert.AreNotSame(parts.LeftPart,
                              chord,
                              "Left part refers to the same object as the original chord.");
            MidiAsserts.AreEqual(chordCreator(), parts.LeftPart, "Left part doesn't equal to the original chord.");
        }

        [Test]
        [Description("Split a chord of zero length.")]
        public void Split_ZeroLength()
        {
            Func<Chord> chordCreator = () => new Chord(new Note((SevenBitNumber)100));

            var chord = chordCreator();
            var time = 0;

            var parts = chord.Split(time);
            ClassicAssert.IsNull(parts.LeftPart,
                          "Left part is not null.");
            ClassicAssert.AreNotSame(parts.RightPart,
                              chord,
                              "Right part refers to the same object as the original chord.");
            MidiAsserts.AreEqual(chordCreator(), parts.RightPart, "Right part doesn't equal to the original chord.");
        }

        [Test]
        [Description("Split by time below the start time of a chord.")]
        public void Split_TimeBelowStartTime()
        {
            var chordCreator = TwoNotesChordCreator;

            var chord = chordCreator();
            var time = 50;

            var parts = chord.Split(time);
            ClassicAssert.IsNull(parts.LeftPart,
                          "Left part is not null.");
            ClassicAssert.AreNotSame(parts.RightPart,
                              chord,
                              "Right part refers to the same object as the original chord.");
            MidiAsserts.AreEqual(chordCreator(), parts.RightPart, "Right part doesn't equal to the original chord.");
        }

        [Test]
        [Description("Split by time above the end time of a chord.")]
        public void Split_TimeAboveEndTime()
        {
            Func<Chord> chordCreator = TwoNotesChordCreator;

            var chord = chordCreator();
            var time = 500;

            var parts = chord.Split(time);
            ClassicAssert.IsNull(parts.RightPart,
                          "Right part is not null.");
            ClassicAssert.AreNotSame(parts.LeftPart,
                              chord,
                              "Left part refers to the same object as the original chord.");
            MidiAsserts.AreEqual(chordCreator(), parts.LeftPart, "Left part doesn't equal to the original chord.");
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

            MidiAsserts.AreEqual(expectedLeftChord, parts.LeftPart, "Left part is invalid.");
            MidiAsserts.AreEqual(expectedRightChord, parts.RightPart, "Right part is invalid.");
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

            MidiAsserts.AreEqual(expectedLeftChord, parts.LeftPart, "Left part is invalid.");
            MidiAsserts.AreEqual(expectedRightChord, parts.RightPart, "Right part is invalid.");
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

        #region Channel

        [Test]
        public void GetChannel_EmptyChord()
        {
            var chord = new Chord();
            ClassicAssert.Throws<InvalidOperationException>(() => { var channel = chord.Channel; }, "Channel returned for empty chord on first get.");
            ClassicAssert.Throws<InvalidOperationException>(() => { var channel = chord.Channel; }, "Channel returned for empty chord on second get.");
        }

        [Test]
        public void GetChannel_ByNotes_OneNote()
        {
            var channel = (FourBitNumber)5;
            var chord = new Chord(new Note(SevenBitNumber.MaxValue) { Channel = channel });
            ClassicAssert.AreEqual(channel, chord.Channel, "Channel is invalid on first get.");
            ClassicAssert.AreEqual(channel, chord.Channel, "Channel is invalid on second get.");
        }

        [Test]
        public void GetChannel_ByNotes_MultipleNotes_SameChannel()
        {
            var channel = (FourBitNumber)5;
            var chord = new Chord(
                new Note(SevenBitNumber.MaxValue) { Channel = channel },
                new Note(SevenBitNumber.MinValue) { Channel = channel });
            ClassicAssert.AreEqual(channel, chord.Channel, "Channel is invalid on first get.");
            ClassicAssert.AreEqual(channel, chord.Channel, "Channel is invalid on second get.");
        }

        [Test]
        public void GetChannel_ByNotes_MultipleNotes_DifferentChannels()
        {
            var channel1 = (FourBitNumber)5;
            var channel2 = (FourBitNumber)3;
            var chord = new Chord(
                new Note(SevenBitNumber.MaxValue) { Channel = channel1 },
                new Note(SevenBitNumber.MinValue) { Channel = channel2 });
            ClassicAssert.Throws<InvalidOperationException>(() => { var channel = chord.Channel; }, "Channel returned for chord with notes with different channels on first get.");
            ClassicAssert.Throws<InvalidOperationException>(() => { var channel = chord.Channel; }, "Channel returned for chord with notes with different channels on second get.");
        }

        [Test]
        public void GetChannel_AfterSet_OneNote()
        {
            var channel = (FourBitNumber)5;
            var chord = new Chord(new Note(SevenBitNumber.MaxValue));
            chord.Channel = channel;

            ClassicAssert.AreEqual(channel, chord.Channel, "Channel is invalid on first get.");
            ClassicAssert.AreEqual(channel, chord.Channel, "Channel is invalid on second get.");
        }

        [Test]
        public void GetChannel_AfterSet_MultipleNotes()
        {
            var channel = (FourBitNumber)5;
            var chord = new Chord(
                new Note(SevenBitNumber.MaxValue),
                new Note(SevenBitNumber.MinValue));
            chord.Channel = channel;

            ClassicAssert.AreEqual(channel, chord.Channel, "Channel is invalid on first get.");
            ClassicAssert.AreEqual(channel, chord.Channel, "Channel is invalid on second get.");
        }

        [Test]
        public void GetChannel_AfterSet_MultipleNotes_DifferentChannels()
        {
            var channel1 = (FourBitNumber)5;
            var channel2 = (FourBitNumber)3;
            var chord = new Chord(
                new Note(SevenBitNumber.MaxValue) { Channel = channel1 },
                new Note(SevenBitNumber.MinValue) { Channel = channel2 });

            var channel = (FourBitNumber)7;
            chord.Channel = channel;

            ClassicAssert.AreEqual(channel, chord.Channel, "Channel is invalid on first get.");
            ClassicAssert.AreEqual(channel, chord.Channel, "Channel is invalid on second get.");
        }

        #endregion

        #region Velocity

        [Test]
        public void GetVelocity_EmptyChord()
        {
            var chord = new Chord();
            ClassicAssert.Throws<InvalidOperationException>(() => { var velocity = chord.Velocity; }, "Velocity returned for empty chord on first get.");
            ClassicAssert.Throws<InvalidOperationException>(() => { var velocity = chord.Velocity; }, "Velocity returned for empty chord on second get.");
        }

        [Test]
        public void GetVelocity_ByNotes_OneNote()
        {
            var velocity = (SevenBitNumber)5;
            var chord = new Chord(new Note(SevenBitNumber.MaxValue) { Velocity = velocity });
            ClassicAssert.AreEqual(velocity, chord.Velocity, "Velocity is invalid on first get.");
            ClassicAssert.AreEqual(velocity, chord.Velocity, "Velocity is invalid on second get.");
        }

        [Test]
        public void GetVelocity_ByNotes_MultipleNotes_SameVelocity()
        {
            var velocity = (SevenBitNumber)5;
            var chord = new Chord(
                new Note(SevenBitNumber.MaxValue) { Velocity = velocity },
                new Note(SevenBitNumber.MinValue) { Velocity = velocity });
            ClassicAssert.AreEqual(velocity, chord.Velocity, "Velocity is invalid on first get.");
            ClassicAssert.AreEqual(velocity, chord.Velocity, "Velocity is invalid on second get.");
        }

        [Test]
        public void GetVelocity_ByNotes_MultipleNotes_DifferentVelocities()
        {
            var velocity1 = (SevenBitNumber)5;
            var velocity2 = (SevenBitNumber)3;
            var chord = new Chord(
                new Note(SevenBitNumber.MaxValue) { Velocity = velocity1 },
                new Note(SevenBitNumber.MinValue) { Velocity = velocity2 });
            ClassicAssert.Throws<InvalidOperationException>(() => { var velocity = chord.Velocity; }, "Velocity returned for chord with notes with different velocities on first get.");
            ClassicAssert.Throws<InvalidOperationException>(() => { var velocity = chord.Velocity; }, "Velocity returned for chord with notes with different velocities on second get.");
        }

        [Test]
        public void GetVelocity_AfterSet_OneNote()
        {
            var velocity = (SevenBitNumber)5;
            var chord = new Chord(new Note(SevenBitNumber.MaxValue));
            chord.Velocity = velocity;

            ClassicAssert.AreEqual(velocity, chord.Velocity, "Velocity is invalid on first get.");
            ClassicAssert.AreEqual(velocity, chord.Velocity, "Velocity is invalid on second get.");
        }

        [Test]
        public void GetVelocity_AfterSet_MultipleNotes()
        {
            var velocity = (SevenBitNumber)5;
            var chord = new Chord(
                new Note(SevenBitNumber.MaxValue),
                new Note(SevenBitNumber.MinValue));
            chord.Velocity = velocity;

            ClassicAssert.AreEqual(velocity, chord.Velocity, "Velocity is invalid on first get.");
            ClassicAssert.AreEqual(velocity, chord.Velocity, "Velocity is invalid on second get.");
        }

        [Test]
        public void GetVelocity_AfterSet_MultipleNotes_DifferentVelocities()
        {
            var velocity1 = (SevenBitNumber)5;
            var velocity2 = (SevenBitNumber)3;
            var chord = new Chord(
                new Note(SevenBitNumber.MaxValue) { Velocity = velocity1 },
                new Note(SevenBitNumber.MinValue) { Velocity = velocity2 });

            var velocity = (SevenBitNumber)7;
            chord.Velocity = velocity;

            ClassicAssert.AreEqual(velocity, chord.Velocity, "Velocity is invalid on first get.");
            ClassicAssert.AreEqual(velocity, chord.Velocity, "Velocity is invalid on second get.");
        }

        #endregion

        #region OffVelocity

        [Test]
        public void GetOffVelocity_EmptyChord()
        {
            var chord = new Chord();
            ClassicAssert.Throws<InvalidOperationException>(() => { var offVelocity = chord.OffVelocity; }, "Off-velocity returned for empty chord on first get.");
            ClassicAssert.Throws<InvalidOperationException>(() => { var offVelocity = chord.OffVelocity; }, "Off-velocity returned for empty chord on second get.");
        }

        [Test]
        public void GetOffVelocity_ByNotes_OneNote()
        {
            var offVelocity = (SevenBitNumber)5;
            var chord = new Chord(new Note(SevenBitNumber.MaxValue) { OffVelocity = offVelocity });
            ClassicAssert.AreEqual(offVelocity, chord.OffVelocity, "Off-velocity is invalid on first get.");
            ClassicAssert.AreEqual(offVelocity, chord.OffVelocity, "Off-velocity is invalid on second get.");
        }

        [Test]
        public void GetOffVelocity_ByNotes_MultipleNotes_SameOffVelocity()
        {
            var offVelocity = (SevenBitNumber)5;
            var chord = new Chord(
                new Note(SevenBitNumber.MaxValue) { OffVelocity = offVelocity },
                new Note(SevenBitNumber.MinValue) { OffVelocity = offVelocity });
            ClassicAssert.AreEqual(offVelocity, chord.OffVelocity, "Off-velocity is invalid on first get.");
            ClassicAssert.AreEqual(offVelocity, chord.OffVelocity, "Off-velocity is invalid on second get.");
        }

        [Test]
        public void GetOffVelocity_ByNotes_MultipleNotes_DifferentOffVelocities()
        {
            var offVelocity1 = (SevenBitNumber)5;
            var offVelocity2 = (SevenBitNumber)3;
            var chord = new Chord(
                new Note(SevenBitNumber.MaxValue) { OffVelocity = offVelocity1 },
                new Note(SevenBitNumber.MinValue) { OffVelocity = offVelocity2 });
            ClassicAssert.Throws<InvalidOperationException>(() => { var offVelocity = chord.OffVelocity; }, "Off-velocity returned for chord with notes with different off-velocities on first get.");
            ClassicAssert.Throws<InvalidOperationException>(() => { var offVelocity = chord.OffVelocity; }, "Off-velocity returned for chord with notes with different off-velocities on second get.");
        }

        [Test]
        public void GetOffVelocity_AfterSet_OneNote()
        {
            var offVelocity = (SevenBitNumber)5;
            var chord = new Chord(new Note(SevenBitNumber.MaxValue));
            chord.OffVelocity = offVelocity;

            ClassicAssert.AreEqual(offVelocity, chord.OffVelocity, "Off-velocity is invalid on first get.");
            ClassicAssert.AreEqual(offVelocity, chord.OffVelocity, "Off-velocity is invalid on second get.");
        }

        [Test]
        public void GetOffVelocity_AfterSet_MultipleNotes()
        {
            var offVelocity = (SevenBitNumber)5;
            var chord = new Chord(
                new Note(SevenBitNumber.MaxValue),
                new Note(SevenBitNumber.MinValue));
            chord.OffVelocity = offVelocity;

            ClassicAssert.AreEqual(offVelocity, chord.OffVelocity, "Off-velocity is invalid on first get.");
            ClassicAssert.AreEqual(offVelocity, chord.OffVelocity, "Off-velocity is invalid on second get.");
        }

        [Test]
        public void GetOffVelocity_AfterSet_MultipleNotes_DifferentOffVelocities()
        {
            var offVelocity1 = (SevenBitNumber)5;
            var offVelocity2 = (SevenBitNumber)3;
            var chord = new Chord(
                new Note(SevenBitNumber.MaxValue) { OffVelocity = offVelocity1 },
                new Note(SevenBitNumber.MinValue) { OffVelocity = offVelocity2 });

            var offVelocity = (SevenBitNumber)7;
            chord.OffVelocity = offVelocity;

            ClassicAssert.AreEqual(offVelocity, chord.OffVelocity, "Off-velocity is invalid on first get.");
            ClassicAssert.AreEqual(offVelocity, chord.OffVelocity, "Off-velocity is invalid on second get.");
        }

        #endregion

        #region Properties

        [Test]
        public void CheckChordChannel()
        {
            var initialChannel = (FourBitNumber)0;

            var chord = new Chord(new Note((SevenBitNumber)70), new Note((SevenBitNumber)80));
            ClassicAssert.AreEqual(initialChannel, chord.Channel, "Invalid channel after chord created.");

            foreach (var note in chord.Notes)
            {
                ClassicAssert.AreEqual(initialChannel, ((NoteOnEvent)note.TimedNoteOnEvent.Event).Channel, "Invalid channel of Note On timed event after chord created.");
                ClassicAssert.AreEqual(initialChannel, ((NoteOffEvent)note.TimedNoteOffEvent.Event).Channel, "Invalid channel of Note Off timed event after chord created.");
                MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after chord created.");
                MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after chord created.");
            }

            var newChannel = (FourBitNumber)6;
            chord.Channel = newChannel;

            ClassicAssert.AreEqual(newChannel, chord.Channel, "Invalid channel after update.");

            foreach (var note in chord.Notes)
            {
                ClassicAssert.AreEqual(newChannel, ((NoteOnEvent)note.TimedNoteOnEvent.Event).Channel, "Invalid channel of Note On timed event after update.");
                ClassicAssert.AreEqual(newChannel, ((NoteOffEvent)note.TimedNoteOffEvent.Event).Channel, "Invalid channel of Note Off timed event after update.");
                MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after update.");
                MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after update.");
            }
        }

        [Test]
        public void CheckChordVelocity()
        {
            var initialVelocity = Note.DefaultVelocity;

            var chord = new Chord(new Note((SevenBitNumber)70), new Note((SevenBitNumber)80));
            ClassicAssert.AreEqual(initialVelocity, chord.Velocity, "Invalid velocity after chord created.");

            foreach (var note in chord.Notes)
            {
                ClassicAssert.AreEqual(initialVelocity, ((NoteOnEvent)note.TimedNoteOnEvent.Event).Velocity, "Invalid velocity of Note On timed event after chord created.");
                ClassicAssert.AreEqual(SevenBitNumber.MinValue, ((NoteOffEvent)note.TimedNoteOffEvent.Event).Velocity, "Invalid velocity of Note Off timed event after chord created.");
                MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after chord created.");
                MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after chord created.");
            }

            var newVelocity = (SevenBitNumber)60;
            chord.Velocity = newVelocity;

            ClassicAssert.AreEqual(newVelocity, chord.Velocity, "Invalid velocity after update.");

            foreach (var note in chord.Notes)
            {
                ClassicAssert.AreEqual(newVelocity, ((NoteOnEvent)note.TimedNoteOnEvent.Event).Velocity, "Invalid velocity of Note On timed event after update.");
                ClassicAssert.AreEqual(SevenBitNumber.MinValue, ((NoteOffEvent)note.TimedNoteOffEvent.Event).Velocity, "Invalid velocity of Note Off timed event after update.");
                MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after update.");
                MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after update.");
            }
        }

        [Test]
        public void CheckChordOffVelocity()
        {
            var initialOffVelocity = SevenBitNumber.MinValue;

            var chord = new Chord(new Note((SevenBitNumber)70), new Note((SevenBitNumber)80));
            ClassicAssert.AreEqual(initialOffVelocity, chord.OffVelocity, "Invalid off velocity after chord created.");

            foreach (var note in chord.Notes)
            {
                ClassicAssert.AreEqual(Note.DefaultVelocity, ((NoteOnEvent)note.TimedNoteOnEvent.Event).Velocity, "Invalid off velocity of Note On timed event after chord created.");
                ClassicAssert.AreEqual(initialOffVelocity, ((NoteOffEvent)note.TimedNoteOffEvent.Event).Velocity, "Invalid off velocity of Note Off timed event after chord created.");
                MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after chord created.");
                MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after chord created.");
            }

            var newOffVelocity = (SevenBitNumber)60;
            chord.OffVelocity = newOffVelocity;

            ClassicAssert.AreEqual(newOffVelocity, chord.OffVelocity, "Invalid off velocity after update.");

            foreach (var note in chord.Notes)
            {
                ClassicAssert.AreEqual(Note.DefaultVelocity, ((NoteOnEvent)note.TimedNoteOnEvent.Event).Velocity, "Invalid off velocity of Note On timed event after update.");
                ClassicAssert.AreEqual(newOffVelocity, ((NoteOffEvent)note.TimedNoteOffEvent.Event).Velocity, "Invalid off velocity of Note Off timed event after update.");
                MidiAsserts.AreEqual(note.TimedNoteOnEvent, note.GetTimedNoteOnEvent(), "Invalid Note On timed event after update.");
                MidiAsserts.AreEqual(note.TimedNoteOffEvent, note.GetTimedNoteOffEvent(), "Invalid Note Off timed event after update.");
            }

        }

        #endregion

        #region GetMusicTheoryChord

        [Test]
        public void GetMusicTheoryChord()
        {
            var chord = new Chord(
                new Note(DryWetMidi.MusicTheory.NoteName.C, 2),
                new Note(DryWetMidi.MusicTheory.NoteName.A, 1),
                new Note(DryWetMidi.MusicTheory.NoteName.DSharp, 2));

            ClassicAssert.AreEqual(
                new DryWetMidi.MusicTheory.Chord(new[]
                {
                    DryWetMidi.MusicTheory.NoteName.A,
                    DryWetMidi.MusicTheory.NoteName.C,
                    DryWetMidi.MusicTheory.NoteName.DSharp
                }),
                chord.GetMusicTheoryChord(),
                "Chord is invalid.");
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

            ClassicAssert.IsFalse(lengthChangedFired, "Length changed event fired.");
            ClassicAssert.IsNull(timeChangedSender, "Sender is not null.");
            ClassicAssert.IsNull(timeChangedEventArgs, "Event args is not null.");
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

            ClassicAssert.IsFalse(lengthChangedFired, "Length changed event fired.");
            ClassicAssert.AreSame(chord, timeChangedSender, "Sender is invalid.");
            ClassicAssert.IsNotNull(timeChangedEventArgs, "Event args is null.");
            ClassicAssert.AreEqual(oldTime, timeChangedEventArgs.OldTime, "Old time is invalid.");
            ClassicAssert.AreEqual(chord.Time, timeChangedEventArgs.NewTime, "New time is invalid.");
            ClassicAssert.AreNotEqual(oldTime, chord.Time, "New time is equal to old one.");
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

            ClassicAssert.IsFalse(timeChangedFired, "Time changed event fired.");
            ClassicAssert.IsNull(lengthChangedSender, "Sender is not null.");
            ClassicAssert.IsNull(lengthChangedEventArgs, "Event args is not null.");
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

            ClassicAssert.IsFalse(timeChangedFired, "Time changed event fired.");
            ClassicAssert.AreSame(chord, lengthChangedSender, "Sender is invalid.");
            ClassicAssert.IsNotNull(lengthChangedEventArgs, "Event args is null.");
            ClassicAssert.AreEqual(oldLength, lengthChangedEventArgs.OldLength, "Old length is invalid.");
            ClassicAssert.AreEqual(chord.Length, lengthChangedEventArgs.NewLength, "New length is invalid.");
            ClassicAssert.AreNotEqual(oldLength, chord.Length, "New length is equal to old one.");
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
