using System;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
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
        /// Represents the largest possible hours value.
        /// </summary>
        public const byte MaxHours = SmpteData.MaxHours;

        /// <summary>
        /// Represents the largest possible minutes value.
        /// </summary>
        public const byte MaxMinutes = SmpteData.MaxMinutes;

        /// <summary>
        /// Represents the largest possible seconds value.
        /// </summary>
        public const byte MaxSeconds = SmpteData.MaxSeconds;

        /// <summary>
        /// Represents the largest possible sub-frames value.
        /// </summary>
        public const byte MaxSubFrames = SmpteData.MaxSubFrames;

        #endregion

        #region Fields

        private SmpteData _smpteData = new SmpteData();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SmpteOffsetEvent"/>.
        /// </summary>
        public SmpteOffsetEvent()
            : base(MidiEventType.SmpteOffset)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmpteOffsetEvent"/> with the
        /// specified SMPTE format, hours, minutes, seconds, number of frames and sub-frames.
        /// </summary>
        /// <param name="format">SMPTE format.</param>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="frames">Number of frames.</param>
        /// <param name="subFrames">Number of sub-frames.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="format"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="hours"/> is out of valid range.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="minutes"/> is out of valid range.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="seconds"/> is out of valid range.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="frames"/> is out of valid range.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="subFrames"/> is out of valid range.</description>
        /// </item>
        /// </list>
        /// </exception>
        public SmpteOffsetEvent(SmpteFormat format, byte hours, byte minutes, byte seconds, byte frames, byte subFrames)
            : this()
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

        /// <summary>
        /// Gets or sets SMPTE format.
        /// </summary>
        public SmpteFormat Format
        {
            get { return _smpteData.Format; }
            set { _smpteData.Format = value; }
        }

        /// <summary>
        /// Gets or sets number of hours.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Hours number is out of valid range (0-23).</exception>
        public byte Hours
        {
            get { return _smpteData.Hours; }
            set { _smpteData.Hours = value; }
        }

        /// <summary>
        /// Gets or sets number of minutes.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Minutes number is out of valid range (0-59).</exception>
        public byte Minutes
        {
            get { return _smpteData.Minutes; }
            set { _smpteData.Minutes = value; }
        }

        /// <summary>
        /// Gets or sets number of seconds.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Seconds number is out of valid range (0-59).</exception>
        public byte Seconds
        {
            get { return _smpteData.Seconds; }
            set { _smpteData.Seconds = value; }
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
            get { return _smpteData.Frames; }
            set { _smpteData.Frames = value; }
        }

        /// <summary>
        /// Gets or sets number of sub-frames.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Sub-frames number is out of valid range (0-99).</exception>
        public byte SubFrames
        {
            get { return _smpteData.SubFrames; }
            set { _smpteData.SubFrames = value; }
        }

        #endregion

        #region Methods

        private byte ProcessValue(byte value, string property, byte max, InvalidMetaEventParameterValuePolicy policy)
        {
            if (value <= max)
                return value;

            switch (policy)
            {
                case InvalidMetaEventParameterValuePolicy.Abort:
                    throw new InvalidMetaEventParameterValueException(EventType, property, value);
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
            _smpteData = SmpteData.Read(
                reader.ReadByte,
                (value, propertyName, max) => ProcessValue(value, propertyName, max, settings.InvalidMetaEventParameterValuePolicy));
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            _smpteData.Write(writer.WriteByte);
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
