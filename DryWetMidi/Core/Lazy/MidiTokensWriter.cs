using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Provides a way to write data to a MIDI file sequentially token by token keeping
    /// low memory consumption. See
    /// <see href="xref:a_file_lazy_reading_writing">Lazy reading/writing</see> article to learn more.
    /// </summary>
    /// <seealso cref="MidiTokensReader"/>
    public sealed class MidiTokensWriter : IDisposable, IAsyncDisposable
    {
        private sealed class CallbackScope
        {
            internal bool Active = true;
        }

        #region Enums

        private enum State
        {
            FileHeader = 0,
            Chunk,
            Event,
        }

        #endregion

        #region Fields

        private readonly Stream _stream;
        private MidiWriter _writer;
        private readonly WritingSettings _settings;
        private readonly bool _disposeStream;
        private readonly long _streamStartPosition;

        private State _state = State.FileHeader;

        private byte? _runningStatus;
        private bool _skipSetTempo;
        private bool _skipKeySignature;
        private bool _skipTimeSignature;
        private long _additionalDeltaTime;
        private bool _lastEventIsEndOfTrack;
        private long _trackChunkPosition;

        private readonly Dictionary<long, uint> _positionsToChunkSizes = new Dictionary<long, uint>();

        private long _tracksNumberPosition;
        private ushort _tracksNumber;

        private bool _disposed;
        private int _operation;
        private int _callbackThread;
        private readonly AsyncLocal<CallbackScope?> _callbackScope = new AsyncLocal<CallbackScope?>();
        private int _callbackWrite;
        private bool _suppressEndCallback;

        #endregion

        #region Constructor

        internal MidiTokensWriter(
            Stream stream,
            WritingSettings? settings,
            bool disposeStream)
        {
            _stream = stream;
            if (!_stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            if (!_stream.CanSeek)
                throw new ArgumentException("Stream doesn't support seeking.", nameof(stream));

            _streamStartPosition = _stream.Position;

            _settings = settings ?? new WritingSettings();

            if (_settings.WriterSettings == null)
                _settings.WriterSettings = new WriterSettings();

            _writer = new MidiWriter(_stream, _settings.WriterSettings);
            _disposeStream = disposeStream;
        }

        #endregion

        #region Properties

        internal long CurrentTime { get; private set; }

        internal Action? BeforeEndTrackChunk { get; set; }

        internal Func<CancellationToken, ValueTask>? BeforeEndTrackChunkAsync { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Starts a new track chunk writing the chunk's header.
        /// </summary>
        /// <remarks>
        /// If the current track chunk is not ended properly via <see cref="EndTrackChunk"/>,
        /// the <see cref="EndTrackChunk"/> method will be called automatically on <see cref="StartTrackChunk"/>
        /// called.
        /// </remarks>
        /// <exception cref="IOException">An I/O error occurred while writing the file.</exception>
        public void StartTrackChunk()
        {
            Execute(StartTrackChunkCore);
        }

        private void StartTrackChunkCore()
        {
            if (_state == State.Event)
                EndTrackChunkCore();

            _trackChunkPosition = _writer.Length;
            MidiChunk.WriteHeader(TrackChunk.Id, 0, _writer, _settings);

            _runningStatus = null;
            _skipSetTempo = true;
            _skipKeySignature = true;
            _skipTimeSignature = true;
            _additionalDeltaTime = 0L;
            _lastEventIsEndOfTrack = false;

            CurrentTime = 0;

            _state = State.Event;
        }

        /// <summary>
        /// Ends the current track chunk.
        /// </summary>
        /// <exception cref="IOException">An I/O error occurred while writing the file.</exception>
        public void EndTrackChunk()
        {
            Execute(EndTrackChunkCore);
        }

        private void EndTrackChunkCore()
        {
            _callbackThread = Environment.CurrentManagedThreadId;
            try
            {
                if (!_suppressEndCallback)
                    BeforeEndTrackChunk?.Invoke();
            }
            finally
            {
                _callbackThread = 0;
            }

            var endOfTrackEvent = new EndOfTrackEvent
            {
                DeltaTime = _lastEventIsEndOfTrack ? _additionalDeltaTime : 0
            };
            var endOfTrackEventWriter = EventWriterFactory.GetWriter(endOfTrackEvent);

            _writer.WriteVlqNumber(endOfTrackEvent.DeltaTime);
            endOfTrackEventWriter.Write(endOfTrackEvent, _writer, _settings, true);

            _positionsToChunkSizes.Add(
                _trackChunkPosition + MidiChunk.IdLength,
                (uint)(_writer.Length - (_trackChunkPosition + MidiChunk.IdLength + 4)));

            _tracksNumber++;

            _state = State.Chunk;
        }

        /// <summary>
        /// Writes the specified MIDI event.
        /// </summary>
        /// <param name="midiEvent"></param>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is <c>null</c>.</exception>
        /// <exception cref="IOException">An I/O error occurred while writing the file.</exception>
        /// <exception cref="InvalidOperationException">
        /// One of the following errors occurred:
        /// <list type="bullet">
        /// <item>
        /// <description>A track chunk is not started (see <see cref="StartTrackChunk"/>).</description>
        /// </item>
        /// <item>
        /// <description><paramref name="midiEvent"/> is of <see cref="EndOfTrackEvent"/> type but
        /// previously written event is of the same type, writing of two End of Track events
        /// is prohibited.</description>
        /// </item>
        /// </list>
        /// </exception>
        public void WriteEvent(MidiEvent midiEvent)
        {
            if (_callbackThread == Environment.CurrentManagedThreadId)
            {
                WriteCallbackEvent(midiEvent);
                return;
            }

            Execute(() => WriteEventCore(midiEvent));
        }

        private void WriteEventCore(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

            if (_state != State.Event)
                throw new InvalidOperationException("A track chunk is not started.");

            if (_lastEventIsEndOfTrack)
                throw new InvalidOperationException("Last written event is 'End of Track' one, so it's not possible to write an event after it.");

            TrackChunk.ProcessEvent(
                midiEvent,
                _settings,
                (eventWriter, e, writeStatusByte) =>
                {
                    _writer.WriteVlqNumber(e.DeltaTime);
                    eventWriter.Write(e, _writer, _settings, writeStatusByte);
                },
                ref _lastEventIsEndOfTrack,
                ref _additionalDeltaTime,
                ref _runningStatus,
                ref _skipSetTempo,
                ref _skipKeySignature,
                ref _skipTimeSignature);

            CurrentTime += midiEvent.DeltaTime;
        }

        /// <summary>
        /// Writes the specified chunk.
        /// </summary>
        /// <param name="chunk">A chunk to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="chunk"/> is <c>null</c>.</exception>
        /// <exception cref="IOException">An I/O error occurred while writing the file.</exception>
        /// <exception cref="InvalidOperationException">Another chunk is being written, end it first.</exception>
        public void WriteChunk(MidiChunk chunk)
        {
            Execute(() => WriteChunkCore(chunk));
        }

        private void WriteChunkCore(MidiChunk chunk)
        {
            ThrowIfArgument.IsNull(nameof(chunk), chunk);

            if (_state != State.Chunk)
                throw new InvalidOperationException("Another chunk is being written, end it first.");

            chunk.Write(_writer, _settings);
        }

        /// <summary>
        /// Starts a track chunk using asynchronous stream writes.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <inheritdoc cref="StartTrackChunk" path="/member/exception"/>
        /// <exception cref="ObjectDisposedException">The writer or its underlying stream is disposed.</exception>
        /// <exception cref="InvalidOperationException">Another writer operation is in progress.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public Task StartTrackChunkAsync(CancellationToken cancellationToken = default) =>
            ExecuteTrackAsync(() => StartTrackChunkCoreAsync(cancellationToken), cancellationToken);

        /// <summary>
        /// Ends a track chunk using asynchronous stream writes.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <inheritdoc cref="EndTrackChunk" path="/member/exception"/>
        /// <exception cref="ObjectDisposedException">The writer or its underlying stream is disposed.</exception>
        /// <exception cref="InvalidOperationException">Another writer operation is in progress.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public Task EndTrackChunkAsync(CancellationToken cancellationToken = default) =>
            ExecuteTrackAsync(() => EndTrackChunkCoreAsync(cancellationToken), cancellationToken, alwaysEnd: true);

        /// <summary>
        /// Writes an event using asynchronous stream writes.
        /// </summary>
        /// <param name="midiEvent">The event to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <remarks>Operations must not overlap. Dispose the writer after a failed or canceled write.</remarks>
        /// <inheritdoc cref="WriteEvent" path="/member/exception"/>
        /// <exception cref="ObjectDisposedException">The writer or its underlying stream is disposed.</exception>
        /// <exception cref="InvalidOperationException">Another writer or callback write is in progress.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public Task WriteEventAsync(MidiEvent midiEvent, CancellationToken cancellationToken = default)
        {
            if (_callbackScope.Value?.Active == true)
                return WriteCallbackEventAsync(midiEvent, cancellationToken);

            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

            return ExecuteAsync(() => WriteEventCoreAsync(midiEvent, cancellationToken), cancellationToken);
        }

        private async Task WriteCallbackEventAsync(MidiEvent midiEvent, CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _callbackWrite, 1, 0) != 0)
                throw new InvalidOperationException("Another callback write is already in progress.");

            try
            {
                ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);
                await WriteEventCoreAsync(midiEvent, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                Volatile.Write(ref _callbackWrite, 0);
            }
        }

        private void WriteCallbackEvent(MidiEvent midiEvent)
        {
            if (_disposed || (_callbackThread != Environment.CurrentManagedThreadId &&
                _callbackScope.Value?.Active != true))
                throw new InvalidOperationException("The track finalization callback has already completed.");

            if (Interlocked.CompareExchange(ref _callbackWrite, 1, 0) != 0)
                throw new InvalidOperationException("Another callback write is already in progress.");
            try
            {
                WriteEventCore(midiEvent);
            }
            finally
            {
                Volatile.Write(ref _callbackWrite, 0);
            }
        }

        /// <summary>
        /// Writes a chunk using asynchronous stream writes.
        /// </summary>
        /// <param name="chunk">The chunk to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <inheritdoc cref="WriteChunk" path="/member/exception"/>
        /// <exception cref="ObjectDisposedException">The writer or its underlying stream is disposed.</exception>
        /// <exception cref="InvalidOperationException">Another writer operation is in progress.</exception>
        /// <exception cref="TooManyTrackChunksException">The track count exceeds the MIDI file limit.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
        public async Task WriteChunkAsync(MidiChunk chunk, CancellationToken cancellationToken = default)
        {
            EnterOperation();
            try
            {
                ThrowIfArgument.IsNull(nameof(chunk), chunk);
                if (_state != State.Chunk)
                    throw new InvalidOperationException("Another chunk is being written, end it first.");

                if (chunk is TrackChunk && _tracksNumber == ushort.MaxValue)
                    throw new TooManyTrackChunksException((int)_tracksNumber + 1);

                await chunk.WriteAsync(_writer, _settings, cancellationToken).ConfigureAwait(false);
                if (chunk is TrackChunk)
                    _tracksNumber++;
            }
            finally
            {
                Volatile.Write(ref _operation, 0);
            }
        }

        private void EnterOperation()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MidiTokensWriter));

            if (Interlocked.CompareExchange(ref _operation, 1, 0) != 0)
                throw new InvalidOperationException("Another writer operation is already in progress.");
        }

        private void Execute(Action action)
        {
            EnterOperation();
            try
            {
                action();
            }
            finally
            {
                Volatile.Write(ref _operation, 0);
            }
        }

        private async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken)
        {
            EnterOperation();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await action().ConfigureAwait(false);
            }
            finally
            {
                Volatile.Write(ref _operation, 0);
            }
        }

        internal Task InitializeAsync(MidiFileFormat format, TimeDivision timeDivision, CancellationToken cancellationToken) =>
            ExecuteAsync(() => WriteFileHeaderAsync(format, timeDivision, cancellationToken), cancellationToken);

        private async Task ExecuteTrackAsync(
            Func<Task> action,
            CancellationToken cancellationToken,
            bool alwaysEnd = false)
        {
            EnterOperation();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if ((alwaysEnd || _state == State.Event) && BeforeEndTrackChunkAsync != null)
                {
                    var scope = new CallbackScope();
                    _callbackScope.Value = scope;
                    try
                    {
                        await BeforeEndTrackChunkAsync(cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        scope.Active = false;
                        _callbackScope.Value = null;
                    }

                    _suppressEndCallback = true;
                }

                try
                {
                    await action().ConfigureAwait(false);
                }
                finally
                {
                    _suppressEndCallback = false;
                }
            }
            finally
            {
                Volatile.Write(ref _operation, 0);
            }
        }

        internal void WriteFileHeader(MidiFileFormat format, TimeDivision timeDivision)
        {
            var headerChunk = new HeaderChunk
            {
                FileFormat = (ushort)format,
                TimeDivision = timeDivision,
                TracksNumber = 0
            };
            headerChunk.Write(_writer, _settings);

            _tracksNumberPosition = _writer.Length - 4;

            _state = State.Chunk;
        }

        private async Task WriteFileHeaderAsync(
            MidiFileFormat format,
            TimeDivision timeDivision,
            CancellationToken cancellationToken)
        {
            var headerChunk = new HeaderChunk
            {
                FileFormat = (ushort)format,
                TimeDivision = timeDivision,
                TracksNumber = 0
            };
            await headerChunk.WriteAsync(_writer, _settings, cancellationToken).ConfigureAwait(false);

            _tracksNumberPosition = _writer.Length - 4;
            _state = State.Chunk;
        }

        private async Task StartTrackChunkCoreAsync(CancellationToken cancellationToken)
        {
            if (_state == State.Event)
                await EndTrackChunkCoreAsync(cancellationToken).ConfigureAwait(false);

            _trackChunkPosition = _writer.Length;
            await MidiChunk.WriteHeaderAsync(TrackChunk.Id, 0, _writer, cancellationToken).ConfigureAwait(false);

            _runningStatus = null;
            _skipSetTempo = true;
            _skipKeySignature = true;
            _skipTimeSignature = true;
            _additionalDeltaTime = 0;
            _lastEventIsEndOfTrack = false;
            CurrentTime = 0;
            _state = State.Event;
        }

        private async Task EndTrackChunkCoreAsync(CancellationToken cancellationToken)
        {
            _callbackThread = Environment.CurrentManagedThreadId;
            try
            {
                if (!_suppressEndCallback)
                    BeforeEndTrackChunk?.Invoke();
            }
            finally
            {
                _callbackThread = 0;
            }

            var endOfTrackEvent = new EndOfTrackEvent
            {
                DeltaTime = _lastEventIsEndOfTrack ? _additionalDeltaTime : 0
            };
            await _writer.WriteVlqNumberAsync(endOfTrackEvent.DeltaTime, cancellationToken).ConfigureAwait(false);
            await EventWriterFactory.GetWriter(endOfTrackEvent).WriteAsync(
                endOfTrackEvent,
                _writer,
                _settings,
                true,
                cancellationToken).ConfigureAwait(false);

            _positionsToChunkSizes.Add(
                _trackChunkPosition + MidiChunk.IdLength,
                (uint)(_writer.Length - (_trackChunkPosition + MidiChunk.IdLength + 4)));
            _tracksNumber++;
            _state = State.Chunk;
        }

        private async Task WriteEventCoreAsync(MidiEvent midiEvent, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_state != State.Event)
                throw new InvalidOperationException("A track chunk is not started.");

            if (_lastEventIsEndOfTrack)
                throw new InvalidOperationException("Last written event is 'End of Track' one, so it's not possible to write an event after it.");

            IEventWriter? eventWriter = null;
            MidiEvent? eventToWrite = null;
            var writeStatus = true;
            TrackChunk.ProcessEvent(
                midiEvent,
                _settings,
                (writer, e, status) =>
                {
                    eventWriter = writer;
                    eventToWrite = e;
                    writeStatus = status;
                },
                ref _lastEventIsEndOfTrack,
                ref _additionalDeltaTime,
                ref _runningStatus,
                ref _skipSetTempo,
                ref _skipKeySignature,
                ref _skipTimeSignature);

            if (eventToWrite != null && eventWriter != null)
            {
                await _writer.WriteVlqNumberAsync(eventToWrite.DeltaTime, cancellationToken).ConfigureAwait(false);
                await eventWriter.WriteAsync(eventToWrite, _writer, _settings, writeStatus, cancellationToken).ConfigureAwait(false);
            }

            CurrentTime += midiEvent.DeltaTime;
        }

        private void UpdateChunkSizes()
        {
            var buffer = new byte[4];

            foreach (var positionToChunkSize in _positionsToChunkSizes)
            {
                _stream.Position = positionToChunkSize.Key;

                buffer[0] = (byte)((positionToChunkSize.Value >> 24) & 0xFF);
                buffer[1] = (byte)((positionToChunkSize.Value >> 16) & 0xFF);
                buffer[2] = (byte)((positionToChunkSize.Value >> 8) & 0xFF);
                buffer[3] = (byte)(positionToChunkSize.Value & 0xFF);

                _stream.Write(buffer, 0, buffer.Length);
            }

            _stream.Flush();
        }

        private void UpdateTracksNumber()
        {
            var buffer = new byte[2];

            buffer[0] = (byte)((_tracksNumber >> 8) & 0xFF);
            buffer[1] = (byte)(_tracksNumber & 0xFF);

            _stream.Position = _tracksNumberPosition;
            _stream.Write(buffer, 0, 2);
            _stream.Flush();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current <see cref="MidiTokensWriter"/>
        /// and also flushes all remaining data to the underlying stream.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
                Execute(() => Dispose(true));
        }

        /// <summary>
        /// Finishes an open track, patches chunk sizes and track count, flushes
        /// asynchronously, and disposes the stream if the writer owns it.
        /// </summary>
        /// <returns>A task representing finalization.</returns>
        /// <exception cref="InvalidOperationException">Another writer operation is in progress.</exception>
        /// <exception cref="ObjectDisposedException">The underlying stream is disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred while finalizing the file.</exception>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            EnterOperation();
            try
            {
                try
                {
                    if (_state == State.Event)
                    {
                        if (BeforeEndTrackChunkAsync != null)
                        {
                            var scope = new CallbackScope();
                            _callbackScope.Value = scope;
                            try
                            {
                                await BeforeEndTrackChunkAsync(CancellationToken.None).ConfigureAwait(false);
                            }
                            finally
                            {
                                scope.Active = false;
                                _callbackScope.Value = null;
                            }

                            _suppressEndCallback = true;
                        }

                        try
                        {
                            await EndTrackChunkCoreAsync(CancellationToken.None).ConfigureAwait(false);
                        }
                        finally
                        {
                            _suppressEndCallback = false;
                        }
                    }

                    await _writer.DisposeAsync().ConfigureAwait(false);
                    _stream.Position = _streamStartPosition + _tracksNumberPosition;
                    var tracks = new[] { (byte)(_tracksNumber >> 8), (byte)_tracksNumber };
                    await _stream.WriteAsync(tracks, 0, tracks.Length).ConfigureAwait(false);

                    foreach (var patch in _positionsToChunkSizes)
                    {
                        await _stream.FlushAsync().ConfigureAwait(false);
                        _stream.Position = _streamStartPosition + patch.Key;
                        var size = patch.Value;
                        var bytes = new[] { (byte)(size >> 24), (byte)(size >> 16), (byte)(size >> 8), (byte)size };
                        await _stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                    }

                    await _stream.FlushAsync().ConfigureAwait(false);
                }
                finally
                {
                    _disposed = true;
                    if (_disposeStream)
                        await _stream.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                Volatile.Write(ref _operation, 0);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                try
                {
                    if (_state == State.Event)
                        EndTrackChunkCore();

                    _writer.Dispose();

                    UpdateTracksNumber();
                    UpdateChunkSizes();
                }
                finally
                {
                    _disposed = true;
                    if (_disposeStream)
                        _stream.Dispose();
                }
            }

            _disposed = true;
        }

        #endregion
    }
}
