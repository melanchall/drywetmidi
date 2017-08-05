using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal enum AnchorPosition
    {
        First,
        Last,
        Nth
    }

    internal sealed class MoveToAnchorAction : IPatternAction
    {
        #region Constructor

        public MoveToAnchorAction(AnchorPosition position)
            : this(null, position)
        {
        }

        public MoveToAnchorAction(object anchor, AnchorPosition position)
            : this(anchor, position, -1)
        {
        }

        public MoveToAnchorAction(AnchorPosition position, int index)
            : this(null, position, index)
        {
        }

        public MoveToAnchorAction(object anchor, AnchorPosition position, int index)
        {
            Anchor = anchor;
            AnchorPosition = position;
            Index = index;

        }

        #endregion

        #region Properties

        public object Anchor { get; }

        public AnchorPosition AnchorPosition { get; }

        public int Index { get; }

        #endregion

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            var anchorTimes = context.GetAnchorTimes(Anchor);
            var newTime = 0L;

            switch (AnchorPosition)
            {
                case AnchorPosition.First:
                    newTime = anchorTimes.First();
                    break;
                case AnchorPosition.Last:
                    newTime = anchorTimes.Last();
                    break;
                case AnchorPosition.Nth:
                    newTime = anchorTimes[Index];
                    break;
            }

            return new PatternActionResult(newTime);
        }

        #endregion
    }
}
