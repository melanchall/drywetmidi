using System;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Settings according to which objects should be randomized.
    /// </summary>
    public abstract class RandomizingSettings<TObject>
    {
        #region Properties

        public Predicate<TObject> Filter { get; set; }

        #endregion
    }
}
