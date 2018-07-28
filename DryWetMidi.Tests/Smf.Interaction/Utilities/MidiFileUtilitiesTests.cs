using System.Linq;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestFixture]
    public sealed class MidiFileUtilitiesTests
    {
        #region Test methods

        [Test]
        public void ShiftEvents_ValidFiles_Midi()
        {
            var distance = 10000;

            foreach (var filePath in TestFilesProvider.GetValidFiles())
            {
                var midiFile = MidiFile.Read(filePath);
                var originalTimes = midiFile.GetTimedEvents().Select(e => e.Time).ToList();

                midiFile.ShiftEvents((MidiTimeSpan)distance);
                var newTimes = midiFile.GetTimedEvents().Select(e => e.Time).ToList();

                Assert.IsTrue(midiFile.GetTimedEvents().All(e => e.Time >= distance), "Some events are not shifted.");
                CollectionAssert.AreEqual(originalTimes, newTimes.Select(t => t - distance));
            }
        }

        [Test]
        public void ShiftEvents_ValidFiles_Metric()
        {
            var distance = new MetricTimeSpan(0, 1, 0);

            foreach (var filePath in TestFilesProvider.GetValidFiles())
            {
                var midiFile = MidiFile.Read(filePath);
                midiFile.ShiftEvents(distance);

                var tempoMap = midiFile.GetTempoMap();

                Assert.IsTrue(midiFile.GetTimedEvents()
                                      .Select(e => e.TimeAs<MetricTimeSpan>(tempoMap).CompareTo(distance))
                                      .All(t => t >= 0),
                              "Some events are not shifted.");
            }
        }

        #endregion
    }
}
