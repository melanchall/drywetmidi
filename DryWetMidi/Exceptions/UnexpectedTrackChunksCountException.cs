using System;
using System.Runtime.Serialization;

namespace Melanchall.DryMidi
{
    [Serializable]
    public sealed class UnexpectedTrackChunksCountException : MidiException
    {
        #region Constants

        private const string ExpectedCountSerializationPropertyName = "ExpectedCount";
        private const string ActualCountSerializationPropertyName = "ActualCount";

        #endregion

        #region Constructors

        public UnexpectedTrackChunksCountException()
            : base()
        {
        }

        public UnexpectedTrackChunksCountException(string message, int expectedCount, int actualCount)
            : base(message)
        {
            ExpectedCount = expectedCount;
            ActualCount = actualCount;
        }

        private UnexpectedTrackChunksCountException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ExpectedCount = info.GetInt32(ExpectedCountSerializationPropertyName);
            ActualCount = info.GetInt32(ActualCountSerializationPropertyName);
        }

        #endregion

        #region Properties

        public int ExpectedCount { get; }

        public int ActualCount { get; }

        #endregion

        #region Overrides

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(ExpectedCountSerializationPropertyName, ExpectedCount);
            info.AddValue(ActualCountSerializationPropertyName, ActualCount);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
