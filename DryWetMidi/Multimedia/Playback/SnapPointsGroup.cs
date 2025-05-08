using Melanchall.DryWetMidi.Core;
using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Represents a group of snap points selected by a predicate
    /// (see <see cref="Playback.SnapToEvents(Predicate{MidiEvent})"/>).
    /// </summary>
    public sealed class SnapPointsGroup
    {
        #region Constructor

        internal SnapPointsGroup(Predicate<MidiEvent> predicate)
        {
            Predicate = predicate;
        }

        #endregion

        #region Properties

        internal Predicate<MidiEvent> Predicate { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the current <see cref="SnapPointsGroup"/> is enabled or not.
        /// If <c>false</c>, the group won't take part in the snap points selection process
        /// (for example, during the <see cref="Playback.MoveToPreviousSnapPoint()"/> method).
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        #endregion
    }
}
