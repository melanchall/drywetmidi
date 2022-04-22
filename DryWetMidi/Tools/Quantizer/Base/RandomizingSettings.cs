using Melanchall.DryWetMidi.Interaction;
using System;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class RandomizingSettings
    {
        #region Properties

        public IBounds Bounds { get; set; }

        public Predicate<ITimedObject> Filter { get; set; }

        #endregion
    }
}
