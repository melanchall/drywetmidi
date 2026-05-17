using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Callback used to process events coming from <see cref="EndpointsConnector.InputEndpoint"/> before
    /// they will be sent to <see cref="EndpointsConnector.OutputEndpoints"/>.
    /// </summary>
    /// <param name="inputMidiEvent">A MIDI event to process.</param>
    /// <returns>New event (or the input one) which is the processed original one.</returns>
    public delegate MidiEvent EndpointsConnectorEventCallback(MidiEvent inputMidiEvent);
}
