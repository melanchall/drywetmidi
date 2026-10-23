using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SanitizerTests
    {
        #region Test methods

        [Test]
        public void Sanitize_NullFile()
        {
            Assert.Throws<ArgumentNullException>(() => ((MidiFile)null).Sanitize());
            ToolsAsyncTests.AssertAsyncException(
                () => ((MidiFile)null).SanitizeAsync(cancellationToken: CancellationToken.None).Wait(),
                typeof(ArgumentNullException));
        }

        #endregion

        #region Private methods

        private static void Sanitize(
            MidiFile midiFile,
            SanitizingSettings settings,
            MidiFile expectedMidiFile)
        {
            var asyncFile = midiFile.Clone();

            try
            {
                midiFile.Sanitize(settings);
            }
            catch (Exception exception)
            {
                ToolsAsyncTests.AssertAsyncException(
                    () => asyncFile.SanitizeAsync(settings, cancellationToken: CancellationToken.None).Wait(),
                    exception.GetType());
                throw;
            }

            MidiAsserts.AreEqual(expectedMidiFile, midiFile, false, "Invalid file after processing.");

            asyncFile.SanitizeAsync(settings, cancellationToken: CancellationToken.None).Wait();
            MidiAsserts.AreEqual(expectedMidiFile, asyncFile, false, "Invalid file after asynchronous processing.");
        }

        #endregion
    }
}
