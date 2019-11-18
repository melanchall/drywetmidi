using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// A comparer for playback events.
    /// </summary>
    /// <seealso cref="IComparer{T}"/>
    /// <seealso cref="PlaybackEvent"/>
    public sealed class PlaybackEventsComparer : IComparer<PlaybackEvent>
    {
        #region IComparer<PlaybackEvent>

        /// <summary>
        /// Compares two playback events and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>A signed integer that indicates the relative values of <paramref name="x">x</paramref> and
        /// <paramref name="y">y</paramref>. If less than 0, <paramref name="x">x</paramref> is less than
        /// <paramref name="y">y</paramref>. If 0, <paramref name="x">x</paramref> equals
        /// <paramref name="y">y</paramref>. If greater than 0, <paramref name="x">x</paramref> is greater than
        /// <paramref name="y">y</paramref>.</returns>
        public int Compare(PlaybackEvent x, PlaybackEvent y)
        {
            var timeDifference = x.RawTime - y.RawTime;
            if (timeDifference != 0)
                return Math.Sign(timeDifference);

            var xChannelEvent = x.Event as ChannelEvent;
            var yChannelEvent = y.Event as ChannelEvent;

            if (xChannelEvent == null || yChannelEvent == null)
                return 0;

            if (!(xChannelEvent is NoteEvent) && yChannelEvent is NoteEvent)
                return -1;

            if (xChannelEvent is NoteEvent && !(yChannelEvent is NoteEvent))
                return 1;

            return 0;
        }

        #endregion
    }
}
