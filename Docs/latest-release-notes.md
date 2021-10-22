# Devices API available for macOS now

DryWetMIDI allows now work with MIDI devices on macOS! More than that, a couple of new classes are available for macOS only:

* [VirtualDevice](https://melanchall.github.io/drywetmidi/articles/devices/Virtual-device.html);
* [DevicesWatcher](https://melanchall.github.io/drywetmidi/articles/devices/Devices-watcher.html).

Also [HighPrecisionTickGenerator](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Devices.HighPrecisionTickGenerator.html) implemented for macOS too so you can now use [Playback](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Devices.Playback.html) with default settings on that platform. Its implementation for macOS is not good (in terms of performance) for now but will be optimized for the next release of the library.

# Breaking changes

This version of the library has following breaking changes:

* `Melanchall.DryWetMidi.Devices` namespace renamed to [Melanchall.DryWetMidi.Multimedia](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.html) to reflect its content more precisely since not only devices are there.
* `DriverManufacturer`, `ProductIdentifier` and `DriverVersion` properties were removed from the [MidiDevice](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Devices.MidiDevice.html) class and replaced by `GetProperty` method [for InputDevice](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.InputDevice.html#Melanchall_DryWetMidi_Multimedia_InputDevice_GetProperty_Melanchall_DryWetMidi_Multimedia_InputDeviceProperty_) and [for OutputDevice](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.OutputDevice.html#Melanchall_DryWetMidi_Multimedia_OutputDevice_GetProperty_Melanchall_DryWetMidi_Multimedia_OutputDeviceProperty_).
* `Channels`, `DeviceType`, `NotesNumber`, `SupportsLeftRightVolumeControl`, `SupportsPatchCaching`, `SupportsVolumeControl`, `VoicesNumber` and `Volume` properties were removed from [OutputDevice](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Common.OutputDevice.html) and replaced by [GetProperty](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.OutputDevice.html#Melanchall_DryWetMidi_Multimedia_OutputDevice_GetProperty_Melanchall_DryWetMidi_Multimedia_OutputDeviceProperty_) method.
* Removed `InvalidSysExEventReceived` and `InvalidShortEventReceived` events from [InputDevice](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.InputDevice.html) and replaced them with [ErrorOccurred](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.MidiDevice.html#Melanchall_DryWetMidi_Multimedia_MidiDevice_ErrorOccurred) one.
* All [obsolete APIs](https://melanchall.github.io/drywetmidi/obsolete/obsolete.html) were removed from the library.

# New features

* Added [VirtualDevice](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.VirtualDevice.html) class which allows create [virtual MIDI devices](https://melanchall.github.io/drywetmidi/articles/devices/Virtual-device.html) on macOS.
* Added `GetProperty` method [for InputDevice](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.InputDevice.html#Melanchall_DryWetMidi_Multimedia_InputDevice_GetProperty_Melanchall_DryWetMidi_Multimedia_InputDeviceProperty_) and [for OutputDevice](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.OutputDevice.html#Melanchall_DryWetMidi_Multimedia_OutputDevice_GetProperty_Melanchall_DryWetMidi_Multimedia_OutputDeviceProperty_).
* Added `GetSupportedProperties` method [for InputDevice](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.InputDevice.html#Melanchall_DryWetMidi_Multimedia_InputDevice_GetSupportedProperties) and [for OutputDevice](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.OutputDevice.html#Melanchall_DryWetMidi_Multimedia_OutputDevice_GetSupportedProperties).
* Added [DevicesWatcher](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.DevicesWatcher.html) class which allows [watch adding or removing a MIDI device](https://melanchall.github.io/drywetmidi/articles/devices/Devices-watcher.html).
* Added `ConvertMultiple` methods to [BytesToMidiEventConverter](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.BytesToMidiEventConverter.html) (#134).
* Added [FfStatusBytesPolicy](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.BytesToMidiEventConverter.html#Melanchall_DryWetMidi_Core_BytesToMidiEventConverter_FfStatusBytePolicy) property to [BytesToMidiEventConverter](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.BytesToMidiEventConverter.html).
* Implemented delta-times support in [BytesToMidiEventConverter](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.BytesToMidiEventConverter.html) and [MidiEventToBytesConverter](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.MidiEventToBytesConverter.html) (#134).
* Exploded `WritingSettings` property of [MidiEventToBytesConverter](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.MidiEventToBytesConverter.html) into separate properties.
* Exploded `ReadingSettings` property of [BytesToMidiEventConverter](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.BytesToMidiEventConverter.html) into separate properties.
* Added [UnknownChannelEventPolicy](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.BytesToMidiEventConverter.html#Melanchall_DryWetMidi_Core_BytesToMidiEventConverter_UnknownChannelEventPolicy) property for [BytesToMidiEventConverter](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.BytesToMidiEventConverter.html).
* Added `GetByIndex` method [for InputDevice](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.InputDevice.html#Melanchall_DryWetMidi_Multimedia_InputDevice_GetByIndex_System_Int32_) and [for OutputDevice](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.OutputDevice.html#Melanchall_DryWetMidi_Multimedia_OutputDevice_GetByIndex_System_Int32_).

# Small changes and bug fixes

* [MidiException](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Common.MidiException.html) class has been moved to [Melanchall.DryWetMidi.Common](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Common.html) namespace.
* [MidiDeviceException](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Multimedia.MidiDeviceException.html) is subclassed from [MidiException](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Common.MidiException.html) now.
* **Fixed:** `GetObjects` methods of the [GetObjectsUtilities](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Interaction.GetObjectsUtilities.html) sometimes return wrong chords when notes are gathered across different track chunks.