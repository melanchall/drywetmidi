using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when the reading engine has encountered an invalid parameter
    /// of a channel event.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.InvalidChannelEventParameterValuePolicy"/>
    /// is set to <see cref="InvalidChannelEventParameterValuePolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    [Serializable]
    public sealed class InvalidChannelEventParameterValueException : MidiException
    {
        #region Constructors

        internal InvalidChannelEventParameterValueException(MidiEventType eventType, byte value)
            : base($"{value} is invalid value for parameter of channel event of {eventType} type.")
        {
            EventType = eventType;
            Value = value;
        }

        private InvalidChannelEventParameterValueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            EventType = (MidiEventType)info.GetValue(nameof(EventType), typeof(MidiEventType));
            Value = info.GetByte(nameof(Value));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of channel event that caused this exception.
        /// </summary>
        public MidiEventType EventType { get; }

        /// <summary>
        /// Gets the value of the channel event's parameter that caused this exception.
        /// </summary>
        public byte Value { get; }

        #endregion

        #region Overrides

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <c>null</c>.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(EventType), EventType);
            info.AddValue(nameof(Value), Value);
        }

        #endregion
    }
}
