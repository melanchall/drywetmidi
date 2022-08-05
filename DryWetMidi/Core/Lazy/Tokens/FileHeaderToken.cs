namespace Melanchall.DryWetMidi.Core
{
    public sealed class FileHeaderToken : MidiToken
    {
        #region Constructor

        internal FileHeaderToken(ushort fileFormat, TimeDivision timeDivision, ushort tracksNumber)
            : base(MidiTokenType.HeaderChunkData)
        {
            FileFormat = fileFormat;
            TimeDivision = timeDivision;
            TracksNumber = tracksNumber;
        }

        #endregion

        #region Properties

        public ushort FileFormat { get; }

        public TimeDivision TimeDivision { get; }

        public ushort TracksNumber { get; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"File header token (file format = {FileFormat}, time division = {TimeDivision}, tracks number = {TracksNumber})";
        }

        #endregion
    }
}
