using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi
{
    [Serializable]
    public sealed class InvalidChunkSizeException : MidiException
    {
        #region Constants

        private const string ExpectedSizeSerializationPropertyName = "ExpectedSize";
        private const string ActualSizeSerializationPropertyName = "ActualSize";

        #endregion

        #region Constructors

        public InvalidChunkSizeException()
            : base("Actual size of a chunk differs from the one declared in the chunk's header.")
        {
        }

        private InvalidChunkSizeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ExpectedSize = info.GetUInt32(ExpectedSizeSerializationPropertyName);
            ActualSize = info.GetUInt32(ActualSizeSerializationPropertyName);
        }

        public InvalidChunkSizeException(uint expectedSize, uint actualSize)
            : this()
        {
            ExpectedSize = expectedSize;
            ActualSize = actualSize;
        }

        #endregion

        #region Properties

        public uint ExpectedSize { get; }

        public uint ActualSize { get; }

        #endregion

        #region Overrides

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(ExpectedSizeSerializationPropertyName, ExpectedSize);
            info.AddValue(ActualSizeSerializationPropertyName, ActualSize);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
