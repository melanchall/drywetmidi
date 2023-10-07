using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Time division that represents subdivisions of a second, in a way consistent with
    /// SMPTE and MIDI time code.
    /// </summary>
    public sealed class SmpteTimeDivision : TimeDivision
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SmpteTimeDivision"/> with the
        /// specified format (frame rate) and resolution.
        /// </summary>
        /// <param name="format">SMPTE format representing the number of frames per second.</param>
        /// <param name="resolution">Resolution within a frame.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="format"/> specified an invalid value.</exception>
        public SmpteTimeDivision(SmpteFormat format, byte resolution)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);

            Format = format;
            Resolution = resolution;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets SMPTE format (frame rate).
        /// </summary>
        public SmpteFormat Format { get; }

        /// <summary>
        /// Gets resolution within a frame.
        /// </summary>
        public byte Resolution { get; }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="SmpteTimeDivision"/> objects are equal.
        /// </summary>
        /// <param name="timeDivision1">The first <see cref="SmpteTimeDivision"/> to compare.</param>
        /// <param name="timeDivision2">The second <see cref="SmpteTimeDivision"/> to compare.</param>
        /// <returns><c>true</c> if the time divisions are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(SmpteTimeDivision timeDivision1, SmpteTimeDivision timeDivision2)
        {
            if (ReferenceEquals(timeDivision1, timeDivision2))
                return true;

            if (ReferenceEquals(null, timeDivision1) || ReferenceEquals(null, timeDivision2))
                return false;

            return timeDivision1.Format == timeDivision2.Format &&
                   timeDivision1.Resolution == timeDivision2.Resolution;
        }

        /// <summary>
        /// Determines if two <see cref="SmpteTimeDivision"/> objects are not equal.
        /// </summary>
        /// <param name="timeDivision1">The first <see cref="SmpteTimeDivision"/> to compare.</param>
        /// <param name="timeDivision2">The second <see cref="SmpteTimeDivision"/> to compare.</param>
        /// <returns><c>false</c> if the time divisions are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(SmpteTimeDivision timeDivision1, SmpteTimeDivision timeDivision2)
        {
            return !(timeDivision1 == timeDivision2);
        }

        #endregion

        #region Overrides

        internal override short ToInt16()
        {
            return (short)DataTypesUtilities.Combine((byte)-(byte)Format, Resolution);
        }

        /// <summary>
        /// Clones time division by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the time division.</returns>
        public override TimeDivision Clone()
        {
            return new SmpteTimeDivision(Format, Resolution);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Format} frames / sec, {Resolution} subdivisions / frame";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return this == (obj as SmpteTimeDivision);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + Format.GetHashCode();
                result = result * 23 + Resolution.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
