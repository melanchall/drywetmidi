using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Specifies settings which control how MIDI files should be merged in sequence.
    /// </summary>
    /// <seealso cref="Merger.MergeSequentially(System.Collections.Generic.IEnumerable{MidiFile}, SequentialMergingSettings)"/>
    public sealed class SequentialMergingSettings
    {
        #region Fields

        private ResultTrackChunksCreationPolicy _resultTrackChunksCreationPolicy = ResultTrackChunksCreationPolicy.CreateForEachFile;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a delay which should be added between files. The default value is
        /// <c>null</c> which means no delay will be added.
        /// </summary>
        public ITimeSpan DelayBetweenFiles { get; set; }

        /// <summary>
        /// Gets or sets a step which should be used to round an input file duration. Calculated
        /// duration affects an offset that will be applied to the data of a next file. The default
        /// value is <c>null</c> which means no rounding should be applied. Note that only rounding
        /// up is supported. See <see cref="TimeSpanRoundingPolicy.RoundUp"/> for more info.
        /// </summary>
        public ITimeSpan FileDurationRoundingStep { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether non-track chunks (like <see cref="UnknownChunk"/> instances
        /// or <see href="xref:a_custom_chunk">custom ones</see>) should be copied to the result file
        /// or not. The default value is <c>true</c>.
        /// </summary>
        public bool CopyNonTrackChunks { get; set; } = true;

        /// <summary>
        /// Gets or sets a factory method to create event that will be placed at the start
        /// of an input MIDI file within the result one.
        /// </summary>
        public Func<MidiFile, MidiEvent> FileStartMarkerEventFactory { get; set; }

        /// <summary>
        /// Gets or sets a factory method to create event that will be placed at the end
        /// of an input MIDI file within the result one.
        /// </summary>
        public Func<MidiFile, MidiEvent> FileEndMarkerEventFactory { get; set; }

        /// <summary>
        /// Gets or sets a strategy for result track chunks creation when merging MIDI files sequentially. The
        /// default value is <see cref="ResultTrackChunksCreationPolicy.CreateForEachFile"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
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
