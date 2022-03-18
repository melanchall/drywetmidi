using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    public abstract class TimedObjectMethods<TObject>
        where TObject : ITimedObject
    {
        #region Methods

        public void SetTime(TObject obj, ITimeSpan time, TempoMap tempoMap)
        {
            SetTime(obj, TimeConverter.ConvertFrom(time, tempoMap));
        }

        public TObject Clone(TObject obj)
        {
            return (TObject)obj.Clone();
        }

        public abstract void SetTime(TObject obj, long time);

        #endregion
    }
}
