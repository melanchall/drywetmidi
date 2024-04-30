using System;
using System.Collections.Generic;
using System.IO;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Provides a way to read a MIDI file sequentially token by token keeping
    /// low memory consumption. See
    /// <see href="xref:a_file_lazy_reading_writing">Lazy reading/writing</see> article to learn more.
    /// </summary>
    /// <seealso cref="MidiTokensWriter"/>
    public sealed class MidiTokensReader : IDisposable
    {
        #region Enums

        private enum State
        {
            Initial = 0,
            ChunkHeader,
            ChunkContent
        }

        private enum InstructionType
        {
            Read,
            ReturnToken
        }

        #endregion

        #region Nested classes

        private sealed class Instruction
        {
            public static readonly Instruction Read = new Instruction(InstructionType.Read, null);

            private Instruction(InstructionType instructionType, MidiToken midiToken)
            {
                InstructionType = instructionType;
                Token = midiToken;
            }

            public InstructionType InstructionType { get; }

            public MidiToken Token { get; }

            public static Instruction ReturnToken(MidiToken midiToken)
            {
                return new Instruction(InstructionType.ReturnToken, midiToken);
            }
        }

        #endregion

        #region Constants

        private static readonly Dictionary<State, State> StatesTransitions = new Dictionary<State, State>
        {
            [State.Initial] = State.ChunkHeader,
            [State.ChunkHeader] = State.ChunkContent,
            [State.ChunkContent] = State.ChunkHeader,
        };

        #endregion

        #region Fields

        private readonly Stream _stream;
        private readonly MidiReader _reader;
        private readonly ReadingSettings _settings;
        private readonly bool _disposeStream;

        private State _state = State.Initial;
        private long? _smfEndPosition = null;
        private string _chunkId = null;
        private uint _chunkSize;

        private long _endReaderPosition;
        private byte? _currentChannelEventStatusByte;

        private bool _disposed;

        #endregion

        #region Constructor

        internal MidiTokensReader(Stream stream, ReadingSettings settings, bool disposeStream)
        {
            _stream = stream;
            if (!_stream.CanRead)
                throw new ArgumentException("Stream doesn't support reading.", nameof(stream));

            _settings = settings;
            if (_settings == null)
                _settings = new ReadingSettings();

            if (_settings.ReaderSettings == null)
                _settings.ReaderSettings = new ReaderSettings();

            _reader = new MidiReader(stream, _settings.ReaderSettings);
            if (_reader.EndReached)
                throw new ArgumentException("Stream is already read.", nameof(stream));

            _disposeStream = disposeStream;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads a next MIDI token from the underlying stream.
        /// </summary>
        /// <returns>An instance of the <see cref="MidiToken"/>.</returns>
        /// <exception cref="IOException">An I/O error occurred while reading the file.</exception>
        /// <exception cref="NoHeaderChunkException">There is no header chunk in a file and that should be treated as error
        /// according to the <see cref="ReadingSettings.NoHeaderChunkPolicy"/> of the used settings.</exception>
        /// <exception cref="InvalidChunkSizeException">Actual header or track chunk's size differs from the one declared
        /// in its header and that should be treated as error according to the <see cref="ReadingSettings.InvalidChunkSizePolicy"/>
        /// of the used settings.</exception>
        /// <exception cref="UnknownChunkException">Chunk to be read has unknown ID and that
        /// should be treated as error according to the <see cref="ReadingSettings.UnknownChunkIdPolicy"/> of the
        /// used settings.</exception>
        /// <exception cref="UnexpectedTrackChunksCountException">Actual track chunks
        /// count differs from the expected one (declared in the file header) and that should be treated as error according to
        /// the <see cref="ReadingSettings.UnexpectedTrackChunksCountPolicy"/> of the used settings.</exception>
        /// <exception cref="UnknownFileFormatException">The header chunk of the file specifies unknown file format and
        /// that should be treated as error according to the <see cref="ReadingSettings.UnknownFileFormatPolicy"/> of
        /// the used settings.</exception>
        /// <exception cref="InvalidChannelEventParameterValueException">Value of a channel event's parameter
        /// just read is invalid (is out of [0; 127] range) and that should be treated as error according to the
        /// <see cref="ReadingSettings.InvalidChannelEventParameterValuePolicy"/> of the used settings.</exception>
        /// <exception cref="InvalidMetaEventParameterValueException">Value of a meta event's parameter
        /// just read is invalid and that should be treated as error according to the
        /// <see cref="ReadingSettings.InvalidMetaEventParameterValuePolicy"/> of the used settings.</exception>
        /// <exception cref="UnknownChannelEventException">Reader has encountered an unknown channel event and that
        /// should be treated as error according to the <see cref="ReadingSettings.UnknownChannelEventPolicy"/> of
        /// the used settings.</exception>
        /// <exception cref="NotEnoughBytesException">MIDI file data cannot be read since the reader's underlying stream doesn't
        /// have enough bytes and that should be treated as error according to the <see cref="ReadingSettings.NotEnoughBytesPolicy"/>
        /// of the used settings.</exception>
        /// <exception cref="UnexpectedRunningStatusException">Unexpected running status is encountered.</exception>
        /// <exception cref="MissedEndOfTrackEventException">Track chunk doesn't end with <c>End Of Track</c> event and that
        /// should be treated as error according to the <see cref="ReadingSettings.MissedEndOfTrackPolicy"/> of
        /// the used settings.</exception>
        /// <exception cref="InvalidOperationException"><see cref="ReaderSettings.Buffer"/> of the used settings
        /// is <c>null</c> in case of <see cref="ReaderSettings.BufferingPolicy"/> set to
        /// <see cref="BufferingPolicy.UseCustomBuffer"/>.</exception>
        public MidiToken ReadToken()
        {
            Instruction instruction = null;

            var startPosition = _reader.Position;

            do
            {
                if (_reader.EndReached || (_smfEndPosition != null && _reader.Position >= _smfEndPosition))
                    return null;

                try
                {
                    switch (_state)
                    {
                        case State.Initial:
                            instruction = ProcessInitialState();
                            break;
                        case State.ChunkHeader:
                            instruction = ProcessChunkHeaderState();
                            break;
                        case State.ChunkContent:
                            instruction = ProcessChunkContentState();
                            break;
                    }
                }
                catch (NotEnoughBytesException ex)
                {
                    ReactOnNotEnoughBytes(_settings.NotEnoughBytesPolicy, ex);
                    return null;
                }
                catch (EndOfStreamException ex)
                {
                    ReactOnNotEnoughBytes(_settings.NotEnoughBytesPolicy, ex);
                    return null;
                }
            }
            while (instruction.InstructionType == InstructionType.Read);

            var token = instruction.Token;
            if (token != null)
            {
                token.Position = startPosition;
                token.Length = _reader.Position - startPosition;
            }

            return token;
        }

        private static void ReactOnNotEnoughBytes(NotEnoughBytesPolicy policy, Exception exception)
        {
            if (policy == NotEnoughBytesPolicy.Abort)
                throw new NotEnoughBytesException("MIDI file cannot be read since the reader's underlying stream doesn't have enough bytes.", exception);
        }

        private Instruction ProcessInitialState()
        {
            MidiFileReadingUtilities.ReadRmidPreamble(_reader, out _smfEndPosition);
            GoToNextState();
            return Instruction.Read;
        }

        private Instruction ProcessChunkHeaderState()
        {
            _chunkId = _reader.ReadString(MidiChunk.IdLength);
            if (_chunkId.Length < MidiChunk.IdLength)
            {
                switch (_settings.NotEnoughBytesPolicy)
                {
                    case NotEnoughBytesPolicy.Abort:
                        throw new NotEnoughBytesException(
                            "Chunk ID cannot be read since the reader's underlying stream doesn't have enough bytes.",
                            MidiChunk.IdLength,
                            _chunkId.Length);
                    case NotEnoughBytesPolicy.Ignore:
                        return Instruction.ReturnToken(null);
                }
            }

            //

            long readerPosition;
            _chunkSize = MidiChunk.ReadSize(_reader, out readerPosition);
            _endReaderPosition = _reader.Position + _chunkSize;
            _currentChannelEventStatusByte = null;

            GoToNextState();
            return Instruction.ReturnToken(new ChunkHeaderToken(_chunkId, _chunkSize));
        }

        private Instruction ProcessChunkContentState()
        {
            switch (_chunkId)
            {
                case HeaderChunk.Id:
                    {
                        ushort fileFormat;
                        ushort tracksNumber;
                        TimeDivision timeDivision;
                        HeaderChunk.ReadData(_reader, _settings, out fileFormat, out timeDivision, out tracksNumber);

                        GoToNextState();
                        return Instruction.ReturnToken(new FileHeaderToken(fileFormat, timeDivision, tracksNumber));
                    }
                case TrackChunk.Id:
                    {
                        if (CanReadChunkData())
                        {
                            var midiEvent = TrackChunk.ReadEvent(_reader, _settings, ref _currentChannelEventStatusByte);
                            if (midiEvent != null)
                                return Instruction.ReturnToken(new MidiEventToken(midiEvent));
                        }
                    }
                    break;
                default:
                    {
                        if (CanReadChunkData())
                        {
                            var packetSize = _settings.ReaderSettings.BytesPacketMaxLength;
                            var remainingBytesCount = (int)(_endReaderPosition - _reader.Position);
                            var data = _reader.ReadBytes(remainingBytesCount < packetSize ? remainingBytesCount : packetSize);
                            return Instruction.ReturnToken(new BytesPacketToken(data));
                        }
                    }
                    break;
            }

            GoToNextState();
            return Instruction.Read;
        }

        private bool CanReadChunkData()
        {
            return _reader.Position < _endReaderPosition && !_reader.EndReached;
        }

        private void GoToNextState()
        {
            _state = StatesTransitions[_state];
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current <see cref="MidiTokensReader"/>.
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
                _reader.Dispose();
                if (_disposeStream)
                    _stream.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
