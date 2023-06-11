using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings which control how a MIDI file should be sanitized with the <see cref="Sanitizer"/> tool.
    /// </summary>
    public sealed class SanitizingSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a minimum length for notes within an input file. All notes that are shorter
        /// than this value will be removed. The default value is <c>null</c> which means notes can
        /// have any length. <see cref="NoteDetectionSettings"/> property affects how notes are detected.
        /// </summary>
        public ITimeSpan NoteMinLength { get; set; }

        /// <summary>
        /// Gets or sets settings which define how notes should be detected and built. More info in the
        /// <see href="xref:a_getting_objects#settings">Getting objects: GetNotes: Settings</see> article.
        /// </summary>
        public NoteDetectionSettings NoteDetectionSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether empty track chunks should be removed or not.
        /// The default value is <c>true</c>.
        /// </summary>
        public bool RemoveEmptyTrackChunks { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether Note On (see <see cref="NoteOnEvent"/>) events without
        /// corresponding Note Off ones should be removed or not. The default value is <c>true</c>.
        /// <see cref="NoteDetectionSettings"/> property affects how notes (and thus orphaned Note On events)
        /// are detected.
        /// </summary>
        public bool RemoveOrphanedNoteOnEvents { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether Note Off (see <see cref="NoteOffEvent"/>) events without
        /// corresponding Note On ones should be removed or not. The default value is <c>true</c>.
        /// <see cref="NoteDetectionSettings"/> property affects how notes (and thus orphaned Note Off events)
        /// are detected.
        /// </summary>
        public bool RemoveOrphanedNoteOffEvents { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether duplicated Set Tempo (see <see cref="SetTempoEvent"/>) events
        /// should be removed or not. The default value is <c>true</c>.
        /// </summary>
        public bool RemoveDuplicatedSetTempoEvents { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether duplicated Time Signature (see <see cref="TimeSignatureEvent"/>)
        /// events should be removed or not. The default value is <c>true</c>.
        /// </summary>
        public bool RemoveDuplicatedTimeSignatureEvents { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether duplicated Pitch Bend (see <see cref="PitchBendEvent"/>)
        /// events should be removed or not. The default value is <c>true</c>.
        /// </summary>
        public bool RemoveDuplicatedPitchBendEvents { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether events on unused channels should be removed or not.
        /// Unused channel means there are no notes on this channel. The default value is <c>true</c>.
        /// </summary>
        public bool RemoveEventsOnUnusedChannels { get; set; } = true;

        #endregion
    }
}
