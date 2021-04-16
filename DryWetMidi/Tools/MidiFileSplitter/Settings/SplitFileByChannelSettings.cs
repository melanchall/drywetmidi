using System;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Defines how a MIDI file should be split by channel using
    /// <see cref="MidiFileSplitter.SplitByChannel(Core.MidiFile, SplitFileByChannelSettings)"/> method.
    /// </summary>
    /// <seealso cref="MidiFileSplitter"/>
    public sealed class SplitFileByChannelSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to copy all meta and system exclusive events
        /// to all the new files or throw them away. The default value is <c>true</c>>.
        /// </summary>
        /// <example>
        /// <para>
        /// Given a MIDI file with two track chunks:
        /// </para>
        /// <code language="image">
        /// +-------------------+
        /// |┌─────────────────┐|
        /// |│. □ .  ○ .  □□  .│|
        /// |└─────────────────┘|
        /// |┌─────────────────┐|
        /// |│ . . .○ . .○  □. │|
        /// |└─────────────────┘|
        /// +-------------------+
        /// </code>
        /// <para>
        /// where <c>○</c> and <c>□</c> mean channel MIDI events on channel 0 and 1 correspondingly;
        /// <c>.</c> is any non-channel event.
        /// </para>
        /// <para>
        /// If <see cref="CopyNonChannelEventsToEachFile"/> set to <c>true</c> (which is the default
        /// value), we'll get following two new files:
        /// </para>
        /// <code language="image">
        /// +-------------------+
        /// |┌─────────────────┐|
        /// |│..□...  ... □□□..│|
        /// |└─────────────────┘|
        /// +-------------------+
        /// +-------------------+
        /// |┌─────────────────┐|
        /// |│.. ...○○...○   ..│|
        /// |└─────────────────┘|
        /// +-------------------+
        /// </code>
        /// <para>
        /// But if <see cref="CopyNonChannelEventsToEachFile"/> set to <c>false</c>, new files will
        /// contain only channel events:
        /// </para>
        /// <code language="image">
        /// +-------------------+
        /// |┌─────────────────┐|
        /// |│  □         □□□  │|
        /// |└─────────────────┘|
        /// +-------------------+
        /// +-------------------+
        /// |┌─────────────────┐|
        /// |│      ○○   ○     │|
        /// |└─────────────────┘|
        /// +-------------------+
        /// </code>
        /// </example>
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
