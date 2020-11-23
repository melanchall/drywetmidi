using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class ChordsBuilder : SequentialObjectsBuilder<ChordsBag, ChordsContext>
    {
        #region Constructors

        public ChordsBuilder(List<ObjectsBag> objectsBags, ObjectsBuildingSettings settings)
            : base(objectsBags, settings)
        {
        }

        #endregion
    }
}
