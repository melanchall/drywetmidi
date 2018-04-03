namespace Melanchall.DryWetMidi.Tools
{
    public sealed class QuantizingCorrectionResult
    {
        #region Constants

        public static readonly QuantizingCorrectionResult Skip = new QuantizingCorrectionResult(QuantizingInstruction.Skip, InvalidTime);
        public static readonly QuantizingCorrectionResult UseNextGridPoint = new QuantizingCorrectionResult(QuantizingInstruction.UseNextGridPoint, InvalidTime);

        private const long InvalidTime = -1;

        #endregion

        #region Constructor

        public QuantizingCorrectionResult(QuantizingInstruction quantizingInstruction, long time)
        {
            QuantizingInstruction = quantizingInstruction;
            Time = time;
        }

        #endregion

        #region Properties

        public QuantizingInstruction QuantizingInstruction { get; }

        public long Time { get; }

        #endregion
    }
}
