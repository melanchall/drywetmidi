using Melanchall.DryWetMidi.Interaction;
using System;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which randomizing should be performed by the <see cref="Quantizer"/>.
    /// </summary>
    public sealed class RandomizingSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets bounds to randomize an object's time within.
        /// </summary>
        public IBounds Bounds { get; set; }

        /// <summary>
        /// Gets or sets a predicate to filter objects that should be randomized. Use <c>null</c> if
        /// all objects should be processed.
        /// </summary>
        public Predicate<ITimedObject> Filter { get; set; }

        #endregion
    }
}
