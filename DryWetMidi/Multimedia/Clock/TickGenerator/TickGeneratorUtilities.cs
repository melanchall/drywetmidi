using Melanchall.DryWetMidi.Common;
using System.Diagnostics.CodeAnalysis;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class TickGeneratorUtilities
    {
        #region Methods

        public static void HandleTickGeneratorNativeApiResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.Interfaces)] TResult>(
            TResult result,
            int errorCode)
        {
            NativeApi.HandleResult(
                result,
                errorCode,
                (message, mainErrorCode, additionalErrorCode) => new TickGeneratorException(message, mainErrorCode, additionalErrorCode));
        }

        #endregion
    }
}
