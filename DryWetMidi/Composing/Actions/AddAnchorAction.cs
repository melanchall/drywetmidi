namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class AddAnchorAction : PatternAction
    {
        #region Constructor

        public AddAnchorAction()
            : this(null)
        {
        }

        public AddAnchorAction(object anchor)
        {
            Anchor = anchor;
        }

        #endregion

        #region Properties

        public object Anchor { get; }

        #endregion

        #region Overrides

        public override PatternActionResult Invoke(long time, PatternContext context)
        {
            if (State == PatternActionState.Enabled)
                context.AnchorTime(Anchor, time);

            return PatternActionResult.DoNothing;
        }

        public override PatternAction Clone()
        {
            return new AddAnchorAction(Anchor);
        }

        #endregion
    }
}
