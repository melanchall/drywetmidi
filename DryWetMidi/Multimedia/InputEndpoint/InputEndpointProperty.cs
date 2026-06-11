namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Additional property attached to an instance of the <see cref="InputEndpoint"/>.
    /// </summary>
    /// <seealso cref="InputEndpoint"/>
    public enum InputEndpointProperty
    {
        /// <summary>
        /// Owner of an input endpoint driver on macOS (see
        /// <see href="https://developer.apple.com/documentation/coremidi/kmidipropertydriverowner">
        /// kMIDIPropertyDriverOwner</see>).
        /// </summary>
        DriverOwner = 4,
    }
}
