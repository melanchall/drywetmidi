using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class TrackChunkUtilities
    {
        #region Methods

        public static void ShiftEvents(this TrackChunk trackChunk, ITimeSpan distance, TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            var convertedDistance = TimeConverter.ConvertFrom(distance, TempoMap.Create(timeDivision));

            var firstEvent = trackChunk.Events.FirstOrDefault();
            if (firstEvent == null)
                return;

            firstEvent.DeltaTime += convertedDistance;
        }

        public static void ShiftEvents(this IEnumerable<TrackChunk> trackChunks, ITimeSpan distance, TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.ShiftEvents(distance, timeDivision);
            }
        }

        #endregion
    }
}
