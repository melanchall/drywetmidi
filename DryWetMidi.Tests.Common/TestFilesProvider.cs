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

        public const string FilesBasePath = @"Resources\MIDI files";
        public const string FilesTestPath = @"..\..\..\..\" + FilesBasePath;

        public const string ValidFilesPath = FilesTestPath + @"\Valid";
        public const string InvalidFilesPath = FilesTestPath + @"\Invalid";

        #endregion

        #region Methods

        public static string GetFileBasePath(string filePath)
        {
            var index = filePath.IndexOf(FilesBasePath);
            return filePath.Substring(index);
        }

        public static string GetRemoteFileAddress(string fileBasePath)
        {
            return $"https://raw.githubusercontent.com/melanchall/drywetmidi/develop/{fileBasePath.Replace("\\", "/")}";
        }

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
