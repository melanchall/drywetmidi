using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class SmpteData
    {
        #region Constants

        public const byte MaxHours = 23;
        public const byte MaxMinutes = 59;
        public const byte MaxSeconds = 59;
        public const byte MaxSubFrames = 99;

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

        private static readonly SmpteFormat[] Formats = new SmpteFormat[]
        {
            SmpteFormat.TwentyFour,
            SmpteFormat.TwentyFive,
            SmpteFormat.ThirtyDrop,
            SmpteFormat.Thirty
        };

        #endregion

        #region Fields

        private SmpteFormat _format = SmpteFormat.TwentyFour;
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
            byte formatByte = 0;
            switch (smpteFormat)
            {
                case SmpteFormat.TwentyFive:
                    formatByte = 1;
                    break;
                case SmpteFormat.ThirtyDrop:
                    formatByte = 2;
                    break;
                case SmpteFormat.Thirty:
                    formatByte = 3;
                    break;
            }

            return (byte)((formatByte << FormatOffset) | hours);
        }

        #endregion
    }
}
