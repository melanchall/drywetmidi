using System.Text;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Settings of the reading engine.
    /// </summary>
    public class ReadingSettings
    {
        /// <summary>
        /// Gets or sets reaction of the reading engine on unexpected track chunks count. The default is
        /// <see cref="UnexpectedTrackChunksCountPolicy.Ignore"/>.
        /// </summary>
        /// <remarks>
        /// This policy will be taken into account if actual track chunks count is less or greater than
        /// tracks number specified in the file's header chunk. If <see cref="UnexpectedTrackChunksCountPolicy.Abort"/>
        /// is used an instance of the <see cref="UnexpectedTrackChunksCountException"/> will be thrown if
        /// track chunks count is unexpected.
        /// </remarks>
        public UnexpectedTrackChunksCountPolicy UnexpectedTrackChunksCountPolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on new track chunk if already read
        /// track chunks count is greater or equals the one declared in the file's header chunk.
        /// The default is <see cref="ExtraTrackChunkPolicy.Read"/>.
        /// </summary>
        public ExtraTrackChunkPolicy ExtraTrackChunkPolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on chunk with unknown ID. The default
        /// is <see cref="UnknownChunkIdPolicy.ReadAsUnknownChunk"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="UnknownChunkIdPolicy.Abort"/> is used an instance of the
        /// <see cref="UnknownChunkException"/> will be thrown if a chunk to be read has unknown ID.
        /// </remarks>
        public UnknownChunkIdPolicy UnknownChunkIdPolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on missed End Of Track event.
        /// The default is <see cref="MissedEndOfTrackPolicy.Ignore"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="MissedEndOfTrackPolicy.Abort"/> is used an instance of the
        /// <see cref="MissedEndOfTrackEventException"/> will be thrown if track chunk
        /// doesn't end with End Of Track event. Although this event is not optional and
        /// therefore missing of it must be treated as error, you can try to read a track chunk
        /// relying on the chunk's size only.
        /// </remarks>
        public MissedEndOfTrackPolicy MissedEndOfTrackPolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on Note On events with velocity 0.
        /// The default is <see cref="SilentNoteOnPolicy.NoteOff"/>. Although it is recommended to treat silent
        /// Note On event as Note Off you can turn this behavior off to get original event stored in the file.
        /// </summary>
        public SilentNoteOnPolicy SilentNoteOnPolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on difference between actual chunk's size and
        /// the one declared in its header. The default is <see cref="InvalidChunkSizePolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="InvalidChunkSizePolicy.Abort"/> is used an instance of the
        /// <see cref="InvalidChunkSizeException"/> will be thrown if actual chunk's size differs from
        /// the one declared in chunk's header.
        /// </remarks>
        public InvalidChunkSizePolicy InvalidChunkSizePolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on unknown file format stored in a header chunk.
        /// The default is <see cref="UnknownFileFormatPolicy.Ignore"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="UnknownFileFormatPolicy.Abort"/> is used an instance of the
        /// <see cref="UnknownFileFormatException"/> will be thrown if file format stored in a header
        /// chunk doesn't belong to values defined by the <see cref="MidiFileFormat"/> enumeration.
        /// </remarks>
        public UnknownFileFormatPolicy UnknownFileFormatPolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on invalid value of a channel event's
        /// parameter value. Valid values are 0-127 so, for example, 128 is the invalid one
        /// and will be processed according with this policy. The default is
        /// <see cref="InvalidChannelEventParameterValuePolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="InvalidChannelEventParameterValuePolicy.Abort"/> is used an instance of the
        /// <see cref="InvalidChannelEventParameterValueException"/> will be thrown if event's parameter value
        /// just read is invalid.
        /// </remarks>
        public InvalidChannelEventParameterValuePolicy InvalidChannelEventParameterValuePolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on invalid value of a meta event's
        /// parameter value. For example, 255 is the invalid value for the <see cref="KeySignatureEvent.Scale"/>
        /// and will be processed according with this policy. The default is
        /// <see cref="InvalidMetaEventParameterValuePolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="InvalidMetaEventParameterValuePolicy.Abort"/> is used an instance of the
        /// <see cref="InvalidMetaEventParameterValueException"/> will be thrown if event's parameter value
        /// just read is invalid.
        /// </remarks>
        public InvalidMetaEventParameterValuePolicy InvalidMetaEventParameterValuePolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on invalid value of a system common event's
        /// parameter value. For example, 255 is the invalid value for the <see cref="SongSelectEvent.Number"/>
        /// and will be processed according with this policy. The default is
        /// <see cref="InvalidSystemCommonEventParameterValuePolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="InvalidSystemCommonEventParameterValuePolicy.Abort"/> is used an instance of the
        /// <see cref="InvalidSystemCommonEventParameterValueException"/> will be thrown if event's parameter value
        /// just read is invalid.
        /// </remarks>
        public InvalidSystemCommonEventParameterValuePolicy InvalidSystemCommonEventParameterValuePolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on lack of bytes in the underlying stream
        /// that are needed to read some value (for example, DWORD requires 4 bytes available).
        /// The default is <see cref="NotEnoughBytesPolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="NotEnoughBytesPolicy.Abort"/> is used an instance of the
        /// <see cref="NotEnoughBytesException"/> will be thrown if the reader's underlying stream doesn't
        /// have enough bytes to read a value.
        /// </remarks>
        public NotEnoughBytesPolicy NotEnoughBytesPolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on missing of the header chunk in the MIDI file.
        /// The default is <see cref="NoHeaderChunkPolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="NoHeaderChunkPolicy.Abort"/> is used an instance of the
        /// <see cref="NoHeaderChunkException"/> will be thrown if the MIDI file doesn't contain the header chunk.
        /// </remarks>
        public NoHeaderChunkPolicy NoHeaderChunkPolicy { get; set; }

        /// <summary>
        /// Gets or sets collection of custom chunks types. These types must be derived from the <see cref="MidiChunk"/>
        /// class and have parameterless constructor. No exception will be thrown if some types don't meet
        /// these requirements.
        /// </summary>
        public ChunkTypesCollection CustomChunkTypes { get; set; }

        /// <summary>
        /// Gets or sets collection of custom meta events types. These types must be derived from the
        /// <see cref="MetaEvent"/> class and have parameterless constructor. No exception will be thrown
        /// if some types don't meet these requirements.
        /// </summary>
        public EventTypesCollection CustomMetaEventTypes { get; set; }

        /// <summary>
        /// Gets or sets an <see cref="Encoding"/> that will be used to read the text of a
        /// text-based meta event. The default is <see cref="Encoding.ASCII"/>.
        /// </summary>
        public Encoding TextEncoding { get; set; } = SmfUtilities.DefaultEncoding;
    }
}
