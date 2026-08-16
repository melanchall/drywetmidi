using System;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [Parallelizable(ParallelScope.Children)]
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

                MidiAsserts.AreEqual(clonedMidiFile, midiFile, true, $"Clone of the '{filePath}' doesn't equal to the original file.");
            }
        }

        [Test]
        public void ReadWriteRead()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                MidiFile midiFile = null;
                MidiFile midiFile2 = null;

                ClassicAssert.DoesNotThrow(() =>
                    {
                        midiFile = MidiFile.Read(filePath);
                        midiFile2 = MidiFileTestUtilities.Read(midiFile, null, null);
                    },
                    $"Read/Write/Read failed for '{filePath}'.");

                ClassicAssert.IsNotNull(midiFile, "MIDI file is null.");
                MidiAsserts.AreEqual(midiFile, midiFile2, true, $"Reread failed for '{filePath}'.");
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
                MidiAsserts.AreEqual(midiFile, referenceMidiFile, false, $"File '{filePath}' read wrong.");
            }
        }

        [Test]
        public void CheckValidFilesAreEqualToSelf()
        {
            foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
            {
                var midiFile1 = MidiFile.Read(filePath);
                var midiFile2 = MidiFile.Read(filePath);
                MidiAsserts.AreEqual(midiFile1, midiFile2, true, $"File '{filePath}' isn't equal to self.");
            }
        }

        [Test]
        public void CheckValidFilesAreNotEqualToAnother()
        {
            var filesPaths = TestFilesProvider.GetValidFilesPaths().ToArray();

            var midiFiles = filesPaths
                .Select(path => (Path: path, File: MidiFile.Read(path)))
                .ToArray();

            for (var i = 0; i < midiFiles.Length; i++)
            {
                for (var j = i + 1; j < midiFiles.Length; j++)
                {
                    MidiAsserts.AreNotEqual(
                        midiFiles[i].File,
                        midiFiles[j].File,
                        true,
                        $"File '{midiFiles[i].Path}' equals to another one '{midiFiles[j].Path}'.");
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
                FileOperations.DeleteFile(filePath);
            }
        }

        #endregion
    }
}
