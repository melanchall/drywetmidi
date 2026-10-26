using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SanitizerTests
    {
        #region Private methods

        private static void Sanitize(
            MidiFile midiFile,
            SanitizingSettings settings,
            MidiFile expectedMidiFile)
        {
            midiFile.Sanitize(settings);
            MidiAsserts.AreEqual(expectedMidiFile, midiFile, false, "Invalid file after processing.");
        }

        #endregion
    }
}
