using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which merging should be performed by the <see cref="Merger"/>.
    /// More info in the <see href="xref:a_merger">Merger</see> article.
    /// </summary>
    /// <seealso cref="Merger"/>
    public class ObjectsMergingSettings
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
        /// Gets or sets maximum distance between two objects to consider them as nearby. The default value
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
        /// Gets or sets a predicate to filter objects out. If predicate returns <c>true</c>,
        /// an object will be processed; if <c>false</c> - it won't. If the property set to <c>null</c>,
        /// (default value) all objects will be processed.
        /// </summary>
        public Predicate<ITimedObject> Filter { get; set; }

        /// <summary>
        /// Gets or sets a factory method to create objects merger (see <see cref="ObjectsMerger"/>) to
        /// implement custom merging logic.
        /// </summary>
        public Func<ILengthedObject, ObjectsMerger> ObjectsMergerFactory { get; set; }

        #endregion
    }
}
