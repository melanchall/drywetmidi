using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi
{
    public sealed class TrackChunk : MidiChunk
    {
        #region Constants

        /// <summary>
        /// ID of the track chunk. This field is constsnt.
        /// </summary>
        public const string Id = "MTrk";

        #endregion

        #region Fields

        private byte? _channelEventStatusByte = null;

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
        /// <exception cref="ArgumentException"><paramref name="events"/> contain an instance of <see cref="EndOfTrackEvent"/>; or
        /// <paramref name="events"/> contain null.
        /// </exception>
        public TrackChunk(IEnumerable<MidiEvent> events)
            : this()
        {
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
        /// Reads content of a <see cref="TrackChunk"/>.
        /// </summary>
        /// <remarks>
        /// Content of a track chunk is collection of MIDI events.
        /// </remarks>
        /// <param name="reader">Reader to read the chunk's content with.</param>
        /// <param name="settings">Settings according to which the chunk's content must be read.</param>
        /// <param name="size">Expected size of the content taken from the chunk's header.</param>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
        {
            var endReaderPosition = reader.Position + size;
            var endOfTrackPresented = false;

            //

            while (reader.Position < endReaderPosition)
            {
                var midiEvent = ReadEvent(reader, settings);
                if (midiEvent is EndOfTrackEvent)
                {
                    endOfTrackPresented = true;
                    break;
                }

                Events.Add(midiEvent);
            }

            _channelEventStatusByte = null;

            //

            if (settings.MissedEndOfTrackPolicy == MissedEndOfTrackPolicy.Abort && !endOfTrackPresented)
                throw new MissedEndOfTrackEventException();
        }

        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            ProcessEvents(settings, (eventWriter, midiEvent, writeStatusByte) =>
            {
                writer.WriteVlqNumber(midiEvent.DeltaTime);
                eventWriter.Write(midiEvent, writer, settings, writeStatusByte);
            });
        }

        protected override uint GetContentSize(WritingSettings settings)
        {
            uint result = 0;

            ProcessEvents(settings, (eventWriter, midiEvent, writeStatusByte) =>
            {
                result += (uint)midiEvent.DeltaTime.GetVlqLength();
                result += (uint)eventWriter.CalculateSize(midiEvent, settings, writeStatusByte);
            });

            return result;
        }

        #endregion

        #region Methods

        private MidiEvent ReadEvent(MidiReader reader, ReadingSettings settings)
        {
            var deltaTime = reader.ReadVlqNumber();

            //

            var statusByte = reader.ReadByte();
            if (statusByte <= SevenBitNumber.MaxValue)
            {
                reader.Position--;
                statusByte = _channelEventStatusByte.Value;
            }

            //

            var eventReader = EventReaderFactory.GetReader(statusByte);
            var midiEvent = eventReader.Read(reader, settings, statusByte);

            //

            if (settings.SilentNoteOnPolicy == SilentNoteOnPolicy.NoteOff)
            {
                var noteOnEvent = midiEvent as NoteOnEvent;
                if (noteOnEvent != null && noteOnEvent.Velocity == 0)
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
                _channelEventStatusByte = statusByte;

            //

            midiEvent.DeltaTime = deltaTime;
            return midiEvent;
        }

        private void ProcessEvents(WritingSettings settings, Action<IEventWriter, MidiEvent, bool> eventHandler)
        {
            byte? runningStatus = null;
            var writeStatusByte = true;

            var skipSetTempo = true;
            var skipKeySignature = true;
            var skipTimeSignature = true;

            foreach (var midiEvent in Events.Concat(new[] { new EndOfTrackEvent() }))
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

                if (eventToWrite is ChannelEvent)
                {
                    var statusByte = eventWriter.GetStatusByte(eventToWrite);
                    writeStatusByte = runningStatus != statusByte || !settings.CompressionPolicy.HasFlag(CompressionPolicy.UseRunningStatus);
                    runningStatus = statusByte;
                }
                else
                {
                    runningStatus = null;
                    writeStatusByte = true;
                }

                //

                eventHandler(eventWriter, eventToWrite, writeStatusByte);
            }
        }

        private static bool TrySkipDefaultSetTempo(MidiEvent midiEvent, ref bool skip)
        {
            if (skip)
            {
                var setTempoEvent = midiEvent as SetTempoEvent;
                if (setTempoEvent != null)
                {
                    if (setTempoEvent.MicrosecondsPerBeat == SetTempoEvent.DefaultTempo)
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
                        timeSignatureEvent.Clocks == TimeSignatureEvent.DefaultClocks &&
                        timeSignatureEvent.NumberOf32ndNotesPerBeat == TimeSignatureEvent.Default32ndNotesPerBeat)
                        return true;

                    skip = false;
                }
            }

            return false;
        }

        #endregion
    }
}
