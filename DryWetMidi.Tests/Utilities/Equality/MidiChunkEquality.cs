using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class MidiChunkEquality
    {
        #region Constants

        private static readonly Dictionary<Type, Func<MidiChunk, MidiChunk, bool>> Comparers =
            new Dictionary<Type, Func<MidiChunk, MidiChunk, bool>>
            {
                [typeof(TrackChunk)] = (c1, c2) =>
                {
                    var events1 = ((TrackChunk)c1).Events;
                    var events2 = ((TrackChunk)c2).Events;

                    if (events1.Count != events2.Count)
                        return false;

                    return events1.Zip(events2, (e1, e2) => new { Event1 = e1, Event2 = e2 })
                                  .All(e => MidiEventEquality.AreEqual(e.Event1, e.Event2, true));
                },
                [typeof(UnknownChunk)] = (c1, c2) =>
                {
                    var unknownChunk1 = (UnknownChunk)c1;
                    var unknownChunk2 = (UnknownChunk)c2;

                    if (unknownChunk1.ChunkId != unknownChunk2.ChunkId)
                        return false;

                    return ArrayEquality.AreEqual(unknownChunk1.Data, unknownChunk2.Data);
                }
            };

        #endregion

        #region Methods

        public static bool AreEqual(MidiChunk chunk1, MidiChunk chunk2)
        {
            if (ReferenceEquals(chunk1, chunk2))
                return true;

            if (ReferenceEquals(null, chunk1) || ReferenceEquals(null, chunk2))
                return false;

            if (chunk1.GetType() != chunk2.GetType())
                return false;

            Func<MidiChunk, MidiChunk, bool> comparer;
            if (Comparers.TryGetValue(chunk1.GetType(), out comparer))
                return comparer(chunk1, chunk2);

            return true;
        }

        #endregion
    }
}
