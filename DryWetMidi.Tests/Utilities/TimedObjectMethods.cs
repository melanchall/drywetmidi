using System.Collections;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    public abstract class TimedObjectMethods<TObject>
        where TObject : ITimedObject
    {
        #region Properties

        protected abstract IComparer Comparer { get; }

        #endregion

        #region Methods

        public void SetTime(TObject obj, ITimeSpan time, TempoMap tempoMap)
        {
            SetTime(obj, TimeConverter.ConvertFrom(time, tempoMap));
        }

        public abstract void SetTime(TObject obj, long time);

        public abstract TObject Clone(TObject obj);

        #endregion
    }
}
