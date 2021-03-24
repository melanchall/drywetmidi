using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which notes should be quantized.
    /// </summary>
    public class NotesQuantizingSettings : LengthedObjectsQuantizingSettings<Note>
    {
        #region Properties

        /// <summary>
        /// Gets or sets settings which define how notes should be detected and built.
        /// </summary>
        public NoteDetectionSettings NoteDetectionSettings { get; set; } = new NoteDetectionSettings();

        #endregion
    }

    /// <summary>
    /// Provides methods to quantize notes time.
    /// </summary>
    /// <remarks>
    /// See <see href="xref:a_quantizer">Quantizer</see> article to learn more.
    /// </remarks>
    public class NotesQuantizer : LengthedObjectsQuantizer<Note, NotesQuantizingSettings>
    {
    }
}
