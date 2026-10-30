using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    public sealed partial class MidiTokensReader
    {
        internal async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (await _reader.EndReachedAsync(cancellationToken).ConfigureAwait(false))
                throw new ArgumentException("Stream is already read.", "stream");
        }

        private async Task<MidiToken?> ReadTokenCoreAsync(CancellationToken cancellationToken)
        {
            var start = _reader.Position;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await _reader.EndReachedAsync(cancellationToken).ConfigureAwait(false) ||
                    (_smfEndPosition != null && _reader.Position >= _smfEndPosition))
                    return null;

                MidiToken? token = null;
                try
                {
                    switch (_state)
                    {
                        case State.Initial:
                            _smfEndPosition = await MidiFileReadingUtilities.ReadRmidPreambleAsync(_reader, cancellationToken).ConfigureAwait(false);
                            GoToNextState();
                            continue;
                        case State.ChunkHeader:
                            var id = await _reader.ReadBytesAsync(4, cancellationToken).ConfigureAwait(false);
                            if (id.Length < 4)
                            {
                                if (_settings.NotEnoughBytesPolicy == NotEnoughBytesPolicy.Abort)
                                    throw new NotEnoughBytesException("Not enough bytes to read a chunk ID.", 4, id.Length);

                                return null;
                            }

                            _chunkId = SmfConstants.DefaultTextEncoding.GetString(id);
                            _chunkSize = await _reader.ReadDwordAsync(cancellationToken).ConfigureAwait(false);
                            _endReaderPosition = _reader.Position + _chunkSize;
                            _currentChannelEventStatusByte = null;
                            GoToNextState();
                            token = new ChunkHeaderToken(_chunkId, _chunkSize);
                            break;
                        case State.ChunkContent:
                            if (_chunkId == HeaderChunk.Id)
                            {
                                var data = await HeaderChunk.ReadDataAsync(_reader, _settings, cancellationToken).ConfigureAwait(false);
                                GoToNextState();
                                token = new FileHeaderToken(data.FileFormat, data.TimeDivision, data.TracksNumber);
                            }
                            else if (_reader.Position < _endReaderPosition)
                            {
                                if (_chunkId == TrackChunk.Id)
                                {
                                    var result = await MidiFileReadingUtilities.ReadEventAsync(
                                        _reader,
                                        _settings,
                                        _currentChannelEventStatusByte,
                                        cancellationToken).ConfigureAwait(false);
                                    _currentChannelEventStatusByte = result.Status;
                                    if (result.Event != null)
                                        token = new MidiEventToken(result.Event);
                                    else
                                        continue;
                                }
                                else
                                {
                                    var size = (int)Math.Min(_endReaderPosition - _reader.Position, _settings.ReaderSettings.BytesPacketMaxLength);
                                    token = new BytesPacketToken(await _reader.ReadBytesAsync(size, cancellationToken).ConfigureAwait(false));
                                }
                            }

                            if (token == null)
                            {
                                GoToNextState();
                                continue;
                            }

                            break;
                        default:
                            throw new InvalidOperationException($"Unexpected state: {_state}");
                    }
                }
                catch (NotEnoughBytesException exception)
                {
                    ReactOnNotEnoughBytes(_settings.NotEnoughBytesPolicy, exception);
                    return null;
                }
                catch (EndOfStreamException exception)
                {
                    ReactOnNotEnoughBytes(_settings.NotEnoughBytesPolicy, exception);
                    return null;
                }

                token.Position = start;
                token.Length = _reader.Position - start;

                return token;
            }
        }
    }
}
