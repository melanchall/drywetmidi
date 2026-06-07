namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class InputEndpointCheckpointsNames
    {
        #region Constants

        public const string ReleaseHandleEntered = "IN A";
        public const string DisconnectEndpointExecutedInReleaseHandle = "IN B";
        public const string DisconnectEndpointSuccessInReleaseHandle = "IN B SUCCESS";
        public const string CloseEndpointExecutedInReleaseHandle = "IN C";
        public const string CloseEndpointSuccessInReleaseHandle = "IN C SUCCESS";

        public const string ReleaseInfoHandleEntered = "IN A 2";
        public const string InfoDeletedInReleaseInfoHandle = "IN B 2";

        public const string MessageDataReceived = "MSG DATA";

        #endregion
    }
}
