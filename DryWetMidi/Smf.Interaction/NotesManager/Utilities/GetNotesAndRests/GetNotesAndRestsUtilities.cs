using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class GetNotesAndRestsUtilities
    {
        #region Constants

        private static readonly object NoSeparationNoteDescriptor = new object();

        #endregion

        #region Methods

        public static IEnumerable<ILengthedObject> GetNotesAndRests(
            this IEnumerable<Note> notes,
            RestSeparationPolicy restSeparationPolicy)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.IsInvalidEnumValue(nameof(restSeparationPolicy), restSeparationPolicy);

            switch (restSeparationPolicy)
            {
                case RestSeparationPolicy.NoSeparation:
                    return GetNotesAndRests(notes,
                                            n => NoSeparationNoteDescriptor,
                                            false,
                                            false);
                case RestSeparationPolicy.SeparateByChannel:
                    return GetNotesAndRests(notes,
                                            n => n.Channel,
                                            true,
                                            false);
                case RestSeparationPolicy.SeparateByNoteNumber:
                    return GetNotesAndRests(notes,
                                            n => n.NoteNumber,
                                            false,
                                            true);
                case RestSeparationPolicy.SeparateByChannelAndNoteNumber:
                    return GetNotesAndRests(notes,
                                            n => new ChannelAndNoteNumber(n.Channel, n.NoteNumber),
                                            true,
                                            true);
            }

            throw new NotSupportedException($"Rest separation policy {restSeparationPolicy} is not supported.");
        }

        private static IEnumerable<ILengthedObject> GetNotesAndRests<TDescriptor>(
            IEnumerable<Note> notes,
            Func<Note, TDescriptor> noteDescriptorGetter,
            bool setRestChannel,
            bool setRestNoteNumber)
        {
            var lastEndTimes = new Dictionary<TDescriptor, long>();

            foreach (var note in notes.Where(n => n != null).OrderBy(n => n.Time))
            {
                var noteDescriptor = noteDescriptorGetter(note);

                long lastEndTime;
                lastEndTimes.TryGetValue(noteDescriptor, out lastEndTime);

                if (note.Time > lastEndTime)
                    yield return new Rest(lastEndTime,
                                          note.Time - lastEndTime,
                                          setRestChannel ? (FourBitNumber?)note.Channel : null,
                                          setRestNoteNumber ? (SevenBitNumber?)note.NoteNumber : null);

                yield return note.Clone();

                lastEndTimes[noteDescriptor] = Math.Max(lastEndTime, note.Time + note.Length);
            }
        }

        #endregion
    }
}
