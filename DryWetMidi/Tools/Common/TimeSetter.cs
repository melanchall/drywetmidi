using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class TimeSetter
    {
        #region Constants

        private static readonly Dictionary<Type, Action<ITimedObject, long>> TimeSetters =
            new Dictionary<Type, Action<ITimedObject, long>>
            {
                [typeof(TimedEvent)] = (obj, time) => ((TimedEvent)obj).Time = time,
                [typeof(Note)] = (obj, time) => ((Note)obj).Time = time,
                [typeof(Chord)] = (obj, time) => ((Chord)obj).Time = time
            };

        #endregion

        #region Methods

        public static void SetObjectTime<TObject>(TObject obj, long time)
            where TObject : ITimedObject
        {
            TimeSetters[obj.GetType()](obj, time);
        }

        #endregion
    }
}
