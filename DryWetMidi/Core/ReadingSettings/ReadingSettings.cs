using System.ComponentModel;
using System.Text;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Settings according to which MIDI data should be read.
    /// </summary>
    public class ReadingSettings
    {
        #region Fields

        private UnexpectedTrackChunksCountPolicy _unexpectedTrackChunksCountPolicy = UnexpectedTrackChunksCountPolicy.Ignore;
        private ExtraTrackChunkPolicy _extraTrackChunkPolicy = ExtraTrackChunkPolicy.Read;
        private UnknownChunkIdPolicy _unknownChunkIdPolicy = UnknownChunkIdPolicy.ReadAsUnknownChunk;
        private MissedEndOfTrackPolicy _missedEndOfTrackPolicy = MissedEndOfTrackPolicy.Ignore;
        private SilentNoteOnPolicy _silentNoteOnPolicy = SilentNoteOnPolicy.NoteOff;
        private InvalidChunkSizePolicy _invalidChunkSizePolicy = InvalidChunkSizePolicy.Abort;
        private UnknownFileFormatPolicy _unknownFileFormatPolicy = UnknownFileFormatPolicy.Ignore;
        private UnknownChannelEventPolicy _unknownChannelEventPolicy = UnknownChannelEventPolicy.Abort;
        private InvalidChannelEventParameterValuePolicy _invalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.Abort;
        private InvalidMetaEventParameterValuePolicy _invalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.Abort;
        private InvalidSystemCommonEventParameterValuePolicy _invalidSystemCommonEventParameterValuePolicy = InvalidSystemCommonEventParameterValuePolicy.Abort;
        private NotEnoughBytesPolicy _notEnoughBytesPolicy = NotEnoughBytesPolicy.Abort;
        private NoHeaderChunkPolicy _noHeaderChunkPolicy = NoHeaderChunkPolicy.Abort;
        private ZeroLengthDataPolicy _zeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsEmptyObject;
        private EndOfTrackStoringPolicy _endOfTrackStoringPolicy = EndOfTrackStoringPolicy.Omit;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the reading engine should stop or not when
        /// the count of read chunks is equal to the value defined in a file's header. The
        /// default value is <c>false</c> which means all chunks will be read.
        /// </summary>
        /// <seealso cref="UnexpectedTrackChunksCountPolicy"/>
        public bool StopReadingOnExpectedTrackChunksCountReached { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on unexpected track chunks count. The default is
        /// <see cref="UnexpectedTrackChunksCountPolicy.Ignore"/>.
        /// </summary>
        /// <remarks>
        /// <para>This policy will be taken into account if actual track chunks count is less or greater than
        /// tracks number specified in the file's header chunk. If <see cref="UnexpectedTrackChunksCountPolicy.Abort"/>
        /// is used, an instance of the <see cref="UnexpectedTrackChunksCountException"/> will be thrown if
        /// track chunks count is unexpected.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public UnexpectedTrackChunksCountPolicy UnexpectedTrackChunksCountPolicy
        {
            get { return _unexpectedTrackChunksCountPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _unexpectedTrackChunksCountPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on new track chunk if already read
        /// track chunks count is greater or equals the one declared in the file's header chunk.
        /// The default is <see cref="ExtraTrackChunkPolicy.Read"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public ExtraTrackChunkPolicy ExtraTrackChunkPolicy
        {
            get { return _extraTrackChunkPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _extraTrackChunkPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on chunk with unknown ID. The default
        /// is <see cref="UnknownChunkIdPolicy.ReadAsUnknownChunk"/>.
        /// </summary>
        /// <remarks>
        /// <para>If <see cref="UnknownChunkIdPolicy.Abort"/> is used, an instance of the
        /// <see cref="UnknownChunkException"/> will be thrown if a chunk to be read has unknown ID.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public UnknownChunkIdPolicy UnknownChunkIdPolicy
        {
            get { return _unknownChunkIdPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _unknownChunkIdPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on missed <c>End Of Track</c> event.
        /// The default is <see cref="MissedEndOfTrackPolicy.Ignore"/>.
        /// </summary>
        /// <remarks>
        /// <para>If <see cref="MissedEndOfTrackPolicy.Abort"/> is used, an instance of the
        /// <see cref="MissedEndOfTrackEventException"/> will be thrown if track chunk
        /// doesn't end with <c>End Of Track</c> event. Although this event is not optional and
        /// therefore missing of it must be treated as error, you can try to read a track chunk
        /// relying only on the chunk's size declared in its header.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public MissedEndOfTrackPolicy MissedEndOfTrackPolicy
        {
            get { return _missedEndOfTrackPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _missedEndOfTrackPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on <c>Note On</c> events with velocity of zero.
        /// The default is <see cref="SilentNoteOnPolicy.NoteOff"/>.
        /// </summary>
        /// <remarks>
        /// <para>Although it is recommended to treat silent <c>Note On</c> event as <c>Note Off</c> you can turn
        /// this behavior off to get original event stored in a MIDI file.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public SilentNoteOnPolicy SilentNoteOnPolicy
        {
            get { return _silentNoteOnPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _silentNoteOnPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on difference between actual chunk's size and
        /// the one declared in its header. The default is <see cref="InvalidChunkSizePolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// <para>If <see cref="InvalidChunkSizePolicy.Abort"/> is used, an instance of the
        /// <see cref="InvalidChunkSizeException"/> will be thrown if actual chunk's size differs from
        /// the one declared in chunk's header.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public InvalidChunkSizePolicy InvalidChunkSizePolicy
        {
            get { return _invalidChunkSizePolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _invalidChunkSizePolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on unknown file format stored in a header chunk.
        /// The default is <see cref="UnknownFileFormatPolicy.Ignore"/>.
        /// </summary>
        /// <remarks>
        /// <para>If <see cref="UnknownFileFormatPolicy.Abort"/> is used, an instance of the
        /// <see cref="UnknownFileFormatException"/> will be thrown if file format stored in a header
        /// chunk doesn't belong to values defined by the <see cref="MidiFileFormat"/> enumeration.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public UnknownFileFormatPolicy UnknownFileFormatPolicy
        {
            get { return _unknownFileFormatPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _unknownFileFormatPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on unknown channel event. The default is
        /// <see cref="UnknownChannelEventPolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// <para>If <see cref="UnknownChannelEventPolicy.Abort"/> is used, an instance of the
        /// <see cref="UnknownChannelEventException"/> will be thrown if channel event has unknown status byte.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public UnknownChannelEventPolicy UnknownChannelEventPolicy
        {
            get { return _unknownChannelEventPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _unknownChannelEventPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets a callback used to read unknown channel event if <see cref="UnknownChannelEventPolicy"/>
        /// set to <see cref="UnknownChannelEventPolicy.UseCallback"/>.
        /// </summary>
        public UnknownChannelEventCallback UnknownChannelEventCallback { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on invalid value of a channel event's
        /// parameter value. The default is <see cref="InvalidChannelEventParameterValuePolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// <para>Valid values are 0-127 so, for example, 128 is the invalid one
        /// and will be processed according with this policy. If <see cref="InvalidChannelEventParameterValuePolicy.Abort"/>
        /// is used, an instance of the <see cref="InvalidChannelEventParameterValueException"/> will be thrown if
        /// event's parameter value just read is invalid.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public InvalidChannelEventParameterValuePolicy InvalidChannelEventParameterValuePolicy
        {
            get { return _invalidChannelEventParameterValuePolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _invalidChannelEventParameterValuePolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on invalid value of a meta event's
        /// parameter value. The default is <see cref="InvalidMetaEventParameterValuePolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// <para>For example, 255 is the invalid value for the <see cref="KeySignatureEvent.Scale"/>
        /// and will be processed according with this policy. If <see cref="InvalidMetaEventParameterValuePolicy.Abort"/>
        /// is used, an instance of the <see cref="InvalidMetaEventParameterValueException"/> will be thrown if event's
        /// parameter value just read is invalid.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public InvalidMetaEventParameterValuePolicy InvalidMetaEventParameterValuePolicy
        {
            get { return _invalidMetaEventParameterValuePolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _invalidMetaEventParameterValuePolicy = value;
            }
        }

        // TODO: test
        /// <summary>
        /// Gets or sets reaction of the reading engine on invalid value of a system common event's
        /// parameter value. The default is <see cref="InvalidSystemCommonEventParameterValuePolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// <para>For example, 255 is the invalid value for the <see cref="SongSelectEvent.Number"/>
        /// and will be processed according with this policy. If <see cref="InvalidSystemCommonEventParameterValuePolicy.Abort"/>
        /// is used, an instance of the <see cref="InvalidSystemCommonEventParameterValueException"/> will be thrown if event's
        /// parameter value just read is invalid.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public InvalidSystemCommonEventParameterValuePolicy InvalidSystemCommonEventParameterValuePolicy
        {
            get { return _invalidSystemCommonEventParameterValuePolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _invalidSystemCommonEventParameterValuePolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on lack of bytes in the underlying stream
        /// that are needed to read MIDI data (for example, DWORD requires 4 bytes available).
        /// The default is <see cref="NotEnoughBytesPolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// <para>If <see cref="NotEnoughBytesPolicy.Abort"/> is used, an instance of the
        /// <see cref="NotEnoughBytesException"/> will be thrown if the reader's underlying stream doesn't
        /// have enough bytes to read MIDI data.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public NotEnoughBytesPolicy NotEnoughBytesPolicy
        {
            get { return _notEnoughBytesPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _notEnoughBytesPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on missing of the header chunk in the MIDI file.
        /// The default is <see cref="NoHeaderChunkPolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// <para>If <see cref="NoHeaderChunkPolicy.Abort"/> is used, an instance of the
        /// <see cref="NoHeaderChunkException"/> will be thrown if the MIDI file doesn't contain the header chunk.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public NoHeaderChunkPolicy NoHeaderChunkPolicy
        {
            get { return _noHeaderChunkPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _noHeaderChunkPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets collection of custom chunks types.
        /// </summary>
        /// <remarks>
        /// <para>Types within this collection must be derived from the <see cref="MidiChunk"/>
        /// class and have parameterless constructor. No exception will be thrown if some types don't meet
        /// these requirements.</para>
        /// </remarks>
        public ChunkTypesCollection CustomChunkTypes { get; set; }

        /// <summary>
        /// Gets or sets collection of custom meta events types.
        /// </summary>
        /// <remarks>
        /// <para>Types within this collection must be derived from the <see cref="MetaEvent"/>
        /// class and have parameterless constructor. No exception will be thrown
        /// if some types don't meet these requirements.</para>
        /// </remarks>
        public EventTypesCollection CustomMetaEventTypes { get; set; }

        /// <summary>
        /// Gets or sets an <see cref="Encoding"/> that will be used to read the text of a
        /// text-based meta events. The default is <see cref="Encoding.ASCII"/>.
        /// </summary>
        /// <remarks>
        /// <para>Value of this property will be used only if <see cref="DecodeTextCallback"/> is not set.</para>
        /// </remarks>
        public Encoding TextEncoding { get; set; } = SmfConstants.DefaultTextEncoding;

        /// <summary>
        /// Gets or sets a callback used to decode a string from the specified bytes during reading a text-based
        /// meta event's text. The default is <c>null</c>.
        /// </summary>
        /// <remarks>
        /// <para>If callback is not set, <see cref="TextEncoding"/> will be used.</para>
        /// </remarks>
        public DecodeTextCallback DecodeTextCallback { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on zero-length objects such as strings or arrays.
        /// The default is <see cref="ZeroLengthDataPolicy.ReadAsEmptyObject"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public ZeroLengthDataPolicy ZeroLengthDataPolicy
        {
            get { return _zeroLengthDataPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _zeroLengthDataPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on End Of Track event encountered.
        /// The default is <see cref="EndOfTrackStoringPolicy.Omit"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public EndOfTrackStoringPolicy EndOfTrackStoringPolicy
        {
            get { return _endOfTrackStoringPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _endOfTrackStoringPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets settings according to which <see cref="MidiReader"/> should read MIDI data.
        /// </summary>
        /// <remarks>
        /// <para>These settings specify reading binary data without knowledge about MIDI data structures.</para>
        /// </remarks>
        public ReaderSettings ReaderSettings { get; set; } = new ReaderSettings();

        #endregion
    }
}
