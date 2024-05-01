using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a track chunk of a standard MIDI file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <see href="https://midi.org/standard-midi-files-specification"/> for detailed MIDI file specification.
    /// </para>
    /// </remarks>
    public sealed class TrackChunk : MidiChunk
    {
        #region Constants

        /// <summary>
        /// ID of the track chunk. This field is constant.
        /// </summary>
        public const string Id = "MTrk";

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackChunk"/>.
        /// </summary>
        public TrackChunk()
            : base(Id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackChunk"/> with the specified events.
        /// </summary>
        /// <param name="events">Events to add to the track chunk.</param>
        /// <remarks>
        /// Note that End Of Track events cannot be added into the collection since it may cause inconsistency in a
        /// track chunk structure. End Of Track event will be written to the track chunk automatically on
        /// <see cref="MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is <c>null</c>.</exception>
        public TrackChunk(IEnumerable<MidiEvent> events)
            : this()
        {
            ThrowIfArgument.IsNull(nameof(events), events);

            Events.AddRange(events);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackChunk"/> with the specified events.
        /// </summary>
        /// <param name="events">Events to add to the track chunk.</param>
        /// <remarks>
        /// Note that End Of Track events cannot be added into the collection since it may cause inconsistency in a
        /// track chunk structure. End Of Track event will be written to the track chunk automatically on
        /// <see cref="MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="events"/> contain an instance of <see cref="EndOfTrackEvent"/>; or
        /// <paramref name="events"/> contain <c>null</c>.
        /// </exception>
        public TrackChunk(params MidiEvent[] events)
            : this()
        {
            Events.AddRange(events);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of events contained in the track chunk.
        /// </summary>
        public EventsCollection Events { get; } = new EventsCollection();

        #endregion

        #region Overrides

        /// <summary>
        /// Clones chunk by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the chunk.</returns>
        public override MidiChunk Clone()
        {
            return new TrackChunk(Events.Select(e => e.Clone()));
        }

        /// <summary>
        /// Reads content of a <see cref="TrackChunk"/>.
        /// </summary>
        /// <remarks>
        /// Content of a <see cref="TrackChunk"/> is collection of MIDI events.
        /// </remarks>
        /// <param name="reader">Reader to read the chunk's content with.</param>
        /// <param name="settings">Settings according to which the chunk's content must be read.</param>
        /// <param name="size">Expected size of the content taken from the chunk's header.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the writer's underlying stream
        /// was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the writer's underlying stream.</exception>
        /// <exception cref="UnexpectedRunningStatusException">Unexpected running status is encountered.</exception>
        /// <exception cref="UnknownChannelEventException">Reader has encountered an unknown channel event.</exception>
        /// <exception cref="NotEnoughBytesException">Not enough bytes to read an event.</exception>
        /// <exception cref="InvalidChannelEventParameterValueException">Value of a channel event's parameter just
        /// read is invalid.</exception>
        /// <exception cref="MissedEndOfTrackEventException">Track chunk doesn't end with End Of Track event.</exception>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
        {
            var endReaderPosition = reader.Position + size;
            var endOfTrackPresented = false;

            byte? currentChannelEventStatusByte = null;

            //

            while (reader.Position < endReaderPosition && !reader.EndReached)
            {
                var midiEvent = ReadEvent(reader, settings, ref currentChannelEventStatusByte);
                if (midiEvent == null)
                    continue;

                if (midiEvent is EndOfTrackEvent)
                {
                    endOfTrackPresented = true;

                    if (settings.EndOfTrackStoringPolicy == EndOfTrackStoringPolicy.Store)
                        Events.Add(midiEvent);

                    break;
                }

                Events.AddInternal(midiEvent);
            }

            //

            if (settings.MissedEndOfTrackPolicy == MissedEndOfTrackPolicy.Abort && !endOfTrackPresented)
                throw new MissedEndOfTrackEventException();

            Events.IsInitialState = true;
        }

        /// <summary>
        /// Writes content of a <see cref="TrackChunk"/>.
        /// </summary>
        /// <remarks>
        /// Content of a <see cref="TrackChunk"/> is collection of MIDI events.
        /// </remarks>
        /// <param name="writer">Writer to write the chunk's content with.</param>
        /// <param name="settings">Settings according to which the chunk's content must be written.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the writer's underlying stream was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the writer's underlying stream.</exception>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            ProcessEvents(settings, (eventWriter, midiEvent, writeStatusByte) =>
            {
                writer.WriteVlqNumber(midiEvent.DeltaTime);
                eventWriter.Write(midiEvent, writer, settings, writeStatusByte);
            });
        }

        /// <summary>
        /// Gets size of <see cref="TrackChunk"/>'s content as number of bytes required to write it according
        /// to specified <see cref="WritingSettings"/>.
        /// </summary>
        /// <param name="settings">Settings according to which the chunk's content will be written.</param>
        /// <returns>Number of bytes required to write <see cref="TrackChunk"/>'s content.</returns>
        protected override uint GetContentSize(WritingSettings settings)
        {
            uint result = 0;

            ProcessEvents(settings, (eventWriter, midiEvent, writeStatusByte) =>
            {
                result += (uint)(midiEvent.DeltaTime.GetVlqLength() +
                                eventWriter.CalculateSize(midiEvent, settings, writeStatusByte));
            });

            return result;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Track chunk ({Events.Count} events)";
        }

        #endregion

        #region Methods

        internal static MidiEvent ReadEvent(MidiReader reader, ReadingSettings settings, ref byte? channelEventStatusByte)
        {
            var deltaTime = reader.ReadVlqLongNumber();
            if (deltaTime < 0)
                deltaTime = 0;

            var statusByte = reader.ReadByte();
            if (statusByte <= SevenBitNumber.MaxValue)
            {
                if (channelEventStatusByte == null)
                    throw new UnexpectedRunningStatusException();

                statusByte = channelEventStatusByte.Value;
                reader.Position--;
            }

            //

            var eventReader = EventReaderFactory.GetReader(statusByte, smfOnly: true);
            var midiEvent = eventReader.Read(reader, settings, statusByte);

            //

            if (midiEvent is ChannelEvent)
                channelEventStatusByte = statusByte;

            //

            if (midiEvent != null)
                midiEvent._deltaTime = deltaTime;

            return midiEvent;
        }

        internal static void ProcessEvent(
            MidiEvent midiEvent,
            WritingSettings settings,
            Action<IEventWriter, MidiEvent, bool> eventHandler,
            ref bool lastEventIsEndOfTrack,
            ref long additionalDeltaTime,
            ref byte? runningStatus,
            ref bool skipSetTempo,
            ref bool skipKeySignature,
            ref bool skipTimeSignature)
        {
            if (midiEvent.EventType == MidiEventType.EndOfTrack)
            {
                lastEventIsEndOfTrack = true;
                additionalDeltaTime += midiEvent.DeltaTime;
                return;
            }

            lastEventIsEndOfTrack = false;

            var eventToWrite = midiEvent;
            if (eventToWrite is SystemCommonEvent || eventToWrite is SystemRealTimeEvent)
                return;

            if (eventToWrite.EventType == MidiEventType.UnknownMeta && settings.DeleteUnknownMetaEvents)
                return;

            //

            if (settings.NoteOffAsSilentNoteOn)
            {
                var noteOffEvent = eventToWrite as NoteOffEvent;
                if (noteOffEvent != null)
                    eventToWrite = new NoteOnEvent
                    {
                        DeltaTime = noteOffEvent.DeltaTime,
                        Channel = noteOffEvent.Channel,
                        NoteNumber = noteOffEvent.NoteNumber
                    };
            }

            //

            if (settings.DeleteDefaultSetTempo && TrySkipDefaultSetTempo(eventToWrite, ref skipSetTempo))
                return;

            if (settings.DeleteDefaultKeySignature && TrySkipDefaultKeySignature(eventToWrite, ref skipKeySignature))
                return;

            if (settings.DeleteDefaultTimeSignature && TrySkipDefaultTimeSignature(eventToWrite, ref skipTimeSignature))
                return;

            //

            IEventWriter eventWriter = EventWriterFactory.GetWriter(eventToWrite);

            var writeStatusByte = true;
            if (eventToWrite is ChannelEvent)
            {
                var statusByte = eventWriter.GetStatusByte(eventToWrite);
                writeStatusByte = runningStatus != statusByte || !settings.UseRunningStatus;
                runningStatus = statusByte;
            }
            else
                runningStatus = null;

            //

            if (additionalDeltaTime > 0)
            {
                eventToWrite = eventToWrite.Clone();
                eventToWrite.DeltaTime += additionalDeltaTime;
            }

            eventHandler(eventWriter, eventToWrite, writeStatusByte);
        }

        private void ProcessEvents(WritingSettings settings, Action<IEventWriter, MidiEvent, bool> eventHandler)
        {
            byte? runningStatus = null;

            var skipSetTempo = true;
            var skipKeySignature = true;
            var skipTimeSignature = true;
            var additionalDeltaTime = 0L;
            var lastEventIsEndOfTrack = false;

            foreach (var midiEvent in Events)
            {
                ProcessEvent(
                    midiEvent,
                    settings,
                    eventHandler,
                    ref lastEventIsEndOfTrack,
                    ref additionalDeltaTime,
                    ref runningStatus,
                    ref skipSetTempo,
                    ref skipKeySignature,
                    ref skipTimeSignature);
            }

            var endOfTrackEvent = new EndOfTrackEvent
            {
                DeltaTime = lastEventIsEndOfTrack ? additionalDeltaTime : 0
            };
            var endOfTrackEventWriter = EventWriterFactory.GetWriter(endOfTrackEvent);
            eventHandler(endOfTrackEventWriter, endOfTrackEvent, true);
        }

        private static bool TrySkipDefaultSetTempo(MidiEvent midiEvent, ref bool skip)
        {
            if (skip)
            {
                var setTempoEvent = midiEvent as SetTempoEvent;
                if (setTempoEvent != null)
                {
                    if (setTempoEvent.MicrosecondsPerQuarterNote == SetTempoEvent.DefaultMicrosecondsPerQuarterNote)
                        return true;

                    skip = false;
                }
            }

            return false;
        }

        private static bool TrySkipDefaultKeySignature(MidiEvent midiEvent, ref bool skip)
        {
            if (skip)
            {
                var keySignatureEvent = midiEvent as KeySignatureEvent;
                if (keySignatureEvent != null)
                {
                    if (keySignatureEvent.Key == KeySignatureEvent.DefaultKey && keySignatureEvent.Scale == KeySignatureEvent.DefaultScale)
                        return true;

                    skip = false;
                }
            }

            return false;
        }

        private static bool TrySkipDefaultTimeSignature(MidiEvent midiEvent, ref bool skip)
        {
            if (skip)
            {
                var timeSignatureEvent = midiEvent as TimeSignatureEvent;
                if (timeSignatureEvent != null)
                {
                    if (timeSignatureEvent.Numerator == TimeSignatureEvent.DefaultNumerator &&
                        timeSignatureEvent.Denominator == TimeSignatureEvent.DefaultDenominator &&
                        timeSignatureEvent.ClocksPerClick == TimeSignatureEvent.DefaultClocksPerClick &&
                        timeSignatureEvent.ThirtySecondNotesPerBeat == TimeSignatureEvent.DefaultThirtySecondNotesPerBeat)
                        return true;

                    skip = false;
                }
            }

            return false;
        }

        #endregion
    }
}
