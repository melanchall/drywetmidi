using System.Collections.Generic;
using System.IO;

namespace Melanchall.DryWetMidi.Tests.Common
{
    public static class FileOperations
    {
        public static void DeleteFile(string filePath) =>
            File.Delete(filePath);

        public static bool IsFileExist(string filePath) =>
            File.Exists(filePath);

        public static string ReadAllFileText(string filePath) =>
            File.ReadAllText(filePath);

        public static byte[] ReadAllFileBytes(string filePath) =>
            File.ReadAllBytes(filePath);

        public static void WriteAllLinesToFile(string filePath, IEnumerable<string> content) =>
            File.WriteAllLines(filePath, content);

        public static void WriteAllBytesToFile(string filePath, byte[] bytes) =>
            File.WriteAllBytes(filePath, bytes);

        public static string[] ReadAllFileLines(string filePath) =>
            File.ReadAllLines(filePath);

        public static string GetTempFilePath() =>
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    }
}
