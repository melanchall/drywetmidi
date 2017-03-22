using System;
using System.Collections.Generic;

namespace Melanchall.DryMidi
{
    /// <summary>
    /// Settings of the reading engine.
    /// </summary>
    public sealed class ReadingSettings
    {
        /// <summary>
        /// Gets or sets reaction of the reading engine on unexpected track chunks count. The defailt is
        /// <see cref="UnexpectedTrackChunksCountPolicy.Ignore"/>.
        /// </summary>
        /// <remarks>
        /// This policy will be taken into account if actual track chunks count is less or greater than
        /// tracks number specified in the file's header chunk. If <see cref="UnknownChunkIdPolicy.Abort"/>
        /// is used an instance of the <see cref="UnexpectedTrackChunksCountException"/> will be thrown if
        /// track chunks count is unexpected.
        /// </remarks>
        public UnexpectedTrackChunksCountPolicy UnexpectedTrackChunksCountPolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on new track chunk if already read
        /// track chunks count is greater or equals the one declared in the file's header chunk.
        /// The defailt is <see cref="ExtraTrackChunkPolicy.Read"/>.
        /// </summary>
        public ExtraTrackChunkPolicy ExtraTrackChunkPolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on chunk with unknown ID. The default
        /// is <see cref="UnknownChunkIdPolicy.ReadAsUnknownChunk"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="UnknownChunkIdPolicy.Abort"/> is used an instance of the
        /// <see cref="UnknownChunkIdException"/> will be thrown if a chunk to be read has unknown ID.
        /// </remarks>
        public UnknownChunkIdPolicy UnknownChunkIdPolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on missed End Of Track message.
        /// The default is <see cref="MissedEndOfTrackPolicy.Ignore"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="MissedEndOfTrackPolicy.Abort"/> is used an instance of the
        /// <see cref="MissedEndOfTrackMessageException"/> will be thrown if track chunk
        /// doesn't end with End Of Track message. Although this message is not optional and
        /// therefore missing of it must be treated as error, you can try to read a track chunk
        /// relying on the chunk's size only.
        /// </remarks>
        public MissedEndOfTrackPolicy MissedEndOfTrackPolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on Note On messages with velocity 0.
        /// The default is <see cref="SilentNoteOnPolicy.NoteOff"/>.
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
        /// Gets or sets reaction of the reading engine on invalid expected size of a message.
        /// The default is <see cref="InvalidMessageSizePolicy.Ignore"/>.
        /// </summary>
        /// <remarks>
        /// Some messages (at now, meta messages only) are written along with the size of their content.
        /// For example, for the Set Tempo message 0 should be written as a size since this message has
        /// no parameters. If read size is not 0 and <see cref="InvalidMessageSizePolicy.Abort"/> option
        /// is used, an exception will be thrown.
        /// </remarks>
        public InvalidMessageSizePolicy InvalidMessageSizePolicy { get; set; }

        /// <summary>
        /// Gets or sets reaction of the reading engine on unknown file format stored in a header chunk.
        /// </summary>
        /// <remarks>
        /// If <see cref="UnknownFileFormatPolicy.Abort"/> is used an instance of the
        /// <see cref="UnknownFileFormatException"/> will be thrown if file format stored in a header
        /// chunk doesn't belong to values defined by the <see cref="MidiFileFormat"/> enumeration.
        /// </remarks>
        public UnknownFileFormatPolicy UnknownFileFormatPolicy { get; set; }

        /// <summary>
        /// Gets or sets collection of custom chunks types. These types must be derived from the <see cref="Chunk"/>
        /// class and have parameterless constructor. No exception will be thrown if some types don't meet
        /// these requirements.
        /// </summary>
        public IEnumerable<Type> CustomChunksTypes { get; set; }
    }
}
