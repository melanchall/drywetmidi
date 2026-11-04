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
        private sealed class ChannelState
        {
            public ushort? PitchValue { get; set; }

            public SevenBitNumber?[] ControlValues { get; } = new SevenBitNumber?[SevenBitNumber.MaxValue + 1];
        }

        #region Constants

        private const byte DefaultPort = 0;

        private static readonly ControlName[] ControlsToSkip = new[]
        {
            ControlName.NonRegisteredParameterNumberLsb,
            ControlName.NonRegisteredParameterNumberMsb,
            ControlName.RegisteredParameterNumberLsb,
            ControlName.RegisteredParameterNumberMsb,
            ControlName.DataEntryMsb,
            ControlName.LsbForDataEntry,
            ControlName.DataIncrement,
            ControlName.DataDecrement,
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
        public static void Sanitize(this MidiFile midiFile, SanitizingSettings? settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            settings = settings ?? new SanitizingSettings();

            if (settings.OrphanedNoteOnEventsPolicy == OrphanedNoteOnEventsPolicy.CompleteNote &&
                settings.NoteMaxLengthForOrphanedNoteOnEvent == null)
                throw new InvalidOperationException($"{nameof(SanitizingSettings.NoteMaxLengthForOrphanedNoteOnEvent)} must be set to complete notes from orphaned Note On events.");

            if (midiFile._originalFormat == (ushort)MidiFileFormat.MultiSequence)
            {
                foreach (var trackChunk in midiFile.GetTrackChunks().ToArray())
                {
                    new MidiFile(trackChunk) { TimeDivision = midiFile.TimeDivision }.Sanitize(settings);
                }

                RemoveEmptyTrackChunks(midiFile, settings);
                return;
            }

            var eventPorts = GetEventPorts(midiFile);
            var originalNotesAndTimedEvents = settings.OrphanedNoteOnEventsPolicy == OrphanedNoteOnEventsPolicy.CompleteNote
                ? midiFile.GetTrackChunks()
                    .SelectMany(trackChunk => trackChunk.GetNotesAndTimedEventsByPort(settings, eventPorts))
                    .ToArray()
                : null;
            var completedNotes = CompleteOrphanedNotes(midiFile, settings);
            eventPorts = GetEventPorts(midiFile);

            RemoveNoteData(midiFile, settings, eventPorts, originalNotesAndTimedEvents, completedNotes);
            var usedChannels = RemoveDuplicatedEventsCollectingUsedChannels(midiFile, settings, eventPorts);
            RemoveEventsOnUnusedChannels(midiFile, settings, usedChannels, eventPorts);
            RemoveEmptyTrackChunks(midiFile, settings);
            TrimFile(midiFile, settings);
        }

        private static Dictionary<NoteOnEvent, Note> CompleteOrphanedNotes(
            MidiFile midiFile,
            SanitizingSettings settings)
        {
            var completedNotes = new Dictionary<NoteOnEvent, Note>();

            if (settings.OrphanedNoteOnEventsPolicy != OrphanedNoteOnEventsPolicy.CompleteNote)
                return completedNotes;

            var maxNoteDuration = settings.NoteMaxLengthForOrphanedNoteOnEvent;
            var trackChunks = midiFile.GetTrackChunks().ToArray();

            var eventPorts = GetEventPorts(midiFile);
            var noteOnLastObjects = new Dictionary<(byte Port, int NoteId), (ITimedObject Object, int TrackChunkIndex)>();
            var data = new List<((ITimedObject Object, int TrackChunkIndex), long? Time)>();

            // TODO: simplify
            var notesAndTimedEvents = trackChunks
                .SelectMany((trackChunk, i) => trackChunk
                    .GetNotesAndTimedEventsByPort(settings, eventPorts)
                    .Select(obj => (Object: obj, TrackChunkIndex: i)))
                .OrderBy(t => t.Object.Time);

            foreach (var timedObjectAt in notesAndTimedEvents)
            {
                var obj = timedObjectAt.Object;

                var note = obj as Note;
                if (note != null)
                {
                    var noteId = (eventPorts[note.TimedNoteOnEvent.Event], note.GetNoteId());

                    if (noteOnLastObjects.TryGetValue(noteId, out var noteOnObject))
                        data.Add((noteOnObject, obj.Time));

                    noteOnLastObjects.Remove(noteId);
                }
                else
                {
                    var noteOnEvent = ((TimedEvent)obj).Event as NoteOnEvent;
                    if (noteOnEvent == null)
                        continue;

                    var noteId = (eventPorts[noteOnEvent], noteOnEvent.GetNoteId());

                    if (noteOnLastObjects.TryGetValue(noteId, out var noteOnObject))
                        data.Add((noteOnObject, obj.Time));

                    noteOnLastObjects[noteId] = timedObjectAt;
                }
            }

            data.AddRange(noteOnLastObjects.Select(kv => (kv.Value, (long?)null)));

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
                        TrackChunkIndex = d.Item1.TrackChunkIndex,
                        NoteOnEvent = noteOnEvent,
                        NoteOnTimedEvent = (TimedEvent)d.Item1.Object,
                        IsZeroLength = noteOffTime == d.Item1.Object.Time,
                        NoteOffTimedEvent = new TimedEvent(
                            new NoteOffEvent(noteOnEvent.NoteNumber, SevenBitNumber.MinValue) { Channel = noteOnEvent.Channel },
                            noteOffTime)
                    };
                })
                .GroupBy(d => d.TrackChunkIndex)
                .ToArray();

            foreach (var noteOffEventData in noteOffEvents.SelectMany(group => group))
            {
                completedNotes.Add(
                    noteOffEventData.NoteOnEvent,
                    new Note(noteOffEventData.NoteOnTimedEvent, noteOffEventData.NoteOffTimedEvent));
            }

            foreach (var noteOffEventsGroup in noteOffEvents)
            {
                var events = trackChunks[noteOffEventsGroup.Key].Events;
                var lastTime = 0L;
                var noteOffEventsProcessed = false;

                foreach (var noteOffEventData in noteOffEventsGroup.Where(e => e.IsZeroLength))
                {
                    events.Insert(events.IndexOf(noteOffEventData.NoteOnEvent) + 1, noteOffEventData.NoteOffTimedEvent.Event);
                }

                using var noteOffEventsEnumerator = noteOffEventsGroup
                    .Where(e => !e.IsZeroLength)
                    .OrderBy(e => e.NoteOffTimedEvent.Time)
                    .GetEnumerator();

                if (!noteOffEventsEnumerator.MoveNext())
                    continue;

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
            }

            var noteOffPorts = noteOffEvents
                .SelectMany(group => group)
                .ToDictionary(e => e.NoteOffTimedEvent.Event, e => eventPorts[e.NoteOnEvent]);

            foreach (var trackChunk in trackChunks)
            {
                var port = DefaultPort;
                var events = trackChunk.Events;

                for (var eventIndex = 0; eventIndex < events.Count; eventIndex++)
                {
                    var midiEvent = events[eventIndex];

                    if (midiEvent is PortPrefixEvent portPrefixEvent)
                        port = portPrefixEvent.Port;

                    if (!noteOffPorts.TryGetValue(midiEvent, out var noteOffPort) || noteOffPort == port)
                        continue;

                    events.Insert(eventIndex, new PortPrefixEvent(noteOffPort) { DeltaTime = midiEvent.DeltaTime });
                    midiEvent.DeltaTime = 0;
                    events.Insert(eventIndex + 2, new PortPrefixEvent(port));
                    eventIndex += 2;
                }
            }

            return completedNotes;
        }

        private static Dictionary<MidiEvent, byte> GetEventPorts(MidiFile midiFile)
        {
            var eventPorts = new Dictionary<MidiEvent, byte>();

            foreach (var trackChunk in midiFile.GetTrackChunks())
            {
                var port = DefaultPort;

                foreach (var midiEvent in trackChunk.Events)
                {
                    if (midiEvent is PortPrefixEvent portPrefixEvent)
                        port = portPrefixEvent.Port;

                    eventPorts[midiEvent] = port;
                }
            }

            return eventPorts;
        }

        private static IEnumerable<ITimedObject> GetNotesAndTimedEventsByPort(
            this TrackChunk trackChunk,
            SanitizingSettings settings,
            Dictionary<MidiEvent, byte> eventPorts)
        {
            return trackChunk.Events
                .GetTimedEventsLazy(null, 0, false)
                .GroupBy(e => eventPorts[e.Event])
                .SelectMany(events => events.GetNotesAndTimedEventsLazy(settings.NoteDetectionSettings))
                .OrderBy(obj => obj.Time);
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
            HashSet<(byte Port, FourBitNumber Channel)> usedChannels,
            Dictionary<MidiEvent, byte> eventPorts)
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
                    return !usedChannels.Contains((eventPorts[midiEvent], channelEvent.Channel));

                return false;
            });
        }

        private static HashSet<(byte Port, FourBitNumber Channel)> RemoveDuplicatedEventsCollectingUsedChannels(
            MidiFile midiFile,
            SanitizingSettings settings,
            Dictionary<MidiEvent, byte> eventPorts)
        {
            var usedChannels = new HashSet<(byte Port, FourBitNumber Channel)>();

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

            var channelStates = new Dictionary<(byte Port, FourBitNumber Channel), ChannelState>();

            midiFile.RemoveTimedEvents(e =>
            {
                var midiEvent = e.Event;
                var midiEventType = midiEvent.EventType;

                var noteEvent = midiEvent as NoteEvent;
                if (noteEvent != null)
                    usedChannels.Add((eventPorts[midiEvent], noteEvent.Channel));

                ChannelState? channelState = null;

                if (midiEvent is ChannelEvent channelEvent)
                {
                    var channelKey = (eventPorts[midiEvent], channelEvent.Channel);

                    if (!channelStates.TryGetValue(channelKey, out channelState))
                    {
                        channelState = new ChannelState();
                        channelStates.Add(channelKey, channelState);
                    }

                    if (midiEvent is ControlChangeEvent resetEvent &&
                        resetEvent.GetControlName() == ControlName.ResetAllControllers)
                    {
                        Array.Clear(channelState.ControlValues, 0, channelState.ControlValues.Length);
                        channelState.PitchValue = null;
                    }
                }

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

                if (midiEventType == MidiEventType.PitchBend &&
                    settings.RemoveDuplicatedPitchBendEvents &&
                    channelState != null)
                {
                    var pitchBendEvent = (PitchBendEvent)midiEvent;
                    var result = pitchBendEvent.PitchValue == channelState.PitchValue;

                    channelState.PitchValue = pitchBendEvent.PitchValue;
                    return result;
                }

                if (midiEventType == MidiEventType.ControlChange &&
                    settings.RemoveDuplicatedControlChangeEvents &&
                    channelState != null)
                {
                    var controlChangeEvent = (ControlChangeEvent)midiEvent;
                    if (controlChangeEvent.ControlNumber >= ControlName.AllSoundOff.AsSevenBitNumber() ||
                        ControlsToSkip.Contains(controlChangeEvent.GetControlName()))
                    {
                        return false;
                    }

                    var result = controlChangeEvent.ControlValue == channelState.ControlValues[controlChangeEvent.ControlNumber];

                    channelState.ControlValues[controlChangeEvent.ControlNumber] = controlChangeEvent.ControlValue;
                    return result;
                }

                return false;
            });

            if (settings.RemoveDuplicatedSequenceTrackNameEvents)
            {
                foreach (var trackChunk in midiFile.GetTrackChunks())
                {
                    string? sequenceTrackName = null;

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
            SanitizingSettings settings,
            Dictionary<MidiEvent, byte> eventPorts,
            IEnumerable<ITimedObject>? originalNotesAndTimedEvents,
            Dictionary<NoteOnEvent, Note> completedNotes)
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

            var retainedNotes = new Dictionary<(byte Port, int NoteId), HashSet<(long Time, long Length)>>();

            bool ShouldRemoveObject(ITimedObject obj)
            {
                var note = obj as Note;
                if (note != null)
                {
                    if (removeShortNotes &&
                        timeSpanType != null &&
                        LengthConverter.ConvertTo((MidiTimeSpan)note.Length, timeSpanType, note.Time, tempoMap).CompareTo(noteMinLength) < 0)
                        return true;

                    if (removeSilentNotes &&
                        note.Velocity < noteMinVelocity)
                        return true;

                    if (settings.RemoveDuplicatedNotes)
                    {
                        var noteKey = (eventPorts[note.TimedNoteOnEvent.Event], note.GetNoteId());

                        if (!retainedNotes.TryGetValue(noteKey, out var noteTimes))
                        {
                            noteTimes = new HashSet<(long Time, long Length)>();
                            retainedNotes.Add(noteKey, noteTimes);
                        }

                        return !noteTimes.Add((note.Time, note.Length));
                    }
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
            }

            var notesAndTimedEvents = originalNotesAndTimedEvents ??
                midiFile.GetTrackChunks()
                    .SelectMany(trackChunk => trackChunk.GetNotesAndTimedEventsByPort(settings, eventPorts));

            foreach (var objAt in notesAndTimedEvents)
            {
                var obj = objAt;

                if (obj is TimedEvent timedEvent &&
                    timedEvent.Event is NoteOnEvent noteOnEvent &&
                    completedNotes.TryGetValue(noteOnEvent, out var completedNote))
                {
                    obj = completedNote;
                }

                if (!ShouldRemoveObject(obj))
                    continue;

                if (obj is Note note)
                {
                    note.TimedNoteOnEvent.Event.MustBeRemoved = true;
                    note.TimedNoteOffEvent.Event.MustBeRemoved = true;
                }
                else
                {
                    ((TimedEvent)obj).Event.MustBeRemoved = true;
                }
            }

            midiFile.RemoveTimedEvents(e => e.Event.MustBeRemoved);
        }

        #endregion
    }
}
