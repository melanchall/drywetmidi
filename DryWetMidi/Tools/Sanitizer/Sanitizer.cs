using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides a way to sanitize a MIDI file applying different options. More info in the
    /// <see href="xref:a_sanitizer">Sanitizer</see> article.
    /// </summary>
    public static class Sanitizer
    {
        #region Constants

        private static readonly ControlName[] ControlsToSkip = new[]
        {
            ControlName.NonRegisteredParameterNumberLsb,
            ControlName.NonRegisteredParameterNumberMsb,
            ControlName.RegisteredParameterNumberLsb,
            ControlName.RegisteredParameterNumberMsb,
            ControlName.DataEntryMsb,
            ControlName.LsbForDataEntry,
        };

        #endregion

        #region Methods

        /// <summary>
        /// Sanitizes a MIDI file according to the specified settings. Note that the input file will be
        /// transformed instead of returning a new one so be sure you've cloned it if needed.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to process.</param>
        /// <param name="settings">Settings which control how the <paramref name="midiFile"/>
        /// should be sanitized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException"><see cref="SanitizingSettings.NoteMaxLengthForOrphanedNoteOnEvent"/>
        /// is <c>null</c> while <see cref="SanitizingSettings.OrphanedNoteOnEventsPolicy"/> is set to
        /// <see cref="OrphanedNoteOnEventsPolicy.CompleteNote"/>.</exception>
        public static void Sanitize(this MidiFile midiFile, SanitizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            settings = settings ?? new SanitizingSettings();

            CompleteOrphanedNotes(midiFile, settings);
            RemoveNoteData(midiFile, settings);
            var usedChannels = RemoveDuplicatedEventsCollectingUsedChannels(midiFile, settings);
            RemoveEventsOnUnusedChannels(midiFile, settings, usedChannels);
            RemoveEmptyTrackChunks(midiFile, settings);
            TrimFile(midiFile, settings);
        }

        private static void CompleteOrphanedNotes(
            MidiFile midiFile,
            SanitizingSettings settings)
        {
            if (settings.OrphanedNoteOnEventsPolicy != OrphanedNoteOnEventsPolicy.CompleteNote)
                return;

            var maxNoteDuration = settings.NoteMaxLengthForOrphanedNoteOnEvent;
            if (maxNoteDuration == null)
                throw new InvalidOperationException($"{nameof(SanitizingSettings.NoteMaxLengthForOrphanedNoteOnEvent)} must be set to complete notes from orphaned Note On events.");

            var trackChunks = midiFile.GetTrackChunks().ToArray();

            var noteOnLastObjects = new Dictionary<NoteId, TimedObjectAt<ITimedObject>>();
            var data = new List<Tuple<TimedObjectAt<ITimedObject>, long?>>();

            var notesAndTimedEvents = trackChunks
                .GetTimedEventsLazy(new TimedEventDetectionSettings(), false)
                .GetNotesAndTimedEventsLazy(settings.NoteDetectionSettings ?? new NoteDetectionSettings());

            foreach (var timedObjectAt in notesAndTimedEvents)
            {
                var obj = timedObjectAt.Object;

                var note = obj as Note;
                if (note != null)
                {
                    var noteId = note.GetNoteId();

                    TimedObjectAt<ITimedObject> noteOnObject;
                    if (noteOnLastObjects.TryGetValue(noteId, out noteOnObject))
                        data.Add(Tuple.Create(noteOnObject, (long?)obj.Time));

                    noteOnLastObjects.Remove(noteId);
                }
                else
                {
                    var noteOnEvent = ((TimedEvent)obj).Event as NoteOnEvent;
                    if (noteOnEvent == null)
                        continue;

                    var noteId = noteOnEvent.GetNoteId();

                    TimedObjectAt<ITimedObject> noteOnObject;
                    if (noteOnLastObjects.TryGetValue(noteId, out noteOnObject))
                        data.Add(Tuple.Create(noteOnObject, (long?)obj.Time));

                    noteOnLastObjects[noteId] = timedObjectAt;
                }
            }

            data.AddRange(noteOnLastObjects.Select(kv => Tuple.Create(kv.Value, (long?)null)));

            //

            var tempoMap = midiFile.GetTempoMap();

            var noteOffEvents = data
                .Select(d =>
                {
                    var noteOffTime = TimeConverter.ConvertFrom(
                        new MidiTimeSpan(d.Item1.Object.Time).Add(maxNoteDuration, TimeSpanMode.TimeLength),
                        tempoMap);
                    if (d.Item2 != null && noteOffTime > d.Item2)
                        noteOffTime = d.Item2.Value;

                    var noteOnEvent = (NoteOnEvent)((TimedEvent)d.Item1.Object).Event;
                    return new
                    {
                        TrackChunkIndex = d.Item1.AtIndex,
                        NoteOffTimedEvent = new TimedEvent(
                            new NoteOffEvent(noteOnEvent.NoteNumber, SevenBitNumber.MinValue) { Channel = noteOnEvent.Channel },
                            noteOffTime)
                    };
                })
                .GroupBy(d => d.TrackChunkIndex)
                .ToArray();

            foreach (var noteOffEventsGroup in noteOffEvents)
            {
                var events = trackChunks[noteOffEventsGroup.Key].Events;
                var lastTime = 0L;
                var noteOffEventsProcessed = false;
                
                var noteOffEventsEnumerator = noteOffEventsGroup.OrderBy(e => e.NoteOffTimedEvent.Time).GetEnumerator();
                noteOffEventsEnumerator.MoveNext();

                for (var i = 0; i < events.Count; i++)
                {
                    var noteOffTimedEvent = noteOffEventsEnumerator.Current.NoteOffTimedEvent;

                    if (lastTime + events[i].DeltaTime >= noteOffTimedEvent.Time)
                    {
                        var noteOffEvent = noteOffTimedEvent.Event;
                        noteOffEvent.DeltaTime = noteOffTimedEvent.Time - lastTime;

                        events[i].DeltaTime = events[i].DeltaTime - noteOffEvent.DeltaTime;
                        events.Insert(i, noteOffEvent);

                        if (!noteOffEventsEnumerator.MoveNext())
                        {
                            noteOffEventsProcessed = true;
                            break;
                        }
                    }

                    lastTime += events[i].DeltaTime;
                }

                if (noteOffEventsProcessed)
                    continue;

                do
                {
                    var noteOffTimedEvent = noteOffEventsEnumerator.Current.NoteOffTimedEvent;
                    var noteOffEvent = noteOffTimedEvent.Event;
                    noteOffEvent.DeltaTime = noteOffTimedEvent.Time - lastTime;

                    events.Add(noteOffEvent);
                    lastTime = noteOffTimedEvent.Time;
                }
                while (noteOffEventsEnumerator.MoveNext());

                noteOffEventsEnumerator.Dispose();
            }
        }

        private static void TrimFile(
            MidiFile midiFile,
            SanitizingSettings settings)
        {
            if (!settings.Trim)
                return;

            var nonEmptyTrackChunks = midiFile
                .GetTrackChunks()
                .Where(c => c.Events.Any())
                .ToArray();

            if (!nonEmptyTrackChunks.Any())
                return;

            var minStartTime = nonEmptyTrackChunks
                .Min(c => c.Events.First().DeltaTime);

            foreach (var trackChunk in nonEmptyTrackChunks)
            {
                trackChunk.Events.First().DeltaTime -= minStartTime;
            }
        }

        private static void RemoveEmptyTrackChunks(
            MidiFile midiFile,
            SanitizingSettings settings)
        {
            if (!settings.RemoveEmptyTrackChunks)
                return;
            
            midiFile.Chunks.RemoveAll(c => (c as TrackChunk)?.Events.Any() == false);
        }

        private static void RemoveEventsOnUnusedChannels(
            MidiFile midiFile,
            SanitizingSettings settings,
            bool[] usedChannels)
        {
            if (!settings.RemoveEventsOnUnusedChannels)
                return;

            midiFile.RemoveTimedEvents(e =>
            {
                var midiEvent = e.Event;

                var noteEvent = midiEvent as NoteEvent;
                if (noteEvent != null)
                    return false;

                var channelEvent = midiEvent as ChannelEvent;
                if (channelEvent != null)
                    return !usedChannels[channelEvent.Channel];

                return false;
            });
        }

        private static bool[] RemoveDuplicatedEventsCollectingUsedChannels(
            MidiFile midiFile,
            SanitizingSettings settings)
        {
            var usedChannels = new bool[FourBitNumber.MaxValue + 1];

            if (!settings.RemoveEventsOnUnusedChannels &&
                !settings.RemoveDuplicatedPitchBendEvents &&
                !settings.RemoveDuplicatedSequenceTrackNameEvents &&
                !settings.RemoveDuplicatedSetTempoEvents &&
                !settings.RemoveDuplicatedTimeSignatureEvents &&
                !settings.RemoveDuplicatedControlChangeEvents)
                return usedChannels;

            var microsecondsPerQuarterNote = SetTempoEvent.DefaultMicrosecondsPerQuarterNote;

            var timeSignatureNumerator = TimeSignatureEvent.DefaultNumerator;
            var timeSignatureDenominator = TimeSignatureEvent.DefaultDenominator;
            var timeSignatureThirtySecondNotesPerBeat = TimeSignatureEvent.DefaultThirtySecondNotesPerBeat;
            var timeSignatureClocksPerClick = TimeSignatureEvent.DefaultClocksPerClick;

            ushort? pitchValue = null;
            FourBitNumber? pitchBendChannel = null;

            var controlValues = new SevenBitNumber?[FourBitNumber.MaxValue + 1, SevenBitNumber.MaxValue + 1];

            midiFile.RemoveTimedEvents(e =>
            {
                var midiEvent = e.Event;
                var midiEventType = midiEvent.EventType;

                var noteEvent = midiEvent as NoteEvent;
                if (noteEvent != null)
                    usedChannels[noteEvent.Channel] = true;

                if (midiEventType == MidiEventType.SetTempo && settings.RemoveDuplicatedSetTempoEvents)
                {
                    var setTempoEvent = (SetTempoEvent)midiEvent;
                    var result = setTempoEvent.MicrosecondsPerQuarterNote == microsecondsPerQuarterNote;

                    microsecondsPerQuarterNote = setTempoEvent.MicrosecondsPerQuarterNote;
                    return result;
                }

                if (midiEventType == MidiEventType.TimeSignature && settings.RemoveDuplicatedTimeSignatureEvents)
                {
                    var timeSignatureEvent = (TimeSignatureEvent)midiEvent;
                    var result = timeSignatureEvent.Numerator == timeSignatureNumerator &&
                                 timeSignatureEvent.Denominator == timeSignatureDenominator &&
                                 timeSignatureEvent.ThirtySecondNotesPerBeat == timeSignatureThirtySecondNotesPerBeat &&
                                 timeSignatureEvent.ClocksPerClick == timeSignatureClocksPerClick;

                    timeSignatureNumerator = timeSignatureEvent.Numerator;
                    timeSignatureDenominator = timeSignatureEvent.Denominator;
                    timeSignatureThirtySecondNotesPerBeat = timeSignatureEvent.ThirtySecondNotesPerBeat;
                    timeSignatureClocksPerClick = timeSignatureEvent.ClocksPerClick;
                    return result;
                }

                if (midiEventType == MidiEventType.PitchBend && settings.RemoveDuplicatedPitchBendEvents)
                {
                    var pitchBendEvent = (PitchBendEvent)midiEvent;
                    var result = pitchBendEvent.PitchValue == pitchValue &&
                                 pitchBendEvent.Channel == pitchBendChannel;

                    pitchValue = pitchBendEvent.PitchValue;
                    pitchBendChannel = pitchBendEvent.Channel;
                    return result;
                }

                if (midiEventType == MidiEventType.ControlChange && settings.RemoveDuplicatedControlChangeEvents)
                {
                    var controlChangeEvent = (ControlChangeEvent)midiEvent;
                    if (ControlsToSkip.Contains(controlChangeEvent.GetControlName()))
                    {
                        return false;
                    }

                    var result = controlChangeEvent.ControlValue == controlValues[controlChangeEvent.Channel, controlChangeEvent.ControlNumber];

                    controlValues[controlChangeEvent.Channel, controlChangeEvent.ControlNumber] = controlChangeEvent.ControlValue;
                    return result;
                }

                return false;
            });

            if (settings.RemoveDuplicatedSequenceTrackNameEvents)
            {
                foreach (var trackChunk in midiFile.GetTrackChunks())
                {
                    string sequenceTrackName = null;

                    trackChunk.RemoveTimedEvents(e =>
                    {
                        var midiEvent = e.Event;
                        var midiEventType = midiEvent.EventType;

                        if (midiEventType == MidiEventType.SequenceTrackName && settings.RemoveDuplicatedSequenceTrackNameEvents)
                        {
                            var sequenceTrackNameEvent = (SequenceTrackNameEvent)midiEvent;
                            var result = sequenceTrackNameEvent.Text == sequenceTrackName;

                            sequenceTrackName = sequenceTrackNameEvent.Text;
                            return result;
                        }

                        return false;
                    });
                }
            }

            return usedChannels;
        }

        private static void RemoveNoteData(
            MidiFile midiFile,
            SanitizingSettings settings)
        {
            var noteMinLength = settings.NoteMinLength;
            var removeShortNotes = noteMinLength != null && !noteMinLength.IsZeroTimeSpan();

            var noteMinVelocity = settings.NoteMinVelocity;
            var removeSilentNotes = noteMinVelocity > 0;

            if (!removeShortNotes &&
                !removeSilentNotes &&
                !settings.RemoveDuplicatedNotes &&
                settings.OrphanedNoteOnEventsPolicy == OrphanedNoteOnEventsPolicy.Ignore &&
                !settings.RemoveOrphanedNoteOffEvents)
                return;

            var tempoMap = midiFile.GetTempoMap();
            var timeSpanType = noteMinLength?.GetType();

            var lastNotes = new Note[FourBitNumber.MaxValue + 1, SevenBitNumber.MaxValue + 1];

            midiFile.RemoveObjects(
                ObjectType.TimedEvent | ObjectType.Note,
                obj =>
                {
                    var note = obj as Note;
                    if (note != null)
                    {
                        var lastNote = lastNotes[note.Channel, note.NoteNumber];
                        lastNotes[note.Channel, note.NoteNumber] = note;

                        if (settings.RemoveDuplicatedNotes &&
                            lastNote?.Time == note.Time &&
                            lastNote?.Length == note.Length)
                            return true;

                        if (removeShortNotes &&
                            LengthConverter.ConvertTo((MidiTimeSpan)note.Length, timeSpanType, note.Time, tempoMap).CompareTo(noteMinLength) < 0)
                            return true;

                        if (removeSilentNotes &&
                            note.Velocity < noteMinVelocity)
                            return true;
                    }
                    else
                    {
                        var timedEvent = (TimedEvent)obj;

                        if (timedEvent.Event.EventType == MidiEventType.NoteOn)
                        {
                            switch (settings.OrphanedNoteOnEventsPolicy)
                            {
                                case OrphanedNoteOnEventsPolicy.Remove:
                                    return true;
                                case OrphanedNoteOnEventsPolicy.CompleteNote:
                                    return false;
                            }
                        }

                        if (settings.RemoveOrphanedNoteOffEvents && timedEvent.Event.EventType == MidiEventType.NoteOff)
                            return true;
                    }

                    return false;
                },
                new ObjectDetectionSettings { NoteDetectionSettings = settings.NoteDetectionSettings });
        }

        #endregion
    }
}
