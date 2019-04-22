using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Standards;

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
        /// Retrieves an instance of the <see cref="Playback"/> for playing musical objects using
        /// the specified program.
        /// </summary>
        /// <typeparam name="TObject">The type of objects to play.</typeparam>
        /// <param name="objects">Objects to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play <paramref name="objects"/> through.</param>
        /// <param name="programNumber">Program that should be used to play <paramref name="objects"/>.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing <paramref name="objects"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        public static Playback GetPlayback<TObject>(this IEnumerable<TObject> objects, TempoMap tempoMap, OutputDevice outputDevice, SevenBitNumber programNumber)
            where TObject : IMusicalObject, ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return GetMusicalObjectsPlayback(objects,
                                             tempoMap,
                                             outputDevice,
                                             channel => new[] { new ProgramChangeEvent(programNumber) { Channel = channel } });
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing musical objects using
        /// the specified General MIDI 1 program.
        /// </summary>
        /// <typeparam name="TObject">The type of objects to play.</typeparam>
        /// <param name="objects">Objects to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play <paramref name="objects"/> through.</param>
        /// <param name="generalMidiProgram">Program that should be used to play <paramref name="objects"/>.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing <paramref name="objects"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="generalMidiProgram"/> specified an invalid value.</exception>
        public static Playback GetPlayback<TObject>(this IEnumerable<TObject> objects, TempoMap tempoMap, OutputDevice outputDevice, GeneralMidiProgram generalMidiProgram)
            where TObject : IMusicalObject, ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);
            ThrowIfArgument.IsInvalidEnumValue(nameof(generalMidiProgram), generalMidiProgram);

            return GetMusicalObjectsPlayback(objects,
                                             tempoMap,
                                             outputDevice,
                                             channel => new[] { generalMidiProgram.GetProgramEvent(channel) });
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing musical objects using
        /// the specified General MIDI 2 program.
        /// </summary>
        /// <typeparam name="TObject">The type of objects to play.</typeparam>
        /// <param name="objects">Objects to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play <paramref name="objects"/> through.</param>
        /// <param name="generalMidi2Program">Program that should be used to play <paramref name="objects"/>.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing <paramref name="objects"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="generalMidi2Program"/> specified an invalid value.</exception>
        public static Playback GetPlayback<TObject>(this IEnumerable<TObject> objects, TempoMap tempoMap, OutputDevice outputDevice, GeneralMidi2Program generalMidi2Program)
            where TObject : IMusicalObject, ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);
            ThrowIfArgument.IsInvalidEnumValue(nameof(generalMidi2Program), generalMidi2Program);

            return GetMusicalObjectsPlayback(objects,
                                             tempoMap,
                                             outputDevice,
                                             channel => generalMidi2Program.GetProgramEvents(channel));
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

        /// <summary>
        /// Plays musical objects using the specified program.
        /// </summary>
        /// <typeparam name="TObject">The type of objects to play.</typeparam>
        /// <param name="objects">Objects to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play <paramref name="objects"/> through.</param>
        /// <param name="programNumber">Program that should be used to play <paramref name="objects"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        public static void Play<TObject>(this IEnumerable<TObject> objects, TempoMap tempoMap, OutputDevice outputDevice, SevenBitNumber programNumber)
            where TObject : IMusicalObject, ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            using (var playback = objects.GetPlayback(tempoMap, outputDevice, programNumber))
            {
                playback.Play();
            }
        }

        /// <summary>
        /// Plays musical objects using the specified General MIDI 1 program.
        /// </summary>
        /// <typeparam name="TObject">The type of objects to play.</typeparam>
        /// <param name="objects">Objects to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play <paramref name="objects"/> through.</param>
        /// <param name="generalMidiProgram">Program that should be used to play <paramref name="objects"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="generalMidiProgram"/> specified an invalid value.</exception>
        public static void Play<TObject>(this IEnumerable<TObject> objects, TempoMap tempoMap, OutputDevice outputDevice, GeneralMidiProgram generalMidiProgram)
            where TObject : IMusicalObject, ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);
            ThrowIfArgument.IsInvalidEnumValue(nameof(generalMidiProgram), generalMidiProgram);

            using (var playback = objects.GetPlayback(tempoMap, outputDevice, generalMidiProgram))
            {
                playback.Play();
            }
        }

        /// <summary>
        /// Plays musical objects using the specified General MIDI 2 program.
        /// </summary>
        /// <typeparam name="TObject">The type of objects to play.</typeparam>
        /// <param name="objects">Objects to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play <paramref name="objects"/> through.</param>
        /// <param name="generalMidi2Program">Program that should be used to play <paramref name="objects"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="objects"/> is null. -or-
        /// <paramref name="tempoMap"/> is null. -or- <paramref name="outputDevice"/> is null.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="generalMidi2Program"/> specified an invalid value.</exception>
        public static void Play<TObject>(this IEnumerable<TObject> objects, TempoMap tempoMap, OutputDevice outputDevice, GeneralMidi2Program generalMidi2Program)
            where TObject : IMusicalObject, ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);
            ThrowIfArgument.IsInvalidEnumValue(nameof(generalMidi2Program), generalMidi2Program);

            using (var playback = objects.GetPlayback(tempoMap, outputDevice, generalMidi2Program))
            {
                playback.Play();
            }
        }

        private static Playback GetMusicalObjectsPlayback<TObject>(IEnumerable<TObject> objects,
                                                                   TempoMap tempoMap,
                                                                   OutputDevice outputDevice,
                                                                   Func<FourBitNumber, IEnumerable<MidiEvent>> programChangeEventsGetter)
            where TObject : IMusicalObject, ITimedObject
        {
            var programChangeEvents = objects.Select(n => n.Channel)
                                             .Distinct()
                                             .SelectMany(programChangeEventsGetter)
                                             .Select(e => (ITimedObject)new TimedEvent(e));

            return new Playback(programChangeEvents.Concat((IEnumerable<ITimedObject>)objects), tempoMap, outputDevice);
        }

        #endregion
    }
}
