using System.IO;
using Melanchall.DryWetMidi.Smf;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf
{
    [TestClass]
    public sealed class MidiFileTests
    {
        #region Constants

        private const string FilesPath = @"..\..\..\Resources\MIDI files\Invalid";

        #endregion

        #region Test methods

        [TestMethod]
        [Description("Read MIDI file with invalid channel event parameter value and treat that as error.")]
        public void Read_InvalidChannelEventParameterValue()
        {
            const string directoryName = "Invalid Channel Event Parameter Value";

            var readingSettings = new ReadingSettings
            {
                InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.Abort
            };

            foreach (var filePath in Directory.GetFiles(GetFilesDirectory(directoryName)))
            {
                Assert.ThrowsException<InvalidChannelEventParameterValueException>(
                    () => MidiFile.Read(filePath, readingSettings),
                    $"Exception is not thrown for {filePath}.");
            }
        }

        #endregion

        #region Private methods

        private static string GetFilesDirectory(string directoryName)
        {
            return Path.Combine(FilesPath, directoryName);
        }

        #endregion
    }
}
