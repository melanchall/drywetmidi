using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    public interface IValueLineSource<TValue> : IEnumerable<ValueChange<TValue>>
    {
        TValue DefaultValue { get; }

        TValue GetValueAtTime(long time);

        void SetValue(long time, TValue value);

        void DeleteValues(long startTime, long endTime);

        void Clear();

        IValueLineSource<TValue> Clone();

        int GetValueChangesCount();

        void Add(ValueChange<TValue> valueChange);
    }
}
