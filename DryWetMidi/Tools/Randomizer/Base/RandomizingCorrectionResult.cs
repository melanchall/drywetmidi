namespace Melanchall.DryWetMidi.Tools
{
    public sealed class RandomizingCorrectionResult
    {
        #region Constants

        public static readonly RandomizingCorrectionResult Skip = new RandomizingCorrectionResult(RandomizingInstruction.Skip, InvalidTime);

        private const long InvalidTime = -1;

        #endregion

        #region Constructor

        public RandomizingCorrectionResult(RandomizingInstruction randomizingInstruction, long time)
        {
            RandomizingInstruction = randomizingInstruction;
            Time = time;
        }

        #endregion

        #region Properties

        public RandomizingInstruction RandomizingInstruction { get; }

        public long Time { get; }

        #endregion
    }
}
