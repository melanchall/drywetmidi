using System;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Defines how a MIDI file should be split by notes using
    /// <see cref="Splitter.SplitByNotes(Core.MidiFile, SplitFileByNotesSettings)"/> method.
    /// </summary>
    /// <seealso cref="Splitter"/>
    public sealed class SplitFileByNotesSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to copy all non-note events
        /// to all the new files or throw them away. The default value is <c>true</c>>.
        /// </summary>
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
        public bool IgnoreChannel { get; set; }

        #endregion
    }
}
