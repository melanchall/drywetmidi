using System;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Provides callbacks to handle MIDI data reading stages.
    /// </summary>
    public abstract class ReadingHandler
    {
        #region Enums

        /// <summary>
        /// Scope of MIDI data to handle.
        /// </summary>
        [Flags]
        public enum TargetScope
        {
            /// <summary>
            /// Handle file-level operations.
            /// </summary>
            File = 1,

            /// <summary>
            /// Handle track chunk-level operations.
            /// </summary>
            TrackChunk = 2,

            /// <summary>
            /// Handle MIDI event-level operations.
            /// </summary>
            Event = 4
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadingHandler"/> with the specified scope of
        /// MIDI data to handle.
        /// </summary>
        /// <param name="scope">Scope of MIDI data to handle.</param>
        /// <remarks>
        /// It's important to set desired scope to avoid performance degradation. For example, if you want to handle
        /// start of file reading and start of track chunk reading, specify <paramref name="scope"/> to
        /// <c>TargetScope.File | TargetScope.TrackChunk</c>.
        /// </remarks>
        public ReadingHandler(TargetScope scope)
        {
            Scope = scope;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the scope of MIDI data to handle.
        /// </summary>
        public TargetScope Scope { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes handler. This method will be called before reading MIDI data.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Handles start of file reading. Called before file reading.
        /// </summary>
        /// <remarks>
        /// This method called within <see cref="TargetScope.File"/> scope.
        /// </remarks>
        public virtual void OnStartFileReading()
        {
        }

        /// <summary>
        /// Handles finish of file reading. Called after file is read.
        /// </summary>
        /// <param name="midiFile">MIDI file read.</param>
        /// <remarks>
        /// This method called within <see cref="TargetScope.File"/> scope.
        /// </remarks>
        public virtual void OnFinishFileReading(MidiFile midiFile)
        {
        }

        /// <summary>
        /// Handles finish of header chunk reading. Called after header chunk is read.
        /// </summary>
        /// <param name="timeDivision">Time division of the file is being read.</param>
        /// <remarks>
        /// This method called within <see cref="TargetScope.File"/> scope.
        /// </remarks>
        public virtual void OnFinishHeaderChunkReading(TimeDivision timeDivision)
        {
        }

        /// <summary>
        /// Handles start of track chunk reading. Called before track chunk reading.
        /// </summary>
        /// <remarks>
        /// This method called within <see cref="TargetScope.TrackChunk"/> scope.
        /// </remarks>
        public virtual void OnStartTrackChunkReading()
        {
        }

        /// <summary>
        /// Handles start of track chunk's content reading. Called after track chunk header is read.
        /// </summary>
        /// <param name="trackChunk">Track chunk is being read.</param>
        /// <remarks>
        /// This method called within <see cref="TargetScope.TrackChunk"/> scope.
        /// </remarks>
        public virtual void OnStartTrackChunkContentReading(TrackChunk trackChunk)
        {
        }

        /// <summary>
        /// Handles finish of track chunk reading. Called after track chunk is read.
        /// </summary>
        /// <param name="trackChunk">Track chunk read.</param>
        /// <remarks>
        /// This method called within <see cref="TargetScope.TrackChunk"/> scope.
        /// </remarks>
        public virtual void OnFinishTrackChunkReading(TrackChunk trackChunk)
        {
        }

        /// <summary>
        /// Handles finish of MIDI event reading. Called after MIDI event is read and before
        /// putting it to <see cref="TrackChunk.Events"/> collection.
        /// </summary>
        /// <param name="midiEvent">MIDI event read.</param>
        /// <param name="absoluteTime">Absolute time of <paramref name="midiEvent"/>.</param>
        /// <remarks>
        /// This method called within <see cref="TargetScope.Event"/> scope.
        /// </remarks>
        public virtual void OnFinishEventReading(MidiEvent midiEvent, long absoluteTime)
        {
        }

        #endregion
    }
}
