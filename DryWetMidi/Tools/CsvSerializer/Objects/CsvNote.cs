using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class CsvNote : CsvObject
    {
        #region Constructor

        public CsvNote(
            SevenBitNumber noteNumber,
            SevenBitNumber velocity,
            SevenBitNumber offVlocity,
            FourBitNumber channel,
            ITimeSpan length,
            int? chunkIndex,
            string chunkId,
            int? objectIndex,
            ITimeSpan time)
            : base(chunkIndex, chunkId, objectIndex)
        {
            NoteNumber = noteNumber;
            Velocity = velocity;
            OffVlocity = offVlocity;
            Channel = channel;
            Time = time;
            Length = length;
        }

        public SevenBitNumber NoteNumber { get; }

        public SevenBitNumber Velocity { get; }

        public SevenBitNumber OffVlocity { get; }

        public FourBitNumber Channel { get; }

        public ITimeSpan Time { get; }

        public ITimeSpan Length { get; }

        #endregion
    }
}
