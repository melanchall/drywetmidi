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
        public MathTime(ITime time, ILength offset, MathOperation operation = default(MathOperation))
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(offset), offset);
            ThrowIfArgument.IsInvalidEnumValue(nameof(operation), operation);

            Time = time;
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
        /// Gets the <see cref="ILength"/> to add to or subtract from <see cref="Time"/> or <see cref="MidiTime"/>.
        /// </summary>
        public ILength Offset { get; }

        /// <summary>
        /// Gets the mathematical operation to perform on the <see cref="Time"/> or <see cref="MidiTime"/>.
        /// </summary>
        public MathOperation Operation { get; }

        #endregion

        #region Methods

        public static bool TryParse(string input, out MathTime time)
        {
            return MathTimeParser.TryParse(input, out time).Status == ParsingStatus.Parsed;
        }

        public static MathTime Parse(string input)
        {
            var parsingResult = MathTimeParser.TryParse(input, out var fraction);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return fraction;

            throw parsingResult.Exception;
        }

        #endregion

        #region Operators

        public static bool operator ==(MathTime time1, MathTime time2)
        {
            if (ReferenceEquals(time1, time2))
                return true;

            if (ReferenceEquals(null, time1) || ReferenceEquals(null, time2))
                return false;

            return time1.Time.Equals(time2.Time) &&
                   time1.Offset.Equals(time2.Offset) &&
                   time1.Operation == time2.Operation;
        }

        public static bool operator !=(MathTime time1, MathTime time2)
        {
            return !(time1 == time2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var operationString = Operation == MathOperation.Add
                ? "+"
                : "-";

            return $"({Time} {operationString} {Offset})";
        }

        public override bool Equals(object obj)
        {
            return this == (obj as MathTime);
        }

        public override int GetHashCode()
        {
            return Time.GetHashCode() ^ Operation.GetHashCode() ^ Offset.GetHashCode();
        }

        #endregion
    }
}
