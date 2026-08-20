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

        #region Overrides

        override public string ToString()
        {
            return $"{StatusByte} ({Convert.ToString(StatusByte, 16).ToUpperInvariant().PadLeft(2, '0')}) -> {Type.Name}";
        }

        #endregion
    }
}
