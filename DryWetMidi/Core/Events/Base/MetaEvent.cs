using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a MIDI file meta event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Meta event specifies non-MIDI information useful to specific applications.
    /// </para>
    /// <para>
    /// As with <see cref="MidiChunk">chunks</see>, future meta events may be designed which may not be known to
    /// existing programs, so programs must properly ignore meta events which they do not recognize, and indeed,
    /// should expect to see them. Programs must never ignore the length of a meta-event which they do recognize,
    /// and they shouldn't be surprised if it's bigger than they expected. If so, they must ignore everything past
    /// what they know about. However, they must not add anything of their own to the end of a meta-event.
    /// </para>
    /// <para>
    /// You can create custom meta events that can be read from and written to a MIDI file.
    /// See <see href="xref:a_custom_meta_event">Custom meta events</see> article to learn more.
    /// </para>
    /// <para>
    /// See <see href="https://midi.org/standard-midi-files-specification"/> for detailed MIDI file specification.
    /// </para>
    /// </remarks>
    public abstract class MetaEvent : MidiEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaEvent"/>.
        /// </summary>
        protected MetaEvent()
            : this(MidiEventType.CustomMeta)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaEvent"/> with the specified event type.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        internal MetaEvent(MidiEventType eventType)
            : base(eventType)
        {
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        internal sealed override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            ReadContent(reader, settings, size);
        }

        /// <inheritdoc/>
        internal sealed override void Write(MidiWriter writer, WritingSettings settings)
        {
            WriteContent(writer, settings);
        }

        /// <inheritdoc/>
        internal sealed override Task ReadAsync(
            MidiReader reader,
            ReadingSettings settings,
            int size,
            CancellationToken cancellationToken)
        {
            return ReadContentAsync(reader, settings, size, cancellationToken);
        }

        /// <inheritdoc/>
        internal sealed override Task WriteAsync(
            MidiWriter writer,
            WritingSettings settings,
            CancellationToken cancellationToken)
        {
            return WriteContentAsync(writer, settings, cancellationToken);
        }

        /// <inheritdoc/>
        internal sealed override int GetSize(WritingSettings settings)
        {
            return GetContentSize(settings);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads content of a MIDI meta event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        /// <exception cref="System.ObjectDisposedException">Method was called after <paramref name="reader"/>
        /// was disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred on the <paramref name="reader"/>'s
        /// underlying stream.</exception>
        protected abstract void ReadContent(MidiReader reader, ReadingSettings settings, int size);

        /// <summary>
        /// Reads content of a MIDI meta event asynchronously.
        /// </summary>
        /// <param name="reader">Original reader to read the content with. The reader remains owned by the caller.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task representing reading the content.</returns>
        /// <remarks>
        /// Use asynchronous reader methods and pass <paramref name="cancellationToken"/> to them.
        /// The reader is positioned at the start of the payload. Any unread payload bytes are
        /// skipped by the caller after this method completes.
        /// </remarks>
        /// <exception cref="System.OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        /// <exception cref="System.ObjectDisposedException">Method was called after <paramref name="reader"/>
        /// was disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred on the <paramref name="reader"/>'s
        /// underlying stream.</exception>
        protected abstract Task ReadContentAsync(
            MidiReader reader,
            ReadingSettings settings,
            int size,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <exception cref="System.ObjectDisposedException">Method was called after <paramref name="writer"/>
        /// was disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred on the <paramref name="writer"/>'s
        /// underlying stream.</exception>
        protected abstract void WriteContent(MidiWriter writer, WritingSettings settings);

        /// <summary>
        /// Writes content of a MIDI meta event asynchronously.
        /// </summary>
        /// <param name="writer">Original writer to write the content with. The writer remains owned by the caller.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing the content.</returns>
        /// <remarks>
        /// Use asynchronous writer methods and pass <paramref name="cancellationToken"/> to them.
        /// Write only the payload, without the event's status or length.
        /// </remarks>
        /// <exception cref="System.OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        /// <exception cref="System.ObjectDisposedException">Method was called after <paramref name="writer"/>
        /// was disposed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred on the <paramref name="writer"/>'s
        /// underlying stream.</exception>
        protected abstract Task WriteContentAsync(
            MidiWriter writer,
            WritingSettings settings,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        protected abstract int GetContentSize(WritingSettings settings);

        #endregion
    }
}
