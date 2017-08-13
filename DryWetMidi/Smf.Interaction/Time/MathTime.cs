using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents a simple mathematical operation on time - addition or subtraction of an arbitrary length.
    /// </summary>
    /// <remarks>
    /// Note that you cannot convert time to <see cref="MathTime"/>. You can only convert <see cref="MathTime"/>
    /// to another time representation.
    /// </remarks>
    public sealed class MathTime : ITime
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MathTime"/> with the specified
        /// time, offset and mathematical operation.
        /// </summary>
        /// <param name="time">The <see cref="ITime"/> to add <paramref name="offset"/> to or
        /// subtract <paramref name="offset"/> from.</param>
        /// <param name="offset"><see cref="ILength"/> that should be added to or subtracted
        /// from <paramref name="time"/>.</param>
        /// <param name="operation">Mathematical operation to perform on <paramref name="time"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is null. -or-
        /// <paramref name="offset"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="operation"/> specified
        /// an invalid value.</exception>
        public MathTime(ITime time, ILength offset, MathOperation operation = MathOperation.Sum)
            : this(offset, operation)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            Time = time;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MathTime"/> with the specified
        /// time, offset and mathematical operation.
        /// </summary>
        /// <param name="time">The MIDI time to add <paramref name="offset"/> to or
        /// subtract <paramref name="offset"/> from.</param>
        /// <param name="offset"><see cref="ILength"/> that should be added to or subtracted
        /// from <paramref name="time"/>.</param>
        /// <param name="operation">Mathematical operation to perform on <paramref name="time"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="offset"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="operation"/> specified
        /// an invalid value.</exception>
        public MathTime(long time, ILength offset, MathOperation operation = MathOperation.Sum)
            : this(offset, operation)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);

            MidiTime = time;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MathTime"/> with the specified
        /// offset and mathematical operation.
        /// </summary>
        /// <param name="offset"><see cref="ILength"/> that should be added to or subtracted
        /// from the specified time.</param>
        /// <param name="operation">Mathematical operation to perform on time.</param>
        /// <exception cref="ArgumentNullException"><paramref name="offset"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="operation"/> specified
        /// an invalid value.</exception>
        private MathTime(ILength offset, MathOperation operation = MathOperation.Sum)
        {
            ThrowIfArgument.IsNull(nameof(offset), offset);
            ThrowIfArgument.IsInvalidEnumValue(nameof(operation), operation);

            Offset = offset;
            Operation = operation;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the time to add the <see cref="Offset"/> to or subtract the <see cref="Offset"/> from.
        /// </summary>
        /// <remarks>
        /// The property can return null if the current instance of the <see cref="MathTime"/> was
        /// initialized with MIDI time rather than with <see cref="ITime"/>.
        /// </remarks>
        public ITime Time { get; }

        /// <summary>
        /// Gets the time to add the <see cref="Offset"/> to or subtract the <see cref="Offset"/> from.
        /// </summary>
        public long MidiTime { get; }

        /// <summary>
        /// Gets the <see cref="ILength"/> to add to or subtract from <see cref="Time"/> or <see cref="MidiTime"/>.
        /// </summary>
        public ILength Offset { get; }

        /// <summary>
        /// Gets the mathematical operation to perform on the <see cref="Time"/> or <see cref="MidiTime"/>.
        /// </summary>
        public MathOperation Operation { get; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var operationString = Operation == MathOperation.Sum
                ? "+"
                : "-";

            return $"({Time ?? (object)MidiTime} {operationString} {Offset})";
        }

        #endregion
    }
}
