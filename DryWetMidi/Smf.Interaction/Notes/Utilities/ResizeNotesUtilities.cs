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
        /// Resizes group of notes to the specified length treating all notes as single object.
        /// </summary>
        /// <param name="notes">Notes to resize.</param>
        /// <param name="length">New length of the notes collection.</param>
        /// <param name="distanceCalculationType">Type of distance calculations.</param>
        /// <param name="tempoMap"></param>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is null. -or-
        /// <paramref name="length"/> is null. -or- <paramref name="tempoMap"/> is null.</exception>
        /// <exception cref="ArgumentException"><see cref="TimeSpanType.BarBeat"/> is used for
        /// <paramref name="distanceCalculationType"/> which is unsupported.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="distanceCalculationType"/> specified an
        /// invalid value.</exception>
        public static void ResizeNotes(this IEnumerable<Note> notes,
                                       ITimeSpan length,
                                       TimeSpanType distanceCalculationType,
                                       TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsInvalidEnumValue(nameof(distanceCalculationType), distanceCalculationType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (distanceCalculationType == TimeSpanType.BarBeat)
                throw new ArgumentException("BarBeat distance calculation type is not supported.", nameof(distanceCalculationType));

            var notNullNotes = notes.Where(n => n != null);
            if (!notNullNotes.Any())
                return;

            //

            var minStartTime = long.MaxValue;
            var maxEndTime = 0L;

            foreach (var note in notNullNotes)
            {
                var noteStartTime = note.Time;
                var noteEndTime = noteStartTime + note.Length;

                minStartTime = Math.Min(minStartTime, noteStartTime);
                maxEndTime = Math.Max(maxEndTime, noteEndTime);
            }

            var totalLength = maxEndTime - minStartTime;

            //

            var oldLength = LengthConverter.ConvertTo(totalLength, distanceCalculationType, minStartTime, tempoMap);
            var newLength = LengthConverter.ConvertTo(length, distanceCalculationType, minStartTime, tempoMap);
            var ratio = TimeSpanUtilities.Divide(newLength, oldLength);

            var startTime = TimeConverter.ConvertTo(minStartTime, distanceCalculationType, tempoMap);

            ResizeNotesByRatio(notNullNotes, ratio, distanceCalculationType, tempoMap, startTime);
        }

        /// <summary>
        /// Resizes group of notes by the specified ratio treating all notes as single object. For example,
        /// resizing by ratio of 0.5 shrinks group of notes by two times.
        /// </summary>
        /// <param name="notes">Notes to resize.</param>
        /// <param name="ratio">Ratio to resize notes by.</param>
        /// <param name="distanceCalculationType">Type of distance calculations.</param>
        /// <param name="tempoMap"></param>
        /// <exception cref="ArgumentNullException"><paramref name="notes"/> is null. -or-
        /// <paramref name="tempoMap"/> is null.</exception>
        /// <exception cref="ArgumentException"><see cref="TimeSpanType.BarBeat"/> is used for
        /// <paramref name="distanceCalculationType"/> which is unsupported.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratio"/> is negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="distanceCalculationType"/> specified an
        /// invalid value.</exception>
        public static void ResizeNotes(this IEnumerable<Note> notes,
                                       double ratio,
                                       TimeSpanType distanceCalculationType,
                                       TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.IsNegative(nameof(ratio), ratio, "Ratio is negative");
            ThrowIfArgument.IsInvalidEnumValue(nameof(distanceCalculationType), distanceCalculationType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (distanceCalculationType == TimeSpanType.BarBeat)
                throw new ArgumentException("BarBeat distance calculation type is not supported.", nameof(distanceCalculationType));

            var notNullNotes = notes.Where(n => n != null);
            if (!notNullNotes.Any())
                return;

            //

            var minStartTime = notNullNotes.Select(n => n.Time).Min();
            var startTime = TimeConverter.ConvertTo(minStartTime, distanceCalculationType, tempoMap);

            ResizeNotesByRatio(notNullNotes, ratio, distanceCalculationType, tempoMap, startTime);
        }

        private static void ResizeNotesByRatio(IEnumerable<Note> notes,
                                               double ratio,
                                               TimeSpanType distanceCalculationType,
                                               TempoMap tempoMap,
                                               ITimeSpan startTime)
        {
            foreach (var note in notes)
            {
                var noteLength = note.LengthAs(distanceCalculationType, tempoMap);
                var noteTime = note.TimeAs(distanceCalculationType, tempoMap);

                var scaledShiftFromStart = noteTime.Subtract(startTime, TimeSpanMode.TimeTime).Multiply(ratio);
                note.Time = TimeConverter.ConvertFrom(startTime.Add(scaledShiftFromStart, TimeSpanMode.TimeLength), tempoMap);

                var scaledLength = noteLength.Multiply(ratio);
                note.Length = LengthConverter.ConvertFrom(scaledLength, note.Time, tempoMap);
            }
        }

        #endregion
    }
}
