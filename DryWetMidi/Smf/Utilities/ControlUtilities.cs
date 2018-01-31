using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Set of extension methods for <see cref="ControlChangeEvent"/> event.
    /// </summary>
    public static class ControlUtilities
    {
        #region Methods

        /// <summary>
        /// Gets name of the controller presented by an instance of <see cref="ControlChangeEvent"/>.
        /// </summary>
        /// <param name="controlChangeEvent">Control Change event to get controller name of.</param>
        /// <returns>Controller name of the <paramref name="controlChangeEvent"/> event.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controlChangeEvent"/> is null.</exception>
        public static ControlName GetControlName(this ControlChangeEvent controlChangeEvent)
        {
            ThrowIfArgument.IsNull(nameof(controlChangeEvent), controlChangeEvent);

            return GetControlName(controlChangeEvent.ControlNumber);
        }

        public static SevenBitNumber AsSevenBitNumber(this ControlName controlName)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(controlName), controlName);

            return (SevenBitNumber)(byte)controlName;
        }

        public static ControlChangeEvent GetControlChangeEvent(this ControlName controlName, SevenBitNumber controlValue)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(controlName), controlName);

            return new ControlChangeEvent(controlName.AsSevenBitNumber(), controlValue);
        }

        /// <summary>
        /// Gets name of the controller presented by control number.
        /// </summary>
        /// <param name="controlNumber">Control number to get controller name of.</param>
        /// <returns>Name of the controller presented by <paramref name="controlNumber"/>.</returns>
        private static ControlName GetControlName(SevenBitNumber controlNumber)
        {
            return Enum.IsDefined(typeof(ControlName), controlNumber)
                ? (ControlName)(byte)controlNumber
                : ControlName.Undefined;
        }

        #endregion
    }
}
