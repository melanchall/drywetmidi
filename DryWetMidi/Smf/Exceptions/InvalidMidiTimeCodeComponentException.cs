using System.Runtime.Serialization;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public sealed class InvalidMidiTimeCodeComponentException : MidiException
    {
        #region Constants

        private const string ValueSerializationPropertyName = "Value";

        #endregion

        #region Constructors

        public InvalidMidiTimeCodeComponentException()
        {
        }

        public InvalidMidiTimeCodeComponentException(string message, byte value)
            : base(message)
        {
            Value = value;
        }

        private InvalidMidiTimeCodeComponentException(SerializationInfo info, StreamingContext context)
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
            ThrowIfArgument.IsNull(nameof(info), info);

            info.AddValue(ValueSerializationPropertyName, Value);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
