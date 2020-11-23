using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class TimedEventsBuilder : SequentialObjectsBuilder<TimedEventsBag, TimedEventsContext>
    {
        #region Constructors

        public TimedEventsBuilder(List<ObjectsBag> objectsBags, ObjectsBuildingSettings settings)
            : base(objectsBags, settings)
        {
        }

        #endregion
    }
}
