namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class InputEndpointCheckpointsNames
    {
        #region Constants

        public const string ReleaseHandleEntered = "IN A";
        public const string DisconnectDeviceExecutedInReleaseHandle = "IN B";
        public const string DisconnectDeviceSuccessInReleaseHandle = "IN B SUCCESS";
        public const string CloseDeviceExecutedInReleaseHandle = "IN C";
        public const string CloseDeviceSuccessInReleaseHandle = "IN C SUCCESS";

        public const string ReleaseInfoHandleEntered = "IN A 2";
        public const string InfoDeletedInReleaseInfoHandle = "IN B 2";

        public const string MessageDataReceived = "MSG DATA";

        #endregion
    }
}
