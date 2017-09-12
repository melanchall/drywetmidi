using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents a simple mathematical operation on length - addition or subtraction of an arbitrary length.
    /// </summary>
    /// <remarks>
    /// Note that you cannot convert length to <see cref="MathLength"/>. You can only convert <see cref="MathLength"/>
    /// to another length representation.
    /// </remarks>
    public sealed class MathLength : ILength
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MathLength"/> with the specified
        /// lengths and mathematical operation.
        /// </summary>
        /// <param name="length1">The <see cref="ILength"/> to add <paramref name="length2"/> to or
        /// subtract <paramref name="length2"/> from.</param>
        /// <param name="length2">The <see cref="ILength"/> to add to or subtract from <paramref name="length1"/>.</param>
        /// <param name="operation">Mathematical operation to perform on <paramref name="length1"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="length1"/> is null. -or-
        /// <paramref name="length2"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="operation"/> specified
        /// an invalid value.</exception>
        public MathLength(ILength length1, ILength length2, MathOperation operation = MathOperation.Add)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);
            ThrowIfArgument.IsInvalidEnumValue(nameof(operation), operation);

            Length1 = length1;
            Length2 = length2;
            Operation = operation;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the length to add the <see cref="Length2"/> to or subtract the <see cref="Length2"/> from.
        /// </summary>
        public ILength Length1 { get; }

        /// <summary>
        /// Gets the length to add to or subtract from the <see cref="Length1"/>.
        /// </summary>
        public ILength Length2 { get; }

        /// <summary>
        /// Gets the mathematical operation to perform on the <see cref="Length1"/>.
        /// </summary>
        public MathOperation Operation { get; }

        #endregion

        #region Methods

        public static bool TryParse(string input, out MathLength length)
        {
            return MathLengthParser.TryParse(input, out length).Status == ParsingStatus.Parsed;
        }

        public static MathLength Parse(string input)
        {
            var parsingResult = MathLengthParser.TryParse(input, out var fraction);
            if (parsingResult.Status == ParsingStatus.Parsed)
                return fraction;

            throw parsingResult.Exception;
        }

        #endregion

        #region Operators

        public static bool operator ==(MathLength length1, MathLength length2)
        {
            if (ReferenceEquals(length1, length2))
                return true;

            if (ReferenceEquals(null, length1) || ReferenceEquals(null, length2))
                return false;

            return length1.Length1.Equals(length2.Length1) &&
                   length1.Length2.Equals(length2.Length2) &&
                   length1.Operation == length2.Operation;
        }

        public static bool operator !=(MathLength length1, MathLength length2)
        {
            return !(length1 == length2);
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

            return $"({Length1} {operationString} {Length2})";
        }

        public override bool Equals(object obj)
        {
            return this == (obj as MathLength);
        }

        public override int GetHashCode()
        {
            return Length1.GetHashCode() ^ Operation.GetHashCode() ^ Length2.GetHashCode();
        }

        #endregion

        #region ILength

        public ILength Add(ILength length)
        {
            ThrowIfArgument.IsNull(nameof(length), length);

            return LengthUtilities.Add(this, length);
        }

        public ILength Subtract(ILength length)
        {
            ThrowIfArgument.IsNull(nameof(length), length);

            return LengthUtilities.Subtract(this, length);
        }

        public ILength Multiply(int multiplier)
        {
            ThrowIfArgument.IsNegative(nameof(multiplier), multiplier, "Multiplier is negative.");

            return new MathLength(Length1.Multiply(multiplier),
                                  Length2.Multiply(multiplier),
                                  Operation);
        }

        public ILength Divide(int divisor)
        {
            ThrowIfArgument.IsNegative(nameof(divisor), divisor, "Divisor is negative.");

            return new MathLength(Length1.Divide(divisor),
                                  Length2.Divide(divisor),
                                  Operation);
        }

        #endregion
    }
}
