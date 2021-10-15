namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Additional property attached to an instance of the <see cref="InputDevice"/>.
    /// </summary>
    /// <seealso cref="InputDevice"/>
    public enum InputDeviceProperty
    {
        /// <summary>
        /// Product/model name (see <c>wPid</c> field
        /// description in <see href="https://docs.microsoft.com/en-us/windows/win32/api/mmeapi/ns-mmeapi-midiincaps">
        /// MIDIINCAPS</see> on Windows; see <see href="https://developer.apple.com/documentation/coremidi/kmidipropertymodel">
        /// kMIDIPropertyModel</see> on macOS).
        /// </summary>
        Product = 0,

        /// <summary>
        /// Manufacturer of an input device (see <c>wMid</c> field
        /// description in <see href="https://docs.microsoft.com/en-us/windows/win32/api/mmeapi/ns-mmeapi-midiincaps">
        /// MIDIINCAPS</see> on Windows; see <see href="https://developer.apple.com/documentation/coremidi/kmidipropertymanufacturer">
        /// kMIDIPropertyManufacturer</see> on macOS).
        /// </summary>
        Manufacturer = 1,

        /// <summary>
        /// Version of an input device driver (see <c>vDriverVersion</c> field
        /// description in <see href="https://docs.microsoft.com/en-us/windows/win32/api/mmeapi/ns-mmeapi-midiincaps">
        /// MIDIINCAPS</see> on Windows; see <see href="https://developer.apple.com/documentation/coremidi/kmidipropertydriverversion">
        /// kMIDIPropertyDriverVersion</see> on macOS).
        /// </summary>
        DriverVersion = 2,

        /// <summary>
        /// Unique identifier of an input device on macOS (see
        /// <see href="https://developer.apple.com/documentation/coremidi/kmidipropertyuniqueid">
        /// kMIDIPropertyUniqueID</see>).
        /// </summary>
        UniqueId = 3,

        /// <summary>
        /// Owner of an input device driver on macOS (see
        /// <see href="https://developer.apple.com/documentation/coremidi/kmidipropertydriverowner">
        /// kMIDIPropertyDriverOwner</see>).
        /// </summary>
        DriverOwner = 4,
    }
}
