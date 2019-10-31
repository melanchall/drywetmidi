using System;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which objects should be randomized.
    /// </summary>
    public abstract class RandomizingSettings<TObject>
    {
        #region Properties

        /// <summary>
        /// Gets or sets a predicate to filter objects that should be randomized. Use null if
        /// all objects should be processed.
        /// </summary>
        public Predicate<TObject> Filter { get; set; }

        #endregion
    }
}
