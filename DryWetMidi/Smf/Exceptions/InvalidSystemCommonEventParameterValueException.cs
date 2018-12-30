using System;
using System.Runtime.Serialization;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    [Serializable]
    public sealed class InvalidSystemCommonEventParameterValueException : MidiException
    {
        #region Constants

        private const string ValueSerializationPropertyName = "Value";

        #endregion

        #region Constructors

        public InvalidSystemCommonEventParameterValueException()
        {
        }

        public InvalidSystemCommonEventParameterValueException(string message, int value)
            : base(message)
        {
            Value = value;
        }

        private InvalidSystemCommonEventParameterValueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Value = info.GetInt32(ValueSerializationPropertyName);
        }

        #endregion

        #region Properties

        public int Value { get; }

        #endregion

        #region Overrides

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ThrowIfArgument.IsNull(nameof(info), info);

            info.AddValue(ValueSerializationPropertyName, Value);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
