using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    internal sealed class SmpteData
    {
        #region Constants

        private const byte MaxHours = 23;
        private const byte MaxMinutes = 59;
        private const byte MaxSeconds = 59;
        private const byte MaxSubFrames = 99;

        private const int FormatMask = 0x60; // 01100000
        private const int FormatOffset = 5;
        private const int HoursMask = 0x1F; // 00011111

        private static readonly Dictionary<SmpteFormat, byte> MaxFrames = new Dictionary<SmpteFormat, byte>
        {
            [SmpteFormat.TwentyFour] = 23,
            [SmpteFormat.TwentyFive] = 24,
            [SmpteFormat.ThirtyDrop] = 28,
            [SmpteFormat.Thirty] = 29
        };

        private static readonly Dictionary<int, SmpteFormat> Formats = new Dictionary<int, SmpteFormat>
        {
            [0] = SmpteFormat.TwentyFour,
            [1] = SmpteFormat.TwentyFive,
            [2] = SmpteFormat.ThirtyDrop,
            [3] = SmpteFormat.Thirty
        };

        #endregion

        #region Fields

        private SmpteFormat _format;
        private byte _hours;
        private byte _minutes;
        private byte _seconds;
        private byte _frames;
        private byte _subFrames;

        #endregion

        #region Constructor

        public SmpteData()
        {
        }

        public SmpteData(SmpteFormat format, byte hours, byte minutes, byte seconds, byte frames, byte subFrames)
        {
            Format = format;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Frames = frames;
            SubFrames = subFrames;
        }

        #endregion

        #region Properties

        public SmpteFormat Format
        {
            get { return _format; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _format = value;
            }
        }

        public byte Hours
        {
            get { return _hours; }
            set
            {
                ThrowIfArgument.IsGreaterThan(nameof(value), value, MaxHours, $"Hours number is out of valid range (0-{MaxHours}).");

                _hours = value;
            }
        }

        public byte Minutes
        {
            get { return _minutes; }
            set
            {
                ThrowIfArgument.IsGreaterThan(nameof(value), value, MaxMinutes, $"Minutes number is out of valid range (0-{MaxMinutes}).");

                _minutes = value;
            }
        }

        public byte Seconds
        {
            get { return _seconds; }
            set
            {
                ThrowIfArgument.IsGreaterThan(nameof(value), value, MaxSeconds, $"Seconds number is out of valid range (0-{MaxSeconds}).");

                _seconds = value;
            }
        }

        public byte Frames
        {
            get { return _frames; }
            set
            {
                var maxFrames = MaxFrames[Format];
                ThrowIfArgument.IsGreaterThan(nameof(value), value, maxFrames, $"Frames number is out of valid range (0-{maxFrames}).");

                _frames = value;
            }
        }

        public byte SubFrames
        {
            get { return _subFrames; }
            set
            {
                ThrowIfArgument.IsGreaterThan(nameof(value), value, MaxSubFrames, $"Sub-frames number is out of valid range (0-{MaxSubFrames}).");

                _subFrames = value;
            }
        }

        #endregion

        #region Methods

        public static SmpteData Read(Func<byte> byteReader, Func<byte, string, byte, byte> valueProcessor)
        {
            var formatAndHours = byteReader();
            var format = GetFormat(formatAndHours);
            var hours = valueProcessor(GetHours(formatAndHours), nameof(Hours), MaxHours);
            var minutes = valueProcessor(byteReader(), nameof(Minutes), MaxMinutes);
            var seconds = valueProcessor(byteReader(), nameof(Seconds), MaxSeconds);
            var frames = valueProcessor(byteReader(), nameof(Frames), MaxFrames[format]);
            var subFrames = valueProcessor(byteReader(), nameof(SubFrames), MaxSubFrames);

            return new SmpteData(format, hours, minutes, seconds, frames, subFrames);
        }

        public void Write(Action<byte> byteWriter)
        {
            byteWriter(GetFormatAndHours());
            byteWriter(Minutes);
            byteWriter(Seconds);
            byteWriter(Frames);
            byteWriter(SubFrames);
        }

        internal static SmpteFormat GetFormat(byte formatAndHours)
        {
            return Formats[(formatAndHours & FormatMask) >> FormatOffset];
        }

        internal static byte GetHours(byte formatAndHours)
        {
            return (byte)(formatAndHours & HoursMask);
        }

        internal byte GetFormatAndHours()
        {
            return GetFormatAndHours(Format, Hours);
        }

        internal static byte GetFormatAndHours(SmpteFormat smpteFormat, byte hours)
        {
            var format = Formats.First(f => f.Value == smpteFormat).Key << FormatOffset;
            return (byte)(format & hours);
        }

        #endregion
    }
}
