using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class MidiFileSplitterTests
    {
        #region Private methods

        private static void CompareTimedEvents(
            IEnumerable<TimedEvent> actualTimedEvents,
            IEnumerable<TimedEvent> expectedTimedEvents,
            string message)
        {
            Assert.IsTrue(TimedEventEquality.AreEqual(
                actualTimedEvents,
                expectedTimedEvents,
                false),
                message);
        }

        private static void CompareTimedEventsSplittingByGrid(
            TimedEvent[][] inputTimedEvents,
            IGrid grid,
            SliceMidiFileSettings settings,
            TimedEvent[][] outputTimedEvents)
        {
            var trackChunks = inputTimedEvents.Select(e => e.ToTrackChunk());
            var midiFile = new MidiFile(trackChunks);

            var newFiles = midiFile.SplitByGrid(grid, settings).ToList();
            Assert.AreEqual(outputTimedEvents.Length, newFiles.Count, "New files count is invalid.");

            for (var i = 0; i < outputTimedEvents.Length; i++)
            {
                CompareTimedEvents(
                    newFiles[i].GetTimedEvents(),
                    outputTimedEvents[i],
                    $"File {i} contains invalid events.");
            }
        }

        #endregion
    }
}
