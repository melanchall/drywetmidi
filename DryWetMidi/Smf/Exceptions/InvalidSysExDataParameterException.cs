using System;
using System.Runtime.Serialization;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    [Serializable]
    public sealed class InvalidSysExDataParameterException : MidiException
    {
        #region Constants

        private const string ValueSerializationPropertyName = "Value";

        #endregion

        #region Constructors

        public InvalidSysExDataParameterException()
        {
        }

        public InvalidSysExDataParameterException(string message, int value)
            : base(message)
        {
            Value = value;
        }

        private InvalidSysExDataParameterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Value = info.GetByte(ValueSerializationPropertyName);
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
