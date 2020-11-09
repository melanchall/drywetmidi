using System.Collections.Generic;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Standards
{
    // TODO: tests
    /// <summary>
    /// Provides utilities for General Sound.
    /// </summary>
    public static class GeneralSoundUtilities
    {
        private const byte RhythmChannelBankMsb = 0x78;

        /// <summary>
        /// Gets MIDI events sequence to switch to the specified General Sound percussion set.
        /// </summary>
        /// <param name="percussionSet"><see cref="GeneralSoundPercussionSet"/> to get events for.</param>
        /// <param name="channel">Channel events should be created for.</param>
        /// <returns>MIDI events sequence to switch to the <paramref name="percussionSet"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussionSet"/> specified an invalid value.</exception>
        public static IEnumerable<MidiEvent> GetPercussionSetEvents(this GeneralSoundPercussionSet percussionSet, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussionSet), percussionSet);

            return new[]
            {
                ControlName.BankSelect.GetControlChangeEvent((SevenBitNumber)RhythmChannelBankMsb, channel),
                ControlName.LsbForBankSelect.GetControlChangeEvent((SevenBitNumber)0, channel),
                percussionSet.GetProgramEvent(channel)
            };
        }

        /// <summary>
        /// Gets Program Change event corresponding to the specified General Sound percussion set.
        /// </summary>
        /// <param name="percussionSet"><see cref="GeneralSoundPercussionSet"/> to get event for.</param>
        /// <param name="channel">Channel event should be created for.</param>
        /// <returns>Program Change event corresponding to the <paramref name="percussionSet"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussionSet"/> specified an invalid value.</exception>
        public static MidiEvent GetProgramEvent(this GeneralSoundPercussionSet percussionSet, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussionSet), percussionSet);

            return new ProgramChangeEvent(percussionSet.AsSevenBitNumber()) { Channel = channel };
        }

        /// <summary>
        /// Converts <see cref="GeneralSoundPercussionSet"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussionSet"><see cref="GeneralSoundPercussionSet"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussionSet"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussionSet"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralSoundPercussionSet percussionSet)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussionSet), percussionSet);

            return (SevenBitNumber)(byte)percussionSet;
        }

        /// <summary>
        /// Converts <see cref="GeneralSoundCm6432LPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundCm6432LPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralSoundCm6432LPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralSoundTr808Percussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundTr808Percussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralSoundTr808Percussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralSoundBrushPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundBrushPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralSoundBrushPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralSoundElectronicPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundElectronicPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralSoundElectronicPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralSoundJazzPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundJazzPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralSoundJazzPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralSoundOrchestraPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundOrchestraPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralSoundOrchestraPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralSoundPowerPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundPowerPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralSoundPowerPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralSoundRoomPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundRoomPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralSoundRoomPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralSoundSfxPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundSfxPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralSoundSfxPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralSoundStandardPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundStandardPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralSoundStandardPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General Sound 'TR-808' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundTr808Percussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralSoundTr808Percussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General Sound 'CM-64/CM-32L' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundCm6432LPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralSoundCm6432LPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General Sound 'Brush' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundBrushPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralSoundBrushPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General Sound 'Electronic' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundElectronicPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralSoundElectronicPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General Sound 'Jazz' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundJazzPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralSoundJazzPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General Sound 'Orchestra' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundOrchestraPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralSoundOrchestraPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General Sound 'Power' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundPowerPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralSoundPowerPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General Sound 'Room' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundRoomPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralSoundRoomPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General Sound 'SFX' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundSfxPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralSoundSfxPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General Sound 'Standard' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundStandardPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralSoundStandardPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General Sound 'TR-808' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundTr808Percussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralSoundTr808Percussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General Sound 'CM-64/CM-32L' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundCm6432LPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralSoundCm6432LPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General Sound 'Brush' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundBrushPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralSoundBrushPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General Sound 'Electronic' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundElectronicPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralSoundElectronicPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General Sound 'Jazz' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundJazzPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralSoundJazzPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General Sound 'Orchestra' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundOrchestraPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralSoundOrchestraPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General Sound 'Power' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundPowerPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralSoundPowerPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General Sound 'Room' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundRoomPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralSoundRoomPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General Sound 'SFX' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundSfxPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralSoundSfxPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General Sound 'Standard' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralSoundStandardPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralSoundStandardPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }
    }
}
