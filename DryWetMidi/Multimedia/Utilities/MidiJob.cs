using System;
using System.Threading;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal readonly struct MidiJob
    {
        public readonly Action Action;

        public readonly ManualResetEventSlim CompletionSignal;

        public MidiJob(Action action, ManualResetEventSlim completionSignal)
        {
            Action = action;
            CompletionSignal = completionSignal;
        }
    }
}
