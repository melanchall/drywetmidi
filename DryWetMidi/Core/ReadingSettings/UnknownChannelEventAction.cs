using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// An action that should be done in case of unknown channel event.
    /// </summary>
    public sealed class UnknownChannelEventAction
    {
        #region Constants

        /// <summary>
        /// Abort reading of MIDI data and throw <see cref="UnknownChannelEventException"/>.
        /// </summary>
        public static readonly UnknownChannelEventAction Abort = new UnknownChannelEventAction(UnknownChannelEventInstruction.Abort, 0);

        #endregion

        #region Constructor

        private UnknownChannelEventAction(UnknownChannelEventInstruction instruction, int dataBytesToSkipCount)
        {
            Instruction = instruction;
            DataBytesToSkipCount = dataBytesToSkipCount;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets instruction for reading engine to react on unknown channel event.
        /// </summary>
        public UnknownChannelEventInstruction Instruction { get; }

        /// <summary>
        /// Gets count of data bytes to skip be reading engine. Data bytes are event bytes without status byte.
        /// </summary>
        public int DataBytesToSkipCount { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of the <see cref="UnknownChannelEventAction"/> to skip data bytes of
        /// unknown channel event.
        /// </summary>
        /// <param name="dataBytesToSkipCount">Count of data bytes to skip be reading engine.
        /// Data bytes are event bytes without status byte.</param>
        /// <returns>an instance of the <see cref="UnknownChannelEventAction"/> to skip data bytes of
        /// unknown channel event.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dataBytesToSkipCount"/> is negative.</exception>
        public static UnknownChannelEventAction SkipData(int dataBytesToSkipCount)
        {
            ThrowIfArgument.IsNegative(nameof(dataBytesToSkipCount), dataBytesToSkipCount, "Count of data bytes to skip is negative.");

            return new UnknownChannelEventAction(UnknownChannelEventInstruction.SkipData, dataBytesToSkipCount);
        }

        #endregion
    }
}
