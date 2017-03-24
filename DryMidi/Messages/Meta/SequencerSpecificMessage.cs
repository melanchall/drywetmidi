using System;

namespace Melanchall.DryMidi
{
    public sealed class SequencerSpecificMessage : MetaMessage
    {
        #region Constructor

        public SequencerSpecificMessage()
        {
        }

        public SequencerSpecificMessage(byte[] data)
            : this()
        {
            Data = data;
        }

        #endregion

        #region Properties

        public byte[] Data { get; set; }

        #endregion

        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Sequencer Specific message.");

            Data = reader.ReadBytes(size);
        }

        internal override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        internal override int GetContentSize()
        {
            return Data?.Length ?? 0;
        }

        protected override Message CloneMessage()
        {
            return new SequencerSpecificMessage(Data?.Clone() as byte[]);
        }

        public override string ToString()
        {
            return "Sequencer Specific";
        }

        #endregion
    }
}
