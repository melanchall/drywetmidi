using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    public sealed class UnknownChannelEventAction
    {
        #region Constants

        public static readonly UnknownChannelEventAction Abort = new UnknownChannelEventAction(UnknownChannelEventInstruction.Abort, 0);

        #endregion

        #region Constructor

        private UnknownChannelEventAction(UnknownChannelEventInstruction instruction, int dataBytesToSkip)
        {
            Instruction = instruction;
            DataBytesToSkip = dataBytesToSkip;
        }

        #endregion

        #region Properties

        public UnknownChannelEventInstruction Instruction { get; }

        public int DataBytesToSkip { get; }

        #endregion

        #region Methods

        public static UnknownChannelEventAction SkipData(int dataBytesToSkip)
        {
            ThrowIfArgument.IsNegative(nameof(dataBytesToSkip), dataBytesToSkip, "Count of data bytes to skip is negative.");

            return new UnknownChannelEventAction(UnknownChannelEventInstruction.SkipData, dataBytesToSkip);
        }

        #endregion
    }
}
