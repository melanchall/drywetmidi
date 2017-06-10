using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class TempoMapManagingUtilities
    {
        #region Methods

        public static TempoMapManager ManageTempoMap(this IEnumerable<EventsCollection> eventsCollections, TimeDivision timeDivision)
        {
            if (eventsCollections == null)
                throw new ArgumentNullException(nameof(eventsCollections));

            if (timeDivision == null)
                throw new ArgumentNullException(nameof(timeDivision));

            return new TempoMapManager(timeDivision, eventsCollections);
        }

        public static TempoMapManager ManageTempoMap(this IEnumerable<TrackChunk> trackChunks, TimeDivision timeDivision)
        {
            if (trackChunks == null)
                throw new ArgumentNullException(nameof(trackChunks));

            if (timeDivision == null)
                throw new ArgumentNullException(nameof(timeDivision));

            return trackChunks.Select(c => c.Events).ManageTempoMap(timeDivision);
        }

        public static TempoMapManager ManageTempoMap(this MidiFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            return file.Chunks.OfType<TrackChunk>().ManageTempoMap(file.TimeDivision);
        }

        public static TempoMap GetTempoMap(this IEnumerable<EventsCollection> eventsCollections, TimeDivision timeDivision)
        {
            if (eventsCollections == null)
                throw new ArgumentNullException(nameof(eventsCollections));

            if (timeDivision == null)
                throw new ArgumentNullException(nameof(timeDivision));

            return eventsCollections.ManageTempoMap(timeDivision).TempoMap;
        }

        public static TempoMap GetTempoMap(this IEnumerable<TrackChunk> trackChunks, TimeDivision timeDivision)
        {
            if (trackChunks == null)
                throw new ArgumentNullException(nameof(trackChunks));

            if (timeDivision == null)
                throw new ArgumentNullException(nameof(timeDivision));

            return trackChunks.ManageTempoMap(timeDivision).TempoMap;
        }

        public static TempoMap GetTempoMap(this MidiFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            return file.ManageTempoMap().TempoMap;
        }

        #endregion
    }
}
