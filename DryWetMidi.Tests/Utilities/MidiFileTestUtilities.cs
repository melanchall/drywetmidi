using System;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework.Legacy;
using System.IO;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests
{
    public static class MidiFileTestUtilities
    {
        #region Methods

        public static MidiFile Read(byte[] midiFile, ReadingSettings readingSettings)
        {
            var filePath = FileOperations.GetTempFilePath();
            FileOperations.WriteAllBytesToFile(filePath, midiFile);

            try
            {
                return ReadWithAsyncChecks(filePath, readingSettings);
            }
            finally
            {
                FileOperations.DeleteFile(filePath);
            }
        }

        public static MidiFile Read(MidiFile midiFile, WritingSettings writingSettings, ReadingSettings readingSettings, MidiFileFormat? format = null)
        {
            var filePath = FileOperations.GetTempFilePath();

            try
            {
                if (format == null)
                {
                    format = MidiFileFormat.MultiTrack;
                    try
                    {
                        format = midiFile.OriginalFormat;
                    }
                    catch
                    {
                    }
                }

                midiFile.Write(filePath, format: format.Value, settings: writingSettings);
                var result = ReadWithAsyncChecks(filePath, readingSettings);
                var expectedBytes = FileOperations.ReadAllFileBytes(filePath);

                midiFile.Clone().WriteAsync(
                    filePath,
                    overwriteFile: true,
                    format: format.Value,
                    settings: writingSettings,
                    cancellationToken: CancellationToken.None).Wait();

                ClassicAssert.AreEqual(expectedBytes, FileOperations.ReadAllFileBytes(filePath), "Async path writing differs.");
                MidiAsserts.AreEqual(result, ReadWithAsyncChecks(filePath, readingSettings), true, "Async round-trip differs.");

                using (var stream = new MemoryStream())
                {
                    midiFile.Clone().WriteAsync(
                        stream,
                        format: format.Value,
                        settings: writingSettings,
                        cancellationToken: CancellationToken.None).Wait();

                    ClassicAssert.AreEqual(expectedBytes, stream.ToArray(), "Async stream writing differs.");
                }

                return result;
            }
            finally
            {
                FileOperations.DeleteFile(filePath);
            }
        }

        public static void Write(MidiFile midiFile, Action<string> action, WritingSettings settings = null, MidiFileFormat? format = null)
        {
            var filePath = FileOperations.GetTempFilePath();

            try
            {
                midiFile.Write(filePath, format: format ?? MidiFileFormat.MultiTrack, settings: settings);
                var expectedBytes = FileOperations.ReadAllFileBytes(filePath);
                action(filePath);

                midiFile.Clone().WriteAsync(
                    filePath,
                    overwriteFile: true,
                    format: format ?? MidiFileFormat.MultiTrack,
                    settings: settings,
                    cancellationToken: CancellationToken.None).Wait();

                ClassicAssert.AreEqual(expectedBytes, FileOperations.ReadAllFileBytes(filePath), "Async path writing differs.");
                action(filePath);

                using (var stream = new MemoryStream())
                {
                    midiFile.Clone().WriteAsync(
                        stream,
                        format: format ?? MidiFileFormat.MultiTrack,
                        settings: settings,
                        cancellationToken: CancellationToken.None).Wait();

                    ClassicAssert.AreEqual(expectedBytes, stream.ToArray(), "Async stream writing differs.");
                }
            }
            finally
            {
                FileOperations.DeleteFile(filePath);
            }
        }

        internal static MidiFile ReadWithAsyncChecks(
            string filePath,
            ReadingSettings settings = null,
            Action<MidiFile> checkAsyncFile = null)
        {
            MidiFile expected;
            try
            {
                expected = MidiFile.Read(filePath, settings);
            }
            catch (Exception exception)
            {
                var aggregate = ClassicAssert.Throws<AggregateException>(() =>
                    MidiFile.ReadAsync(filePath, settings, cancellationToken: CancellationToken.None).Wait(), "Async path reading did not throw.");

                ClassicAssert.AreEqual(exception.GetType(), aggregate.InnerException.GetType(), "Async path exception differs.");

                using (var stream = File.OpenRead(filePath))
                {
                    aggregate = ClassicAssert.Throws<AggregateException>(() =>
                        MidiFile.ReadAsync(stream, settings, cancellationToken: CancellationToken.None).Wait(), "Async stream reading did not throw.");

                    ClassicAssert.AreEqual(exception.GetType(), aggregate.InnerException.GetType(), "Async stream exception differs.");
                }

                throw;
            }

            var actual = MidiFile.ReadAsync(filePath, settings, cancellationToken: CancellationToken.None).Result;

            MidiAsserts.AreEqual(expected, actual, true, "Async path reading differs.");
            checkAsyncFile?.Invoke(actual);

            using (var stream = File.OpenRead(filePath))
            {
                actual = MidiFile.ReadAsync(stream, settings, cancellationToken: CancellationToken.None).Result;

                MidiAsserts.AreEqual(expected, actual, true, "Async stream reading differs.");
                checkAsyncFile?.Invoke(actual);
            }

            return expected;
        }

        #endregion
    }
}
