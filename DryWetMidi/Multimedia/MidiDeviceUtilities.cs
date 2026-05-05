using Melanchall.DryWetMidi.Common;
using System.Diagnostics.CodeAnalysis;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class MidiDeviceUtilities
    {
        #region Methods

        public static void HandleDevicesNativeApiResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.Interfaces)] TResult>(
            TResult result,
            int errorCode)
        {
            NativeApi.HandleResult(
                result,
                errorCode,
                (message, mainErrorCode, additionalErrorCode) => new MidiDeviceException(message, mainErrorCode, additionalErrorCode));
        }

        #endregion
    }
}
