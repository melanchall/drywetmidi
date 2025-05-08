using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Specifies how sanitizing engine should react on Note On (see <see cref="NoteOnEvent"/>) events without
    /// corresponding Note Off ones. The default value is <see cref="Remove"/>. More info in the
    /// <see href="xref:a_sanitizer#orphanednoteoneventspolicy">Sanitizer: OrphanedNoteOnEventsPolicy</see> article.
    /// </summary>
    public enum OrphanedNoteOnEventsPolicy
    {
        /// <summary>
        /// Remove orphaned Note On events.
        /// </summary>
        Remove = 0,

        /// <summary>
        /// Don't touch orphaned Note On events.
        /// </summary>
        Ignore,

        /// <summary>
        /// Try to complete a note which starts from an orphaned Note On event.
        /// </summary>
        /// <seealso cref="SanitizingSettings.NoteMaxLengthForOrphanedNoteOnEvent"/>
        CompleteNote,
    }
}
