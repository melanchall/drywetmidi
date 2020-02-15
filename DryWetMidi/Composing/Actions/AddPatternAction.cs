namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class AddPatternAction : PatternAction
    {
        #region Constructor

        public AddPatternAction(Pattern pattern)
        {
            Pattern = pattern;
        }

        #endregion

        #region Properties

        public Pattern Pattern { get; }

        #endregion

        #region Overrides

        public override PatternActionResult Invoke(long time, PatternContext context)
        {
            context.SaveTime(time);

            var newContext = new PatternContext(context.TempoMap, context.Channel);
            return Pattern.InvokeActions(time, newContext);
        }

        public override PatternAction Clone()
        {
            return new AddPatternAction(Pattern.Clone());
        }

        #endregion
    }
}
