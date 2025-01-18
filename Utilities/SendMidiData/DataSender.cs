using Melanchall.Common;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Melanchall.SendMidiData
{
    internal static class DataSender
    {
        #region Methods

        public static SendResult SendData(IOutputDevice outputDevice)
        {
            UiUtilities.Write("Specify data to send: ");
            var array = UiUtilities.ReadArray();

            if (DryWetMidi.MusicTheory.Note.TryParse(array[0], out var musicTheoryNote))
                return SendNote(array, musicTheoryNote, outputDevice);

            return SendData(array, outputDevice);
        }

        private static SendResult SendData(string[] array, IOutputDevice outputDevice)
        {
            var bytes = new List<byte>();

            foreach (var part in array)
            {
                try
                {
                    var b = Convert.ToByte(part, 16);
                    bytes.Add(b);
                }
                catch (Exception ex)
                {
                    UiUtilities.WriteLine($"Failed to parse data byte '{part}': {ex.Message}");
                    return SendResult.InvalidInput;
                }
            }

            try
            {
                using (var converter = new BytesToMidiEventConverter())
                {
                    var midiEvents = converter.ConvertMultiple(bytes.ToArray());
                    var playback = midiEvents.GetPlayback(TempoMap.Default, outputDevice);

                    playback.Start();
                    SpinWait.SpinUntil(() => !playback.IsRunning);

                    return SendResult.Sent;
                }
            }
            catch (Exception ex)
            {
                UiUtilities.WriteLine($"Failed to parse MIDI events from data bytes: {ex.Message}");
                return SendResult.InvalidInput;
            }
        }

        private static SendResult SendNote(string[] array, DryWetMidi.MusicTheory.Note musicTheoryNote, IOutputDevice outputDevice)
        {
            ITimeSpan length = new MetricTimeSpan(0, 0, 1);

            if (array.Length > 1)
            {
                if (!TimeSpanUtilities.TryParse(array[1], out length))
                {
                    UiUtilities.WriteLine($"Invalid format of a note's length '{array[1]}'. Try again");
                    return SendResult.InvalidInput;
                }
            }

            if (array.Length > 2)
            {
                if (!SevenBitNumber.TryParse(array[2], out var velocity))
                {
                    UiUtilities.WriteLine($"Invalid format of a note's velocity '{array[2]}'. Try again");
                    return SendResult.InvalidInput;
                }
            }

            var note = new Note(musicTheoryNote.NoteNumber).SetLength(length, TempoMap.Default);
            var playback = new[] { note }.GetPlayback(TempoMap.Default, outputDevice, DryWetMidi.Standards.GeneralMidiProgram.AcousticGrandPiano);

            playback.Start();
            SpinWait.SpinUntil(() => !playback.IsRunning);

            return SendResult.Sent;
        }

        #endregion
    }
}
