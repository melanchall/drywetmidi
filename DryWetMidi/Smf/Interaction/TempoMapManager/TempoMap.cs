using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class TempoMap
    {
        #region Constructor

        public TempoMap(TimeDivision timeDivision)
        {
            if (timeDivision == null)
                throw new ArgumentNullException(nameof(timeDivision));

            TimeDivision = timeDivision;
            TimeSignatureLine = new ValuesLine<TimeSignature>(TimeSignature.Default);
            TempoLine = new ValuesLine<Tempo>(Tempo.Default);
        }

        #endregion

        #region Properties

        public TimeDivision TimeDivision { get; }

        public ValuesLine<TimeSignature> TimeSignatureLine { get; }

        public ValuesLine<Tempo> TempoLine { get; }

        #endregion
    }
}
