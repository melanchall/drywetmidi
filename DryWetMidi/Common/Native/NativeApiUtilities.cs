using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Common
{
    internal static class NativeApiUtilities
    {
        #region Methods

        public static bool IsOsSupported() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        // TODO: add exception info in triple-slash comments
        public static void EnsureOsIsSupported()
        {
            if (IsOsSupported())
                return;

            throw new FeatureNotAvailableException("Current operating system is not supported.");
        }

        public static void HandleEndpointNativeApiResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.Interfaces)] TResult>(
            TResult result,
            int errorCode)
        {
            NativeApi.HandleResult(
                result,
                errorCode,
                (message, mainErrorCode, additionalErrorCode) => new NativeApiException(message, mainErrorCode, additionalErrorCode));
        }

        #endregion
    }
}
