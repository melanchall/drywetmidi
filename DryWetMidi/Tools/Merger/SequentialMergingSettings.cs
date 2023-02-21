using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class SequentialMergingSettings
    {
        #region Fields

        private ResultTrackChunksCreationPolicy _resultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.CreateForEachFile;

        #endregion

        #region Properties

        public ITimeSpan DelayBetweenFiles { get; set; }

        public ITimeSpan FileDurationRoundingStep { get; set; }

        public bool CopyNonTrackChunks { get; set; } = true;

        public Func<MidiFile, MidiEvent> FileStartMarkerEventFactory { get; set; }

        public Func<MidiFile, MidiEvent> FileEndMarkerEventFactory { get; set; }

        public ResultTrackChunksCreationPolicy ResultTrackChunksCreationPolicy
        {
            get { return _resultTrackChunksCreationPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _resultTrackChunksCreationPolicy = value;
            }
        }

        #endregion
    }
}
