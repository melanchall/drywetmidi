using System;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Defines how a MIDI file should be split by channel using
    /// <see cref="Splitter.SplitByChannel(MidiFile, SplitFileByChannelSettings)"/> method.
    /// </summary>
    /// <seealso cref="Splitter"/>
    public sealed class SplitFileByChannelSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to copy all meta and system exclusive events
        /// to all the new files or throw them away. The default value is <c>true</c>>.
        /// </summary>
        public bool CopyNonChannelEventsToEachFile { get; set; } = true;

        /// <summary>
        /// Gets or sets a predicate to filter events out before processing. If predicate returns <c>true</c>,
        /// an event will be processed; if <c>false</c> - it won't. If the property set to <c>null</c>
        /// (default value), all MIDI events will be processed.
        /// </summary>
        public Predicate<TimedEvent> Filter { get; set; }

        #endregion
    }
}
