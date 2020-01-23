using System;

namespace Melanchall.DryWetMidi.Core
{
    public abstract class ReadingHandler
    {
        #region Enums

        [Flags]
        public enum TargetScope
        {
            File = 1,
            TrackChunk = 2,
            Event = 4
        }

        #endregion

        #region Constructor

        public ReadingHandler(TargetScope scope)
        {
            Scope = scope;
        }

        #endregion

        #region Properties

        public TargetScope Scope { get; }

        #endregion

        #region Methods

        public virtual void Initialize()
        {
        }

        public virtual void OnStartFileReading()
        {
        }

        public virtual void OnFinishFileReading(MidiFile midiFile)
        {
        }

        public virtual void OnFinishHeaderChunkReading(TimeDivision timeDivision)
        {
        }

        public virtual void OnStartTrackChunkReading()
        {
        }

        public virtual void OnStartTrackChunkContentReading(TrackChunk trackChunk)
        {
        }

        public virtual void OnFinishTrackChunkReading(TrackChunk trackChunk)
        {
        }

        public virtual void OnFinishEventReading(MidiEvent midiEvent, long absoluteTime)
        {
        }

        #endregion
    }
}
