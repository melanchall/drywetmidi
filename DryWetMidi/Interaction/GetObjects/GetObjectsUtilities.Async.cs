using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class GetObjectsUtilities
    {
        /// <summary>
        /// Extracts objects of the specified types lazily from an asynchronous collection of
        /// <see cref="MidiEvent"/>. Pending notes and chords are retained until they can be emitted
        /// in the same order as <see cref="EnumerateObjects"/>.
        /// </summary>
        /// <param name="midiEvents">Asynchronous collection of events. Null elements are ignored.</param>
        /// <param name="objectType">Combination of desired object types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <param name="cancellationToken">Token to cancel enumeration.</param>
        /// <returns>A lazy asynchronous collection of objects built on cloned source events.
        /// Disposing its enumerator disposes the source enumerator, but not the source itself.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvents"/> is <c>null</c>.</exception>
        /// <exception cref="OperationCanceledException">Enumeration was canceled.</exception>
        public static IAsyncEnumerable<ITimedObject> EnumerateObjectsAsync(
            this IAsyncEnumerable<MidiEvent?> midiEvents,
            ObjectType objectType,
            ObjectDetectionSettings? settings = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            return EnumerateObjectsAsyncCore(midiEvents, objectType, settings, cancellationToken);
        }

        private static async IAsyncEnumerable<ITimedObject> EnumerateObjectsAsyncCore(
            IAsyncEnumerable<MidiEvent?> midiEvents,
            ObjectType objectType,
            ObjectDetectionSettings? settings,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var getChords = objectType.HasFlag(ObjectType.Chord);
            var getNotes = objectType.HasFlag(ObjectType.Note);
            var getTimedEvents = objectType.HasFlag(ObjectType.TimedEvent);
            IAsyncEnumerable<ITimedObject> timedObjects = GetTimedEventsAsync(
                midiEvents,
                settings?.TimedEventDetectionSettings,
                cancellationToken);

            if (getChords || getNotes)
            {
                timedObjects = timedObjects.GetNotesAndTimedEventsAsync(
                    settings?.NoteDetectionSettings ?? new NoteDetectionSettings(),
                    cancellationToken);

                if (getChords)
                {
                    timedObjects = timedObjects.GetChordsAndNotesAndTimedEventsAsync(
                        settings?.ChordDetectionSettings ?? new ChordDetectionSettings(),
                        cancellationToken);
                }
            }

            await foreach (var timedObject in timedObjects.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ((getChords && timedObject is Chord) ||
                    (getNotes && timedObject is Note) ||
                    (getTimedEvents && timedObject is TimedEvent))
                {
                    yield return timedObject;
                }
                else if (getTimedEvents && timedObject is Note note)
                {
                    yield return note.GetTimedNoteOnEvent();

                    cancellationToken.ThrowIfCancellationRequested();
                    yield return note.GetTimedNoteOffEvent();
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private static async IAsyncEnumerable<ITimedObject> GetTimedEventsAsync(
            IAsyncEnumerable<MidiEvent?> midiEvents,
            TimedEventDetectionSettings? settings,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var constructor = settings?.Constructor;
            var time = 0L;
            var index = 0;

            cancellationToken.ThrowIfCancellationRequested();

            await foreach (var midiEvent in midiEvents.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (midiEvent == null)
                    continue;

                time += midiEvent.DeltaTime;

                var timedEvent = constructor?.Invoke(new TimedEventData(midiEvent.Clone(), time, 0, index));
                if (timedEvent == null)
                {
                    timedEvent = new TimedEvent(midiEvent.Clone());
                    timedEvent._time = time;
                }

                yield return timedEvent;

                index++;
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
