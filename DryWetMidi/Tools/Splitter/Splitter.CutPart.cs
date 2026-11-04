using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
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
        /// <exception cref="InvalidOperationException">The calculated end of the part is
        /// equal to or less than its start.</exception>
        public static MidiFile CutPart(this MidiFile midiFile, ITimeSpan partStart, ITimeSpan partLength, SliceMidiFileSettings? settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(partStart), partStart);
            ThrowIfArgument.IsNull(nameof(partLength), partLength);

            var grid = new ArbitraryGrid(partStart, partStart.Add(partLength, TimeSpanMode.TimeLength));

            var partStartId = Guid.NewGuid().ToString();
            var partEndId = Guid.NewGuid().ToString();

            settings = settings ?? new SliceMidiFileSettings();

            var internalSettings = new SliceMidiFileSettings
            {
                PreserveTrackChunks = true,
                PreserveTimes = settings.PreserveTimes,
                SplitNotes = settings.SplitNotes,
                Markers = new SliceMidiFileMarkers
                {
                    PartStartMarkerEventFactory = () => new MarkerEvent(partStartId),
                    PartEndMarkerEventFactory = () => new MarkerEvent(partEndId)
                },
                NoteDetectionSettings = settings.NoteDetectionSettings
            };

            var tempoMap = midiFile.GetTempoMap();
            var times = GetValidatedGridTimes(grid, tempoMap).ToArray();

            // A note spanning the whole cut needs no synthetic events at the splice.
            midiFile = PrepareMidiFileForSlicing(
                midiFile,
                grid,
                internalSettings,
                obj => !(obj is Note note && note.Time < times[0] && note.EndTime > times[1]));

            var result = new MidiFile
            {
                TimeDivision = midiFile.TimeDivision
            };

            using (var slicer = MidiFileSlicer.CreateFromFile(midiFile))
            {
                var startPart = slicer.GetNextSlice(times[0], internalSettings);
                slicer.GetNextSlice(times[1], internalSettings);
                var endPart = slicer.GetNextSlice(long.MaxValue, internalSettings, times[0]);

                if (internalSettings.PreserveTimes)
                {
                    var partLengthInTicks = times[1] - times[0];
                    endPart.ProcessTimedEvents(e => e.Time -= partLengthInTicks);
                }

                using (var startPartTrackChunksEnumerator = startPart.GetTrackChunks().GetEnumerator())
                using (var endPartTrackChunksEnumerator = endPart.GetTrackChunks().GetEnumerator())
                {
                    while (startPartTrackChunksEnumerator.MoveNext() &&
                           endPartTrackChunksEnumerator.MoveNext())
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

                            return markerEvent.Text == partStartId || markerEvent.Text == partEndId;
                        });

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
