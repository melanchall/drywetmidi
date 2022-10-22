namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents the data of a header chunk of a MIDI file.
    /// </summary>
    /// <seealso cref="MidiTokensReader"/>
    public sealed class FileHeaderToken : MidiToken
    {
        #region Constructor

        internal FileHeaderToken(ushort fileFormat, TimeDivision timeDivision, ushort tracksNumber)
            : base(MidiTokenType.FileHeader)
        {
            FileFormat = fileFormat;
            TimeDivision = timeDivision;
            TracksNumber = tracksNumber;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the format (see <see cref="MidiFileFormat"/>) of a MIDI file.
        /// </summary>
        public ushort FileFormat { get; }

        /// <summary>
        /// Gets the time division of a MIDI file.
        /// </summary>
        public TimeDivision TimeDivision { get; }

        /// <summary>
        /// Gets the number of track chunks in a MIDI file.
        /// </summary>
        public ushort TracksNumber { get; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"File header token (file format = {FileFormat}, time division = {TimeDivision}, tracks number = {TracksNumber})";
        }

        #endregion
    }
}
