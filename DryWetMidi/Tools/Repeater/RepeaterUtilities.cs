using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to repeat MIDI data using different options. More info in the
    /// <see href="xref:a_repeater">Repeater</see> article.
    /// </summary>
    public static class RepeaterUtilities
    {
        #region Methods

        /// <summary>
        /// Repeats a MIDI file specified number of times. More info in the
        /// <see href="xref:a_repeater">Repeater</see> article.
        /// </summary>
        /// <param name="midiFile">The file to repeat.</param>
        /// <param name="repeatsNumber">Number of times the <paramref name="midiFile"/> should be repeated.</param>
        /// <param name="settings">Settings according to which the operation should be done.</param>
        /// <returns>A new instance of the <see cref="MidiFile"/> which is the <paramref name="midiFile"/>
        /// repeated <paramref name="repeatsNumber"/> times using <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatsNumber"/> is negative.</exception>
        /// <exception cref="ArgumentException"><see cref="RepeatingSettings.Shift"/> of the <paramref name="settings"/>
        /// is <c>null</c> for fixed-value shift.</exception>
        public static MidiFile Repeat(this MidiFile midiFile, int repeatsNumber, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNegative(nameof(repeatsNumber), repeatsNumber, "Repeats number is negative.");

            return new Repeater().Repeat(midiFile, repeatsNumber, settings);
        }

        /// <summary>
        /// Repeats a <see cref="TrackChunk"/> specified number of times. More info in the
        /// <see href="xref:a_repeater">Repeater</see> article.
        /// </summary>
        /// <param name="trackChunk">The <see cref="TrackChunk"/> to repeat.</param>
        /// <param name="repeatsNumber">Number of times the <paramref name="trackChunk"/> should be repeated.</param>
        /// <param name="tempoMap">Tempo map used to perform time spans calculations.</param>
        /// <param name="settings">Settings according to which the operation should be done.</param>
        /// <returns>A new instance of the <see cref="TrackChunk"/> which is the <paramref name="trackChunk"/>
        /// repeated <paramref name="repeatsNumber"/> times using <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatsNumber"/> is negative.</exception>
        /// <exception cref="ArgumentException"><see cref="RepeatingSettings.Shift"/> of the <paramref name="settings"/>
        /// is <c>null</c> for fixed-value shift.</exception>
        public static TrackChunk Repeat(this TrackChunk trackChunk, int repeatsNumber, TempoMap tempoMap, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNegative(nameof(repeatsNumber), repeatsNumber, "Repeats number is negative.");

            return new Repeater().Repeat(trackChunk, repeatsNumber, tempoMap, settings);
        }

        /// <summary>
        /// Repeats a collection of <see cref="TrackChunk"/> specified number of times. More info in the
        /// <see href="xref:a_repeater">Repeater</see> article.
        /// </summary>
        /// <param name="trackChunks">The collection of <see cref="TrackChunk"/> to repeat.</param>
        /// <param name="repeatsNumber">Number of times the <paramref name="trackChunks"/> should be repeated.</param>
        /// <param name="tempoMap">Tempo map used to perform time spans calculations.</param>
        /// <param name="settings">Settings according to which the operation should be done.</param>
        /// <returns>A collection of new <see cref="TrackChunk"/> instances which are the <paramref name="trackChunks"/>
        /// repeated <paramref name="repeatsNumber"/> times using <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatsNumber"/> is negative.</exception>
        /// <exception cref="ArgumentException"><see cref="RepeatingSettings.Shift"/> of the <paramref name="settings"/>
        /// is <c>null</c> for fixed-value shift.</exception>
        public static ICollection<TrackChunk> Repeat(this IEnumerable<TrackChunk> trackChunks, int repeatsNumber, TempoMap tempoMap, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNegative(nameof(repeatsNumber), repeatsNumber, "Repeats number is negative.");

            return new Repeater().Repeat(trackChunks, repeatsNumber, tempoMap, settings);
        }

        /// <summary>
        /// Repeats a collection of timed objects specified number of times. More info in the
        /// <see href="xref:a_repeater">Repeater</see> article.
        /// </summary>
        /// <param name="timedObjects">The collection of timed objects to repeat.</param>
        /// <param name="repeatsNumber">Number of times the <paramref name="timedObjects"/> should be repeated.</param>
        /// <param name="tempoMap">Tempo map used to perform time spans calculations.</param>
        /// <param name="settings">Settings according to which the operation should be done.</param>
        /// <returns>A collection of new <see cref="TrackChunk"/> instances which are the <paramref name="timedObjects"/>
        /// repeated <paramref name="repeatsNumber"/> times using <paramref name="settings"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timedObjects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatsNumber"/> is negative.</exception>
        /// <exception cref="ArgumentException"><see cref="RepeatingSettings.Shift"/> of the <paramref name="settings"/>
        /// is <c>null</c> for fixed-value shift.</exception>
        public static ICollection<ITimedObject> Repeat(this IEnumerable<ITimedObject> timedObjects, int repeatsNumber, TempoMap tempoMap, RepeatingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNegative(nameof(repeatsNumber), repeatsNumber, "Repeats number is negative.");

            return new Repeater().Repeat(timedObjects, repeatsNumber, tempoMap, settings);
        }

        #endregion
    }
}
