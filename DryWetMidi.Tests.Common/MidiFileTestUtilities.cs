using System;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tests.Common
{
    public static class MidiFileTestUtilities
    {
        #region Methods

        public static MidiFile Read(MidiFile midiFile, WritingSettings writingSettings, ReadingSettings readingSettings, MidiFileFormat? format = null)
        {
            var filePath = FileOperations.GetTempFilePath();

            try
            {
                if (format == null)
                {
                    format = MidiFileFormat.MultiTrack;
                    try
                    {
                        format = midiFile.OriginalFormat;
                    }
                    catch { }
                }

                midiFile.Write(filePath, format: format.Value, settings: writingSettings);
                return MidiFile.Read(filePath, readingSettings);
            }
            finally
            {
                FileOperations.DeleteFile(filePath);
            }
        }

        public static void Write(MidiFile midiFile, Action<string> action, WritingSettings settings = null, MidiFileFormat? format = null)
        {
            var filePath = FileOperations.GetTempFilePath();

            try
            {
                midiFile.Write(filePath, format: format ?? MidiFileFormat.MultiTrack, settings: settings);
                action(filePath);
            }
            finally
            {
                FileOperations.DeleteFile(filePath);
            }
        }

        #endregion
    }
}
