using System;
using System.Threading;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public sealed class OctaveTests
    {
        #region Test methods

        [TestCase("-2")]
        [TestCase("23")]
        public void Parse_Invalid(string input)
        {
            ClassicAssert.Throws<FormatException>(() => Octave.Parse(input), "Invalid octave parsed.");
        }

        [TestCase("5", 5)]
        [TestCase("3", 3)]
        [TestCase("-1", -1)]
        [TestCase("9", 9)]
        public void Parse(string input, int expectedOctaveNumber)
        {
            var parsedOctave = Octave.Parse(input);
            var expectedOctave = Octave.Get(expectedOctaveNumber);
            ClassicAssert.AreEqual(expectedOctave, parsedOctave, "Parsed octave is invalid.");
        }

        [Test]
        public void GetOctavesFromDifferentThreads()
        {
            var minOctaveNumber = Octave.MinOctaveNumber;
            var maxOctaveNumber = Octave.MinOctaveNumber;

            var thread1 = new Thread(() =>
            {
                for (var octaveNumber = minOctaveNumber; octaveNumber <= maxOctaveNumber; octaveNumber++)
                {
                    var octave = Octave.Get(octaveNumber);
                }
            });

            var thread2 = new Thread(() =>
            {
                for (var octaveNumber = maxOctaveNumber; octaveNumber >= minOctaveNumber; octaveNumber--)
                {
                    var octave = Octave.Get(octaveNumber);
                }
            });

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();
        }

        #endregion
    }
}
