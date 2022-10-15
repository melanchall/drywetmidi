using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Melanchall.DryWetMidi.Core
{
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

        #region Methods

        public void StartTrackChunk()
        {
            _trackChunkPosition = _writer.Length;
            MidiChunk.WriteHeader(TrackChunk.Id, 0, _writer, _settings);

            _runningStatus = null;
            _skipSetTempo = true;
            _skipKeySignature = true;
            _skipTimeSignature = true;
            _additionalDeltaTime = 0L;
            _lastEventIsEndOfTrack = false;

            _state = State.Event;
        }

        public void EndTrackChunk()
        {
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

            _state = State.Chunk;
        }

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
        }

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
                // TODO
                //TracksNumber = (ushort)trackChunksCount
            };
            headerChunk.Write(_writer, _settings);

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

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _writer.Dispose();
                UpdateChunkSizes();

                if (_disposeStream)
                    _stream.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
