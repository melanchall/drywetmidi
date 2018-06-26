using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class Rest : ILengthedObject
    {
        #region Constructor

        internal Rest(long time, long length, FourBitNumber? channel, SevenBitNumber? noteNumber)
        {
            Time = time;
            Length = length;
            Channel = channel;
            NoteNumber = noteNumber;
        }

        #endregion

        #region Properties

        public long Time { get; }

        public long Length { get; }

        public FourBitNumber? Channel { get; }

        public SevenBitNumber? NoteNumber { get; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Rest (channel = {Channel}, note number = {NoteNumber})";
        }

        #endregion
    }
}
