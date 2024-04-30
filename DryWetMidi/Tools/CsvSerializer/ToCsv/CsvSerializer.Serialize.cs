using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to serialize MIDI data to CSV and deserialize it back. More info in the
    /// <see href="xref:a_csv_serializer">CSV serializer</see> article.
    /// </summary>
    /// <seealso cref="CsvSerializer"/>
    public static partial class CsvSerializer
    {
        #region Methods

        /// <summary>
        /// Writes the specified <see cref="MidiFile"/> to a stream in CSV format.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to serialize to CSV representation.</param>
        /// <param name="stream"><see cref="Stream"/> to write the <paramref name="midiFile"/> to.</param>
        /// <param name="settings">Settings according to which <paramref name="midiFile"/> should be serialized.</param>
        /// <param name="objectType">Types of objects within track chunks of the <paramref name="midiFile"/>
        /// to serialize (see Remarks section).</param>
        /// <param name="objectDetectionSettings">Settings according to which objects within track chunks should be
        /// detected and built.</param>
        /// <remarks>
        /// <paramref name="objectType"/> defines what kind of objects to put to the <paramref name="objectType"/>
        /// as CSV. For example, if you specify <see cref="ObjectType.TimedEvent"/>, all events within track chunks
        /// of the <paramref name="midiFile"/> will be written to the stream. If you specify <see cref="ObjectType.Note"/>,
        /// only notes will be written. If you specify <see cref="ObjectType.Note"/> | <see cref="ObjectType.TimedEvent"/>,
        /// events and notes will be written. Please see <see href="xref:a_getting_objects#getobjects">Getting objects: GetObjects</see>
        /// article to learn more.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="stream"/> doesn't support writing.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="stream"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SerializeToCsv(
            this MidiFile midiFile,
            Stream stream,
            CsvSerializationSettings settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(stream), stream);

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            settings = settings ?? new CsvSerializationSettings();
            objectDetectionSettings = objectDetectionSettings ?? new ObjectDetectionSettings();

            var chunkIndex = 0;
            var tempoMap = midiFile.GetTempoMap();

            using (var writer = new CsvWriter(stream, settings))
            {
                WriteHeaderChunk(midiFile, writer, settings, chunkIndex++);

                foreach (var chunk in midiFile.Chunks)
                {
                    WriteChunk(chunk, writer, settings, tempoMap, objectType, objectDetectionSettings, chunkIndex++);
                }
            }
        }

        /// <summary>
        /// Writes the specified <see cref="MidiFile"/> to a file in CSV format.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to serialize to CSV representation.</param>
        /// <param name="filePath">Path to the file to write the <paramref name="midiFile"/> to.</param>
        /// <param name="overwriteFile">If <c>true</c> and file specified by <paramref name="filePath"/> already
        /// exists it will be overwritten; if <c>false</c> and the file exists exception will be thrown.</param>
        /// <param name="settings">Settings according to which <paramref name="midiFile"/> should be serialized.</param>
        /// <param name="objectType">Types of objects within track chunks of the <paramref name="midiFile"/>
        /// to serialize (see Remarks section).</param>
        /// <param name="objectDetectionSettings">Settings according to which objects within track chunks should be
        /// detected and built.</param>
        /// <remarks>
        /// <paramref name="objectType"/> defines what kind of objects to put to the <paramref name="objectType"/>
        /// as CSV. For example, if you specify <see cref="ObjectType.TimedEvent"/>, all events within track chunks
        /// of the <paramref name="midiFile"/> will be written to the stream. If you specify <see cref="ObjectType.Note"/>,
        /// only notes will be written. If you specify <see cref="ObjectType.Note"/> | <see cref="ObjectType.TimedEvent"/>,
        /// events and notes will be written. Please see <see href="xref:a_getting_objects#getobjects">Getting objects: GetObjects</see>
        /// article to learn more.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string,
        /// contains only white space, or contains one or more invalid characters as defined by
        /// <see cref="Path.InvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static void SerializeToCsv(
            this MidiFile midiFile,
            string filePath,
            bool overwriteFile,
            CsvSerializationSettings settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            using (var fileStream = FileUtilities.OpenFileForWrite(filePath, overwriteFile))
            {
                midiFile.SerializeToCsv(fileStream, settings, objectType, objectDetectionSettings);
            }
        }

        /// <summary>
        /// Writes the specified collection of <see cref="MidiChunk"/> to a stream in CSV format.
        /// </summary>
        /// <param name="midiChunks">Collection of <see cref="MidiChunk"/> to serialize to CSV representation.</param>
        /// <param name="stream"><see cref="Stream"/> to write the <paramref name="midiChunks"/> to.</param>
        /// <param name="tempoMap"><see cref="TempoMap"/> to use for time/length conversions.</param>
        /// <param name="settings">Settings according to which <paramref name="midiChunks"/> should be serialized.</param>
        /// <param name="objectType">Types of objects within track chunks of the <paramref name="midiChunks"/>
        /// to serialize (see Remarks section).</param>
        /// <param name="objectDetectionSettings">Settings according to which objects within track chunks should be
        /// detected and built.</param>
        /// <remarks>
        /// <paramref name="objectType"/> defines what kind of objects to put to the <paramref name="objectType"/>
        /// as CSV. For example, if you specify <see cref="ObjectType.TimedEvent"/>, all events within track chunks
        /// of the <paramref name="midiChunks"/> will be written to the stream. If you specify <see cref="ObjectType.Note"/>,
        /// only notes will be written. If you specify <see cref="ObjectType.Note"/> | <see cref="ObjectType.TimedEvent"/>,
        /// events and notes will be written. Please see <see href="xref:a_getting_objects#getobjects">Getting objects: GetObjects</see>
        /// article to learn more.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="stream"/> doesn't support writing.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="stream"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SerializeToCsv(
            this IEnumerable<MidiChunk> midiChunks,
            Stream stream,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiChunks), midiChunks);
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            settings = settings ?? new CsvSerializationSettings();
            objectDetectionSettings = objectDetectionSettings ?? new ObjectDetectionSettings();

            var chunkIndex = 0;

            using (var writer = new CsvWriter(stream, settings))
            {
                foreach (var midiChunk in midiChunks)
                {
                    WriteChunk(midiChunk, writer, settings, tempoMap, objectType, objectDetectionSettings, chunkIndex++);
                }
            }
        }

        /// <summary>
        /// Writes the specified collection of <see cref="MidiChunk"/> to a file in CSV format.
        /// </summary>
        /// <param name="midiChunks">Collection of <see cref="MidiChunk"/> to serialize to CSV representation.</param>
        /// <param name="filePath">Path to the file to write the <paramref name="midiChunks"/> to.</param>
        /// <param name="overwriteFile">If <c>true</c> and file specified by <paramref name="filePath"/> already
        /// exists it will be overwritten; if <c>false</c> and the file exists exception will be thrown.</param>
        /// <param name="tempoMap"><see cref="TempoMap"/> to use for time/length conversions.</param>
        /// <param name="settings">Settings according to which <paramref name="midiChunks"/> should be serialized.</param>
        /// <param name="objectType">Types of objects within track chunks of the <paramref name="midiChunks"/>
        /// to serialize (see Remarks section).</param>
        /// <param name="objectDetectionSettings">Settings according to which objects within track chunks should be
        /// detected and built.</param>
        /// <remarks>
        /// <paramref name="objectType"/> defines what kind of objects to put to the <paramref name="objectType"/>
        /// as CSV. For example, if you specify <see cref="ObjectType.TimedEvent"/>, all events within track chunks
        /// of the <paramref name="midiChunks"/> will be written to the stream. If you specify <see cref="ObjectType.Note"/>,
        /// only notes will be written. If you specify <see cref="ObjectType.Note"/> | <see cref="ObjectType.TimedEvent"/>,
        /// events and notes will be written. Please see <see href="xref:a_getting_objects#getobjects">Getting objects: GetObjects</see>
        /// article to learn more.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string,
        /// contains only white space, or contains one or more invalid characters as defined by
        /// <see cref="Path.InvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SerializeToCsv(
            this IEnumerable<MidiChunk> midiChunks,
            string filePath,
            bool overwriteFile,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiChunks), midiChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            using (var fileStream = FileUtilities.OpenFileForWrite(filePath, overwriteFile))
            {
                midiChunks.SerializeToCsv(fileStream, tempoMap, settings, objectType, objectDetectionSettings);
            }
        }

        /// <summary>
        /// Writes the specified <see cref="MidiChunk"/> to a stream in CSV format.
        /// </summary>
        /// <param name="midiChunk"><see cref="MidiChunk"/> to serialize to CSV representation.</param>
        /// <param name="stream"><see cref="Stream"/> to write the <paramref name="midiChunk"/> to.</param>
        /// <param name="tempoMap"><see cref="TempoMap"/> to use for time/length conversions.</param>
        /// <param name="settings">Settings according to which <paramref name="midiChunk"/> should be serialized.</param>
        /// <param name="objectType">Types of objects within <paramref name="midiChunk"/> to serialize
        /// if it's a track chunk (see Remarks section).</param>
        /// <param name="objectDetectionSettings">Settings according to which objects within a track chunk should be
        /// detected and built.</param>
        /// <remarks>
        /// <paramref name="objectType"/> defines what kind of objects to put to the <paramref name="objectType"/>
        /// as CSV. For example, if you specify <see cref="ObjectType.TimedEvent"/>, all events within <paramref name="midiChunk"/>
        /// will be written to the stream if it's a track chunk. If you specify <see cref="ObjectType.Note"/>,
        /// only notes will be written. If you specify <see cref="ObjectType.Note"/> | <see cref="ObjectType.TimedEvent"/>,
        /// events and notes will be written. Please see <see href="xref:a_getting_objects#getobjects">Getting objects: GetObjects</see>
        /// article to learn more.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="stream"/> doesn't support writing.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="stream"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SerializeToCsv(
            this MidiChunk midiChunk,
            Stream stream,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiChunk), midiChunk);
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            settings = settings ?? new CsvSerializationSettings();
            objectDetectionSettings = objectDetectionSettings ?? new ObjectDetectionSettings();

            using (var writer = new CsvWriter(stream, settings))
            {
                WriteChunk(midiChunk, writer, settings, tempoMap, objectType, objectDetectionSettings, 0);
            }
        }

        /// <summary>
        /// Writes the specified <see cref="MidiChunk"/> to a file in CSV format.
        /// </summary>
        /// <param name="midiChunk"><see cref="MidiChunk"/> to serialize to CSV representation.</param>
        /// <param name="filePath">Path to the file to write the <paramref name="midiChunk"/> to.</param>
        /// <param name="overwriteFile">If <c>true</c> and file specified by <paramref name="filePath"/> already
        /// exists it will be overwritten; if <c>false</c> and the file exists exception will be thrown.</param>
        /// <param name="tempoMap"><see cref="TempoMap"/> to use for time/length conversions.</param>
        /// <param name="settings">Settings according to which <paramref name="midiChunk"/> should be serialized.</param>
        /// <param name="objectType">Types of objects within <paramref name="midiChunk"/> to serialize
        /// if it's a track chunk (see Remarks section).</param>
        /// <param name="objectDetectionSettings">Settings according to which objects within a track chunk should be
        /// detected and built.</param>
        /// <remarks>
        /// <paramref name="objectType"/> defines what kind of objects to put to the <paramref name="objectType"/>
        /// as CSV. For example, if you specify <see cref="ObjectType.TimedEvent"/>, all events within <paramref name="midiChunk"/>
        /// will be written to the stream if it's a track chunk. If you specify <see cref="ObjectType.Note"/>,
        /// only notes will be written. If you specify <see cref="ObjectType.Note"/> | <see cref="ObjectType.TimedEvent"/>,
        /// events and notes will be written. Please see <see href="xref:a_getting_objects#getobjects">Getting objects: GetObjects</see>
        /// article to learn more.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string,
        /// contains only white space, or contains one or more invalid characters as defined by
        /// <see cref="Path.InvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SerializeToCsv(
            this MidiChunk midiChunk,
            string filePath,
            bool overwriteFile,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null,
            ObjectType objectType = ObjectType.TimedEvent,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiChunk), midiChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            using (var fileStream = FileUtilities.OpenFileForWrite(filePath, overwriteFile))
            {
                midiChunk.SerializeToCsv(fileStream, tempoMap, settings, objectType, objectDetectionSettings);
            }
        }

        /// <summary>
        /// Writes the specified objects to a stream in CSV format.
        /// </summary>
        /// <param name="timedObjects">Collection of <see cref="ITimedObject"/> to serialize to CSV representation.</param>
        /// <param name="stream"><see cref="Stream"/> to write the <paramref name="timedObjects"/> to.</param>
        /// <param name="tempoMap"><see cref="TempoMap"/> to use for time/length conversions.</param>
        /// <param name="settings">Settings according to which <paramref name="timedObjects"/> should be serialized.</param>
        /// <exception cref="ArgumentException"><paramref name="stream"/> doesn't support writing.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timedObjects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="stream"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SerializeToCsv(
            this IEnumerable<ITimedObject> timedObjects,
            Stream stream,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!stream.CanWrite)
                throw new ArgumentException("Stream doesn't support writing.", nameof(stream));

            settings = settings ?? new CsvSerializationSettings();

            using (var writer = new CsvWriter(stream, settings))
            {
                WriteObjects(timedObjects, writer, settings, tempoMap, null, null);
            }
        }

        /// <summary>
        /// Writes the specified objects to a file in CSV format.
        /// </summary>
        /// <param name="timedObjects">Collection of <see cref="ITimedObject"/> to serialize to CSV representation.</param>
        /// <param name="filePath">Path to the file to write the <paramref name="timedObjects"/> to.</param>
        /// <param name="overwriteFile">If <c>true</c> and file specified by <paramref name="filePath"/> already
        /// exists it will be overwritten; if <c>false</c> and the file exists exception will be thrown.</param>
        /// <param name="tempoMap"><see cref="TempoMap"/> to use for time/length conversions.</param>
        /// <param name="settings">Settings according to which <paramref name="timedObjects"/> should be serialized.</param>
        /// <exception cref="ArgumentException"><paramref name="filePath"/> is a zero-length string,
        /// contains only white space, or contains one or more invalid characters as defined by
        /// <see cref="Path.InvalidPathChars"/>.</exception>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timedObjects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SerializeToCsv(
            this IEnumerable<ITimedObject> timedObjects,
            string filePath,
            bool overwriteFile,
            TempoMap tempoMap,
            CsvSerializationSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNullOrEmptyString(nameof(filePath), filePath, "File path");

            using (var fileStream = FileUtilities.OpenFileForWrite(filePath, overwriteFile))
            {
                timedObjects.SerializeToCsv(fileStream, tempoMap, settings);
            }
        }

        private static void WriteChunk(
            MidiChunk midiChunk,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings,
            int chunkIndex)
        {
            var trackChunk = midiChunk as TrackChunk;
            if (trackChunk != null)
            {
                WriteTrackChunk(trackChunk, writer, settings, tempoMap, objectType, objectDetectionSettings, chunkIndex);
                return;
            }

            var unknownChunk = midiChunk as UnknownChunk;
            if (unknownChunk != null)
            {
                WriteUnknownChunk(unknownChunk, writer, settings, chunkIndex);
                return;
            }

            WriteCustomChunk(midiChunk, writer, settings, chunkIndex);
        }

        private static void WriteHeaderChunk(
            MidiFile midiFile,
            CsvWriter writer,
            CsvSerializationSettings settings,
            int chunkIndex)
        {
            writer.WriteRecord(
                chunkIndex,
                HeaderChunk.Id,
                0,
                Record.HeaderType,
                midiFile.TimeDivision.ToInt16());
        }

        private static void WriteTrackChunk(
            TrackChunk trackChunk,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings,
            int chunkIndex)
        {
            WriteObjects(
                trackChunk.GetObjects(objectType, objectDetectionSettings),
                writer,
                settings,
                tempoMap,
                chunkIndex,
                TrackChunk.Id);
        }

        private static void WriteUnknownChunk(
            UnknownChunk unknownChunk,
            CsvWriter writer,
            CsvSerializationSettings settings,
            int chunkIndex)
        {
            // TODO: WriteUnknownChunk
        }

        private static void WriteCustomChunk(
            MidiChunk midiChunk,
            CsvWriter writer,
            CsvSerializationSettings settings,
            int chunkIndex)
        {
            // TODO: WriteCustomChunk
        }

        private static void WriteObjects(
            IEnumerable<ITimedObject> timedObjects,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId)
        {
            var objectIndex = 0;

            foreach (var obj in timedObjects)
            {
                WriteObject(obj, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex++);
            }
        }

        private static void WriteObject(
            ITimedObject timedObject,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId,
            int objectIndex)
        {
            var timedEvent = timedObject as TimedEvent;
            if (timedEvent != null)
            {
                WriteTimedEvent(timedEvent, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
                return;
            }

            var note = timedObject as Note;
            if (note != null)
            {
                WriteNote(note, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
                return;
            }

            var chord = timedObject as Chord;
            if (chord != null)
            {
                WriteChord(chord, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
                return;
            }

            var registeredParameter = timedObject as RegisteredParameter;
            if (registeredParameter != null)
            {
                WriteRegisteredParameter(registeredParameter, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
                return;
            }

            var rest = timedObject as Rest;
            if (rest != null)
                return;

            WriteCustomObject(timedObject, writer, settings, chunkIndex, chunkId, objectIndex);
        }

        private static void WriteRegisteredParameter(
            RegisteredParameter registeredParameter,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId,
            int objectIndex)
        {
            foreach (var timedEvent in registeredParameter.GetTimedEvents())
            {
                WriteTimedEvent(timedEvent, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
            }
        }

        private static void WriteTimedEvent(
            TimedEvent timedEvent,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId,
            int objectIndex)
        {
            var midiEvent = timedEvent.Event;
            if (midiEvent is EndOfTrackEvent)
                return;

            var eventParameters = EventParametersProvider.GetEventParameters(midiEvent, settings);

            WriteObjectRecord(
                writer,
                settings,
                tempoMap,
                chunkIndex,
                chunkId,
                objectIndex,
                timedEvent,
                eventParameters);
        }

        private static void WriteNote(
            Note note,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId,
            int objectIndex)
        {
            WriteObjectRecord(
                writer,
                settings,
                tempoMap,
                chunkIndex,
                chunkId,
                objectIndex,
                note,
                CsvFormattingUtilities.FormatLength(note, settings.LengthType, tempoMap),
                note.Channel,
                CsvFormattingUtilities.FormatNoteNumber(note.NoteNumber, settings.NoteFormat),
                note.Velocity,
                note.OffVelocity);
        }

        private static void WriteChord(
            Chord chord,
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId,
            int objectIndex)
        {
            foreach (var note in chord.Notes)
            {
                WriteNote(note, writer, settings, tempoMap, chunkIndex, chunkId, objectIndex);
            }
        }

        private static void WriteCustomObject(
            ITimedObject timedObject,
            CsvWriter writer,
            CsvSerializationSettings settings,
            int? chunkIndex,
            string chunkId,
            int objectIndex)
        {
            // TODO: WriteCustomObject
        }

        private static void WriteObjectRecord(
            CsvWriter writer,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            int? chunkIndex,
            string chunkId,
            int objectIndex,
            ITimedObject obj,
            params object[] values)
        {
            writer.WriteRecord(
                new[]
                {
                    chunkIndex,
                    chunkId,
                    objectIndex,
                    obj is TimedEvent ? ((TimedEvent)obj).Event.EventType.ToString() : Record.NoteType,
                    CsvFormattingUtilities.FormatTime(obj, settings.TimeType, tempoMap)
                }
                .Where(v => v != null)
                .Concat(values));
        }

        #endregion
    }
}
