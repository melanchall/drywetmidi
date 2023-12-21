using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Splitter
    {
        #region Methods

        /// <summary>
        /// Cuts a part of the specified length from a MIDI file (starting at the specified time within the file)
        /// and returns a new instance of <see cref="MidiFile"/> which is the original one without the part. More info
        /// in the <see href="xref:a_file_splitting#cutpart">MIDI file splitting: CutPart</see> article.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to cut part from.</param>
        /// <param name="partStart">The start time of part to cut.</param>
        /// <param name="partLength">The length of part to cut.</param>
        /// <param name="settings">Settings according to which <paramref name="midiFile"/>
        /// should be split.</param>
        /// <returns><see cref="MidiFile"/> which is the <paramref name="midiFile"/> without a part defined by
        /// <paramref name="partStart"/> and <paramref name="partLength"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="partStart"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="partLength"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static MidiFile CutPart(this MidiFile midiFile, ITimeSpan partStart, ITimeSpan partLength, SliceMidiFileSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(partStart), partStart);
            ThrowIfArgument.IsNull(nameof(partLength), partLength);

            var grid = new ArbitraryGrid(partStart, partStart.Add(partLength, TimeSpanMode.TimeLength));

            var partsStartId = Guid.NewGuid().ToString();
            var partEndId = Guid.NewGuid().ToString();

            settings = settings ?? new SliceMidiFileSettings();

            var internalSettings = new SliceMidiFileSettings
            {
                PreserveTrackChunks = true,
                PreserveTimes = settings.PreserveTimes,
                SplitNotes = settings.SplitNotes,
                Markers = new SliceMidiFileMarkers
                {
                    PartStartMarkerEventFactory = () => new MarkerEvent(partsStartId),
                    PartEndMarkerEventFactory = () => new MarkerEvent(partEndId)
                },
                NoteDetectionSettings = settings.NoteDetectionSettings
            };

            var tempoMap = midiFile.GetTempoMap();
            var times = grid.GetTimes(tempoMap).ToArray();

            //

            var notesToSplitDescriptors = settings.SplitNotes
                ? midiFile
                    .GetTrackChunks()
                    .Select(c =>
                    {
                        var notes = c.Events
                            .GetTimedEventsLazy(null)
                            .GetNotesAndTimedEventsLazy(settings.NoteDetectionSettings)
                            .OfType<Note>();

                        var descriptors = new List<Tuple<NoteId, SevenBitNumber, SevenBitNumber>>();

                        foreach (var note in notes)
                        {
                            if (note.EndTime <= times[0])
                                continue;

                            if (note.Time >= times[1])
                                break;

                            if (note.Time < times[0] && note.EndTime > times[1])
                                descriptors.Add(Tuple.Create((NoteId)note.GetObjectId(), note.Velocity, note.OffVelocity));
                        }

                        return descriptors;
                    })
                    .ToList()
                : midiFile.GetTrackChunks().Select(c => new List<Tuple<NoteId, SevenBitNumber, SevenBitNumber>>());

            //

            midiFile = PrepareMidiFileForSlicing(midiFile, grid, internalSettings);

            var result = new MidiFile
            {
                TimeDivision = midiFile.TimeDivision
            };

            using (var slicer = MidiFileSlicer.CreateFromFile(midiFile))
            {
                var startPart = slicer.GetNextSlice(times[0], internalSettings);
                slicer.GetNextSlice(times[1], internalSettings);
                var endPart = slicer.GetNextSlice(Math.Max(times.Last(), midiFile.GetDuration<MidiTimeSpan>()) + 1, internalSettings);

                if (internalSettings.PreserveTimes)
                {
                    var partLengthInTicks = times[1] - times[0];
                    endPart.ProcessTimedEvents(e => e.Time -= partLengthInTicks);
                }

                using (var startPartTrackChunksEnumerator = startPart.GetTrackChunks().GetEnumerator())
                using (var endPartTrackChunksEnumerator = endPart.GetTrackChunks().GetEnumerator())
                using (var notesToSplitDescriptorsEnumerator = notesToSplitDescriptors.GetEnumerator())
                {
                    while (startPartTrackChunksEnumerator.MoveNext() &&
                           endPartTrackChunksEnumerator.MoveNext() &&
                           notesToSplitDescriptorsEnumerator.MoveNext())
                    {
                        var newTrackChunk = new TrackChunk();
                        var eventsCollection = newTrackChunk.Events;
                        eventsCollection.AddRange(startPartTrackChunksEnumerator.Current.Events);
                        eventsCollection.AddRange(endPartTrackChunksEnumerator.Current.Events);

                        eventsCollection.RemoveTimedEvents(e =>
                        {
                            var markerEvent = e.Event as MarkerEvent;
                            if (markerEvent == null)
                                return false;

                            return markerEvent.Text == partsStartId || markerEvent.Text == partEndId;
                        });

                        if (settings.SplitNotes && notesToSplitDescriptorsEnumerator.Current.Any())
                        {
                            var timedEvents = eventsCollection
                                .GetTimedEventsLazy(null, false)
                                .SkipWhile(e => e.Time < times[0])
                                .TakeWhile(e => e.Time == times[0])
                                .ToList();

                            var eventsToRemove = new List<MidiEvent>();

                            foreach (var notesDescriptor in notesToSplitDescriptorsEnumerator.Current)
                            {
                                var timedEventsToRemove = timedEvents
                                    .Where(e =>
                                    {
                                        var noteEvent = e.Event as NoteEvent;
                                        if (noteEvent == null)
                                            return false;

                                        if (!new NoteId(noteEvent.Channel, noteEvent.NoteNumber).Equals(notesDescriptor.Item1))
                                            return false;

                                        var noteOnEvent = noteEvent as NoteOnEvent;
                                        if (noteOnEvent != null)
                                            return noteOnEvent.Velocity == notesDescriptor.Item2;

                                        return ((NoteOffEvent)noteEvent).Velocity == notesDescriptor.Item3;
                                    })
                                    .ToArray();

                                foreach (var timedEvent in timedEventsToRemove)
                                {
                                    timedEvents.Remove(timedEvent);
                                    eventsToRemove.Add(timedEvent.Event);
                                }
                            }

                            eventsCollection.RemoveTimedEvents(e => eventsToRemove.Contains(e.Event));
                        }

                        if (!settings.PreserveTrackChunks && !eventsCollection.Any())
                            continue;

                        result.Chunks.Add(newTrackChunk);
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
