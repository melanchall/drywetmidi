namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Additional property attached to an instance of the <see cref="OutputDevice"/>.
    /// </summary>
    /// <seealso cref="OutputDevice"/>
    /// <see cref="OutputDeviceTechnology"/>
    /// <see cref="OutputDeviceOption"/>
    public enum OutputDeviceProperty
    {
        /// <summary>
        /// Product/model name (see <c>wPid</c> field
        /// description in <see href="https://docs.microsoft.com/en-us/windows/win32/api/mmeapi/ns-mmeapi-midioutcaps">
        /// MIDIOUTCAPS</see> on Windows; see <see href="https://developer.apple.com/documentation/coremidi/kmidipropertymodel">
        /// kMIDIPropertyModel</see> on macOS).
        /// </summary>
        Product = 0,

        /// <summary>
        /// Manufacturer of an output device (see <c>wMid</c> field
        /// description in <see href="https://docs.microsoft.com/en-us/windows/win32/api/mmeapi/ns-mmeapi-midioutcaps">
        /// MIDIOUTCAPS</see> on Windows; see <see href="https://developer.apple.com/documentation/coremidi/kmidipropertymanufacturer">
        /// kMIDIPropertyManufacturer</see> on macOS).
        /// </summary>
        Manufacturer = 1,

        /// <summary>
        /// Version of an output device driver (see <c>vDriverVersion</c> field
        /// description in <see href="https://docs.microsoft.com/en-us/windows/win32/api/mmeapi/ns-mmeapi-midioutcaps">
        /// MIDIOUTCAPS</see> on Windows; see <see href="https://developer.apple.com/documentation/coremidi/kmidipropertydriverversion">
        /// kMIDIPropertyDriverVersion</see> on macOS).
        /// </summary>
        DriverVersion = 2,

        /// <summary>
        /// Type of an output device on Windows (see <c>wTechnology</c> field
        /// description in <see href="https://docs.microsoft.com/en-us/windows/win32/api/mmeapi/ns-mmeapi-midioutcaps">
        /// MIDIOUTCAPS</see>).
        /// </summary>
        Technology = 3,

        /// <summary>
        /// Unique identifier of an output device on macOS (see
        /// <see href="https://developer.apple.com/documentation/coremidi/kmidipropertyuniqueid">
        /// kMIDIPropertyUniqueID</see>).
        /// </summary>
        UniqueId = 4,

        /// <summary>
        /// Number of voices supported by an internal synthesizer device on Windows (see <c>wVoices</c> field
        /// description in <see href="https://docs.microsoft.com/en-us/windows/win32/api/mmeapi/ns-mmeapi-midioutcaps">
        /// MIDIOUTCAPS</see>).
        /// </summary>
        VoicesNumber = 5,

        /// <summary>
        /// Maximum number of simultaneous notes that can be played by an internal synthesizer device
        /// on Windows (see <c>wNotes</c> field description in
        /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/mmeapi/ns-mmeapi-midioutcaps">MIDIOUTCAPS</see>).
        /// </summary>
        NotesNumber = 6,

        /// <summary>
        /// Channels that an internal synthesizer device responds to on Windows (see <c>wChannelMask</c> field
        /// description in <see href="https://docs.microsoft.com/en-us/windows/win32/api/mmeapi/ns-mmeapi-midioutcaps">
        /// MIDIOUTCAPS</see>).
        /// </summary>
        Channels = 7,

        /// <summary>
        /// Optional functionality supported by the device on Windows (see <c>dwSupport</c> field
        /// description in <see href="https://docs.microsoft.com/en-us/windows/win32/api/mmeapi/ns-mmeapi-midioutcaps">
        /// MIDIOUTCAPS</see>).
        /// </summary>
        Options = 8,

        /// <summary>
        /// Owner of an output device driver on macOS (see
        /// <see href="https://developer.apple.com/documentation/coremidi/kmidipropertydriverowner">
        /// kMIDIPropertyDriverOwner</see>).
        /// </summary>
        DriverOwner = 9,
    }
}
