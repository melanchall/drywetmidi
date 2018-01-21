using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public sealed class ChordsManagerTests
    {
        #region Test methods

        [TestMethod]
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

        #endregion
    }
}
