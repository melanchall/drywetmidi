namespace Melanchall.DryWetMidi.Core
{
    internal static class EventsCollectionEquality
    {
        #region Methods

        public static bool Equals(EventsCollection eventsCollection1, EventsCollection eventsCollection2, MidiEventEqualityCheckSettings settings, out string message)
        {
            message = null;

            if (ReferenceEquals(eventsCollection1, eventsCollection2))
                return true;

            if (ReferenceEquals(null, eventsCollection1) || ReferenceEquals(null, eventsCollection2))
            {
                message = "One of events collections is null.";
                return false;
            }

            if (eventsCollection1.Count != eventsCollection2.Count)
            {
                message = $"Counts of events are different ({eventsCollection1.Count} vs {eventsCollection2.Count}).";
                return false;
            }

            for (var i = 0; i < eventsCollection1.Count; i++)
            {
                var event1 = eventsCollection1[i];
                var event2 = eventsCollection2[i];

                string eventsComparingMessage;
                if (!MidiEvent.Equals(event1, event2, settings, out eventsComparingMessage))
                {
                    message = $"Events at position {i} are different. {eventsComparingMessage}";
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
