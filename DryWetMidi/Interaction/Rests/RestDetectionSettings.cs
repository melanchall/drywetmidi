using System;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class RestDetectionSettings
    {
        #region Properties

        public Func<ITimedObject, object> KeySelector { get; set; }

        #endregion
    }
}
