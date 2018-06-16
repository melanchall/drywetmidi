using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public enum ResizeMode
    {
        Absolute,
        Relative
    }

    public static class ResizeNotesUtilities
    {
        #region Methods

        public static void ResizeNotes(this IEnumerable<Note> notes,
                                       ITimeSpan length,
                                       TimeSpanType lengthType,
                                       ResizeMode mode,
                                       TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsInvalidEnumValue(nameof(mode), mode);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!notes.Any())
                return;

            var minStartTime = notes.Min(n => n.Time);
            var maxEndTime = notes.Max(n => n.Time + n.Length);

            // If length of the notes group is 0, just set new length

            var totalLength = maxEndTime - minStartTime;
            if (totalLength == 0)
            {
                var convertedLength = LengthConverter.ConvertFrom(length, minStartTime, tempoMap);

                foreach (var note in notes)
                {
                    note.Length = convertedLength;
                }

                return;
            }

            //

            var oldLength = LengthConverter.ConvertTo(totalLength, lengthType, minStartTime, tempoMap);
            var newLength = LengthConverter.ConvertTo(length, lengthType, minStartTime, tempoMap);

            switch (mode)
            {
                case ResizeMode.Absolute:
                    ResizeNotesAbsolute(notes, lengthType, tempoMap, oldLength, newLength);
                    break;
                case ResizeMode.Relative:
                    ResizeNotesRelative(notes, lengthType, tempoMap, oldLength, newLength);
                    break;
            }
        }

        private static void ResizeNotesAbsolute(IEnumerable<Note> notes,
                                                TimeSpanType lengthType,
                                                TempoMap tempoMap,
                                                ITimeSpan oldLength,
                                                ITimeSpan newLength)
        {
            var relation = newLength.CompareTo(oldLength);
            if (relation == 0)
                return;

            var offset = relation > 0
                ? newLength.Subtract(oldLength, TimeSpanMode.LengthLength)
                : oldLength.Subtract(newLength, TimeSpanMode.LengthLength);

            foreach (var note in notes)
            {
                var noteLength = note.LengthAs(lengthType, tempoMap);
                if (relation < 0 && noteLength.CompareTo(offset) <= 0)
                {
                    // TODO: strategies to handle this case
                    note.Length = 0;
                    continue;
                }

                noteLength = relation > 0
                    ? noteLength.Add(offset, TimeSpanMode.LengthLength)
                    : noteLength.Subtract(offset, TimeSpanMode.LengthLength);

                note.Length = LengthConverter.ConvertFrom(noteLength, note.Time, tempoMap);
            }
        }

        private static void ResizeNotesRelative(IEnumerable<Note> notes,
                                                TimeSpanType lengthType,
                                                TempoMap tempoMap,
                                                ITimeSpan oldLength,
                                                ITimeSpan newLength)
        {
            if (lengthType == TimeSpanType.BarBeat)
                throw new ArgumentException("BarBeat length type is not supported for relative resizing.", nameof(lengthType));

            var ratio = TimeSpanUtilities.Divide(newLength, oldLength);

            foreach (var note in notes)
            {
                // TODO: strategies to handle this case
                if (note.Length == 0)
                    continue;

                var scaledLength = note.LengthAs(lengthType, tempoMap).Multiply(ratio);
                note.Length = LengthConverter.ConvertFrom(scaledLength, note.Time, tempoMap);
            }
        }

        #endregion
    }
}
