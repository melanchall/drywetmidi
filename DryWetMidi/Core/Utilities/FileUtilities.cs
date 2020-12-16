using Melanchall.DryWetMidi.Common;
using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Core
{
    internal static class FileUtilities
    {
        #region Extern methods

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeFileHandle CreateFile(string lpFileName,
                                                        uint dwDesiredAccess,
                                                        uint dwShareMode,
                                                        IntPtr lpSecurityAttributes,
                                                        uint dwCreationDisposition,
                                                        uint dwFlagsAndAttributes,
                                                        IntPtr hTemplateFile);

        #endregion

        #region Constants

        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;

        private const uint CREATE_NEW = 1;
        private const uint CREATE_ALWAYS = 2;
        private const uint OPEN_EXISTING = 3;

        private const uint FILE_SHARE_NONE = 0;

        #endregion

        #region Methods

        internal static FileStream OpenFileForRead(string filePath)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path");

            try
            {
                return File.OpenRead(filePath);
            }
            catch (PathTooLongException)
            {
                var fileHandle = GetFileHandle(filePath, GENERIC_READ, OPEN_EXISTING);
                return new FileStream(fileHandle, FileAccess.Read);
            }
        }

        internal static FileStream OpenFileForWrite(string filePath, bool overwriteFile)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(filePath), filePath, "File path");

            try
            {
                return File.Open(filePath, overwriteFile ? FileMode.Create : FileMode.CreateNew);
            }
            catch (PathTooLongException)
            {
                var fileHandle = GetFileHandle(filePath, GENERIC_WRITE, overwriteFile ? CREATE_ALWAYS : CREATE_NEW);
                return new FileStream(fileHandle, FileAccess.Write);
            }
        }

        private static SafeFileHandle GetFileHandle(string filePath, uint fileAccess, uint creationDisposition)
        {
            SafeFileHandle fileHandle = CreateFile($@"\\?\{filePath}",
                                                   fileAccess,
                                                   FILE_SHARE_NONE,
                                                   IntPtr.Zero,
                                                   creationDisposition,
                                                   0,
                                                   IntPtr.Zero);

            var lastWin32Error = Marshal.GetLastWin32Error();
            if (fileHandle.IsInvalid)
                throw new Win32Exception(lastWin32Error);

            return fileHandle;
        }

        #endregion
    }
}
