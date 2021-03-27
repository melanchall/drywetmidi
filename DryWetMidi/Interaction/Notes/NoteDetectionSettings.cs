using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Settings which define how notes should be detected and built.
    /// </summary>
    /// <seealso cref="NotesManagingUtilities"/>
    public sealed class NoteDetectionSettings
    {
        #region Fields

        private NoteStartDetectionPolicy _noteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn;
        private NoteSearchContext _noteSearchContext = NoteSearchContext.SingleEventsCollection;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets how start event of a note should be found in case of overlapping notes with
        /// the same note number and channel. The default value is <see cref="NoteStartDetectionPolicy.FirstNoteOn"/>.
        /// </summary>
        /// <remarks>
        /// See Remarks section of the <see cref="Interaction.NoteStartDetectionPolicy"/> enum.
        /// </remarks>
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

        /// <summary>
        /// Gets or sets a value defining a context to search notes within. The default value is
        /// <see cref="NoteSearchContext.SingleEventsCollection"/>.
        /// </summary>
        /// <remarks>
        /// See Remarks section of the <see cref="Interaction.NoteSearchContext"/> enum.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public NoteSearchContext NoteSearchContext
        {
            get { return _noteSearchContext; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _noteSearchContext = value;
            }
        }

        #endregion
    }
}
