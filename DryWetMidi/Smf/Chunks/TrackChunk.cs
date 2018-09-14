using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a track chunk of a standard MIDI file.
    /// </summary>
    /// <remarks>
    /// Track chunk contains actual MIDI data as set of events.
    /// </remarks>
    public sealed class TrackChunk : MidiChunk
    {
        #region Constants

        /// <summary>
        /// ID of the track chunk. This field is constsnt.
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
        /// Note that End Of Track events cannot be added into the collection since it may cause inconsistence in a
        /// track chunk structure. End Of Track event will be written to the track chunk automatically on
        /// <see cref="MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
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
        /// Note that End Of Track events cannot be added into the collection since it may cause inconsistence in a
        /// track chunk structure. End Of Track event will be written to the track chunk automatically on
        /// <see cref="MidiFile.Write(string, bool, MidiFileFormat, WritingSettings)"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="events"/> contain an instance of <see cref="EndOfTrackEvent"/>; or
        /// <paramref name="events"/> contain null.
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
                if (midiEvent is EndOfTrackEvent)
                {
                    endOfTrackPresented = true;
                    break;
                }

                Events.Add(midiEvent);
            }

            //

            if (settings.MissedEndOfTrackPolicy == MissedEndOfTrackPolicy.Abort && !endOfTrackPresented)
                throw new MissedEndOfTrackEventException();
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

        #endregion

        #region Methods

        /// <summary>
        /// Reads an event from the reader's underlying stream.
        /// </summary>
        /// <param name="reader">Reader to read an event.</param>
        /// <param name="settings">Settings according to which an event must be read.</param>
        /// <param name="channelEventStatusByte">Current channel event status byte used as running status.</param>
        /// <returns>Instance of the <see cref="MidiEvent"/> representing a MIDI event.</returns>
        /// <exception cref="ObjectDisposedException">Method was called after the writer's underlying stream
        /// was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the writer's underlying stream.</exception>
        /// <exception cref="UnexpectedRunningStatusException">Unexpected running status is encountered.</exception>
        /// <exception cref="UnknownChannelEventException">Reader has encountered an unknown channel event.</exception>
        /// <exception cref="NotEnoughBytesException">Not enough bytes to read an event.</exception>
        /// <exception cref="InvalidChannelEventParameterValueException">Value of a channel event's parameter just
        /// read is invalid.</exception>
        private MidiEvent ReadEvent(MidiReader reader, ReadingSettings settings, ref byte? channelEventStatusByte)
        {
            var deltaTime = reader.ReadVlqLongNumber();
            if (deltaTime < 0)
                deltaTime = 0;

            //

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

            if (settings.SilentNoteOnPolicy == SilentNoteOnPolicy.NoteOff)
            {
                var noteOnEvent = midiEvent as NoteOnEvent;
                if (noteOnEvent?.Velocity == 0)
                {
                    midiEvent = new NoteOffEvent
                    {
                        DeltaTime = noteOnEvent.DeltaTime,
                        Channel = noteOnEvent.Channel,
                        NoteNumber = noteOnEvent.NoteNumber
                    };
                }
            }

            //

            if (midiEvent is ChannelEvent)
                channelEventStatusByte = statusByte;

            //

            midiEvent.DeltaTime = deltaTime;
            return midiEvent;
        }

        private void ProcessEvents(WritingSettings settings, Action<IEventWriter, MidiEvent, bool> eventHandler)
        {
            byte? runningStatus = null;

            var skipSetTempo = true;
            var skipKeySignature = true;
            var skipTimeSignature = true;

            foreach (var midiEvent in GetEventsToWrite())
            {
                var eventToWrite = midiEvent;
                if (eventToWrite is UnknownMetaEvent && settings.CompressionPolicy.HasFlag(CompressionPolicy.DeleteUnknownMetaEvents))
                    continue;

                //

                if (settings.CompressionPolicy.HasFlag(CompressionPolicy.NoteOffAsSilentNoteOn))
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

                if (settings.CompressionPolicy.HasFlag(CompressionPolicy.DeleteDefaultSetTempo) &&
                    TrySkipDefaultSetTempo(eventToWrite, ref skipSetTempo))
                    continue;

                if (settings.CompressionPolicy.HasFlag(CompressionPolicy.DeleteDefaultKeySignature) &&
                    TrySkipDefaultKeySignature(eventToWrite, ref skipKeySignature))
                    continue;

                if (settings.CompressionPolicy.HasFlag(CompressionPolicy.DeleteDefaultTimeSignature) &&
                    TrySkipDefaultTimeSignature(eventToWrite, ref skipTimeSignature))
                    continue;

                //

                IEventWriter eventWriter = EventWriterFactory.GetWriter(eventToWrite);

                var writeStatusByte = true;
                if (eventToWrite is ChannelEvent)
                {
                    var statusByte = eventWriter.GetStatusByte(eventToWrite);
                    writeStatusByte = runningStatus != statusByte || !settings.CompressionPolicy.HasFlag(CompressionPolicy.UseRunningStatus);
                    runningStatus = statusByte;
                }
                else
                    runningStatus = null;

                //

                eventHandler(eventWriter, eventToWrite, writeStatusByte);
            }
        }

        private IEnumerable<MidiEvent> GetEventsToWrite()
        {
            return Events.Where(e => !(e is SystemCommonEvent) && !(e is SystemRealTimeEvent))
                         .Concat(new[] { new EndOfTrackEvent() });
        }

        private static bool TrySkipDefaultSetTempo(MidiEvent midiEvent, ref bool skip)
        {
            if (skip)
            {
                var setTempoEvent = midiEvent as SetTempoEvent;
                if (setTempoEvent != null)
                {
                    if (setTempoEvent.MicrosecondsPerQuarterNote == SetTempoEvent.DefaultTempo)
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
