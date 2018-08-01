using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class TestFilesProvider
    {
        #region Constants

        public static class Filters
        {
            public static Predicate<MidiFile> SimpleTempoMap = f =>
            {
                var tempoMap = f.GetTempoMap();
                return tempoMap.Tempo.Count() <= 3 && tempoMap.TimeSignature.Count() <= 3;
            };
        }

        private const string ValidFilesPath = @"..\..\..\Resources\MIDI files\Valid";

        #endregion

        #region Methods

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
