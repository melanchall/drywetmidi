using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a SMPTE Offset meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI SMPTE offset meta message specifies an offset for the starting point
    /// of a MIDI track from the start of a sequence in terms of SMPTE time
    /// (hours:minutes:seconds:frames:subframes).
    /// </remarks>
    public sealed class SmpteOffsetEvent : MetaEvent
    {
        #region Constants

        /// <summary>
        /// Maximum value for the <see cref="Hours"/>.
        /// </summary>
        public const byte MaxHours = 23;

        /// <summary>
        /// Maximum value for the <see cref="Minutes"/>.
        /// </summary>
        public const byte MaxMinutes = 59;

        /// <summary>
        /// Maximum value for the <see cref="Seconds"/>.
        /// </summary>
        public const byte MaxSeconds = 59;

        /// <summary>
        /// Maximum value for the <see cref="SubFrames"/>.
        /// </summary>
        public const byte MaxSubFrames = 99;

        private static readonly Dictionary<SmpteFormat, byte> MaxFrames = new Dictionary<SmpteFormat, byte>
        {
            [SmpteFormat.TwentyFour] = 23,
            [SmpteFormat.TwentyFive] = 24,
            [SmpteFormat.ThirtyDrop] = 28,
            [SmpteFormat.Thirty] = 29
        };

        private const int FormatMask = 0x60; // 01100000
        private const int FormatOffset = 5;
        private const int HoursMask = 0x1F; // 00011111

        private static readonly Dictionary<int, SmpteFormat> Formats = new Dictionary<int, SmpteFormat>
        {
            [0] = SmpteFormat.TwentyFour,
            [1] = SmpteFormat.TwentyFive,
            [2] = SmpteFormat.ThirtyDrop,
            [3] = SmpteFormat.Thirty
        };

        #endregion

        #region Fields

        private byte _hours;
        private byte _minutes;
        private byte _seconds;
        private byte _frames;
        private byte _subFrames;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SmpteOffsetEvent"/>.
        /// </summary>
        public SmpteOffsetEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmpteOffsetEvent"/> with the
        /// specified SMPE format, hours, minutes, seconds, number of frames and sub-frames.
        /// </summary>
        /// <param name="format">SMPTE format.</param>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="frames">Number of frames.</param>
        /// <param name="subFrames">Number of sub-frames.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="format"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Hours number is out of valid range. -or-
        /// Minutes number is out of valid range. -or- Seconds number is out of valid range. -or-
        /// Frames number is out of valid range. -or- Sub-frames number is out of valid range.</exception>
        public SmpteOffsetEvent(SmpteFormat format, byte hours, byte minutes, byte seconds, byte frames, byte subFrames)
            : this()
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(format), format);

            Format = format;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Frames = frames;
            SubFrames = subFrames;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets SMPTE format.
        /// </summary>
        public SmpteFormat Format { get; set; }

        /// <summary>
        /// Gets or sets number of hours.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Hours number is out of valid range (0-23).</exception>
        public byte Hours
        {
            get { return _hours; }
            set
            {
                ThrowIfArgument.IsOutOfRange(nameof(value),
                                             value,
                                             0,
                                             MaxHours,
                                             $"Hours number is out of valid range (0-{MaxHours}).");

                _hours = value;
            }
        }

        /// <summary>
        /// Gets or sets number of minutes.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Minutes number is out of valid range (0-59).</exception>
        public byte Minutes
        {
            get { return _minutes; }
            set
            {
                ThrowIfArgument.IsOutOfRange(nameof(value),
                                             value,
                                             0,
                                             MaxMinutes,
                                             $"Minutes number is out of valid range (0-{MaxMinutes}).");

                _minutes = value;
            }
        }

        /// <summary>
        /// Gets or sets number of seconds.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Seconds number is out of valid range (0-59).</exception>
        public byte Seconds
        {
            get { return _seconds; }
            set
            {
                ThrowIfArgument.IsOutOfRange(nameof(value),
                                             value,
                                             0,
                                             MaxSeconds,
                                             $"Seconds number is out of valid range (0-{MaxSeconds}).");

                _seconds = value;
            }
        }

        /// <summary>
        /// Gets or sets number of frames.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Frames number is out of valid range.</exception>
        /// <remarks>
        /// Maximum valid value for the <see cref="Frames"/> depends on the frame rate specified by the
        /// <see cref="Format"/>: 23 for <see cref="SmpteFormat.TwentyFour"/>, 24 for <see cref="SmpteFormat.TwentyFive"/>,
        /// 28 for <see cref="SmpteFormat.ThirtyDrop"/> and 29 for <see cref="SmpteFormat.Thirty"/>.
        /// </remarks>
        public byte Frames
        {
            get { return _frames; }
            set
            {
                var maxFrames = MaxFrames[Format];
                ThrowIfArgument.IsOutOfRange(nameof(value),
                                             value,
                                             0,
                                             maxFrames,
                                             $"Frames number is out of valid range (0-{maxFrames}).");

                _frames = value;
            }
        }

        /// <summary>
        /// Gets or sets number of sub-frames.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Sub-frames number is out of valid range (0-99).</exception>
        public byte SubFrames
        {
            get { return _subFrames; }
            set
            {
                ThrowIfArgument.IsOutOfRange(nameof(value),
                                             value,
                                             0,
                                             MaxSubFrames,
                                             $"Sub-frames number is out of valid range (0-{MaxSubFrames}).");

                _subFrames = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets SMPTE format from a byte containing format and number of hours.
        /// </summary>
        /// <param name="formatAndHours">Byte containing format and number of hours.</param>
        /// <returns>SMPTE format in terms of frame rate.</returns>
        internal static SmpteFormat GetFormat(byte formatAndHours)
        {
            return Formats[(formatAndHours & FormatMask) >> FormatOffset];
        }

        /// <summary>
        /// Gets number of hours from a byte containing format and number of hours.
        /// </summary>
        /// <param name="formatAndHours">Byte containing format and number of hours.</param>
        /// <returns>Number of hours.</returns>
        internal static byte GetHours(byte formatAndHours)
        {
            return (byte)(formatAndHours & HoursMask);
        }

        /// <summary>
        /// Gets byte containing SMPTE format and number of hours.
        /// </summary>
        /// <returns>Byte containing SMPTE format and number of hours.</returns>
        internal byte GetFormatAndHours()
        {
            var format = Formats.First(f => f.Value == Format).Key << FormatOffset;
            return (byte)(format & Hours);
        }

        private static byte ProcessValue(byte value, string property, byte max, InvalidMetaEventParameterValuePolicy policy)
        {
            if (value <= max)
                return value;

            switch (policy)
            {
                case InvalidMetaEventParameterValuePolicy.Abort:
                    throw new InvalidMetaEventParameterValueException($"{value} is invalid value for the {property} of a SMPTE Offset event.", value);
                case InvalidMetaEventParameterValuePolicy.SnapToLimits:
                    return Math.Min(value, max);
            }

            return value;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a MIDI meta event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            var invalidMetaEventParameterValuePolicy = settings.InvalidMetaEventParameterValuePolicy;

            var formatAndHours = reader.ReadByte();
            Format = GetFormat(formatAndHours);
            Hours = ProcessValue(GetHours(formatAndHours),
                                 nameof(Hours),
                                 MaxHours,
                                 invalidMetaEventParameterValuePolicy);
            Minutes = ProcessValue(reader.ReadByte(),
                                   nameof(Minutes),
                                   MaxMinutes,
                                   invalidMetaEventParameterValuePolicy);
            Seconds = ProcessValue(reader.ReadByte(),
                                   nameof(Seconds),
                                   MaxSeconds,
                                   invalidMetaEventParameterValuePolicy);
            Frames = ProcessValue(reader.ReadByte(),
                                  nameof(Frames),
                                  MaxFrames[Format],
                                  invalidMetaEventParameterValuePolicy);
            SubFrames = ProcessValue(reader.ReadByte(),
                                     nameof(SubFrames),
                                     MaxSubFrames,
                                     invalidMetaEventParameterValuePolicy);
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(GetFormatAndHours());
            writer.WriteByte(Minutes);
            writer.WriteByte(Seconds);
            writer.WriteByte(Frames);
            writer.WriteByte(SubFrames);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize(WritingSettings settings)
        {
            return 5;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new SmpteOffsetEvent(Format, Hours, Minutes, Seconds, Frames, SubFrames);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"SMPTE Offset ({Format}, {Hours}:{Minutes}:{Seconds}:{Frames}:{SubFrames})";
        }

        #endregion
    }
}
