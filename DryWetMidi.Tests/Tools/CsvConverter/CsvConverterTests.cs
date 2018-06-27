using System;
using System.IO;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class CsvConverterTests
    {
        #region Test methods

        [Test]
        [Description("Convert MIDI file to CSV using DryWetMIDI layout.")]
        public void ConvertMidiFileToCsv_DryWetMidi()
        {
            var settings = new MidiFileCsvConversionSettings
            {
                CsvLayout = MidiFileCsvLayout.DryWetMidi
            };

            ConvertMidiFileToCsv(settings);
        }

        [Test]
        [Description("Convert MIDI file to CSV using Ubuntu MIDI CSV layout.")]
        public void ConvertMidiFileToCsv_UbuntuMidiCsv()
        {
            var settings = new MidiFileCsvConversionSettings
            {
                CsvLayout = MidiFileCsvLayout.UbuntuMidiCsv
            };

            ConvertMidiFileToCsv(settings);
        }

        #endregion

        #region Private methods

        private static void ConvertMidiFileToCsv(MidiFileCsvConversionSettings settings)
        {
            var tempPath = Path.GetTempPath();
            var outputDirectory = Path.Combine(tempPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDirectory);

            try
            {
                foreach (var filePath in TestFilesProvider.GetValidFiles())
                {
                    var midiFile = MidiFile.Read(filePath);
                    var outputFilePath = Path.Combine(outputDirectory, Path.GetFileName(Path.ChangeExtension(filePath, "csv")));

                    new CsvConverter().ConvertMidiFileToCsv(midiFile, outputFilePath, true, settings);
                }
            }
            finally
            {
                Directory.Delete(outputDirectory, true);
            }
        }

        #endregion
    }
}
