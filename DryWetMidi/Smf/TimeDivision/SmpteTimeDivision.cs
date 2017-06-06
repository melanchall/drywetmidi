using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Smf
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
        /// specified format (frame rate) and resoltion.
        /// </summary>
        /// <param name="format">SMPTE format representing the number of frames per second.</param>
        /// <param name="resolution">Resoltuion within a frame.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="format"/> specified an invalid value.</exception>
        public SmpteTimeDivision(SmpteFormat format, byte resolution)
        {
            if (!Enum.IsDefined(typeof(SmpteFormat), format))
                throw new InvalidEnumArgumentException(nameof(format), (int)format, typeof(SmpteFormat));

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
        /// Gets resoltion within a frame.
        /// </summary>
        public byte Resolution { get; }

        #endregion

        #region Overrides

        internal override short ToInt16()
        {
            return (short)-DataTypesUtilities.Combine((byte)Format, Resolution);
        }

        /// <summary>
        /// Clones time division by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the time division.</returns>
        public override TimeDivision Clone()
        {
            return new SmpteTimeDivision(Format, Resolution);
        }

        #endregion
    }
}
