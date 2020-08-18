namespace Melanchall.DryWetMidi.Core
{
    internal static class MidiChunkEquality
    {
        #region Methods

        public static bool Equals(MidiChunk midiChunk1, MidiChunk midiChunk2, MidiChunkEqualityCheckSettings settings, out string message)
        {
            message = null;

            if (ReferenceEquals(midiChunk1, midiChunk2))
                return true;

            if (ReferenceEquals(null, midiChunk1) || ReferenceEquals(null, midiChunk2))
            {
                message = "One of chunks is null.";
                return false;
            }

            var type1 = midiChunk1.GetType();
            var type2 = midiChunk2.GetType();
            if (type1 != type2)
            {
                message = $"Types of chunks are different ({type1} vs {type2}).";
                return false;
            }

            var trackChunk1 = midiChunk1 as TrackChunk;
            if (trackChunk1 != null)
            {
                var trackChunk2 = (TrackChunk)midiChunk2;

                var events1 = trackChunk1.Events;
                var events2 = trackChunk2.Events;

                if (events1.Count != events2.Count)
                {
                    message = $"Counts of events in track chunks are different ({events1.Count} vs {events2.Count}).";
                    return false;
                }

                for (var i = 0; i < events1.Count; i++)
                {
                    var event1 = events1[i];
                    var event2 = events2[i];

                    string eventsComparingMessage;
                    if (!MidiEvent.Equals(event1, event2, settings.EventEqualityCheckSettings, out eventsComparingMessage))
                    {
                        message = $"Events at position {i} in track chunks are different. {eventsComparingMessage}";
                        return false;
                    }
                }

                return true;
            }

            var unknownChunk1 = midiChunk1 as UnknownChunk;
            if (unknownChunk1 != null)
            {
                var unknownChunk2 = (UnknownChunk)midiChunk2;

                var chunkId1 = unknownChunk1.ChunkId;
                var chunkId2 = unknownChunk2.ChunkId;

                if (chunkId1 != chunkId2)
                {
                    message = $"IDs of unknown chunks are different ({chunkId1} vs {chunkId2}).";
                    return false;
                }

                if (!ArrayUtilities.Equals(unknownChunk1.Data, unknownChunk2.Data))
                {
                    message = "Unknown chunks data are different.";
                    return false;
                }

                return true;
            }

            var result = midiChunk1.Equals(midiChunk2);
            if (!result)
                message = $"Chunks {midiChunk1} and {midiChunk2} are not equal by result of Equals call on first chunk.";

            return result;
        }

        #endregion
    }
}
