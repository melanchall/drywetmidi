using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Melanchall.DryWetMidi.Tests.Common
{
    public static class FileOperations
    {
        public static void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        public static bool IsFileExist(string filePath)
        {
            return File.Exists(filePath);
        }

        public static string ReadAllFileText(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public static byte[] ReadAllFileBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        public static void WriteAllLinesToFile(string filePath, IEnumerable<string> content)
        {
            File.WriteAllLines(filePath, content);
        }

        public static string[] ReadAllFileLines(string filePath)
        {
            return File.ReadAllLines(filePath);
        }
    }
}
