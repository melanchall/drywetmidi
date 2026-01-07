namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class NativeApiUtilities
    {
        public static void HandleDevicesNativeApiResult<TResult>(
            TResult result,
            int errorCode)
        {
            NativeApi.HandleResult(
                result,
                errorCode,
                (message, mainErrorCode, additionalErrorCode) => new MidiDeviceException(message, mainErrorCode, additionalErrorCode));
        }

        public static void HandleTickGeneratorNativeApiResult<TResult>(
            TResult result,
            int errorCode)
        {
            NativeApi.HandleResult(
                result,
                errorCode,
                (message, mainErrorCode, additionalErrorCode) => new TickGeneratorException(message, mainErrorCode, additionalErrorCode));
        }
    }
}
