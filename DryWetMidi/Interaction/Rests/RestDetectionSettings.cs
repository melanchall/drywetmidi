using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Settings which define how rests should be detected and built. More info in the
    /// <see href="xref:a_getting_objects#rests">Getting objects: Rests</see> article.
    /// </summary>
    /// <seealso cref="RestsUtilities"/>
    public sealed class RestDetectionSettings
    {
        #region Constants

        private static readonly Func<ITimedObject, object> NoNotesKeySelector = obj => obj is Note ? "Note" : null;

        /// <summary>
        /// Rests will be built only at spaces without notes at all.
        /// </summary>
        public static readonly RestDetectionSettings NoNotes = new RestDetectionSettings
        {
            KeySelector = NoNotesKeySelector
        };

        /// <summary>
        /// Rests will be built between notes separately for each channel.
        /// </summary>
        public static readonly RestDetectionSettings NoNotesByChannel = new RestDetectionSettings
        {
            KeySelector = obj => (obj as Note)?.Channel
        };

        /// <summary>
        /// Rests will be built between notes separately for each note number.
        /// </summary>
        public static readonly RestDetectionSettings NoNotesByNoteNumber = new RestDetectionSettings
        {
            KeySelector = obj => (obj as Note)?.NoteNumber
        };

        /// <summary>
        /// Rests will be built between notes separately for each channel and note number.
        /// </summary>
        public static readonly RestDetectionSettings NoNotesByChannelAndNoteNumber = new RestDetectionSettings
        {
            KeySelector = obj => obj is Note ? Tuple.Create(((Note)obj).Channel, ((Note)obj).NoteNumber) : null
        };

        /// <summary>
        /// Rests will be built only at spaces without chords at all.
        /// </summary>
        public static readonly RestDetectionSettings NoChords = new RestDetectionSettings
        {
            KeySelector = obj => obj is Chord ? "Chord" : null
        };

        /// <summary>
        /// Rests will be built between chords separately for each channel.
        /// </summary>
        public static readonly RestDetectionSettings NoChordsByChannel = new RestDetectionSettings
        {
            KeySelector = obj => (obj as Chord)?.Channel
        };

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a function that returns the key of an object. Please read
        /// <see href="xref:a_getting_objects#rests">Getting objects: Rests</see> article to
        /// understand the key concept. The default key selector is
        /// <c>obj => obj is Note ? "Note" : null</c> which means rests will be built
        /// between notes where there are no notes at all.
        /// </summary>
        public Func<ITimedObject, object> KeySelector { get; set; } = NoNotesKeySelector;

        #endregion
    }
}
