using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Provides a way to write data to a MIDI file sequentially token by token keeping
    /// low memory consumption. See
    /// <see href="xref:a_file_lazy_reading_writing">Lazy reading/writing</see> article to learn more.
    /// </summary>
    /// <seealso cref="MidiTokensReader"/>
    public sealed class MidiTokensWriter : IDisposable
    {
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
        private readonly MidiWriter _writer;
        private readonly WritingSettings _settings;
        private readonly bool _disposeStream;

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

        #endregion

        #region Constructor

        internal MidiTokensWriter(
            Stream stream,
            WritingSettings settings,
            bool disposeStream,
            MidiFileFormat format,
            TimeDivision timeDivision)
        {
            _stream = stream;
            if (!_stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));
            if (!_stream.CanSeek)
                throw new ArgumentException("Stream doesn't support seeking.", nameof(stream));

            _settings = settings;
            if (_settings == null)
                _settings = new WritingSettings();

            if (_settings.WriterSettings == null)
                _settings.WriterSettings = new WriterSettings();

            _writer = new MidiWriter(stream, _settings.WriterSettings);
            _disposeStream = disposeStream;

            WriteFileHeader(format, timeDivision);
        }

        #endregion

        #region Properties

        internal long CurrentTime { get; private set; }

        internal Action BeforeEndTrackChunk { get; set; }

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
            if (_state == State.Event)
                EndTrackChunk();

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
            BeforeEndTrackChunk?.Invoke();

            var endOfTrackEvent = new EndOfTrackEvent
            {
                DeltaTime = _lastEventIsEndOfTrack ? _additionalDeltaTime : 0
            };
            var endOfTrackEventWriter = EventWriterFactory.GetWriter(endOfTrackEvent);

            // TODO: put to constant in TrackChunk
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
            ThrowIfArgument.IsNull(nameof(chunk), chunk);

            if (_state != State.Chunk)
                throw new InvalidOperationException("Another chunk is being written, end it first.");

            chunk.Write(_writer, _settings);
        }

        private void WriteFileHeader(MidiFileFormat format, TimeDivision timeDivision)
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
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_state == State.Event)
                    EndTrackChunk();

                _writer.Dispose();

                UpdateTracksNumber();
                UpdateChunkSizes();

                if (_disposeStream)
                    _stream.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
