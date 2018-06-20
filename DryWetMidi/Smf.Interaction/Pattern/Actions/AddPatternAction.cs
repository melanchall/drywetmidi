namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class AddPatternAction : IPatternAction
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

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            context.SaveTime(time);

            var newContext = new PatternContext(context.TempoMap, context.Channel);
            return Pattern.InvokeActions(time, newContext);
        }

        #endregion
    }
}
