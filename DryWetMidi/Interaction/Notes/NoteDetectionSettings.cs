using System;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Settings which define how notes should be detected and built. More info in the
    /// <see href="xref:a_getting_objects#settings">Getting objects: GetNotes: Settings</see> article.
    /// </summary>
    /// <seealso cref="NotesManagingUtilities"/>
    /// <seealso cref="GetObjectsUtilities"/>
    public sealed class NoteDetectionSettings
    {
        #region Fields

        private NoteStartDetectionPolicy _noteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets custom construction method for <see cref="Note"/>. If <c>null</c>,
        /// default method will be used (via one of the <see cref="Note"/>'s constructors).
        /// </summary>
        public Func<NoteData, Note> Constructor { get; set; }

        /// <summary>
        /// Gets or sets how start event of a note should be found in case of overlapping notes with
        /// the same note number and channel. The default value is <see cref="NoteStartDetectionPolicy.FirstNoteOn"/>.
        /// More info in the
        /// <see href="xref:a_getting_objects#notestartdetectionpolicy">Getting objects: GetNotes: Settings: NoteStartDetectionPolicy</see>
        /// article.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public NoteStartDetectionPolicy NoteStartDetectionPolicy
        {
            get { return _noteStartDetectionPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _noteStartDetectionPolicy = value;
            }
        }

        #endregion
    }
}
