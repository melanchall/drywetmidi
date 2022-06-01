using Melanchall.DryWetMidi.Interaction;
using System;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class SplitByObjectsSettings
    {
        #region Properties

        public Predicate<ITimedObject> Filter { get; set; }

        public Predicate<ITimedObject> AllFilesObjectsFilter { get; set; }

        #endregion
    }
}
