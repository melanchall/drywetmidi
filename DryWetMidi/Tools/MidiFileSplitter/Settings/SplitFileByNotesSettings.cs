using System;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Defines how a MIDI file should be split by notes using
    /// <see cref="MidiFileSplitter.SplitByNotes(Core.MidiFile, SplitFileByNotesSettings)"/> method.
    /// </summary>
    /// <seealso cref="MidiFileSplitter"/>
    public sealed class SplitFileByNotesSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to copy all non-note events
        /// to all the new files or throw them away. The default value is <c>true</c>>.
        /// </summary>
        /// <example>
        /// <para>
        /// For example, a MIDI file contains following notes:
        /// </para>
        /// <code language="image">
        /// +----------------------+
        /// |┌────────────────────┐|
        /// |│. [ A ]  [     B ]   . .│
        /// |└────────────────────┘|
        /// |┌────────────────────┐|
        /// |│ .  [ B ]  [    A]    . │
        /// |└────────────────────┘|
        /// +----------------------+
        /// </code>
        /// <para>
        /// where <c>A</c> and <c>B</c> mean different notes; <c>.</c> is any non-note event.
        /// </para>
        /// <para>
        /// If <see cref="CopyNonNoteEventsToEachFile"/> set to <c>true</c> (default value), we'll get following
        /// two new files:
        /// </para>
        /// <code language="image">
        /// +----------------------+
        /// |┌────────────────────┐|
        /// |│..[ A ]    [    A]   ...│
        /// |└────────────────────┘|
        /// +----------------------+
        /// +----------------------+
        /// |┌────────────────────┐|
        /// |│..  [ B ][     B ]   ...│
        /// |└────────────────────┘|
        /// +----------------------+
        /// </code>
        /// <para>
        /// But if <see cref="CopyNonNoteEventsToEachFile"/> set to <c>false</c>, each new file will contain
        /// only note events:
        /// </para>
        /// <code language="image">
        /// +----------------------+
        /// |┌────────────────────┐|
        /// |│  [ A ]    [    A]      │
        /// |└────────────────────┘|
        /// +----------------------+
        /// +----------------------+
        /// |┌────────────────────┐|
        /// |│    [ B ][     B ]      │
        /// |└────────────────────┘|
        /// +----------------------+
        /// </code>
        /// </example>
        public bool CopyNonNoteEventsToEachFile { get; set; } = true;

        /// <summary>
        /// Gets or sets a predicate to filter events out before processing. If predicate returns <c>true</c>,
        /// an event will be processed; if <c>false</c> - it won't. If the property set to <c>null</c>
        /// (default value), all MIDI events will be processed.
        /// </summary>
        public Predicate<TimedEvent> Filter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a note channel should be ignored or not.
        /// If set to <c>true</c>, notes with the same note number but different channels will be
        /// treated as same ones. The default value is <c>false</c>.
        /// </summary>
        /// <example>
        /// <para>
        /// For example, a MIDI file contains following notes:
        /// </para>
        /// <code language="image">
        /// +----------------------------------+
        /// |┌────────────────────────────────┐|
        /// |│  [ A0 ]  [    B1]    [      ]A1      │
        /// |└────────────────────────────────┘|
        /// |┌────────────────────────────────┐|
        /// |│    [ B2 ]  [    A0]                │
        /// |└────────────────────────────────┘|
        /// +----------------------------------+
        /// </code>
        /// <para>
        /// where <c>AN</c> and <c>BN</c> mean different notes on channel <c>N</c>.
        /// </para>
        /// <para>
        /// If <see cref="IgnoreChannel"/> set to <c>false</c> (which is the default value),
        /// we'll get four new files (since different channel means different note):
        /// </para>
        /// <code language="image">
        /// +----------------------------------+
        /// |┌────────────────────────────────┐|
        /// |│  [ A0 ]    [    A0]                │
        /// |└────────────────────────────────┘|
        /// +----------------------------------+
        /// +----------------------------------+
        /// |┌────────────────────────────────┐|
        /// |│                      [  A1  ]    │
        /// |└────────────────────────────────┘|
        /// +----------------------------------+
        /// +----------------------------------+
        /// |┌────────────────────────────────┐|
        /// |│          [  B1  ]                │
        /// |└────────────────────────────────┘|
        /// +----------------------------------+
        /// +----------------------------------+
        /// |┌────────────────────────────────┐|
        /// |│    [ B2 ]                        │
        /// |└────────────────────────────────┘|
        /// +----------------------------------+
        /// </code>
        /// <para>
        /// But if <see cref="IgnoreChannel"/> set to <c>true</c>, we'll get two files:
        /// </para>
        /// <code language="image">
        /// +----------------------------------+
        /// |┌────────────────────────────────┐|
        /// |│  [ A0 ]    [    A0]  [      ]A1      │
        /// |└────────────────────────────────┘|
        /// +----------------------------------+
        /// +----------------------------------+
        /// |┌────────────────────────────────┐|
        /// |│    [ B2 ][    B1]                  │
        /// |└────────────────────────────────┘|
        /// +----------------------------------+
        /// </code>
        /// </example>
        public bool IgnoreChannel { get; set; }

        #endregion
    }
}
