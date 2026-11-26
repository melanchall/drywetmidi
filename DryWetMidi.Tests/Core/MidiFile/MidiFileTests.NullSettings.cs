using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Core
{
    public sealed partial class MidiFileTests
    {
        #region Nested types

        public enum NullReadingSetting
        {
            TextEncoding,
            ReaderSettings
        }

        public enum NullWritingSetting
        {
            TextEncoding,
            WriterSettings
        }

        #endregion

        #region Constants

        private const string NonAsciiText = "Test▶▶▶";

        #endregion

        #region Test methods

        [Test]
        public async Task Read_NullSetting_UsesDefaultValue([Values] NullReadingSetting nullReadingSetting)
        {
            var midiFileBytes = GetUtf8TextMidiFileBytes();
            var expectedMidiFile = MidiFile.Read(new MemoryStream(midiFileBytes), new ReadingSettings());

            ClassicAssert.AreEqual(
                Encoding.ASCII.GetString(Encoding.UTF8.GetBytes(NonAsciiText)),
                expectedMidiFile.GetEvents().OfType<TextEvent>().Single().Text,
                "Text is not decoded with the default encoding.");

            using (var inputStream = new MemoryStream(midiFileBytes))
            {
                var midiFile = MidiFile.Read(inputStream, CreateReadingSettings(nullReadingSetting));
                MidiAsserts.AreEqual(expectedMidiFile, midiFile, true, "Synchronously read file is invalid.");
            }

            using (var inputStream = new MemoryStream(midiFileBytes))
            {
                var midiFile = await MidiFile.ReadAsync(inputStream, CreateReadingSettings(nullReadingSetting));
                MidiAsserts.AreEqual(expectedMidiFile, midiFile, true, "Asynchronously read file is invalid.");
            }
        }

        [Test]
        public void ReadLazy_NullSetting_UsesDefaultValue([Values] NullReadingSetting nullReadingSetting)
        {
            var midiFileBytes = GetUtf8TextMidiFileBytes();

            MidiToken[] expectedTokens;
            using (var inputStream = new MemoryStream(midiFileBytes))
            using (var tokensReader = MidiFile.ReadLazy(inputStream, new ReadingSettings()))
            {
                expectedTokens = ReadAllTokens(tokensReader);
            }

            using (var inputStream = new MemoryStream(midiFileBytes))
            using (var tokensReader = MidiFile.ReadLazy(inputStream, CreateReadingSettings(nullReadingSetting)))
            {
                var actualTokens = ReadAllTokens(tokensReader);
                ClassicAssert.AreEqual(expectedTokens.Length, actualTokens.Length, "Invalid tokens count.");

                for (var i = 0; i < expectedTokens.Length; i++)
                {
                    ClassicAssert.IsTrue(AreTokensEqual(expectedTokens[i], actualTokens[i]), $"Invalid token {i}.");
                }
            }

            using (var inputStream = new MemoryStream(midiFileBytes))
            using (var tokensReader = MidiFile.ReadLazyAsync(inputStream, CreateReadingSettings(nullReadingSetting)).Result)
            {
                CheckAsyncTokens(tokensReader, expectedTokens);
            }
        }

        [Test]
        public async Task Write_NullSetting_UsesDefaultValue([Values] NullWritingSetting nullWritingSetting)
        {
            var midiFile = new MidiFile(new TrackChunk(
                new TextEvent(NonAsciiText),
                new NoteOnEvent { DeltaTime = 10 }));

            var expectedBytes = WriteToBytes(outputStream => midiFile.Write(outputStream, settings: new WritingSettings()));
            CheckDefaultTextEncodingIsUsed(expectedBytes);

            var actualBytes = WriteToBytes(outputStream => midiFile.Write(outputStream, settings: CreateWritingSettings(nullWritingSetting)));
            ClassicAssert.AreEqual(expectedBytes, actualBytes, "Synchronously written bytes are invalid.");

            using (var outputStream = new MemoryStream())
            {
                await midiFile.WriteAsync(outputStream, settings: CreateWritingSettings(nullWritingSetting));
                ClassicAssert.AreEqual(expectedBytes, outputStream.ToArray(), "Asynchronously written bytes are invalid.");
            }
        }

        [Test]
        public async Task WriteLazy_NullSetting_UsesDefaultValue([Values] NullWritingSetting nullWritingSetting)
        {
            var expectedBytes = WriteToBytes(outputStream => WriteLazyTextAndNote(outputStream, new WritingSettings()));
            CheckDefaultTextEncodingIsUsed(expectedBytes);

            var actualBytes = WriteToBytes(outputStream => WriteLazyTextAndNote(outputStream, CreateWritingSettings(nullWritingSetting)));
            ClassicAssert.AreEqual(expectedBytes, actualBytes, "Synchronously lazily written bytes are invalid.");

            using (var outputStream = new MemoryStream())
            {
                var tokensWriter = await MidiFile.WriteLazyAsync(outputStream, CreateWritingSettings(nullWritingSetting));

                await using (tokensWriter)
                {
                    await tokensWriter.StartTrackChunkAsync();
                    await tokensWriter.WriteEventAsync(new TextEvent(NonAsciiText));
                    await tokensWriter.WriteEventAsync(new NoteOnEvent { DeltaTime = 10 });
                }

                ClassicAssert.AreEqual(expectedBytes, outputStream.ToArray(), "Asynchronously lazily written bytes are invalid.");
            }
        }

        #endregion

        #region Private methods

        private static ReadingSettings CreateReadingSettings(NullReadingSetting nullReadingSetting)
        {
            switch (nullReadingSetting)
            {
                case NullReadingSetting.TextEncoding:
                    return new ReadingSettings { TextEncoding = null };
                case NullReadingSetting.ReaderSettings:
                    return new ReadingSettings { ReaderSettings = null };
                default:
                    throw new ArgumentOutOfRangeException(nameof(nullReadingSetting));
            }
        }

        private static WritingSettings CreateWritingSettings(NullWritingSetting nullWritingSetting)
        {
            switch (nullWritingSetting)
            {
                case NullWritingSetting.TextEncoding:
                    return new WritingSettings { TextEncoding = null };
                case NullWritingSetting.WriterSettings:
                    return new WritingSettings { WriterSettings = null };
                default:
                    throw new ArgumentOutOfRangeException(nameof(nullWritingSetting));
            }
        }

        private static byte[] GetUtf8TextMidiFileBytes()
        {
            var midiFile = new MidiFile(new TrackChunk(
                new TextEvent(NonAsciiText),
                new NoteOnEvent { DeltaTime = 10 }));

            return WriteToBytes(outputStream => midiFile.Write(outputStream, settings: new WritingSettings { TextEncoding = Encoding.UTF8 }));
        }

        private static byte[] WriteToBytes(Action<Stream> writeAction)
        {
            using (var outputStream = new MemoryStream())
            {
                writeAction(outputStream);
                return outputStream.ToArray();
            }
        }

        private static void WriteLazyTextAndNote(Stream outputStream, WritingSettings settings)
        {
            using (var tokensWriter = MidiFile.WriteLazy(outputStream, settings))
            {
                tokensWriter.StartTrackChunk();
                tokensWriter.WriteEvent(new TextEvent(NonAsciiText));
                tokensWriter.WriteEvent(new NoteOnEvent { DeltaTime = 10 });
            }
        }

        private static void CheckDefaultTextEncodingIsUsed(byte[] midiFileBytes)
        {
            var asciiTextBytes = Encoding.ASCII.GetBytes(NonAsciiText);
            var textEventBytes = new byte[] { 0xFF, 0x01 }
                .Concat(DataTypesUtilities.GetVlqBytes(asciiTextBytes.Length))
                .Concat(asciiTextBytes)
                .ToArray();

            ClassicAssert.IsTrue(
                Enumerable.Range(0, midiFileBytes.Length - textEventBytes.Length + 1)
                    .Any(offset => midiFileBytes.Skip(offset).Take(textEventBytes.Length).SequenceEqual(textEventBytes)),
                "Text is not encoded with the default encoding.");
        }

        #endregion
    }
}
