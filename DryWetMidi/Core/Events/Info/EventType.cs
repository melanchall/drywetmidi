using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents an event's identity described by its type and corresponding status byte.
    /// </summary>
    public sealed class EventType
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EventType"/> with the specified type and
        /// status byte.
        /// </summary>
        /// <param name="type">Type of an event.</param>
        /// <param name="statusByte">Status byte of an event.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        public EventType(Type type, byte statusByte)
        {
            ThrowIfArgument.IsNull(nameof(type), type);

            Type = type;
            StatusByte = statusByte;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of events described by this instance of the <see cref="EventType"/>.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the status byte of events described by this instance of the <see cref="EventType"/>.
        /// </summary>
        public byte StatusByte { get; }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="EventType"/> objects are equal.
        /// </summary>
        /// <param name="eventType1">The first <see cref="EventType"/> to compare.</param>
        /// <param name="eventType2">The second <see cref="EventType"/> to compare.</param>
        /// <returns><c>true</c> if the objects are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(EventType? eventType1, EventType? eventType2)
        {
            if (ReferenceEquals(eventType1, eventType2))
                return true;

            if (ReferenceEquals(null, eventType1) || ReferenceEquals(null, eventType2))
                return false;

            return eventType1.Type == eventType2.Type &&
                   eventType1.StatusByte == eventType2.StatusByte;
        }

        /// <summary>
        /// Determines if two <see cref="EventType"/> objects are not equal.
        /// </summary>
        /// <param name="eventType1">The first <see cref="EventType"/> to compare.</param>
        /// <param name="eventType2">The second <see cref="EventType"/> to compare.</param>
        /// <returns><c>false</c> if the objects are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(EventType? eventType1, EventType? eventType2)
        {
            return !(eventType1 == eventType2);
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        override public string ToString()
        {
            return $"{StatusByte} ({Convert.ToString(StatusByte, 16).ToUpperInvariant().PadLeft(2, '0')}) -> {Type.Name}";
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return this == (obj as EventType);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + Type.GetHashCode();
                result = result * 23 + StatusByte.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
