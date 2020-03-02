using System;
using System.IO;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tests.Common
{
    public static class MidiFileTestUtilities
    {
        #region Methods

        public static MidiFile ReadUsingHandlers(MidiFile midiFile, params ReadingHandler[] handlers)
        {
            var settings = new ReadingSettings();

            foreach (var handler in handlers)
            {
                settings.ReadingHandlers.Add(handler);
            }

            return Read(midiFile, null, settings);
        }

        public static MidiFile Read(MidiFile midiFile, WritingSettings writingSettings, ReadingSettings readingSettings, MidiFileFormat? format = null)
        {
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

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
                File.Delete(filePath);
            }
        }

        public static void Write(MidiFile midiFile, Action<string> action, WritingSettings settings = null, MidiFileFormat? format = null)
        {
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            try
            {
                midiFile.Write(filePath, format: format ?? MidiFileFormat.MultiTrack, settings: settings);
                action(filePath);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        #endregion
    }
}
