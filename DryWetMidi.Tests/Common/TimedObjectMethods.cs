using System.Collections;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tests.Common
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
            obj.Time = TimeConverter.ConvertFrom(time, tempoMap);
        }

        public abstract TObject Create(long time, long length);

        public abstract TObject Clone(TObject obj);

        #endregion
    }
}
