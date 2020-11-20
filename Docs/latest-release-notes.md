# Obsolete API

New section has been added to [documentation](https://melanchall.github.io/drywetmidi) site: [Obsolete](https://melanchall.github.io/drywetmidi/obsolete/obsolete.html). This section contains the list of methods and classes that are obsolete and thus will be removed by a next release.

The current list of obsolete API:

* [OBS1: WritingSettings.CompressionPolicy](https://melanchall.github.io/drywetmidi/obsolete/obsolete.html#obs1)
* [OBS2: ReaderSettings.BufferingPolicy = BufferingPolicy.BufferAllData](https://melanchall.github.io/drywetmidi/obsolete/obsolete.html#obs2)
* [OBS3: TempoMap.TimeSignature](https://melanchall.github.io/drywetmidi/obsolete/obsolete.html#obs3)
* [OBS4: TempoMap.Tempo](https://melanchall.github.io/drywetmidi/obsolete/obsolete.html#obs4)

# New features and improvements

* Added [ReaderSettings.BufferingPolicy](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.ReaderSettings.html#Melanchall_DryWetMidi_Core_ReaderSettings_BufferingPolicy) property.
* Increased speed of [MidiFile.Read](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.MidiFile.html#Melanchall_DryWetMidi_Core_MidiFile_Read_System_String_Melanchall_DryWetMidi_Core_ReadingSettings_) method.
* Increased speed of [MidiFile.Write](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.MidiFile.html#Melanchall_DryWetMidi_Core_MidiFile_Write_System_String_System_Boolean_Melanchall_DryWetMidi_Core_MidiFileFormat_Melanchall_DryWetMidi_Core_WritingSettings_) method.
* Implemented buffered MIDI file writing (see [WriterSettings.UseBuffering](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.WriterSettings.html#Melanchall_DryWetMidi_Core_WriterSettings_UseBuffering)).
* Decreased memory usage of [MidiWriter.Write3ByteDword](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.MidiWriter.html#Melanchall_DryWetMidi_Core_MidiWriter_Write3ByteDword_System_UInt32_) and [MidiWriter.WriteVlqNumber](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.MidiWriter.html#Melanchall_DryWetMidi_Core_MidiWriter_WriteVlqNumber_System_Int32_).
* Reduced memory usage for [ChannelEvent](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.ChannelEvent.html).
* Added [ReadingSettings.ZeroLengthDataPolicy](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.ReadingSettings.html#Melanchall_DryWetMidi_Core_ReadingSettings_ZeroLengthDataPolicy) property.
* Added RPN classes (see [RegisteredParameter](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Interaction.RegisteredParameter.html)).
* Added [TempoMap.GetTimeSignatureChanges](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Interaction.TempoMap.html#Melanchall_DryWetMidi_Interaction_TempoMap_GetTimeSignatureChanges) and [TempoMap.GetTimeSignatureAtTime](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Interaction.TempoMap.html#Melanchall_DryWetMidi_Interaction_TempoMap_GetTimeSignatureAtTime_Melanchall_DryWetMidi_Interaction_ITimeSpan_) methods.
* Added [WritingSettings.WriteHeaderChunk](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.WritingSettings.html#Melanchall_DryWetMidi_Core_WritingSettings_WriteHeaderChunk) property (#94).
* Added GS standard percussion API (#93).
* Added [SliceMidiFileSettings.Markers](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Tools.SliceMidiFileSettings.html#Melanchall_DryWetMidi_Tools_SliceMidiFileSettings_Markers) property (#99).
* Non-SMF events will be filtered out on saving to MIDI file or track chunk (#103).

# Small changes and bug fixes

* Added value validation in setters of some MIDI events classes properties.
* Edge Note Off events will always be put to current part on MIDI file splitting (#99).
* **Fixed:** Events transferred incorrectly between parts on MIDI file splitting (#99).
* **Fixed:** [SmpteOffsetEvent](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.SmpteOffsetEvent.html) writes in wrong way.
* **Fixed:** [SmpteOffsetEvent](https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.SmpteOffsetEvent.html) equality check is wrong.
* **Fixed:** Failed to use devices API in UWP app (#95).