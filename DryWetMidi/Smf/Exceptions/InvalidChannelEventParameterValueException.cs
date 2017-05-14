using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    [Serializable]
    public sealed class InvalidChannelEventParameterValueException : MidiException
    {
        #region Constants

        private const string ValueSerializationPropertyName = "Value";

        #endregion

        #region Constructors

        public InvalidChannelEventParameterValueException()
            : base()
        {
        }

        public InvalidChannelEventParameterValueException(string message, byte value)
            : base(message)
        {
            Value = value;
        }

        private InvalidChannelEventParameterValueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Value = info.GetByte(ValueSerializationPropertyName);
        }

        #endregion

        #region Properties

        public byte Value { get; }

        #endregion

        #region Overrides

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(ValueSerializationPropertyName, Value);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
