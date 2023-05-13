using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Standards;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Contains methods to play MIDI data and retrieving an instance of the <see cref="Playback"/>
    /// which provides advanced features for MIDI data playing. More info in the
    /// <see href="xref:a_playback_overview">Playback</see> article.
    /// </summary>
    public static class PlaybackUtilities
    {
        #region Methods

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events collection.
        /// Events will be scheduled for playback according to their delta-times.
        /// </summary>
        /// <param name="events">MIDI events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing <paramref name="events"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="events"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Playback GetPlayback(this IEnumerable<MidiEvent> events, TempoMap tempoMap, IOutputDevice outputDevice, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(events), events);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            var timedObjects = events
                .GetTimedEventsLazy(playbackSettings?.TimedEventDetectionSettings)
                .GetNotesAndTimedEventsLazy(playbackSettings?.NoteDetectionSettings ?? new NoteDetectionSettings());

            return new Playback(timedObjects, tempoMap, outputDevice, playbackSettings);
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events collection.
        /// Events will be scheduled for playback according to their delta-times.
        /// </summary>
        /// <param name="events">MIDI events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing <paramref name="events"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="events"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Playback GetPlayback(this IEnumerable<MidiEvent> events, TempoMap tempoMap, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(events), events);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var timedObjects = events
                .GetTimedEventsLazy(playbackSettings?.TimedEventDetectionSettings)
                .GetNotesAndTimedEventsLazy(playbackSettings?.NoteDetectionSettings ?? new NoteDetectionSettings());

            return new Playback(timedObjects, tempoMap, playbackSettings);
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> containing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the <paramref name="trackChunk"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Playback GetPlayback(this TrackChunk trackChunk, TempoMap tempoMap, IOutputDevice outputDevice, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            var timedObjects = trackChunk
                .Events
                .GetTimedEventsLazy(playbackSettings?.TimedEventDetectionSettings)
                .GetNotesAndTimedEventsLazy(playbackSettings?.NoteDetectionSettings ?? new NoteDetectionSettings());

            return new Playback(timedObjects, tempoMap, outputDevice, playbackSettings);
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> containing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the <paramref name="trackChunk"/>.</returns>
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
        public static Playback GetPlayback(this TrackChunk trackChunk, TempoMap tempoMap, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var timedObjects = trackChunk
                .Events
                .GetTimedEventsLazy(playbackSettings?.TimedEventDetectionSettings)
                .GetNotesAndTimedEventsLazy(playbackSettings?.NoteDetectionSettings ?? new NoteDetectionSettings());

            return new Playback(timedObjects, tempoMap, playbackSettings);
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> containing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the <paramref name="trackChunks"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Playback GetPlayback(this IEnumerable<TrackChunk> trackChunks, TempoMap tempoMap, IOutputDevice outputDevice, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            var timedObjects = trackChunks
                .GetTimedEventsLazy(playbackSettings?.TimedEventDetectionSettings)
                .GetNotesAndTimedEventsLazy(playbackSettings?.NoteDetectionSettings ?? new NoteDetectionSettings())
                .Select(o => o.Object);

            return new Playback(timedObjects, tempoMap, outputDevice, playbackSettings);
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> containing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the <paramref name="trackChunks"/>.</returns>
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
        public static Playback GetPlayback(this IEnumerable<TrackChunk> trackChunks, TempoMap tempoMap, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var timedObjects = trackChunks
                .GetTimedEventsLazy(playbackSettings?.TimedEventDetectionSettings)
                .GetNotesAndTimedEventsLazy(playbackSettings?.NoteDetectionSettings ?? new NoteDetectionSettings())
                .Select(o => o.Object);

            return new Playback(timedObjects, tempoMap, playbackSettings);
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> containing events to play.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the <paramref name="midiFile"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Playback GetPlayback(this MidiFile midiFile, IOutputDevice outputDevice, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return GetPlayback(midiFile.GetTrackChunks(), midiFile.GetTempoMap(), outputDevice, playbackSettings);
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> containing events to play.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing MIDI events contained in
        /// the <paramref name="midiFile"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static Playback GetPlayback(this MidiFile midiFile, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            return GetPlayback(midiFile.GetTrackChunks(), midiFile.GetTempoMap(), playbackSettings);
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events that will be
        /// produced by specified <see cref="Pattern"/>.
        /// </summary>
        /// <param name="pattern"><see cref="Pattern"/> producing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="channel">MIDI channel to play channel events on.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing MIDI events that will be
        /// produced by the <paramref name="pattern"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="pattern"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Playback GetPlayback(this Pattern pattern, TempoMap tempoMap, FourBitNumber channel, IOutputDevice outputDevice, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return pattern.ToTrackChunk(tempoMap, channel).GetPlayback(tempoMap, outputDevice, playbackSettings);
        }

        /// <summary>
        /// Retrieves an instance of the <see cref="Playback"/> for playing MIDI events that will be
        /// produced by specified <see cref="Pattern"/>.
        /// </summary>
        /// <param name="pattern"><see cref="Pattern"/> producing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="channel">MIDI channel to play channel events on.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing MIDI events that will be
        /// produced by the <paramref name="pattern"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="pattern"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Playback GetPlayback(this Pattern pattern, TempoMap tempoMap, FourBitNumber channel, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return pattern.ToTrackChunk(tempoMap, channel).GetPlayback(tempoMap, playbackSettings);
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
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing <paramref name="objects"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static Playback GetPlayback<TObject>(this IEnumerable<TObject> objects, TempoMap tempoMap, IOutputDevice outputDevice, SevenBitNumber programNumber, PlaybackSettings playbackSettings = null)
            where TObject : IMusicalObject, ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            return GetMusicalObjectsPlayback(objects,
                                             tempoMap,
                                             outputDevice,
                                             channel => new[] { new ProgramChangeEvent(programNumber) { Channel = channel } },
                                             playbackSettings);
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
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing <paramref name="objects"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="generalMidiProgram"/> specified an invalid value.</exception>
        public static Playback GetPlayback<TObject>(this IEnumerable<TObject> objects, TempoMap tempoMap, IOutputDevice outputDevice, GeneralMidiProgram generalMidiProgram, PlaybackSettings playbackSettings = null)
            where TObject : IMusicalObject, ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);
            ThrowIfArgument.IsInvalidEnumValue(nameof(generalMidiProgram), generalMidiProgram);

            return GetMusicalObjectsPlayback(objects,
                                             tempoMap,
                                             outputDevice,
                                             channel => new[] { generalMidiProgram.GetProgramEvent(channel) },
                                             playbackSettings);
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
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <returns>An instance of the <see cref="Playback"/> for playing <paramref name="objects"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="generalMidi2Program"/> specified an invalid value.</exception>
        public static Playback GetPlayback<TObject>(this IEnumerable<TObject> objects, TempoMap tempoMap, IOutputDevice outputDevice, GeneralMidi2Program generalMidi2Program, PlaybackSettings playbackSettings = null)
            where TObject : IMusicalObject, ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);
            ThrowIfArgument.IsInvalidEnumValue(nameof(generalMidi2Program), generalMidi2Program);

            return GetMusicalObjectsPlayback(objects,
                                             tempoMap,
                                             outputDevice,
                                             channel => generalMidi2Program.GetProgramEvents(channel),
                                             playbackSettings);
        }

        /// <summary>
        /// Plays MIDI events contained in the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> containing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void Play(this TrackChunk trackChunk, TempoMap tempoMap, IOutputDevice outputDevice, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            using (var playback = trackChunk.GetPlayback(tempoMap, outputDevice, playbackSettings))
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
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void Play(this IEnumerable<TrackChunk> trackChunks, TempoMap tempoMap, IOutputDevice outputDevice, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            using (var playback = trackChunks.GetPlayback(tempoMap, outputDevice, playbackSettings))
            {
                playback.Play();
            }
        }

        /// <summary>
        /// Plays MIDI events contained in the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> containing events to play.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void Play(this MidiFile midiFile, IOutputDevice outputDevice, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            midiFile.GetTrackChunks().Play(midiFile.GetTempoMap(), outputDevice, playbackSettings);
        }

        /// <summary>
        /// Plays MIDI events that will be produced by specified <see cref="Pattern"/>.
        /// </summary>
        /// <param name="pattern"><see cref="Pattern"/> producing events to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="channel">MIDI channel to play channel events on.</param>
        /// <param name="outputDevice">Output MIDI device to play events through.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="pattern"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void Play(this Pattern pattern, TempoMap tempoMap, FourBitNumber channel, IOutputDevice outputDevice, PlaybackSettings playbackSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(pattern), pattern);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            pattern.ToTrackChunk(tempoMap, channel).Play(tempoMap, outputDevice, playbackSettings);
        }

        /// <summary>
        /// Plays musical objects using the specified program.
        /// </summary>
        /// <typeparam name="TObject">The type of objects to play.</typeparam>
        /// <param name="objects">Objects to play.</param>
        /// <param name="tempoMap">Tempo map used to calculate events times.</param>
        /// <param name="outputDevice">Output MIDI device to play <paramref name="objects"/> through.</param>
        /// <param name="programNumber">Program that should be used to play <paramref name="objects"/>.</param>
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void Play<TObject>(this IEnumerable<TObject> objects, TempoMap tempoMap, IOutputDevice outputDevice, SevenBitNumber programNumber, PlaybackSettings playbackSettings = null)
            where TObject : IMusicalObject, ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);

            using (var playback = objects.GetPlayback(tempoMap, outputDevice, programNumber, playbackSettings))
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
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="generalMidiProgram"/> specified an invalid value.</exception>
        public static void Play<TObject>(this IEnumerable<TObject> objects, TempoMap tempoMap, IOutputDevice outputDevice, GeneralMidiProgram generalMidiProgram, PlaybackSettings playbackSettings = null)
            where TObject : IMusicalObject, ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);
            ThrowIfArgument.IsInvalidEnumValue(nameof(generalMidiProgram), generalMidiProgram);

            using (var playback = objects.GetPlayback(tempoMap, outputDevice, generalMidiProgram, playbackSettings))
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
        /// <param name="playbackSettings">Settings according to which a playback should be created.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputDevice"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="generalMidi2Program"/> specified an invalid value.</exception>
        public static void Play<TObject>(this IEnumerable<TObject> objects, TempoMap tempoMap, IOutputDevice outputDevice, GeneralMidi2Program generalMidi2Program, PlaybackSettings playbackSettings = null)
            where TObject : IMusicalObject, ITimedObject
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfArgument.IsNull(nameof(outputDevice), outputDevice);
            ThrowIfArgument.IsInvalidEnumValue(nameof(generalMidi2Program), generalMidi2Program);

            using (var playback = objects.GetPlayback(tempoMap, outputDevice, generalMidi2Program, playbackSettings))
            {
                playback.Play();
            }
        }

        private static Playback GetMusicalObjectsPlayback<TObject>(IEnumerable<TObject> objects,
                                                                   TempoMap tempoMap,
                                                                   IOutputDevice outputDevice,
                                                                   Func<FourBitNumber, IEnumerable<MidiEvent>> programChangeEventsGetter,
                                                                   PlaybackSettings playbackSettings)
            where TObject : IMusicalObject, ITimedObject
        {
            var programChangeEvents = objects.Select(n => n.Channel)
                                             .Distinct()
                                             .SelectMany(programChangeEventsGetter)
                                             .Select(e => (ITimedObject)new TimedEvent(e));

            return new Playback(programChangeEvents.Concat((IEnumerable<ITimedObject>)objects), tempoMap, outputDevice, playbackSettings);
        }

        #endregion
    }
}
