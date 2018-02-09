using System;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class ChordUtilitiesTests
    {
        #region Test methods

        [Test]
        [Description("Get inversion of the empty chord.")]
        public void GetInversion_EmptyChord()
        {
            var chord = GetEmptyChord();

            Assert.Throws<ArgumentOutOfRangeException>(() => chord.GetInversion(2));
        }

        [Test]
        [Description("Get inversion of unallowable number (below lower limit).")]
        public void GetInversion_OutOfRange_Below()
        {
            var chord = GetChord_CMajorTriad();

            Assert.Throws<ArgumentOutOfRangeException>(() => chord.GetInversion(0));
        }

        [Test]
        [Description("Get inversion of unallowable number (above upper limit).")]
        public void GetInversion_OutOfRange_Above()
        {
            var chord = GetChord_CMajorTriad();

            Assert.Throws<ArgumentOutOfRangeException>(() => chord.GetInversion(3));
        }

        [Test]
        [Description("Get first inversion.")]
        public void GetInversion_First()
        {
            var chord = GetChord_CMajorTriad();

            Assert.AreEqual(new Chord(NoteName.E, NoteName.G, NoteName.C), chord.GetInversion(1));
        }

        [Test]
        [Description("Get second inversion.")]
        public void GetInversion_Second()
        {
            var chord = GetChord_CMajorTriad();

            Assert.AreEqual(new Chord(NoteName.G, NoteName.C, NoteName.E), chord.GetInversion(2));
        }

        [Test]
        [Description("Get inversions.")]
        public void GetInversions()
        {
            var chord = GetChord_CMajorTriad();

            CollectionAssert.AreEqual(new[]
                {
                    new Chord(NoteName.E, NoteName.G, NoteName.C),
                    new Chord(NoteName.G, NoteName.C, NoteName.E),
                },
                chord.GetInversions());
        }

        #endregion

        #region Private methods

        private static Chord GetEmptyChord()
        {
            return new Chord();
        }

        private static Chord GetChord_CMajorTriad()
        {
            return new Chord(NoteName.C,
                             NoteName.E,
                             NoteName.G);
        }

        #endregion
    }
}
