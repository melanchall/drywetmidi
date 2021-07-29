using System.Text;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Settings according to which MIDI data should be written.
    /// </summary>
    public class WritingSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether 'running status' (to turn off writing of the status
        /// bytes of consecutive events of the same type) should be used or not. The default value is <c>false</c>.
        /// </summary>
        public bool UseRunningStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Note Off events should be written as Note On ones
        /// with velocity of zero, or not. In conjunction with <see cref="UseRunningStatus"/> set to <c>true</c>
        /// can give some compression of MIDI data. The default value is <c>false</c>.
        /// </summary>
        public bool NoteOffAsSilentNoteOn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Time Signature events with default data should be deleted
        /// if there are no non-default ones before them, or not. The default value is <c>false</c>.
        /// </summary>
        public bool DeleteDefaultTimeSignature { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Key Signature events with default data should be deleted
        /// if there are no non-default ones before them, or not. The default value is <c>false</c>.
        /// </summary>
        public bool DeleteDefaultKeySignature { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Set Tempo events with default data should be deleted
        /// if there are no non-default ones before them, or not. The default value is <c>false</c>.
        /// </summary>
        public bool DeleteDefaultSetTempo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether events of <see cref="UnknownMetaEvent"/> type should be
        /// deleted or not. The default value is <c>false</c>.
        /// </summary>
        public bool DeleteUnknownMetaEvents { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether chunks of <see cref="UnknownChunk"/> type should be
        /// deleted or not. The default value is <c>false</c>.
        /// </summary>
        public bool DeleteUnknownChunks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether header chunk should be written to a MIDI file or not.
        /// The default value is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Note that missed header chunk violates SMF specification and thus applications may not read
        /// such files.
        /// </remarks>
        public bool WriteHeaderChunk { get; set; } = true;

        /// <summary>
        /// Gets or sets collection of custom meta events types.
        /// </summary>
        /// <remarks>
        /// <para>Types within this collection must be derived from the <see cref="MetaEvent"/>
        /// class and have parameterless constructor. No exception will be thrown
        /// while writing a MIDI file if some types don't meet these requirements.</para>
        /// </remarks>
        public EventTypesCollection CustomMetaEventTypes { get; set; }

        /// <summary>
        /// Gets or sets an <see cref="Encoding"/> that will be used to write the text of a
        /// text-based meta event. The default is <see cref="Encoding.ASCII"/>.
        /// </summary>
        public Encoding TextEncoding { get; set; } = SmfConstants.DefaultTextEncoding;

        /// <summary>
        /// Gets or sets settings according to which <see cref="MidiWriter"/> should write MIDI data.
        /// </summary>
        /// <remarks>
        /// <para>These settings specify reading binary data without knowledge about MIDI data structures.</para>
        /// </remarks>
        public WriterSettings WriterSettings { get; set; } = new WriterSettings();

        #endregion
    }
}
