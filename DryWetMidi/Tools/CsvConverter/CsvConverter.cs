using System;
using System.IO;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class CsvConverter
    {
        #region Methods

        public void ConvertMidiFileToCsv(MidiFile midiFile, string outputFilePath, bool overwriteFile = false, MidiFileCsvConversionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            using (var fileStream = FileUtilities.OpenFileForWrite(outputFilePath, overwriteFile))
            {
                ConvertMidiFileToCsv(midiFile, fileStream, settings);
            }
        }

        public void ConvertMidiFileToCsv(MidiFile midiFile, Stream stream, MidiFileCsvConversionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(stream), stream);

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            MidiFileToCsvConverter.ConvertToCsv(midiFile, stream, settings ?? new MidiFileCsvConversionSettings());
        }

        public MidiFile ConvertCsvToMidiFile(string filePath, MidiFileCsvConversionSettings settings = null)
        {
            using (var fileStream = FileUtilities.OpenFileForRead(filePath))
            {
                return ConvertCsvToMidiFile(fileStream, settings);
            }
        }

        public MidiFile ConvertCsvToMidiFile(Stream stream, MidiFileCsvConversionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);

            if (!stream.CanRead)
                throw new ArgumentException("Stream doesn't support reading.", nameof(stream));

            return CsvToMidiFileConverter.ConvertToMidiFile(stream, settings ?? new MidiFileCsvConversionSettings());
        }

        #endregion
    }
}
