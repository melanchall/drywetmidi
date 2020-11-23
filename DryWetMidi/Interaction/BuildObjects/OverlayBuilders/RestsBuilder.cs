using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class RestsBuilder : IOverlayBuilder
    {
        #region Constants

        private static readonly object NoSeparationNoteDescriptor = new object();

        #endregion

        #region Methods

        public IEnumerable<ITimedObject> BuildObjects(
            IEnumerable<ITimedObject> inputTimedObjects,
            IEnumerable<ITimedObject> resultTimedObjects,
            ObjectsBuildingSettings settings)
        {
            var notes = settings.BuildNotes
                ? resultTimedObjects.OfType<Note>()
                : inputTimedObjects.BuildObjects(new ObjectsBuildingSettings { BuildNotes = true }).OfType<Note>();

            var restBuildingSettings = settings.RestBuilderSettings ?? new RestBuilderSettings();

            switch (restBuildingSettings.RestSeparationPolicy)
            {
                case RestSeparationPolicy.NoSeparation:
                    return GetRests(
                        notes,
                        n => NoSeparationNoteDescriptor,
                        setRestChannel: false,
                        setRestNoteNumber: false);
                case RestSeparationPolicy.SeparateByChannel:
                    return GetRests(
                        notes,
                        n => n.Channel,
                        setRestChannel: true,
                        setRestNoteNumber: false);
                case RestSeparationPolicy.SeparateByNoteNumber:
                    return GetRests(
                        notes,
                        n => n.NoteNumber,
                        setRestChannel: false,
                        setRestNoteNumber: true);
                case RestSeparationPolicy.SeparateByChannelAndNoteNumber:
                    return GetRests(
                        notes,
                        n => n.GetNoteId(),
                        setRestChannel: true,
                        setRestNoteNumber: true);
            }

            throw new NotSupportedException($"Rest separation policy {restBuildingSettings.RestSeparationPolicy} is not supported.");
        }

        private static IEnumerable<ILengthedObject> GetRests<TDescriptor>(
            IEnumerable<Note> notes,
            Func<Note, TDescriptor> noteDescriptorGetter,
            bool setRestChannel,
            bool setRestNoteNumber)
        {
            var lastEndTimes = new Dictionary<TDescriptor, long>();

            var result = new List<Rest>();

            foreach (var note in notes)
            {
                var noteDescriptor = noteDescriptorGetter(note);

                long lastEndTime;
                lastEndTimes.TryGetValue(noteDescriptor, out lastEndTime);

                if (note.Time > lastEndTime)
                    result.Add(new Rest(
                        lastEndTime,
                        note.Time - lastEndTime,
                        setRestChannel ? (FourBitNumber?)note.Channel : null,
                        setRestNoteNumber ? (SevenBitNumber?)note.NoteNumber : null));

                lastEndTimes[noteDescriptor] = Math.Max(lastEndTime, note.Time + note.Length);
            }

            return result;
        }

        #endregion
    }
}
