using System;
using System.IO;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class HeaderChunk : MidiChunk
    {
        #region Constants

        /// <summary>
        /// ID of the header chunk. This field is constsnt.
        /// </summary>
        public const string Id = "MThd";

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderChunk"/>.
        /// </summary>
        internal HeaderChunk()
            : base(Id)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets format of a file where current instance of the <see cref="HeaderChunk"/>
        /// is written or will be written.
        /// </summary>
        /// <remarks>
        /// MIDI file can be stored in one of the three possible formats:
        /// <see cref="MidiFileFormat.SingleTrack"/> - the file contains a single multi-channel track;
        /// <see cref="MidiFileFormat.MultiTrack"/> - the file contains one or more simultaneous tracks
        /// (or MIDI outputs) of a sequence;
        /// <see cref="MidiFileFormat.MultiSequence"/> - the file contains one or more sequentially independent
        /// single-track patterns.
        /// </remarks>
        public ushort FileFormat { get; set; }

        /// <summary>
        /// Gets or sets time division used in a file where current instance of the <see cref="HeaderChunk"/>
        /// is written or will be written.
        /// </summary>
        /// <remarks>
        /// Time division specifies the meaning of the delta-times of events. There are two types of
        /// the time division: ticks per quarter note and SMPTE. Time division of the first type has bit 15 set
        /// to 0. In this case bits 14 thru 0 represent the number of ticks which make up a quarter-note.
        /// Division of the second type has bit 15 set to 1. In this case bits 14 thru 8 contain one of the four
        /// values: -24, -25, -29, or -30, corresponding to the four standard SMPTE and MIDI Time Code formats
        /// (-29 corresponds to 30 drop frame), and represents the number of frames per second. Bits 7 thru 0
        /// (which represent a byte stored positive) is the resolution within a frame: typical values may be 4
        /// (MIDI Time Code resolution), 8, 10, 80 (bit resolution), or 100.
        /// </remarks>
        public TimeDivision TimeDivision { get; set; }

        /// <summary>
        /// Gets or sets the number of track chunks that are stored or will be stored in a file
        /// follow the header chunk.
        /// </summary>
        /// <remarks>
        /// Number of tracks should be 1 for a file stored in <see cref="MidiFileFormat.SingleTrack"/> format.
        /// </remarks>
        public ushort TracksNumber { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Clones chunk by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the chunk.</returns>
        public override MidiChunk Clone()
        {
            throw new NotSupportedException("Cloning of a header chunk isnot supported.");
        }

        public override string ToString()
        {
            return $"Header chunk (file format = {FileFormat}, time division = {TimeDivision}, tracks number = {TracksNumber})";
        }

        /// <summary>
        /// Reads content of a <see cref="HeaderChunk"/>.
        /// </summary>
        /// <remarks>
        /// Content of a <see cref="HeaderChunk"/> is format of the file, number of track chunks and time division.
        /// </remarks>
        /// <param name="reader">Reader to read the chunk's content with.</param>
        /// <param name="settings">Settings according to which the chunk's content must be read.</param>
        /// <param name="size">Expected size of the content taken from the chunk's header.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the reader's underlying stream was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the reader's underlying stream.</exception>
        /// <exception cref="UnknownFileFormatException">The header chunk contains unknown file format and
        /// <see cref="ReadingSettings.UnknownFileFormatPolicy"/> property of the <paramref name="settings"/> set to
        /// <see cref="UnknownFileFormatPolicy.Abort"/>.</exception>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
        {
            // LOGREAD: hc c a

            var fileFormat = reader.ReadWord();
            if (settings.UnknownFileFormatPolicy == UnknownFileFormatPolicy.Abort && !Enum.IsDefined(typeof(MidiFileFormat), fileFormat))
                throw new UnknownFileFormatException(fileFormat);

            FileFormat = fileFormat;
            TracksNumber = reader.ReadWord();
            TimeDivision = TimeDivisionFactory.GetTimeDivision(reader.ReadInt16());

            // LOGREAD: hc c z
        }

        /// <summary>
        /// Writes content of a <see cref="HeaderChunk"/>.
        /// </summary>
        /// <remarks>
        /// Content of a <see cref="HeaderChunk"/> is format of the file, number of track chunks and time division.
        /// Six bytes required to write all of this information.
        /// </remarks>
        /// <param name="writer">Writer to write the chunk's content with.</param>
        /// <param name="settings">Settings according to which the chunk's content must be written.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the writer's underlying stream was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the writer's underlying stream.</exception>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteWord(FileFormat);
            writer.WriteWord(TracksNumber);
            writer.WriteInt16(TimeDivision.ToInt16());
        }

        /// <summary>
        /// Gets size of <see cref="HeaderChunk"/>'s content as number of bytes required to write it according
        /// to specified <see cref="WritingSettings"/>.
        /// </summary>
        /// <param name="settings">Settings according to which the chunk's content will be written.</param>
        /// <returns>Number of bytes required to write <see cref="HeaderChunk"/>'s content.</returns>
        /// <remarks>
        /// This method must always return 6.
        /// </remarks>
        protected override uint GetContentSize(WritingSettings settings)
        {
            return 6;
        }

        #endregion
    }
}
