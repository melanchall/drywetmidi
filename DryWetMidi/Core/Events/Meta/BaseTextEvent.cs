using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a text meta event.
    /// </summary>
    /// <remarks>
    /// There are several meta events that have text content and the same structure. All these
    /// events are derived from <see cref="BaseTextEvent"/>.
    /// </remarks>
    public abstract class BaseTextEvent : MetaEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTextEvent"/>.
        /// </summary>
        protected BaseTextEvent(MidiEventType eventType)
            : base(eventType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTextEvent"/> with the specified text.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        /// <param name="text">Text contained in the event.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="eventType"/> specified an invalid value.</exception>
        protected BaseTextEvent(MidiEventType eventType, string text)
            : this(eventType)
        {
            Text = text;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets text contained in the event.
        /// </summary>
        public string Text { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a MIDI meta event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        /// <exception cref="ArgumentOutOfRangeException">Text event cannot be read since the size is
        /// negative number.</exception>
        protected sealed override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            ThrowIfArgument.IsNegative(
                nameof(size),
                size,
                "Text event cannot be read since the size is negative number.");

            if (size == 0)
            {
                switch (settings.ZeroLengthDataPolicy)
                {
                    case ZeroLengthDataPolicy.ReadAsEmptyObject:
                        Text = string.Empty;
                        break;
                    case ZeroLengthDataPolicy.ReadAsNull:
                        Text = null;
                        break;
                }

                return;
            }

            var bytes = reader.ReadBytes(size);
            if (bytes.Length != size && settings.NotEnoughBytesPolicy == NotEnoughBytesPolicy.Abort)
                throw new NotEnoughBytesException("Not enough bytes in the stream to read the text of a text based meta event.", size, bytes.Length);

            var encoding = settings.TextEncoding ?? SmfConstants.DefaultTextEncoding;

            var decodeTextCallback = settings.DecodeTextCallback;
            Text = decodeTextCallback != null
                ? decodeTextCallback(bytes, settings)
                : encoding.GetString(bytes);
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected sealed override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            var text = Text;
            if (string.IsNullOrEmpty(text))
                return;

            var encoding = settings.TextEncoding ?? SmfConstants.DefaultTextEncoding;
            var bytes = encoding.GetBytes(text);
            writer.WriteBytes(bytes);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        protected sealed override int GetContentSize(WritingSettings settings)
        {
            var text = Text;
            if (string.IsNullOrEmpty(text))
                return 0;

            var encoding = settings.TextEncoding ?? SmfConstants.DefaultTextEncoding;
            return encoding.GetByteCount(Text);
        }

        #endregion
    }
}
