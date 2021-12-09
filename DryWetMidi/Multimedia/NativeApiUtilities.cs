namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class NativeApiUtilities
    {
        public static void HandleDevicesNativeApiResult<TResult>(TResult result)
        {
            NativeApi.HandleResult(
                result,
                (message, errorCode) => new MidiDeviceException(message, errorCode));
        }

        public static void HandleTickGeneratorNativeApiResult<TResult>(TResult result)
        {
            NativeApi.HandleResult(
                result,
                (message, errorCode) => new TickGeneratorException(message, errorCode));
        }
    }
}
