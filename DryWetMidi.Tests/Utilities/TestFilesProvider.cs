using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class TestFilesProvider
    {
        #region Constants

        private const string ValidFilesPath = @"..\..\..\Resources\MIDI files\Valid";

        #endregion

        #region Methods

        public static IEnumerable<string> GetValidFiles()
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
