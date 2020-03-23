using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides a way to iterate through a collection of <see cref="TimedEvent"/> returning
    /// <see cref="Note"/> for Note On/Note Off event pairs and original <see cref="TimedEvent"/>
    /// for all other events.
    /// </summary>
    public static class GetTimedEventsAndNotesUtilities
    {
        #region Nested classes

        internal sealed class NoteEventsDescriptor
        {
            #region Constructor

            public NoteEventsDescriptor(TimedEvent noteOnTimedEvent, IEnumerable<TimedEvent> eventsTail)
            {
                NoteOnTimedEvent = noteOnTimedEvent;
                EventsTail = eventsTail;
            }

            #endregion

            #region Properties

            public TimedEvent NoteOnTimedEvent { get; }

            public TimedEvent NoteOffTimedEvent { get; private set; }

            public IEnumerable<TimedEvent> EventsTail { get; }

            public bool IsNoteCompleted { get; private set; }

            #endregion

            #region Methods

            public void CompleteNote(TimedEvent noteOffTimedEvent)
            {
                NoteOffTimedEvent = noteOffTimedEvent;
                IsNoteCompleted = true;
            }

            public bool IsCorrespondingNoteOffEvent(NoteOffEvent noteOffEvent)
            {
                return NoteEventUtilities.IsNoteOnCorrespondToNoteOff((NoteOnEvent)NoteOnTimedEvent.Event,
                                                                      noteOffEvent) &&
                       !IsNoteCompleted;
            }

            public IEnumerable<ITimedObject> GetTimedObjects()
            {
                if (IsNoteCompleted)
                    yield return new Note(NoteOnTimedEvent, NoteOffTimedEvent);
                else
                    yield return NoteOnTimedEvent;

                foreach (var eventFromTail in EventsTail)
                {
                    yield return eventFromTail;
                }
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// Iterates through the specified collection of <see cref="TimedEvent"/> returning
        /// <see cref="Note"/> for Note On/Note Off event pairs and original <see cref="TimedEvent"/>
        /// for all other events.
        /// </summary>
        /// <remarks>
        /// If there is no corresponding Note Off event for Note On (or if there is no correspinding
        /// Note On event for Note Off) the event will be returned as is.
        /// </remarks>
        /// <param name="timedEvents">Collection of <see cref="TimedEvent"/> to iterate over.</param>
        /// <returns>Collection of <see cref="ITimedObject"/> where an element either <see cref="TimedEvent"/>
        /// or <see cref="Note"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timedEvents"/> is <c>null</c>.</exception>
        public static IEnumerable<ITimedObject> GetTimedEventsAndNotes(this IEnumerable<TimedEvent> timedEvents)
        {
            ThrowIfArgument.IsNull(nameof(timedEvents), timedEvents);

            var noteEventsDescriptors = new List<NoteEventsDescriptor>();
            var eventsTail = new ObjectWrapper<List<TimedEvent>>();

            foreach (var timedEvent in timedEvents)
            {
                foreach (var timedObject in GetTimedEventsAndNotes(timedEvent, noteEventsDescriptors, eventsTail))
                {
                    yield return timedObject;
                }
            }

            foreach (var timedObject in noteEventsDescriptors.SelectMany(d => d.GetTimedObjects()))
            {
                yield return timedObject;
            }
        }

        /// <summary>
        /// Iterates through the events contained in the specified <see cref="TrackChunk"/> returning
        /// <see cref="Note"/> for Note On/Note Off event pairs and original <see cref="TimedEvent"/>
        /// for all other events.
        /// </summary>
        /// <remarks>
        /// If there is no corresponding Note Off event for Note On (or if there is no correspinding
        /// Note On event for Note Off) the event will be returned as is.
        /// </remarks>
        /// <param name="trackChunk"><see cref="TrackChunk"/> containing events to iterate over.</param>
        /// <returns>Collection of <see cref="ITimedObject"/> where an element either <see cref="TimedEvent"/>
        /// or <see cref="Note"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static IEnumerable<ITimedObject> GetTimedEventsAndNotes(this TrackChunk trackChunk)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.GetTimedEvents().GetTimedEventsAndNotes();
        }

        /// <summary>
        /// Iterates through the events contained in the specified collection of <see cref="TrackChunk"/> returning
        /// <see cref="Note"/> for Note On/Note Off event pairs and original <see cref="TimedEvent"/>
        /// for all other events.
        /// </summary>
        /// <remarks>
        /// If there is no corresponding Note Off event for Note On (or if there is no correspinding
        /// Note On event for Note Off) the event will be returned as is.
        /// </remarks>
        /// <param name="trackChunks"><see cref="TrackChunk"/> containing events to iterate over.</param>
        /// <returns>Collection of <see cref="ITimedObject"/> where an element either <see cref="TimedEvent"/>
        /// or <see cref="Note"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        public static IEnumerable<ITimedObject> GetTimedEventsAndNotes(this IEnumerable<TrackChunk> trackChunks)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return trackChunks.GetTimedEvents().GetTimedEventsAndNotes();
        }

        /// <summary>
        /// Iterates through the events contained in the specified <see cref="MidiFile"/> returning
        /// <see cref="Note"/> for Note On/Note Off event pairs and original <see cref="TimedEvent"/>
        /// for all other events.
        /// </summary>
        /// <remarks>
        /// If there is no corresponding Note Off event for Note On (or if there is no correspinding
        /// Note On event for Note Off) the event will be returned as is.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> containing events to iterate over.</param>
        /// <returns>Collection of <see cref="ITimedObject"/> where an element either <see cref="TimedEvent"/>
        /// or <see cref="Note"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static IEnumerable<ITimedObject> GetTimedEventsAndNotes(this MidiFile midiFile)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            return midiFile.GetTimedEvents().GetTimedEventsAndNotes();
        }

        internal static IEnumerable<ITimedObject> GetTimedEventsAndNotes(TimedEvent timedEvent, List<NoteEventsDescriptor>  noteEventsDescriptors, ObjectWrapper<List<TimedEvent>> eventsTail)
        {
            var midiEvent = timedEvent?.Event;

            var noteOnEvent = midiEvent as NoteOnEvent;
            if (noteOnEvent != null)
            {
                noteEventsDescriptors.Add(new NoteEventsDescriptor(timedEvent, eventsTail.Object = new List<TimedEvent>()));
                yield break;
            }

            var noteOffEvent = midiEvent as NoteOffEvent;
            if (noteOffEvent != null)
            {
                var noteEventsDescriptor = noteEventsDescriptors.FirstOrDefault(d => d.IsCorrespondingNoteOffEvent(noteOffEvent));
                if (noteEventsDescriptor != null)
                {
                    noteEventsDescriptor.CompleteNote(timedEvent);
                    if (noteEventsDescriptors.First() != noteEventsDescriptor)
                        yield break;

                    for (int i = 0; i < noteEventsDescriptors.Count; i++)
                    {
                        var descriptor = noteEventsDescriptors[i];
                        if (!descriptor.IsNoteCompleted)
                            break;

                        foreach (var timedObject in descriptor.GetTimedObjects())
                        {
                            yield return timedObject;
                        }

                        noteEventsDescriptors.RemoveAt(i);
                        i--;
                    }

                    if (!noteEventsDescriptors.Any())
                        eventsTail.Object = null;

                    yield break;
                }
            }

            if (eventsTail.Object != null)
                eventsTail.Object.Add(timedEvent);
            else
                yield return timedEvent;
        }

        #endregion
    }
}
