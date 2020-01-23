using System;
using System.IO;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tests.Common
{
    public static class MidiFileReadingUtilities
    {
        #region Methods

        public static void ReadUsingHandlers(MidiFile midiFile, params ReadingHandler[] handlers)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.mid");

            midiFile.Write(filePath);

            try
            {
                var settings = new ReadingSettings();

                foreach (var handler in handlers)
                {
                    settings.ReadingHandlers.Add(handler);
                }

                midiFile = MidiFile.Read(filePath, settings);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        #endregion
    }
}
