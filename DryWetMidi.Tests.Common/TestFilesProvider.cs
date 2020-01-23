using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Common
{
    public static class TestFilesProvider
    {
        #region Constants

        private const string ValidFilesPath = @"..\..\..\..\Resources\MIDI files\Valid";

        #endregion

        #region Methods

        public static string GetMiscFile_14000events()
        {
            return GetMiscFile("Misc_14000_events.mid");
        }

        public static string GetMiscFile(string fileName)
        {
            return Path.Combine(ValidFilesPath, "Misc", fileName);
        }

        public static IEnumerable<MidiFile> GetValidFiles(params Predicate<MidiFile>[] filters)
        {
            return GetValidFilesPaths().Select(p => MidiFile.Read(p)).Where(file => filters.All(f => f(file)));
        }

        public static IEnumerable<string> GetValidFilesPaths()
        {
            return Directory.GetFiles(GetValidFilesDirectory(), "*.*", SearchOption.AllDirectories);
        }

        public static string GetValidFilesDirectory()
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, ValidFilesPath);
        }

        #endregion
    }
}
