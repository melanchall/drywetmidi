using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public sealed class TimedEventsManagerTests
    {
        #region Test methods

        [TestMethod]
        [Description("Check that TimedObjectsCollection is sorted when enumerated.")]
        public void Enumeration_Sorted()
        {
            using (var timedEventsManager = new TrackChunk().ManageTimedEvents())
            {
                var events = timedEventsManager.Events;

                events.AddEvent(new NoteOnEvent(), 123);
                events.AddEvent(new NoteOnEvent(), 1);
                events.AddEvent(new NoteOnEvent(), 10);
                events.AddEvent(new NoteOnEvent(), 45);

                TimedObjectsCollectionTestUtilities.CheckTimedObjectsCollectionTimes(events, 1, 10, 45, 123);
            }
        }

        #endregion
    }
}
