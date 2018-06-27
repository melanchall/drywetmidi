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
                MidiFileToCsvConverter.ConvertToCsv(midiFile,
                                                    fileStream,
                                                    settings ?? new MidiFileCsvConversionSettings());
            }
        }

        #endregion
    }
}
