using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Common
{
    public static class TestFilesProvider
    {
        #region Constants

        public static readonly string FilesBasePath = Path.Combine("Resources", "MIDI files");
        public static readonly string FilesTestPath = Path.Combine("..", "..", "..", "..", FilesBasePath);

        public static readonly string ValidFilesPath = Path.Combine(FilesTestPath, "Valid");
        public static readonly string InvalidFilesPath = Path.Combine(FilesTestPath, "Invalid");
        public static readonly string ValidFilesReferencesPath = Path.Combine(FilesTestPath, "ValidReferences");

        #endregion

        #region Methods

        public static MidiFile GetValidFileReference(string midiFilePath, out bool noFile)
        {
            var referencePath = midiFilePath.Replace("Valid", "ValidReferences").Replace(".mid", ".txt");
            noFile = !File.Exists(referencePath);
            if (noFile)
                return null;

            var constructionCode = File.ReadAllText(referencePath);

            var scriptOptions = ScriptOptions.Default
                .WithReferences(
                    Assembly.GetAssembly(typeof(MidiFile)),
                    Assembly.GetAssembly(typeof(FourBitNumber)))
                .WithImports(
                    "Melanchall.DryWetMidi.Common",
                    "Melanchall.DryWetMidi.Core");

            return CSharpScript.EvaluateAsync<MidiFile>(constructionCode, scriptOptions).Result;
        }

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
            return Directory
                .GetFiles(GetValidFilesDirectory(), "*.*", SearchOption.AllDirectories)
#if COVERAGE
                .Take(1)
#endif
                ;
        }

        public static string GetValidFilesDirectory()
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, ValidFilesPath);
        }

        public static IEnumerable<string> GetInvalidFilesPaths(string directoryName)
        {
            return Directory
                .GetFiles(GetInvalidFilesDirectory(directoryName))
#if COVERAGE
                .Take(1)
#endif
                ;
        }

        public static string GetInvalidFilesDirectory(string directoryName)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, InvalidFilesPath, directoryName);
        }

        #endregion
    }
}
