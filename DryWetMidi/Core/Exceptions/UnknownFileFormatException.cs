using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when a MIDI file format is unknown.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.UnknownFileFormatPolicy"/>
    /// is set to <see cref="UnknownFileFormatPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    public sealed class UnknownFileFormatException : MidiException
    {
        #region Constructors

        internal UnknownFileFormatException(ushort fileFormat)
            : base($"File format {fileFormat} is unknown.")
        {
            FileFormat = fileFormat;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number that represents format of a MIDI file.
        /// </summary>
        public ushort FileFormat { get; }

        #endregion
    }
}
