namespace Melanchall.DryWetMidi.Composing
{
    internal abstract class PatternAction
    {
        #region Properties

        public PatternActionState State { get; set; }

        #endregion

        #region Methods

        public abstract PatternActionResult Invoke(long time, PatternContext context);

        public abstract PatternAction Clone();

        #endregion
    }
}
