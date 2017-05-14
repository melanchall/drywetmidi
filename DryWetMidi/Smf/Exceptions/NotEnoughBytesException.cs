using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    [Serializable]
    public sealed class NotEnoughBytesException : MidiException
    {
        #region Constants

        private const string ExpectedCountSerializationPropertyName = "ExpectedCount";
        private const string ActualCountSerializationPropertyName = "ActualCount";

        #endregion

        #region Constructors

        public NotEnoughBytesException()
            : base()
        {
        }

        public NotEnoughBytesException(string message)
            : base(message)
        {
        }

        public NotEnoughBytesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private NotEnoughBytesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ExpectedCount = info.GetInt64(ExpectedCountSerializationPropertyName);
            ActualCount = info.GetInt64(ActualCountSerializationPropertyName);
        }

        public NotEnoughBytesException(string message, long expectedCount, long actualCount)
            : this(message)
        {
            ExpectedCount = expectedCount;
            ActualCount = actualCount;
        }

        #endregion

        #region Properties

        public long ExpectedCount { get; }

        public long ActualCount { get; }

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
