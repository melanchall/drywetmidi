using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class RegisteredParametersBuilder : SequentialObjectsBuilder<RegisteredParametersBag, RegisteredParametersContext>
    {
        #region Constructors

        public RegisteredParametersBuilder(List<ObjectsBag> objectsBags, ObjectsBuildingSettings settings)
            : base(objectsBags, settings)
        {
        }

        #endregion
    }
}
