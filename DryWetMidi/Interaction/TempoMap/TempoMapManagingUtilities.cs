using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Extension methods for managing tempo map. More info in the <see href="xref:a_tempo_map">Tempo map</see> article.
    /// </summary>
    public static class TempoMapManagingUtilities
    {
        #region Methods

        /// <summary>
        /// Creates an instance of the <see cref="TempoMapManager"/> initializing it with the
        /// specified events collections and time division. More info in the <see href="xref:a_tempo_map">Tempo map</see> article.
        /// </summary>
        /// <param name="eventsCollections">Collection of <see cref="EventsCollection"/> which hold events
        /// that represent tempo map of a MIDI file.</param>
        /// <param name="timeDivision">MIDI file time division which specifies the meaning of the time
        /// used by events of the file.</param>
        /// <returns>An instance of the <see cref="TempoMapManager"/> that can be used to manage
        /// tempo map represented by the <paramref name="eventsCollections"/> and <paramref name="timeDivision"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollections"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeDivision"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static TempoMapManager ManageTempoMap(this IEnumerable<EventsCollection> eventsCollections, TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollections), eventsCollections);
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            return new TempoMapManager(timeDivision, eventsCollections);
        }

        /// <summary>
        /// Creates an instance of the <see cref="TempoMapManager"/> initializing it with the
        /// specified time division and events collections of the specified track chunks. More info in the
        /// <see href="xref:a_tempo_map">Tempo map</see> article.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> which hold events
        /// that represent tempo map of a MIDI file.</param>
        /// <param name="timeDivision">MIDI file time division which specifies the meaning of the time
        /// used by events of the file.</param>
        /// <returns>An instance of the <see cref="TempoMapManager"/> that can be used to manage
        /// tempo map represented by the <paramref name="trackChunks"/> and <paramref name="timeDivision"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeDivision"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static TempoMapManager ManageTempoMap(this IEnumerable<TrackChunk> trackChunks, TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            return trackChunks.Select(c => c.Events).ManageTempoMap(timeDivision);
        }

        /// <summary>
        /// Creates an instance of the <see cref="TempoMapManager"/> initializing it with the
        /// events collections of the specified MIDI file. More info in the <see href="xref:a_tempo_map">Tempo map</see> article.
        /// </summary>
        /// <param name="file">MIDI file to manage tempo map of.</param>
        /// <returns>An instance of the <see cref="TempoMapManager"/> that can be used to manage
        /// tempo map of the <paramref name="file"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        public static TempoMapManager ManageTempoMap(this MidiFile file)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.GetTrackChunks().ManageTempoMap(file.TimeDivision);
        }

        /// <summary>
        /// Gets tempo map represented by the specified time division and events collections of
        /// the specified track chunks.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> which hold events
        /// that represent tempo map of a MIDI file.</param>
        /// <param name="timeDivision">MIDI file time division which specifies the meaning of the time
        /// used by events of the file.</param>
        /// <returns>Tempo map represented by the <paramref name="trackChunks"/> and <paramref name="timeDivision"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeDivision"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static TempoMap GetTempoMap(this IEnumerable<TrackChunk> trackChunks, TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            //

            var result = new TempoMap(timeDivision);

            var eventsCollections = trackChunks.Select(c => c.Events).Where(e => e.HasTempoMapEvents).ToArray();
            var eventsCollectionsCount = eventsCollections.Length;
            if (eventsCollectionsCount == 0)
                return result;

            var eventsCollectionIndices = new int[eventsCollectionsCount];
            var eventsCollectionMaxIndices = eventsCollections.Select(e => e.IsInitialState ? e.LastTempoMapEventIndex : e.Count - 1).ToArray();
            var eventsCollectionTimes = new long[eventsCollectionsCount];

            while (true)
            {
                var eventsCollectionIndex = 0;
                var minTime = long.MaxValue;
                var endReached = true;

                for (var j = 0; j < eventsCollectionsCount; j++)
                {
                    var index = eventsCollectionIndices[j];
                    if (index > eventsCollectionMaxIndices[j])
                        continue;

                    var eventTime = eventsCollections[j][index].DeltaTime + eventsCollectionTimes[j];
                    if (eventTime < minTime)
                    {
                        minTime = eventTime;
                        eventsCollectionIndex = j;
                    }

                    endReached = false;
                }

                if (endReached)
                    break;

                var midiEvent = eventsCollections[eventsCollectionIndex][eventsCollectionIndices[eventsCollectionIndex]];

                switch (midiEvent.EventType)
                {
                    case MidiEventType.TimeSignature:
                        {
                            var timeSignatureEvent = (TimeSignatureEvent)midiEvent;
                            result.TimeSignatureLine.SetValue(minTime, new TimeSignature(
                                timeSignatureEvent.Numerator,
                                timeSignatureEvent.Denominator));
                        }
                        break;
                    case MidiEventType.SetTempo:
                        {
                            var setTempoEvent = (SetTempoEvent)midiEvent;
                            result.TempoLine.SetValue(minTime, new Tempo(
                                setTempoEvent.MicrosecondsPerQuarterNote));
                        }
                        break;
                }

                eventsCollectionTimes[eventsCollectionIndex] = minTime;
                eventsCollectionIndices[eventsCollectionIndex]++;
            }

            return result;
        }

        /// <summary>
        /// Gets tempo map of the specified MIDI file.
        /// </summary>
        /// <param name="file">MIDI file to get tempo map of.</param>
        /// <returns>Tempo map of the <paramref name="file"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        public static TempoMap GetTempoMap(this MidiFile file)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.GetTrackChunks().GetTempoMap(file.TimeDivision);
        }

        /// <summary>
        /// Replaces tempo map contained in the specified collection of the <see cref="EventsCollection"/> with
        /// another one.
        /// </summary>
        /// <param name="eventsCollections">Collection of the <see cref="EventsCollection"/> holding a tempo map to replace.</param>
        /// <param name="tempoMap">Tempo map to replace the one contained in the <paramref name="eventsCollections"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollections"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void ReplaceTempoMap(this IEnumerable<EventsCollection> eventsCollections, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollections), eventsCollections);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!eventsCollections.Any())
                return;

            using (var tempoMapManager = eventsCollections.ManageTempoMap(tempoMap.TimeDivision))
            {
                tempoMapManager.ReplaceTempoMap(tempoMap);
            }
        }


        /// <summary>
        /// Replaces tempo map contained in the specified collection of the <see cref="TrackChunk"/> with
        /// another one.
        /// </summary>
        /// <param name="trackChunks">Collection of the <see cref="TrackChunk"/> holding a tempo map to replace.</param>
        /// <param name="tempoMap">Tempo map to replace the one contained in the <paramref name="trackChunks"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void ReplaceTempoMap(this IEnumerable<TrackChunk> trackChunks, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunks.Select(c => c.Events).ReplaceTempoMap(tempoMap);
        }

        /// <summary>
        /// Replaces tempo map contained in the specified <see cref="MidiFile"/> with another one.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> holding a tempo map to replace.</param>
        /// <param name="tempoMap">Tempo map to replace the one contained in the <paramref name="file"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void ReplaceTempoMap(this MidiFile file, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!file.GetTrackChunks().Any() && (tempoMap.GetTempoChanges().Any() || tempoMap.GetTimeSignatureChanges().Any()))
                file.Chunks.Add(new TrackChunk());

            var trackChunks = file.GetTrackChunks();
            trackChunks.ReplaceTempoMap(tempoMap);

            file.TimeDivision = tempoMap.TimeDivision.Clone();
        }

        #endregion
    }
}
