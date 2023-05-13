using System;
using System.IO;

namespace Melanchall.DryWetMidi.Core
{
    internal sealed class HeaderChunk : MidiChunk
    {
        #region Constants

        /// <summary>
        /// ID of the header chunk. This field is constant.
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

        public ushort FileFormat { get; set; }

        public TimeDivision TimeDivision { get; set; }

        public ushort TracksNumber { get; set; }

        #endregion

        #region Methods

        internal static void ReadData(
            MidiReader reader,
            ReadingSettings settings,
            out ushort fileFormat,
            out TimeDivision timeDivision,
            out ushort tracksNumber)
        {
            fileFormat = reader.ReadWord();
            if (settings.UnknownFileFormatPolicy == UnknownFileFormatPolicy.Abort && !Enum.IsDefined(typeof(MidiFileFormat), fileFormat))
                throw new UnknownFileFormatException(fileFormat);

            tracksNumber = reader.ReadWord();
            timeDivision = TimeDivisionFactory.GetTimeDivision(reader.ReadInt16());
        }

        #endregion

        #region Overrides

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
            ushort fileFormat;
            ushort tracksNumber;
            TimeDivision timeDivision;
            ReadData(reader, settings, out fileFormat, out timeDivision, out tracksNumber);

            FileFormat = fileFormat;
            TimeDivision = timeDivision;
            TracksNumber = tracksNumber;
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
