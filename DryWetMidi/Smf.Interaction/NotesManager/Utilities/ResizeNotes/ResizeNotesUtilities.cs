using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Provides ways to resize collection of notes.
    /// </summary>
    public static class ResizeNotesUtilities
    {
        #region Methods

        /// <summary>
        /// Resizes collection of notes to the specified length using absolute or relative
        /// length calculation.
        /// </summary>
        /// <remarks>
        /// Absolute resizing means 
        /// </remarks>
        /// <param name="notes">Notes to resize.</param>
        /// <param name="length">New length of the notes collection.</param>
        /// <param name="lengthType">Type of length calculations.</param>
        /// <param name="mode">Resizing mode which determines whether length calculation should be
        /// absolute or relative (by ratio).</param>
        /// <param name="tempoMap"></param>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is null. -or-
        /// <paramref name="length"/> is null. -or- <paramref name="tempoMap"/> is null.</exception>
        /// <exception cref="ArgumentException"><see cref="TimeSpanType.BarBeat"/> used for <paramref name="lengthType"/>
        /// with relative resizing.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an
        /// invalid value. -or- <paramref name="mode"/> specified an invalid value.</exception>
        public static void ResizeNotes(this IEnumerable<Note> notes,
                                       ITimeSpan length,
                                       TimeSpanType lengthType,
                                       ResizingMode mode,
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
                case ResizingMode.Absolute:
                    ResizeNotesAbsolute(notes, lengthType, tempoMap, oldLength, newLength);
                    break;
                case ResizingMode.Relative:
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
