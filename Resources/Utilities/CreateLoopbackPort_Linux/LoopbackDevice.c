#include <alsa/asoundlib.h>
#include <stdio.h>
#include <stdlib.h>
#include <signal.h>
#include <string.h>

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

static volatile int keep_running = 1;

void signal_handler(int sig)
{
    keep_running = 0;
}

int main(int argc, char* argv[])
{
    snd_seq_t* seq;
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

    PortInfo* ports = malloc(sizeof(PortInfo) * (argc - 1));
    int port_count = argc - 1;

    for (int i = 1; i < argc; i++)
    {
        printf("Creating port '%s'...\n", argv[i]);

        printf("    creating source...\n");

        // Create input port (source) - others read from this
        char input_name[256];
        snprintf(input_name, sizeof(input_name), "%s In", argv[i]);

        ports[i - 1].input_port = snd_seq_create_simple_port(seq, input_name,
            SND_SEQ_PORT_CAP_READ | SND_SEQ_PORT_CAP_SUBS_READ,
            SND_SEQ_PORT_TYPE_MIDI_GENERIC | SND_SEQ_PORT_TYPE_APPLICATION);

        if (ports[i - 1].input_port < 0)
        {
            fprintf(stderr, "Error creating input port: %s\n", snd_strerror(ports[i - 1].input_port));
            return LPBCREATE_FAILEDCREATESOURCE;
        }

        printf("    creating destination...\n");

        // Create output port (destination) - others write to this
        char output_name[256];
        snprintf(output_name, sizeof(output_name), "%s Out", argv[i]);

        ports[i - 1].output_port = snd_seq_create_simple_port(seq, output_name,
            SND_SEQ_PORT_CAP_WRITE | SND_SEQ_PORT_CAP_SUBS_WRITE,
            SND_SEQ_PORT_TYPE_MIDI_GENERIC | SND_SEQ_PORT_TYPE_APPLICATION);

        if (ports[i - 1].output_port < 0)
        {
            fprintf(stderr, "Error creating output port: %s\n", snd_strerror(ports[i - 1].output_port));
            return LPBCREATE_FAILEDCREATEDESTINATION;
        }

        printf("OK\n");
    }

    printf("Waiting for data...\n");

    // Create poll descriptors for event handling
    int npfds = snd_seq_poll_descriptors_count(seq, POLLIN);
    struct pollfd* pfds = malloc(sizeof(struct pollfd) * npfds);
    snd_seq_poll_descriptors(seq, pfds, npfds, POLLIN);

    // Main event loop
    while (keep_running)
    {
        if (poll(pfds, npfds, 1000) > 0)
        {
            snd_seq_event_t* ev;

            while (snd_seq_event_input(seq, &ev) > 0)
            {
                if (ev == NULL)
                    continue;

                // Find which output port received the event and forward to corresponding input port
                for (int i = 0; i < port_count; i++)
                {
                    if (ev->dest.port == ports[i].output_port)
                    {
                        // Forward event to the input port (loopback)
                        snd_seq_ev_set_source(ev, ports[i].input_port);
                        snd_seq_ev_set_subs(ev);
                        snd_seq_ev_set_direct(ev);
                        snd_seq_event_output_direct(seq, ev);
                        break;
                    }
                }

                snd_seq_free_event(ev);
            }
        }
    }

    printf("\nCleaning up...\n");

    free(pfds);
    free(ports);
    snd_seq_close(seq);

    return LPBCREATE_OK;
}