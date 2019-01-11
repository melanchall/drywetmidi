using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Contains methods to play MIDI data and retrieving an instance of the <see cref="Playback"/>
    /// which provides advanced features for MIDI data playing.
    /// </summary>
    public static class PlaybackUtilities
    {
        #region Methods

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> containing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the <paramref name="trackChunk"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        public static Playback GetPlayback(this TrackChunk trackChunk, TempoMap tempoMap, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return new Playback(trackChunk.Events, tempoMap, outputDevice);
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> containing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the <paramref name="trackChunks"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        public static Playback GetPlayback(this IEnumerable<TrackChunk> trackChunks, TempoMap tempoMap, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return new Playback(trackChunks.Select(c => c.Events), tempoMap, outputDevice);
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> containing events to play.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the <paramref name="midiFile"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null. -or-
        /// <paramref name="outputDevice"/> is null.</exception>
        public static Playback GetPlayback(this MidiFile midiFile, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return GetPlayback(midiFile.GetTrackChunks(), midiFile.GetTempoMap(), outputDevice);
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events that will be
        /// produced by specified <see cref="Pattern"/>.
        /// </summary>
        /// <param name="pattern"><see cref="Pattern"/> producing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="channel">MIDI channel to play channel events on.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing MIDI events that will be
        /// produced by the <paramref name="pattern"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        public static Playback GetPlayback(this Pattern pattern, TempoMap tempoMap, FourBitNumber channel, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return pattern.ToTrackChunk(tempoMap, channel).GetPlayback(tempoMap, outputDevice);
        }

        /// <summary>
        /// Plays MIDI events contained in the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> containing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        public static void Play(this TrackChunk trackChunk, TempoMap tempoMap, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            using (var playback = trackChunk.GetPlayback(tempoMap, outputDevice))
            {
                playback.Play();
            }
        }

        /// <summary>
        /// Plays MIDI events contained in the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> containing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        public static void Play(this IEnumerable<TrackChunk> trackChunks, TempoMap tempoMap, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            using (var playback = trackChunks.GetPlayback(tempoMap, outputDevice))
            {
                playback.Play();
            }
        }

        /// <summary>
        /// Plays MIDI events contained in the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> containing events to play.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null. -or-
        /// <paramref name="outputDevice"/> is null.</exception>
        public static void Play(this MidiFile midiFile, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            midiFile.GetTrackChunks().Play(midiFile.GetTempoMap(), outputDevice);
        }

        /// <summary>
        /// Plays MIDI events that will be produced by specified <see cref="Pattern"/>.
        /// </summary>
        /// <param name="pattern"><see cref="Pattern"/> producing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="channel">MIDI channel to play channel events on.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pattern"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        public static void Play(this Pattern pattern, TempoMap tempoMap, FourBitNumber channel, OutputDevice outputDevice)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            pattern.ToTrackChunk(tempoMap, channel).Play(tempoMap, outputDevice);
        }

        #endregion
    }
}
