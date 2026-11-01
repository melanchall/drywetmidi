using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

        // TODO: prevent setting null?
        public TimeDivision TimeDivision { get; set; } = new TicksPerQuarterNoteTimeDivision();

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

        internal static async Task<(ushort FileFormat, TimeDivision TimeDivision, ushort TracksNumber)> ReadDataAsync(
            MidiReader reader,
            ReadingSettings settings,
            CancellationToken cancellationToken)
        {
            var fileFormat = await reader.ReadWordAsync(cancellationToken).ConfigureAwait(false);
            if (settings.UnknownFileFormatPolicy == UnknownFileFormatPolicy.Abort && !Enum.IsDefined(typeof(MidiFileFormat), fileFormat))
                throw new UnknownFileFormatException(fileFormat);

            var tracksNumber = await reader.ReadWordAsync(cancellationToken).ConfigureAwait(false);
            var timeDivision = TimeDivisionFactory.GetTimeDivision(await reader.ReadInt16Async(cancellationToken).ConfigureAwait(false));

            return (fileFormat, timeDivision, tracksNumber);
        }

        #endregion

        #region Overrides

        public override MidiChunk Clone()
        {
            throw new NotSupportedException("Cloning of a header chunk isnot supported.");
        }

        public override string ToString()
        {
            return $"Header chunk (file format = {FileFormat}, time division = {TimeDivision}, number of tracks = {TracksNumber})";
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Content of a <see cref="HeaderChunk"/> is format of the file, number of track chunks and time division.
        /// </remarks>
        /// <exception cref="UnknownFileFormatException">The header chunk contains unknown file format and
        /// <see cref="ReadingSettings.UnknownFileFormatPolicy"/> property of the <paramref name="settings"/> set to
        /// <see cref="UnknownFileFormatPolicy.Abort"/>.</exception>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
        {
            ReadData(reader, settings, out var fileFormat, out var timeDivision, out var tracksNumber);

            FileFormat = fileFormat;
            TimeDivision = timeDivision;
            TracksNumber = tracksNumber;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Content of a <see cref="HeaderChunk"/> is format of the file, number of track chunks and time division.
        /// Six bytes required to write all of this information.
        /// </remarks>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteWord(FileFormat);
            writer.WriteWord(TracksNumber);
            writer.WriteInt16(TimeDivision.ToInt16());
        }

        protected override async Task ReadContentAsync(
            MidiReader reader,
            ReadingSettings settings,
            uint size,
            CancellationToken cancellationToken = default)
        {
            var data = await ReadDataAsync(reader, settings, cancellationToken).ConfigureAwait(false);

            FileFormat = data.FileFormat;
            TimeDivision = data.TimeDivision;
            TracksNumber = data.TracksNumber;
        }

        protected override async Task WriteContentAsync(
            MidiWriter writer,
            WritingSettings settings,
            CancellationToken cancellationToken = default)
        {
            await writer.WriteWordAsync(FileFormat, cancellationToken).ConfigureAwait(false);
            await writer.WriteWordAsync(TracksNumber, cancellationToken).ConfigureAwait(false);
            await writer.WriteInt16Async(TimeDivision.ToInt16(), cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
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
