using System;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which nearby notes should be merged.
    /// </summary>
    public sealed class NotesMergingSettings
    {
        #region Fields

        private VelocityMergingPolicy _velocityMergingPolicy = VelocityMergingPolicy.First;
        private VelocityMergingPolicy _offVelocityMergingPolicy = VelocityMergingPolicy.Last;

        private ITimeSpan _tolerance = new MidiTimeSpan();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a policy which determines how <see cref="Note.Velocity"/> of notes should be merged.
        /// The default value is <see cref="VelocityMergingPolicy.First"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public VelocityMergingPolicy VelocityMergingPolicy
        {
            get { return _velocityMergingPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _velocityMergingPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets a policy which determines how <see cref="Note.OffVelocity"/> of notes should be merged.
        /// The default value is <see cref="VelocityMergingPolicy.Last"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public VelocityMergingPolicy OffVelocityMergingPolicy
        {
            get { return _offVelocityMergingPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _offVelocityMergingPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets maximum distance between two notes to consider them as nearby. The default value
        /// is time span of zero length.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
        public ITimeSpan Tolerance
        {
            get { return _tolerance; }
            set
            {
                ThrowIfArgument.IsNull(nameof(value), value);

                _tolerance = value;
            }
        }

        /// <summary>
        /// Gets or sets a predicate to filter notes out. If predicate returns <c>true</c>,
        /// a note will be processed; if <c>false</c> - it won't. If the property set to <c>null</c>,
        /// all notes will be processed.
        /// </summary>
        public Predicate<Note> Filter { get; set; }

        /// <summary>
        /// Gets or sets settings which define how notes should be detected and built. You can set it to
        /// <c>null</c> to use default settings.
        /// </summary>
        public NoteDetectionSettings NoteDetectionSettings { get; set; } = new NoteDetectionSettings();

        #endregion
    }
}
