using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides a way to sanitize a MIDI file applying different options. More info in the
    /// <see href="xref:a_sanitizer">Sanitizer</see> article.
    /// </summary>
    public static class Sanitizer
    {
        #region Methods

        /// <summary>
        /// Sanitizes a MIDI file according to the specified settings. Note that the input file will be
        /// transformed instead of returning a new one so be sure you've cloned it if needed.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to process.</param>
        /// <param name="settings">Settings which control how the <paramref name="midiFile"/>
        /// should be sanitized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static void Sanitize(this MidiFile midiFile, SanitizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            settings = settings ?? new SanitizingSettings();

            RemoveNoteData(midiFile, settings);
            var usedChannels = RemoveDuplicatedEventsCollectingUsedChannels(midiFile, settings);
            RemoveEventsOnUnusedChannels(midiFile, settings, usedChannels);
            RemoveEmptyTrackChunks(midiFile, settings);
            TrimFile(midiFile, settings);
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
            var microsecondsPerQuarterNote = SetTempoEvent.DefaultMicrosecondsPerQuarterNote;

            var timeSignatureNumerator = TimeSignatureEvent.DefaultNumerator;
            var timeSignatureDenominator = TimeSignatureEvent.DefaultDenominator;
            var timeSignatureThirtySecondNotesPerBeat = TimeSignatureEvent.DefaultThirtySecondNotesPerBeat;
            var timeSignatureClocksPerClick = TimeSignatureEvent.DefaultClocksPerClick;

            ushort? pitchValue = null;
            FourBitNumber? pitchBendChannel = null;

            var usedChannels = new bool[FourBitNumber.MaxValue + 1];

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

                return false;
            });

            return usedChannels;
        }

        private static void RemoveNoteData(
            MidiFile midiFile,
            SanitizingSettings settings)
        {
            var noteMinLength = settings.NoteMinLength;
            var removeShortNotes = noteMinLength != null && !noteMinLength.IsZeroTimeSpan();

            if (!removeShortNotes && !settings.RemoveOrphanedNoteOnEvents && !settings.RemoveOrphanedNoteOffEvents)
                return;

            var tempoMap = midiFile.GetTempoMap();
            var timeSpanType = noteMinLength?.GetType();

            foreach (var trackChunk in midiFile.GetTrackChunks())
            {
                using (var objectsManager = new TimedObjectsManager(
                    trackChunk.Events,
                    ObjectType.TimedEvent | ObjectType.Note,
                    new ObjectDetectionSettings { NoteDetectionSettings = settings.NoteDetectionSettings }))
                {
                    objectsManager.Objects.RemoveAll(obj =>
                    {
                        var note = obj as Note;
                        if (note != null)
                        {
                            if (removeShortNotes &&
                                LengthConverter.ConvertTo((MidiTimeSpan)note.Length, timeSpanType, note.Time, tempoMap).CompareTo(noteMinLength) < 0)
                                return true;
                        }
                        else
                        {
                            var timedEvent = (TimedEvent)obj;

                            if ((settings.RemoveOrphanedNoteOnEvents && timedEvent.Event.EventType == MidiEventType.NoteOn) ||
                                (settings.RemoveOrphanedNoteOffEvents && timedEvent.Event.EventType == MidiEventType.NoteOff))
                                return true;
                        }

                        return false;
                    });
                }
            }
        }

        #endregion
    }
}
