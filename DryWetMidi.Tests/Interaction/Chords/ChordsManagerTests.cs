using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class ChordsManagerTests
    {
        #region Constants

        private static readonly ChordMethods ChordMethods = new ChordMethods();

        #endregion

        #region Test methods

        [Test]
        [Description("Check that ChordsCollection is sorted when enumerated.")]
        public void Enumeration_Sorted()
        {
            using (var chordsManager = new TrackChunk().ManageChords())
            {
                var chords = chordsManager.Chords;

                chords.Add(ChordTestUtilities.GetChordByTime(123));
                chords.Add(ChordTestUtilities.GetChordByTime(1));
                chords.Add(ChordTestUtilities.GetChordByTime(10));
                chords.Add(ChordTestUtilities.GetChordByTime(45));

                TimedObjectsCollectionTestUtilities.CheckTimedObjectsCollectionTimes(chords, 1, 10, 45, 123);
            }
        }

        [Test]
        public void ChordsByNotes_EmptyCollection()
        {
            var notes = Enumerable.Empty<Note>();
            var chords = notes.GetChords();
            ChordMethods.AssertCollectionsAreEqual(Enumerable.Empty<Chord>(), chords);
        }

        [Test]
        public void ChordsByNotes_SingleChannel()
        {
            var notes = new[]
            {
                new Note((SevenBitNumber)100, 100, 0),
                new Note((SevenBitNumber)120, 100, 0),
                new Note((SevenBitNumber)100, 100, 10),
                new Note((SevenBitNumber)30, 100, 10)
            };
            var chords = notes.GetChords();
            ChordMethods.AssertCollectionsAreEqual(
                new[]
                {
                    new Chord(
                        new Note((SevenBitNumber)100, 100, 0),
                        new Note((SevenBitNumber)120, 100, 0)),
                    new Chord(
                        new Note((SevenBitNumber)100, 100, 10),
                        new Note((SevenBitNumber)30, 100, 10))
                },
                chords);
        }

        [Test]
        public void ChordsByNotes_MultipleChannels()
        {
            var notes = new[]
            {
                new Note((SevenBitNumber)1, 100, 0) { Channel = (FourBitNumber)10 },
                new Note((SevenBitNumber)2, 100, 0) { Channel = (FourBitNumber)10 },
                new Note((SevenBitNumber)3, 100, 100) { Channel = (FourBitNumber)10 },
                new Note((SevenBitNumber)4, 100, 100) { Channel = (FourBitNumber)10 },
                new Note((SevenBitNumber)5, 100, 1000),
                new Note((SevenBitNumber)6, 100, 1000)
            };
            var chords = notes.GetChords();
            ChordMethods.AssertCollectionsAreEqual(
                new[]
                {
                    new Chord(
                        new Note((SevenBitNumber)1, 100, 0) { Channel = (FourBitNumber)10 },
                        new Note((SevenBitNumber)2, 100, 0) { Channel = (FourBitNumber)10 }),
                    new Chord(
                        new Note((SevenBitNumber)3, 100, 100) { Channel = (FourBitNumber)10 },
                        new Note((SevenBitNumber)4, 100, 100) { Channel = (FourBitNumber)10 }),
                    new Chord(
                        new Note((SevenBitNumber)5, 100, 1000),
                        new Note((SevenBitNumber)6, 100, 1000))
                },
                chords);
        }

        #endregion
    }
}
