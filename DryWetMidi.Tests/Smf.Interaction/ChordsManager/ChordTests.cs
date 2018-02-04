using System;
using System.Linq.Expressions;
using System.Reflection;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
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

            [TestMethod]
            [Description("Get property value of a chord without notes.")]
            public void Get_NoNotes()
            {
                var chord = new Chord();

                Assert.ThrowsException<InvalidOperationException>(() => GetPropertyValue(_chordPropertySelector, chord));
            }

            [TestMethod]
            [Description("Get property value of a chord containing notes with the same value of the specified property.")]
            public void Get_SameValues()
            {
                var chord = GetChord(_notePropertySelector, _value1, _value1);

                Assert.AreEqual(_value1, GetPropertyValue(_chordPropertySelector, chord));
            }

            [TestMethod]
            [Description("Get property value of a chord containing notes with different values of the specified property.")]
            public void Get_DifferentValues()
            {
                var chord = GetChord(_notePropertySelector, _value1, _value2);

                Assert.ThrowsException<InvalidOperationException>(() => GetPropertyValue(_chordPropertySelector, chord));
            }

            [TestMethod]
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

        [TestClass]
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

        [TestClass]
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

        [TestClass]
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

        #region Test methods

        [TestMethod]
        [Description("Get length of Chord without notes.")]
        public void Length_Get_NoNotes()
        {
            Assert.AreEqual(0, GetChord_NoNotes().Length);
        }

        [TestMethod]
        [Description("Get length of Chord with time of zero.")]
        public void Length_Get_ZeroTime()
        {
            Assert.AreEqual(200, GetChord_ZeroTime().Length);
        }

        [TestMethod]
        [Description("Get length of Chord with time of nonzero number.")]
        public void Length_Get_NonzeroTime()
        {
            Assert.AreEqual(200, GetChord_NonzeroTime().Length);
        }

        [TestMethod]
        [Description("Set length of Chord without notes.")]
        public void Length_Set_NoNotes()
        {
            var chord = GetChord_NoNotes();
            chord.Length = 100;

            Assert.AreEqual(0, chord.Length);
        }

        [TestMethod]
        [Description("Set length of Chord with time of zero.")]
        public void Length_Set_ZeroTime()
        {
            var chord = GetChord_ZeroTime();
            chord.Length = 500;

            Assert.AreEqual(500, chord.Length);
        }

        [TestMethod]
        [Description("Set length of Chord with time of nonzero number.")]
        public void Length_Set_NonzeroTime()
        {
            var chord = GetChord_NonzeroTime();
            chord.Length = 500;

            Assert.AreEqual(500, chord.Length);
        }

        #endregion

        #region Private methods

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
