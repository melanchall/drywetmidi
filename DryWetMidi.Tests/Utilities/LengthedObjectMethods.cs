using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    public abstract class LengthedObjectMethods<TObject>
        where TObject : ILengthedObject
    {
        #region Methods

        public TObject Create(ITimeSpan time, ITimeSpan length, TempoMap tempoMap)
        {
            var convertedTime = TimeConverter.ConvertFrom(time, tempoMap);
            return Create(convertedTime, LengthConverter.ConvertFrom(length, convertedTime, tempoMap));
        }

        public abstract TObject Create(long time, long length);

        #endregion
    }
}
