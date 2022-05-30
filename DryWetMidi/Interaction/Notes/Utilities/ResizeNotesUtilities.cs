using System;
using System.Collections.Generic;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Tools;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides ways to resize collection of notes.
    /// </summary>
    [Obsolete("OBS18")]
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
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="notes"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException"><see cref="TimeSpanType.BarBeatTicks"/> or <see cref="TimeSpanType.BarBeatFraction"/>
        /// is used for <paramref name="distanceCalculationType"/> which is unsupported.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="distanceCalculationType"/> specified an
        /// invalid value.</exception>
        [Obsolete("OBS18")]
        public static void ResizeNotes(this IEnumerable<Note> notes,
                                       ITimeSpan length,
                                       TimeSpanType distanceCalculationType,
                                       TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsInvalidEnumValue(nameof(distanceCalculationType), distanceCalculationType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            notes.ResizeObjectsGroup(length, tempoMap, new ObjectsGroupResizingSettings
            {
                DistanceCalculationType = distanceCalculationType
            });
        }

        /// <summary>
        /// Resizes group of notes by the specified ratio treating all notes as single object. For example,
        /// resizing by ratio of 0.5 shrinks group of notes by two times.
        /// </summary>
        /// <param name="notes">Notes to resize.</param>
        /// <param name="ratio">Ratio to resize notes by.</param>
        /// <param name="distanceCalculationType">Type of distance calculations.</param>
        /// <param name="tempoMap"></param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="notes"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException"><see cref="TimeSpanType.BarBeatTicks"/> or <see cref="TimeSpanType.BarBeatFraction"/>
        /// is used for <paramref name="distanceCalculationType"/> which is unsupported.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratio"/> is negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="distanceCalculationType"/> specified an
        /// invalid value.</exception>
        [Obsolete("OBS18")]
        public static void ResizeNotes(this IEnumerable<Note> notes,
                                       double ratio,
                                       TimeSpanType distanceCalculationType,
                                       TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);
            ThrowIfArgument.IsNegative(nameof(ratio), ratio, "Ratio is negative");
            ThrowIfArgument.IsInvalidEnumValue(nameof(distanceCalculationType), distanceCalculationType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            notes.ResizeObjectsGroup(ratio, tempoMap, new ObjectsGroupResizingSettings
            {
                DistanceCalculationType = distanceCalculationType
            });
        }

        #endregion
    }
}
