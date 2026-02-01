#include <alsa/asoundlib.h>
#include <stdio.h>
#include <stdlib.h>
#include <signal.h>
#include <string.h>
#include <unistd.h>

typedef int LPBCREATE_RESULT;

#define LPBCREATE_OK 0
#define LPBCREATE_FAILEDCREATECLIENT 1
#define LPBCREATE_FAILEDCREATESOURCE 2
#define LPBCREATE_FAILEDCREATEDESTINATION 3

typedef struct
{
    int input_port;   // Corresponds to srcRef (others read from this)
    int output_port;  // Corresponds to destRef (others write to this)
} PortInfo;

typedef struct
{
    snd_seq_t *seq;
    PortInfo *ports;
    int port_count;
} ClientContext;

static volatile int keep_running = 1;

void signal_handler(int sig)
{
    keep_running = 0;
}

// This is analogous to ReadProc in CoreMIDI
void midi_event_handler(snd_async_handler_t *handler)
{
    ClientContext *context = (ClientContext *)snd_async_handler_get_callback_private(handler);
    snd_seq_t *seq = context->seq;
    snd_seq_event_t *ev;
    
    while (snd_seq_event_input(seq, &ev) > 0)
    {
        if (ev == NULL)
            continue;
        
        // Find which output port received the event and forward to corresponding input port
        for (int i = 0; i < context->port_count; i++)
        {
            if (ev->dest.port == context->ports[i].output_port)
            {
                // Forward event to the input port (loopback)
                snd_seq_ev_set_source(ev, context->ports[i].input_port);
                snd_seq_ev_set_subs(ev);
                snd_seq_ev_set_direct(ev);
                snd_seq_event_output_direct(seq, ev);
                break;
            }
        }
        
        snd_seq_free_event(ev);
    }
}

int main(int argc, char *argv[])
{
    snd_seq_t *seq;
    snd_async_handler_t *async_handler;
    int err;
    
    printf("Creating client...\n");
    
    err = snd_seq_open(&seq, "default", SND_SEQ_OPEN_DUPLEX, 0);
    if (err < 0)
    {
        fprintf(stderr, "Error opening ALSA sequencer: %s\n", snd_strerror(err));
        return LPBCREATE_FAILEDCREATECLIENT;
    }
    
    snd_seq_set_client_name(seq, "LoopbackClient");
    
    signal(SIGINT, signal_handler);
    signal(SIGTERM, signal_handler);
    
    ClientContext context;
    context.seq = seq;
    context.port_count = argc - 1;
    context.ports = malloc(sizeof(PortInfo) * context.port_count);
    
    for (int i = 1; i < argc; i++)
    {
        printf("Creating port '%s'...\n", argv[i]);
        
        printf("    creating source...\n");
        
        // Create input port (source) - others read from this
        char input_name[256];
        snprintf(input_name, sizeof(input_name), "%s In", argv[i]);
        
        context.ports[i-1].input_port = snd_seq_create_simple_port(seq, input_name,
            SND_SEQ_PORT_CAP_READ | SND_SEQ_PORT_CAP_SUBS_READ,
            SND_SEQ_PORT_TYPE_MIDI_GENERIC | SND_SEQ_PORT_TYPE_APPLICATION);
        
        if (context.ports[i-1].input_port < 0)
        {
            fprintf(stderr, "Error creating input port: %s\n", snd_strerror(context.ports[i-1].input_port));
            return LPBCREATE_FAILEDCREATESOURCE;
        }
        
        printf("    creating destination...\n");
        
        // Create output port (destination) - others write to this
        char output_name[256];
        snprintf(output_name, sizeof(output_name), "%s Out", argv[i]);
        
        context.ports[i-1].output_port = snd_seq_create_simple_port(seq, output_name,
            SND_SEQ_PORT_CAP_WRITE | SND_SEQ_PORT_CAP_SUBS_WRITE,
            SND_SEQ_PORT_TYPE_MIDI_GENERIC | SND_SEQ_PORT_TYPE_APPLICATION);
        
        if (context.ports[i-1].output_port < 0)
        {
            fprintf(stderr, "Error creating output port: %s\n", snd_strerror(context.ports[i-1].output_port));
            return LPBCREATE_FAILEDCREATEDESTINATION;
        }
        
        printf("OK\n");
    }
    
    // Set up async handler (callback) - analogous to passing ReadProc to CoreMIDI
    err = snd_async_add_seq_handler(&async_handler, seq, midi_event_handler, &context);
    if (err < 0)
    {
        fprintf(stderr, "Error setting up async handler: %s\n", snd_strerror(err));
        snd_seq_close(seq);
        free(context.ports);
        return LPBCREATE_FAILEDCREATECLIENT;
    }
    
    printf("Waiting for data...\n");
    
    // Just sleep - events are handled by callback
    while (keep_running)
    {
        usleep(1000);
    }
    
    printf("\nCleaning up...\n");
    
    free(context.ports);
    snd_seq_close(seq);
    
    return LPBCREATE_OK;
}