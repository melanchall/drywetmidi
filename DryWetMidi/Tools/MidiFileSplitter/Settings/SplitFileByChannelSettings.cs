using System;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Defines how a MIDI file should be split by channel using
    /// <see cref="MidiFileSplitter.SplitByChannel(Core.MidiFile, SplitFileByChannelSettings)"/> method.
    /// </summary>
    public sealed class SplitFileByChannelSettings
    {
        #region Properties

        /// Gets or sets a value indicating whether to copy all meta and system exclusive events
        /// to all the new files or throw them away. The default value is <c>true</c>>.
        public bool CopyNonChannelEventsToEachFile { get; set; } = true;

        /// <summary>
        /// Gets or sets a predicate to filter events out before processing. If predicate returns <c>true</c>,
        /// an event will be processed; if <c>false</c> - it won't. If the property set to <c>null</c>,
        /// all MIDI events will be processed.
        /// </summary>
        public Predicate<TimedEvent> Filter { get; set; }

        #endregion
    }
}
