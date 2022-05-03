using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    // TODO: use NoteDetectionSettings
    public static partial class Splitter
    {
        #region Methods

        /// <summary>
        /// Splits <see cref="MidiFile"/> by notes.
        /// </summary>
        /// <remarks>
        /// The method produces new files where each one contains Note On and Note Off events for single
        /// note number and channel (if it's not ignored according to <see cref="SplitFileByNotesSettings.IgnoreChannel"/>
        /// of <paramref name="settings"/>). Also files can contain all non-note events as defined by
        /// <see cref="SplitFileByNotesSettings.CopyNonNoteEventsToEachFile"/> of <paramref name="settings"/>. If
        /// an input file doesn't contain note events, result file will be just a copy of the input one.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split.</param>
        /// <param name="settings">Settings accoridng to which notes should be detected and built.</param>
        /// <returns>Collection of <see cref="MidiFile"/> where each file contains events for single note and
        /// other events as defined by <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <example>
        /// <para>
        /// For example, a MIDI file contains following notes:
        /// </para>
        /// <code language="image">
        /// +---------------------+
        /// |┌───────────────────┐|
        /// |│. [ A ]  [     B ]  . .│
        /// |└───────────────────┘|
        /// |┌───────────────────┐|
        /// |│ .  [ B ]  [    A]   . │
        /// |└───────────────────┘|
        /// +---------------------+
        /// </code>
        /// <para>
        /// where <c>A</c> and <c>B</c> mean different notes; <c>.</c> is any non-note event.
        /// </para>
        /// <para>
        /// After split we'll get following two new files:
        /// </para>
        /// <code language="image">
        /// +---------------------+
        /// |┌───────────────────┐|
        /// |│..[ A ]    [    A]  ...│
        /// |└───────────────────┘|
        /// +---------------------+
        /// +---------------------+
        /// |┌───────────────────┐|
        /// |│..  [ B ][     B ]  ...│
        /// |└───────────────────┘|
        /// +---------------------+
        /// </code>
        /// <para>
        /// So each new file contains one track chunk with notes of the same note number and channel, and
        /// also all non-note events. New files will have the same time division (<see cref="MidiFile.TimeDivision"/>)
        /// as the original one.
        /// </para>
        /// </example>
        public static IEnumerable<MidiFile> SplitByNotes(this MidiFile midiFile, SplitFileByNotesSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            settings = settings ?? new SplitFileByNotesSettings();

            return settings.IgnoreChannel
                ? midiFile.SplitByNotes(noteEvent => noteEvent.NoteNumber, settings.Filter, settings.CopyNonNoteEventsToEachFile)
                : midiFile.SplitByNotes(noteEvent => noteEvent.GetNoteId(), settings.Filter, settings.CopyNonNoteEventsToEachFile);
        }

        private static IEnumerable<MidiFile> SplitByNotes<TNoteId>(
            this MidiFile midiFile,
            Func<NoteEvent, TNoteId> getNoteId,
            Predicate<TimedEvent> filter,
            bool copyNonNoteEventsToEachFile)
        {
            var timedEventsByIds = new Dictionary<TNoteId, List<TimedEvent>>();
            var nonNoteEvents = new List<TimedEvent>();

            var timedEvents = midiFile.GetTrackChunks().GetTimedEventsLazy(null);
            if (filter != null)
                timedEvents = timedEvents.Where(e => filter(e.Item1));

            foreach (var timedEventTuple in timedEvents)
            {
                var timedEvent = timedEventTuple.Item1;

                var noteEvent = timedEvent.Event as NoteEvent;
                if (noteEvent != null)
                {
                    var noteId = getNoteId(noteEvent);

                    List<TimedEvent> timedEventsById;
                    if (!timedEventsByIds.TryGetValue(noteId, out timedEventsById))
                    {
                        timedEventsByIds.Add(noteId, timedEventsById = new List<TimedEvent>());

                        if (copyNonNoteEventsToEachFile)
                            timedEventsById.AddRange(nonNoteEvents);
                    }

                    timedEventsById.Add(timedEvent);
                }
                else if (copyNonNoteEventsToEachFile)
                {
                    foreach (var timedEventsById in timedEventsByIds)
                    {
                        timedEventsById.Value.Add(timedEvent);
                    }

                    nonNoteEvents.Add(timedEvent);
                }
            }

            if (!timedEventsByIds.Any())
            {
                var midiFileClone = midiFile.Clone();
                if (filter != null)
                    midiFileClone.RemoveTimedEvents(e => !filter(e));

                yield return midiFileClone;
                yield break;
            }

            foreach (var timedEventsById in timedEventsByIds)
            {
                var newFile = timedEventsById.Value.ToFile();
                newFile.TimeDivision = midiFile.TimeDivision.Clone();

                yield return newFile;
            }
        }

        #endregion
    }
}
