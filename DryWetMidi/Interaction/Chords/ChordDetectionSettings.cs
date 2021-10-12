using System;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Settings which define how chords should be detected and built.
    /// </summary>
    /// <seealso cref="ChordsManagingUtilities"/>
    public sealed class ChordDetectionSettings
    {
        #region Constants

        private const int DefaultNotesMinCount = 1;
        private static readonly long DefaultNotesTolerance = 0;

        #endregion

        #region Fields

        private int _notesMinCount = DefaultNotesMinCount;
        private long _notesTolerance = DefaultNotesTolerance;

        private ChordSearchContext _chordSearchContext = ChordSearchContext.SingleEventsCollection;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a minimum count of notes a chord can contain. So if count of simultaneously sounding
        /// notes is less than this value, they won't make up a chord. The default value is <c>1</c>.
        /// </summary>
        /// <remarks>
        /// Please see <see href="xref:a_getting_objects#notesmincount">Getting objects
        /// (section GetChords → Settings → NotesMinCount)</see> article to learn more.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
        public int NotesMinCount
        {
            get { return _notesMinCount; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _notesMinCount = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum distance of notes from the start of the first note of a chord.
        /// Notes within this tolerance will be included in a chord. The default value is <c>0</c>.
        /// </summary>
        /// <remarks>
        /// Please see <see href="xref:a_getting_objects#notestolerance">Getting objects
        /// (section GetChords → Settings → NotesTolerance)</see> article to learn more.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
        public long NotesTolerance
        {
            get { return _notesTolerance; }
            set
            {
                ThrowIfArgument.IsNegative(nameof(value), value, "Value is negative.");

                _notesTolerance = value;
            }
        }

        /// <summary>
        /// Gets or sets settings according to which notes should be detected and built. Chords will be
        /// built on top of those notes.
        /// </summary>
        public NoteDetectionSettings NoteDetectionSettings { get; set; } = new NoteDetectionSettings();

        /// <summary>
        /// Gets or sets a value defining a context to search chords within. The default value is
        /// <see cref="ChordSearchContext.SingleEventsCollection"/>.
        /// </summary>
        /// <remarks>
        /// See Remarks section of the <see cref="Interaction.ChordSearchContext"/> enum.
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public ChordSearchContext ChordSearchContext
        {
            get { return _chordSearchContext; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _chordSearchContext = value;
            }
        }

        #endregion
    }
}
