using System;
using System.Linq;

namespace Melanchall.DryMidi
{
    public sealed class TrackChunk : Chunk
    {
        #region Constants

        /// <summary>
        /// ID of the track chunk. This field is constsnt.
        /// </summary>
        public const string Id = "MTrk";

        #endregion

        #region Fields

        private byte? _currentStatusByte = null;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackChunk"/>.
        /// </summary>
        public TrackChunk()
            : base(Id)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of messages contained in the track chunk.
        /// </summary>
        public MessagesCollection Messages { get; } = new MessagesCollection();

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a <see cref="TrackChunk"/>.
        /// </summary>
        /// <remarks>
        /// Content of a track chunk is collection of MIDI messages.
        /// </remarks>
        /// <param name="reader">Reader to read the chunk's content with.</param>
        /// <param name="settings">Settings according to which the chunk's content must be read.</param>
        /// <param name="size">Expected size of the content taken from the chunk's header.</param>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
        {
            var endReaderPosition = reader.Position + size;
            var endOfTrackPresented = false;

            //

            while (reader.Position < endReaderPosition)
            {
                var message = ReadMessage(reader, settings);
                if (message is EndOfTrackMessage)
                {
                    endOfTrackPresented = true;
                    break;
                }

                Messages.Add(message);
            }

            _currentStatusByte = null;

            //

            if (settings.MissedEndOfTrackPolicy == MissedEndOfTrackPolicy.Abort && !endOfTrackPresented)
                throw new MissedEndOfTrackMessageException();
        }

        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            ProcessMessages(settings, (messageWriter, message, writeStatusByte) =>
            {
                writer.WriteVlqNumber(message.DeltaTime);
                messageWriter.Write(message, writer, settings, writeStatusByte);
            });
        }

        protected override uint GetContentSize(WritingSettings settings)
        {
            uint result = 0;

            ProcessMessages(settings, (messageWriter, message, writeStatusByte) =>
            {
                result += (uint)message.DeltaTime.GetVlqLength();
                result += (uint)messageWriter.CalculateSize(message, settings, writeStatusByte);
            });

            return result;
        }

        #endregion

        #region Methods

        private Message ReadMessage(MidiReader reader, ReadingSettings settings)
        {
            var deltaTime = reader.ReadVlqNumber();

            //

            var statusByte = reader.ReadByte();
            if (statusByte <= SevenBitNumber.MaxValue)
                reader.Position--;
            else
                _currentStatusByte = statusByte;

            //

            var messageReader = MessageReaderFactory.GetReader(_currentStatusByte.Value);
            var message = messageReader.Read(reader, settings, _currentStatusByte.Value);

            //

            if (settings.SilentNoteOnPolicy == SilentNoteOnPolicy.NoteOff)
            {
                var noteOnMessage = message as NoteOnMessage;
                if (noteOnMessage != null && noteOnMessage.Velocity == 0)
                {
                    message = new NoteOffMessage
                    {
                        DeltaTime = noteOnMessage.DeltaTime,
                        Channel = noteOnMessage.Channel,
                        NoteNumber = noteOnMessage.NoteNumber
                    };
                }
            }

            //

            message.DeltaTime = deltaTime;
            return message;
        }

        private void ProcessMessages(WritingSettings settings, Action<IMessageWriter, Message, bool> messageHandler)
        {
            byte? runningStatus = null;
            var writeStatusByte = true;
            var deleteDefaultSetTempo = true;

            foreach (var message in Messages.Concat(new[] { new EndOfTrackMessage() }))
            {
                var messageToWrite = message;
                if (messageToWrite is UnknownMetaMessage && settings.CompressionPolicy.HasFlag(CompressionPolicy.DeleteUnknownMetaMessages))
                    continue;

                if (settings.CompressionPolicy.HasFlag(CompressionPolicy.NoteOffAsSilentNoteOn))
                {
                    var noteOffMessage = messageToWrite as NoteOffMessage;
                    if (noteOffMessage != null)
                        messageToWrite = new NoteOnMessage
                        {
                            DeltaTime = noteOffMessage.DeltaTime,
                            Channel = noteOffMessage.Channel,
                            NoteNumber = noteOffMessage.NoteNumber
                        };
                }

                if (settings.CompressionPolicy.HasFlag(CompressionPolicy.DeleteDefaultSetTempo) && deleteDefaultSetTempo)
                {
                    var setTempoMessage = messageToWrite as SetTempoMessage;
                    if (setTempoMessage != null)
                    {
                        if (setTempoMessage.MicrosecondsPerBeat == SetTempoMessage.DefaultTempo)
                            continue;

                        deleteDefaultSetTempo = false;
                    }
                }

                IMessageWriter messageWriter = MessageWriterFactory.GetWriter(messageToWrite);

                if (messageToWrite is ChannelMessage)
                {
                    var statusByte = messageWriter.GetStatusByte(messageToWrite);
                    writeStatusByte = runningStatus != statusByte || !settings.CompressionPolicy.HasFlag(CompressionPolicy.UseRunningStatus);
                    runningStatus = statusByte;
                }
                else
                {
                    runningStatus = null;
                    writeStatusByte = true;
                }

                messageHandler(messageWriter, messageToWrite, writeStatusByte);
            }
        }

        #endregion
    }
}
