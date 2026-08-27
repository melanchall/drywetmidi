using Melanchall.DryWetMidi.Common;
using System.IO;

namespace Melanchall.DryWetMidi.Core
{
    internal static class FileUtilities
    {
        #region Methods

        internal static FileStream OpenFileForRead(string filePath)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path");

            return File.OpenRead(filePath);
        }

        internal static FileStream OpenFileForWrite(string filePath, bool overwriteFile)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path");

            return File.Open(filePath, overwriteFile ? FileMode.Create : FileMode.CreateNew);
        }

        #endregion
    }
}
