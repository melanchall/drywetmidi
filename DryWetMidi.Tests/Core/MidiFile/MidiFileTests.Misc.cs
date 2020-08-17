using System;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
    {
        #region Properties

        public TestContext TestContext { get; set; }

        #endregion

        #region Set up

        [SetUp]
        public void SetupTest()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }

        #endregion

        #region Test methods

        [Test]
        [Description("Check whether a clone of a MIDI file equals to the original file.")]
        public void Clone()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var midiFile = MidiFile.Read(filePath);
                var clonedMidiFile = midiFile.Clone();

                MidiAsserts.AreFilesEqual(clonedMidiFile, midiFile, true, $"Clone of the '{filePath}' doesn't equal to the original file.");
            }
        }

        [Test]
        public void ReadWriteRead()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                MidiFile midiFile = null;
                MidiFile midiFile2 = null;

                Assert.DoesNotThrow(() =>
                    {
                        midiFile = MidiFile.Read(filePath);
                        midiFile2 = MidiFileTestUtilities.Read(midiFile, null, null);
                    },
                    $"Read/Write/Read failed for '{filePath}'.");

                Assert.IsNotNull(midiFile, "MIDI file is null.");
                MidiAsserts.AreFilesEqual(midiFile, midiFile2, true, $"Reread failed for '{filePath}'.");
            }
        }

        [Test]
        public void CheckValidFilesReadingByReferences()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var referenceMidiFile = TestFilesProvider.GetValidFileReference(filePath, out var noFile);
                if (noFile)
                    continue;

                var midiFile = MidiFile.Read(filePath);
                MidiAsserts.AreFilesEqual(midiFile, referenceMidiFile, false, $"File '{filePath}' read wrong.");
            }
        }

        [Test]
        public void CheckValidFilesAreEqualToSelf()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var midiFile1 = MidiFile.Read(filePath);
                var midiFile2 = MidiFile.Read(filePath);
                MidiAsserts.AreFilesEqual(midiFile1, midiFile2, true, $"File '{filePath}' isn't equal to self.");
            }
        }

        [Test]
        public void CheckValidFilesAreNotEqualToAnother()
        {
            var filesPaths = TestFilesProvider.GetValidFilesPaths().ToArray();

            for (var i = 0; i < filesPaths.Length; i++)
            {
                var iFilePath = filesPaths[i];
                var iMidiFile = MidiFile.Read(iFilePath);

                for (var j = i + 1; j < filesPaths.Length; j++)
                {
                    var jFilePath = filesPaths[j];
                    var jMidiFile = MidiFile.Read(jFilePath);

                    MidiAsserts.AreFilesNotEqual(iMidiFile, jMidiFile, true, $"File '{iFilePath}' equals to another one '{jFilePath}'.");
                }
            }
        }

        #endregion
        
        #region Private methods

        private MidiFile WriteRead(MidiFile midiFile, WritingSettings writingSettings = null, ReadingSettings readingSettings = null)
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.mid");

            try
            {
                midiFile.Write(filePath, settings: writingSettings);
                midiFile = MidiFile.Read(filePath, readingSettings);
                return midiFile;
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        #endregion
    }
}
