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

        public static MidiFile Read(MidiFile midiFile, WritingSettings writingSettings, ReadingSettings readingSettings)
        {
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            try
            {
                midiFile.Write(filePath, settings: writingSettings);
                return MidiFile.Read(filePath, readingSettings);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        #endregion
    }
}
