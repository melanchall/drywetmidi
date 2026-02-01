#include <alsa/asoundlib.h>
#include <pthread.h>
#include <time.h>
#include <signal.h>
#include <sched.h>
#include <unistd.h>
#include <sys/syscall.h>
#include <errno.h>
#include <poll.h>

#include <atomic>
#include <vector>
#include <new>
#include <cstdint>
#include <cstring>
#include <map>
#include <string>

#include "../Common/NativeApi-Constants.h"

#define PROPERTY_VALUE_BUFFER_SIZE 256

#define API_EXPORT extern "C" __attribute__((visibility("default")))

/* ================================
   Common
================================ */

API_EXPORT API_TYPE GetApiType()
{
    return API_TYPE_LINUX;
}

API_EXPORT char CanCompareDevices()
{
    return 1;
}

/* ================================
   High-precision tick generator
 ================================ */

struct TickGeneratorSessionHandle
{
    pthread_t thread;
    std::atomic<char> active;
    TGSESSION_OPENRESULT threadStartResult;
    int threadStartError;
};

struct TickGeneratorInfo
{
    void (*callback)(void);
    timer_t timerId;
    std::atomic<char> active;
};

void TimerSignalHandler(union sigval sv)
{
    TickGeneratorInfo* info = static_cast<TickGeneratorInfo*>(sv.sival_ptr);
    if (info->active.load() == 1)
    {
        info->callback();
    }
}

void* TickGeneratorSessionThreadRoutine(void* data)
{
    TickGeneratorSessionHandle* sessionHandle = static_cast<TickGeneratorSessionHandle*>(data);

    // Set realtime priority
    struct sched_param param;
    param.sched_priority = sched_get_priority_max(SCHED_FIFO);

    if (sched_setscheduler(0, SCHED_FIFO, &param) != 0)
    {
        sessionHandle->threadStartError = errno;
        sessionHandle->threadStartResult = TGSESSION_OPENRESULT_FAILEDTOSETREALTIMEPRIORITY;
        return nullptr;
    }

    sessionHandle->active.store(1);

    // Keep thread alive
    while (sessionHandle->active.load() == 1)
    {
        usleep(100000); // 100ms
    }

    return nullptr;
}

API_EXPORT TGSESSION_OPENRESULT OpenTickGeneratorSession(void** handle, int* errorCode)
{
    *errorCode = 0;

    TickGeneratorSessionHandle* sessionHandle = new TickGeneratorSessionHandle();

    sessionHandle->threadStartResult = TGSESSION_OPENRESULT_OK;
    sessionHandle->active.store(0);

    int pthreadCreateResult;
    if ((pthreadCreateResult = pthread_create(&sessionHandle->thread, nullptr, TickGeneratorSessionThreadRoutine, sessionHandle)) != 0)
    {
        delete sessionHandle;
        *errorCode = pthreadCreateResult;
        return TGSESSION_OPENRESULT_THREADSTARTERROR;
    }

    while (sessionHandle->active.load() == 0)
    {
        if (sessionHandle->threadStartResult != TGSESSION_OPENRESULT_OK)
        {
            TGSESSION_OPENRESULT res = sessionHandle->threadStartResult;
            *errorCode = sessionHandle->threadStartError;
            delete sessionHandle;
            return res;
        }

        struct timespec ts = { 0, 1000000 }; // 1ms
        nanosleep(&ts, nullptr);
    }

    *handle = sessionHandle;

    return TGSESSION_OPENRESULT_OK;
}

API_EXPORT TG_STARTRESULT StartHighPrecisionTickGenerator_Linux(int interval, void* sessionHandle, void (*callback)(void), TickGeneratorInfo** info, int* errorCode)
{
    *errorCode = 0;

    TickGeneratorInfo* tickGeneratorInfo = new TickGeneratorInfo();
    tickGeneratorInfo->callback = callback;
    tickGeneratorInfo->active.store(1);

    struct sigevent sev;
    sev.sigev_notify = SIGEV_THREAD;
    sev.sigev_notify_function = TimerSignalHandler;
    sev.sigev_notify_attributes = nullptr;
    sev.sigev_value.sival_ptr = tickGeneratorInfo;

    if (timer_create(CLOCK_MONOTONIC, &sev, &tickGeneratorInfo->timerId) != 0)
    {
        delete tickGeneratorInfo;
        *errorCode = errno;
        return TG_STARTRESULT_UNKNOWNERROR;
    }

    struct itimerspec its;
    its.it_value.tv_sec = interval / 1000;
    its.it_value.tv_nsec = (interval % 1000) * 1000000;
    its.it_interval.tv_sec = its.it_value.tv_sec;
    its.it_interval.tv_nsec = its.it_value.tv_nsec;

    if (timer_settime(tickGeneratorInfo->timerId, 0, &its, nullptr) != 0)
    {
        timer_delete(tickGeneratorInfo->timerId);
        delete tickGeneratorInfo;
        *errorCode = errno;
        return TG_STARTRESULT_UNKNOWNERROR;
    }

    *info = tickGeneratorInfo;

    return TG_STARTRESULT_OK;
}

API_EXPORT TG_STOPRESULT StopHighPrecisionTickGenerator(TickGeneratorSessionHandle* sessionHandle, TickGeneratorInfo* tickGeneratorInfo, int* errorCode)
{
    *errorCode = 0;

    tickGeneratorInfo->active.store(0);
    timer_delete(tickGeneratorInfo->timerId);
    delete tickGeneratorInfo;

    return TG_STOPRESULT_OK;
}

/* ================================
   Devices common
 ================================ */

struct InputDeviceInfo
{
    int client;
    int port;
    std::string name;
    std::string manufacturer;
    std::string model;
};

struct OutputDeviceInfo
{
    int client;
    int port;
    std::string name;
    std::string manufacturer;
    std::string model;
};

/* ================================
   Session
 ================================ */

typedef void (*InputDeviceCallback)(void* info, char operation);
typedef void (*OutputDeviceCallback)(void* info, char operation);

struct SessionHandle
{
    char* name;
    snd_seq_t* seq;
    pthread_t thread;
    std::atomic<char> clientCreated;
    std::atomic<char> sessionClosed;
    int clientCreationError;
    InputDeviceCallback inputDeviceCallback;
    OutputDeviceCallback outputDeviceCallback;
};

void* SessionThreadProc(void* data)
{
    SessionHandle* sessionHandle = static_cast<SessionHandle*>(data);

    int err = snd_seq_open(&sessionHandle->seq, "default", SND_SEQ_OPEN_DUPLEX, 0);
    if (err < 0)
    {
        sessionHandle->clientCreationError = err;
        sessionHandle->clientCreated.store(1);
        return nullptr;
    }

    snd_seq_set_client_name(sessionHandle->seq, sessionHandle->name);
    sessionHandle->clientCreationError = 0;
    sessionHandle->clientCreated.store(1);

    // Poll for device changes (ALSA announces port changes via events)
    int npfds = snd_seq_poll_descriptors_count(sessionHandle->seq, POLLIN);
    struct pollfd* pfds = new struct pollfd[npfds];
    snd_seq_poll_descriptors(sessionHandle->seq, pfds, npfds, POLLIN);

    while (sessionHandle->sessionClosed.load() == 0)
    {
        if (poll(pfds, npfds, 1000) > 0)
        {
            snd_seq_event_t* ev;
            while (snd_seq_event_input(sessionHandle->seq, &ev) > 0)
            {
                if (ev->type == SND_SEQ_EVENT_PORT_START || ev->type == SND_SEQ_EVENT_PORT_EXIT)
                {
                    snd_seq_client_info_t* cinfo;
                    snd_seq_port_info_t* pinfo;
                    snd_seq_client_info_alloca(&cinfo);
                    snd_seq_port_info_alloca(&pinfo);

                    if (snd_seq_get_any_client_info(sessionHandle->seq, ev->data.addr.client, cinfo) == 0 &&
                        snd_seq_get_any_port_info(sessionHandle->seq, ev->data.addr.client, ev->data.addr.port, pinfo) == 0)
                    {
                        unsigned int caps = snd_seq_port_info_get_capability(pinfo);
                        unsigned int type = snd_seq_port_info_get_type(pinfo);

                        // Skip kernel and hardware ports that aren't MIDI
                        if (!(type & SND_SEQ_PORT_TYPE_MIDI_GENERIC))
                            continue;

                        char operation = (ev->type == SND_SEQ_EVENT_PORT_START) ? 1 : 0;

                        if ((caps & SND_SEQ_PORT_CAP_READ) && (caps & SND_SEQ_PORT_CAP_SUBS_READ))
                        {
                            InputDeviceInfo* inputDeviceInfo = new InputDeviceInfo();
                            inputDeviceInfo->client = ev->data.addr.client;
                            inputDeviceInfo->port = ev->data.addr.port;
                            inputDeviceInfo->name = snd_seq_port_info_get_name(pinfo);
                            inputDeviceInfo->manufacturer = snd_seq_client_info_get_name(cinfo);
                            inputDeviceInfo->model = snd_seq_port_info_get_name(pinfo);

                            sessionHandle->inputDeviceCallback(inputDeviceInfo, operation);
                        }

                        if ((caps & SND_SEQ_PORT_CAP_WRITE) && (caps & SND_SEQ_PORT_CAP_SUBS_WRITE))
                        {
                            OutputDeviceInfo* outputDeviceInfo = new OutputDeviceInfo();
                            outputDeviceInfo->client = ev->data.addr.client;
                            outputDeviceInfo->port = ev->data.addr.port;
                            outputDeviceInfo->name = snd_seq_port_info_get_name(pinfo);
                            outputDeviceInfo->manufacturer = snd_seq_client_info_get_name(cinfo);
                            outputDeviceInfo->model = snd_seq_port_info_get_name(pinfo);

                            sessionHandle->outputDeviceCallback(outputDeviceInfo, operation);
                        }
                    }
                }

                snd_seq_free_event(ev);
            }
        }
    }

    delete[] pfds;
    return nullptr;
}

API_EXPORT SESSION_OPENRESULT OpenSession_Linux(char* name, InputDeviceCallback inputDeviceCallback, OutputDeviceCallback outputDeviceCallback, void** handle, int* errorCode)
{
    *errorCode = 0;

    SessionHandle* sessionHandle = new SessionHandle();

    sessionHandle->name = name;
    sessionHandle->inputDeviceCallback = inputDeviceCallback;
    sessionHandle->outputDeviceCallback = outputDeviceCallback;
    sessionHandle->clientCreated.store(0);
    sessionHandle->sessionClosed.store(0);

    int pthreadCreateResult;
    if ((pthreadCreateResult = pthread_create(&sessionHandle->thread, nullptr, SessionThreadProc, sessionHandle)) != 0)
    {
        delete sessionHandle;
        *errorCode = pthreadCreateResult;
        return SESSION_OPENRESULT_THREADSTARTERROR;
    }

    while (sessionHandle->clientCreated.load() == 0) {}

    if (sessionHandle->clientCreationError != 0)
    {
        int err = sessionHandle->clientCreationError;
        delete sessionHandle;
        *errorCode = err;
        return SESSION_OPENRESULT_UNKNOWNERROR;
    }

    *handle = sessionHandle;

    return SESSION_OPENRESULT_OK;
}

API_EXPORT SESSION_CLOSERESULT CloseSession(void* handle)
{
    SessionHandle* sessionHandle = static_cast<SessionHandle*>(handle);

    if (sessionHandle->sessionClosed.load() == 1)
        return SESSION_CLOSERESULT_OK;

    sessionHandle->sessionClosed.store(1);

    pthread_join(sessionHandle->thread, nullptr);

    if (sessionHandle->seq)
        snd_seq_close(sessionHandle->seq);

    delete sessionHandle;
    return SESSION_CLOSERESULT_OK;
}

/* ================================
   Input device
 ================================ */

struct InputDeviceHandle
{
    InputDeviceInfo* info;
    snd_seq_t* seq;
    int port;
    pthread_t thread;
    std::atomic<char> threadActive;
    void (*callback)(snd_seq_event_t* ev, void* userData);
};

void* InputDeviceThreadProc(void* data)
{
    InputDeviceHandle* handle = static_cast<InputDeviceHandle*>(data);

    int npfds = snd_seq_poll_descriptors_count(handle->seq, POLLIN);
    struct pollfd* pfds = new struct pollfd[npfds];
    snd_seq_poll_descriptors(handle->seq, pfds, npfds, POLLIN);

    while (handle->threadActive.load() == 1)
    {
        if (poll(pfds, npfds, 1000) > 0)
        {
            snd_seq_event_t* ev;
            while (snd_seq_event_input(handle->seq, &ev) > 0)
            {
                if (ev && handle->callback)
                {
                    handle->callback(ev, nullptr);
                }
                snd_seq_free_event(ev);
            }
        }
    }

    delete[] pfds;
    return nullptr;
}

API_EXPORT IN_GETCOUNTRESULT GetInputDevicesCount(int* count)
{
    snd_seq_t* seq;
    if (snd_seq_open(&seq, "default", SND_SEQ_OPEN_DUPLEX, 0) < 0)
    {
        *count = 0;
        return IN_GETCOUNTRESULT_OK;
    }

    snd_seq_client_info_t* cinfo;
    snd_seq_port_info_t* pinfo;
    snd_seq_client_info_alloca(&cinfo);
    snd_seq_port_info_alloca(&pinfo);

    int deviceCount = 0;

    snd_seq_client_info_set_client(cinfo, -1);
    while (snd_seq_query_next_client(seq, cinfo) >= 0)
    {
        int client = snd_seq_client_info_get_client(cinfo);

        snd_seq_port_info_set_client(pinfo, client);
        snd_seq_port_info_set_port(pinfo, -1);
        while (snd_seq_query_next_port(seq, pinfo) >= 0)
        {
            unsigned int caps = snd_seq_port_info_get_capability(pinfo);
            unsigned int type = snd_seq_port_info_get_type(pinfo);

            if (!(type & SND_SEQ_PORT_TYPE_MIDI_GENERIC))
                continue;

            if ((caps & SND_SEQ_PORT_CAP_READ) && (caps & SND_SEQ_PORT_CAP_SUBS_READ))
            {
                deviceCount++;
            }
        }
    }

    snd_seq_close(seq);
    *count = deviceCount;
    return IN_GETCOUNTRESULT_OK;
}

API_EXPORT IN_GETINFORESULT GetInputDeviceInfo(int deviceIndex, void** info, int* errorCode)
{
    *errorCode = 0;

    snd_seq_t* seq;
    if (snd_seq_open(&seq, "default", SND_SEQ_OPEN_DUPLEX, 0) < 0)
    {
        return IN_GETINFORESULT_UNKNOWNERROR;
    }

    snd_seq_client_info_t* cinfo;
    snd_seq_port_info_t* pinfo;
    snd_seq_client_info_alloca(&cinfo);
    snd_seq_port_info_alloca(&pinfo);

    int currentIndex = 0;

    snd_seq_client_info_set_client(cinfo, -1);
    while (snd_seq_query_next_client(seq, cinfo) >= 0)
    {
        int client = snd_seq_client_info_get_client(cinfo);

        snd_seq_port_info_set_client(pinfo, client);
        snd_seq_port_info_set_port(pinfo, -1);
        while (snd_seq_query_next_port(seq, pinfo) >= 0)
        {
            unsigned int caps = snd_seq_port_info_get_capability(pinfo);
            unsigned int type = snd_seq_port_info_get_type(pinfo);

            if (!(type & SND_SEQ_PORT_TYPE_MIDI_GENERIC))
                continue;

            if ((caps & SND_SEQ_PORT_CAP_READ) && (caps & SND_SEQ_PORT_CAP_SUBS_READ))
            {
                if (currentIndex == deviceIndex)
                {
                    InputDeviceInfo* inputDeviceInfo = new InputDeviceInfo();
                    inputDeviceInfo->client = client;
                    inputDeviceInfo->port = snd_seq_port_info_get_port(pinfo);
                    inputDeviceInfo->name = snd_seq_port_info_get_name(pinfo);
                    inputDeviceInfo->manufacturer = snd_seq_client_info_get_name(cinfo);
                    inputDeviceInfo->model = snd_seq_port_info_get_name(pinfo);

                    *info = inputDeviceInfo;
                    snd_seq_close(seq);
                    return IN_GETINFORESULT_OK;
                }
                currentIndex++;
            }
        }
    }

    snd_seq_close(seq);
    return IN_GETINFORESULT_UNKNOWNERROR;
}

API_EXPORT int GetInputDeviceHashCode(void* info)
{
    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);
    return (inputDeviceInfo->client << 16) | inputDeviceInfo->port;
}

API_EXPORT char AreInputDevicesEqual(void* info1, void* info2)
{
    InputDeviceInfo* inputDeviceInfo1 = static_cast<InputDeviceInfo*>(info1);
    InputDeviceInfo* inputDeviceInfo2 = static_cast<InputDeviceInfo*>(info2);

    return static_cast<char>(inputDeviceInfo1->client == inputDeviceInfo2->client &&
        inputDeviceInfo1->port == inputDeviceInfo2->port);
}

API_EXPORT IN_GETPROPERTYRESULT GetInputDeviceName(void* info, char** value, int* errorCode)
{
    *errorCode = 0;
    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);

    char* buffer = new char[PROPERTY_VALUE_BUFFER_SIZE];
    strncpy(buffer, inputDeviceInfo->name.c_str(), PROPERTY_VALUE_BUFFER_SIZE - 1);
    buffer[PROPERTY_VALUE_BUFFER_SIZE - 1] = '\0';

    *value = buffer;
    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_GETPROPERTYRESULT GetInputDeviceManufacturer(void* info, char** value, int* errorCode)
{
    *errorCode = 0;
    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);

    char* buffer = new char[PROPERTY_VALUE_BUFFER_SIZE];
    strncpy(buffer, inputDeviceInfo->manufacturer.c_str(), PROPERTY_VALUE_BUFFER_SIZE - 1);
    buffer[PROPERTY_VALUE_BUFFER_SIZE - 1] = '\0';

    *value = buffer;
    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_GETPROPERTYRESULT GetInputDeviceProduct(void* info, char** value, int* errorCode)
{
    *errorCode = 0;
    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);

    char* buffer = new char[PROPERTY_VALUE_BUFFER_SIZE];
    strncpy(buffer, inputDeviceInfo->model.c_str(), PROPERTY_VALUE_BUFFER_SIZE - 1);
    buffer[PROPERTY_VALUE_BUFFER_SIZE - 1] = '\0';

    *value = buffer;
    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_GETPROPERTYRESULT GetInputDeviceDriverVersion(void* info, int* value, int* errorCode)
{
    *errorCode = 0;
    *value = 0; // ALSA doesn't provide driver version
    return IN_GETPROPERTYRESULT_UNKNOWNPROPERTY;
}

API_EXPORT IN_GETPROPERTYRESULT GetInputDeviceUniqueId(void* info, int* value, int* errorCode)
{
    *errorCode = 0;
    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);
    *value = (inputDeviceInfo->client << 16) | inputDeviceInfo->port;
    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_GETPROPERTYRESULT GetInputDeviceDriverOwner(void* info, char** value, int* errorCode)
{
    *errorCode = 0;
    *value = nullptr;
    return IN_GETPROPERTYRESULT_UNKNOWNPROPERTY;
}

API_EXPORT IN_OPENRESULT OpenInputDevice_Linux(void* info, void* sessionHandle, void (*callback)(snd_seq_event_t*, void*), void** handle, int* errorCode)
{
    *errorCode = 0;

    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);
    SessionHandle* pSessionHandle = static_cast<SessionHandle*>(sessionHandle);

    InputDeviceHandle* inputDeviceHandle = new InputDeviceHandle();
    inputDeviceHandle->info = inputDeviceInfo;
    inputDeviceHandle->seq = pSessionHandle->seq;
    inputDeviceHandle->callback = callback;
    inputDeviceHandle->threadActive.store(0);

    inputDeviceHandle->port = snd_seq_create_simple_port(pSessionHandle->seq, "IN",
        SND_SEQ_PORT_CAP_WRITE | SND_SEQ_PORT_CAP_SUBS_WRITE,
        SND_SEQ_PORT_TYPE_MIDI_GENERIC | SND_SEQ_PORT_TYPE_APPLICATION);

    if (inputDeviceHandle->port < 0)
    {
        delete inputDeviceHandle;
        *errorCode = inputDeviceHandle->port;
        return IN_OPENRESULT_UNKNOWNERROR;
    }

    *handle = inputDeviceHandle;
    return IN_OPENRESULT_OK;
}

API_EXPORT IN_CLOSERESULT CloseInputDevice(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputDeviceHandle* inputDeviceHandle = static_cast<InputDeviceHandle*>(handle);

    if (inputDeviceHandle->threadActive.load() == 1)
    {
        inputDeviceHandle->threadActive.store(0);
        pthread_join(inputDeviceHandle->thread, nullptr);
    }

    if (inputDeviceHandle->port >= 0)
        snd_seq_delete_simple_port(inputDeviceHandle->seq, inputDeviceHandle->port);

    delete inputDeviceHandle->info;
    delete inputDeviceHandle;

    return IN_CLOSERESULT_OK;
}

API_EXPORT IN_CONNECTRESULT ConnectToInputDevice(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputDeviceHandle* inputDeviceHandle = static_cast<InputDeviceHandle*>(handle);

    snd_seq_addr_t sender;
    sender.client = inputDeviceHandle->info->client;
    sender.port = inputDeviceHandle->info->port;

    snd_seq_addr_t dest;
    dest.client = snd_seq_client_id(inputDeviceHandle->seq);
    dest.port = inputDeviceHandle->port;

    int err = snd_seq_connect_from(inputDeviceHandle->seq, inputDeviceHandle->port, sender.client, sender.port);
    if (err < 0)
    {
        *errorCode = err;
        return IN_CONNECTRESULT_UNKNOWNERROR;
    }

    inputDeviceHandle->threadActive.store(1);
    if (pthread_create(&inputDeviceHandle->thread, nullptr, InputDeviceThreadProc, inputDeviceHandle) != 0)
    {
        snd_seq_disconnect_from(inputDeviceHandle->seq, inputDeviceHandle->port, sender.client, sender.port);
        *errorCode = errno;
        return IN_CONNECTRESULT_UNKNOWNERROR;
    }

    return IN_CONNECTRESULT_OK;
}

API_EXPORT IN_DISCONNECTRESULT DisconnectFromInputDevice(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputDeviceHandle* inputDeviceHandle = static_cast<InputDeviceHandle*>(handle);

    if (inputDeviceHandle->threadActive.load() == 1)
    {
        inputDeviceHandle->threadActive.store(0);
        pthread_join(inputDeviceHandle->thread, nullptr);
    }

    snd_seq_addr_t sender;
    sender.client = inputDeviceHandle->info->client;
    sender.port = inputDeviceHandle->info->port;

    int err = snd_seq_disconnect_from(inputDeviceHandle->seq, inputDeviceHandle->port, sender.client, sender.port);
    if (err < 0)
    {
        *errorCode = err;
        return IN_DISCONNECTRESULT_UNKNOWNERROR;
    }

    return IN_DISCONNECTRESULT_OK;
}

API_EXPORT IN_GETEVENTDATARESULT GetEventDataFromInputDevice(snd_seq_event_t* ev, int packetIndex, unsigned char** data, int* length, int* packetsCount)
{
    *packetsCount = 1; // ALSA delivers one event at a time

    if (packetIndex != 0)
        return IN_GETEVENTDATARESULT_OK;

    if (ev->type == SND_SEQ_EVENT_SYSEX)
    {
        *data = static_cast<unsigned char*>(ev->data.ext.ptr);
        *length = ev->data.ext.len;
    }
    else
    {
        // Convert ALSA event to MIDI bytes
        static unsigned char midi_buffer[3];
        int len = 0;

        switch (ev->type)
        {
        case SND_SEQ_EVENT_NOTEOFF:
            midi_buffer[0] = 0x80 | ev->data.note.channel;
            midi_buffer[1] = ev->data.note.note;
            midi_buffer[2] = ev->data.note.velocity;
            len = 3;
            break;
        case SND_SEQ_EVENT_NOTEON:
            midi_buffer[0] = 0x90 | ev->data.note.channel;
            midi_buffer[1] = ev->data.note.note;
            midi_buffer[2] = ev->data.note.velocity;
            len = 3;
            break;
        case SND_SEQ_EVENT_KEYPRESS:
            midi_buffer[0] = 0xA0 | ev->data.note.channel;
            midi_buffer[1] = ev->data.note.note;
            midi_buffer[2] = ev->data.note.velocity;
            len = 3;
            break;
        case SND_SEQ_EVENT_CONTROLLER:
            midi_buffer[0] = 0xB0 | ev->data.control.channel;
            midi_buffer[1] = ev->data.control.param;
            midi_buffer[2] = ev->data.control.value;
            len = 3;
            break;
        case SND_SEQ_EVENT_PGMCHANGE:
            midi_buffer[0] = 0xC0 | ev->data.control.channel;
            midi_buffer[1] = ev->data.control.value;
            len = 2;
            break;
        case SND_SEQ_EVENT_CHANPRESS:
            midi_buffer[0] = 0xD0 | ev->data.control.channel;
            midi_buffer[1] = ev->data.control.value;
            len = 2;
            break;
        case SND_SEQ_EVENT_PITCHBEND:
            midi_buffer[0] = 0xE0 | ev->data.control.channel;
            midi_buffer[1] = (ev->data.control.value + 8192) & 0x7F;
            midi_buffer[2] = ((ev->data.control.value + 8192) >> 7) & 0x7F;
            len = 3;
            break;
        default:
            len = 0;
            break;
        }

        *data = midi_buffer;
        *length = len;
    }

    return IN_GETEVENTDATARESULT_OK;
}

API_EXPORT char IsInputDevicePropertySupported(IN_PROPERTY property)
{
    switch (property)
    {
    case IN_PROPERTY_PRODUCT:
    case IN_PROPERTY_MANUFACTURER:
    case IN_PROPERTY_UNIQUEID:
        return 1;
    case IN_PROPERTY_DRIVERVERSION:
    case IN_PROPERTY_DRIVEROWNER:
        return 0;
    }

    return 0;
}

/* ================================
   Output device
 ================================ */

struct OutputDeviceHandle
{
    OutputDeviceInfo* info;
    snd_seq_t* seq;
    int port;
};

API_EXPORT OUT_GETCOUNTRESULT GetOutputDevicesCount(int* count)
{
    snd_seq_t* seq;
    if (snd_seq_open(&seq, "default", SND_SEQ_OPEN_DUPLEX, 0) < 0)
    {
        *count = 0;
        return OUT_GETCOUNTRESULT_OK;
    }

    snd_seq_client_info_t* cinfo;
    snd_seq_port_info_t* pinfo;
    snd_seq_client_info_alloca(&cinfo);
    snd_seq_port_info_alloca(&pinfo);

    int deviceCount = 0;

    snd_seq_client_info_set_client(cinfo, -1);
    while (snd_seq_query_next_client(seq, cinfo) >= 0)
    {
        int client = snd_seq_client_info_get_client(cinfo);

        snd_seq_port_info_set_client(pinfo, client);
        snd_seq_port_info_set_port(pinfo, -1);
        while (snd_seq_query_next_port(seq, pinfo) >= 0)
        {
            unsigned int caps = snd_seq_port_info_get_capability(pinfo);
            unsigned int type = snd_seq_port_info_get_type(pinfo);

            if (!(type & SND_SEQ_PORT_TYPE_MIDI_GENERIC))
                continue;

            if ((caps & SND_SEQ_PORT_CAP_WRITE) && (caps & SND_SEQ_PORT_CAP_SUBS_WRITE))
            {
                deviceCount++;
            }
        }
    }

    snd_seq_close(seq);
    *count = deviceCount;
    return OUT_GETCOUNTRESULT_OK;
}

API_EXPORT OUT_GETINFORESULT GetOutputDeviceInfo(int deviceIndex, void** info, int* errorCode)
{
    *errorCode = 0;

    snd_seq_t* seq;
    if (snd_seq_open(&seq, "default", SND_SEQ_OPEN_DUPLEX, 0) < 0)
    {
        return OUT_GETINFORESULT_UNKNOWNERROR;
    }

    snd_seq_client_info_t* cinfo;
    snd_seq_port_info_t* pinfo;
    snd_seq_client_info_alloca(&cinfo);
    snd_seq_port_info_alloca(&pinfo);

    int currentIndex = 0;

    snd_seq_client_info_set_client(cinfo, -1);
    while (snd_seq_query_next_client(seq, cinfo) >= 0)
    {
        int client = snd_seq_client_info_get_client(cinfo);

        snd_seq_port_info_set_client(pinfo, client);
        snd_seq_port_info_set_port(pinfo, -1);
        while (snd_seq_query_next_port(seq, pinfo) >= 0)
        {
            unsigned int caps = snd_seq_port_info_get_capability(pinfo);
            unsigned int type = snd_seq_port_info_get_type(pinfo);

            if (!(type & SND_SEQ_PORT_TYPE_MIDI_GENERIC))
                continue;

            if ((caps & SND_SEQ_PORT_CAP_WRITE) && (caps & SND_SEQ_PORT_CAP_SUBS_WRITE))
            {
                if (currentIndex == deviceIndex)
                {
                    OutputDeviceInfo* outputDeviceInfo = new OutputDeviceInfo();
                    outputDeviceInfo->client = client;
                    outputDeviceInfo->port = snd_seq_port_info_get_port(pinfo);
                    outputDeviceInfo->name = snd_seq_port_info_get_name(pinfo);
                    outputDeviceInfo->manufacturer = snd_seq_client_info_get_name(cinfo);
                    outputDeviceInfo->model = snd_seq_port_info_get_name(pinfo);

                    *info = outputDeviceInfo;
                    snd_seq_close(seq);
                    return OUT_GETINFORESULT_OK;
                }
                currentIndex++;
            }
        }
    }

    snd_seq_close(seq);
    return OUT_GETINFORESULT_UNKNOWNERROR;
}

API_EXPORT int GetOutputDeviceHashCode(void* info)
{
    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);
    return (outputDeviceInfo->client << 16) | outputDeviceInfo->port;
}

API_EXPORT char AreOutputDevicesEqual(void* info1, void* info2)
{
    OutputDeviceInfo* outputDeviceInfo1 = static_cast<OutputDeviceInfo*>(info1);
    OutputDeviceInfo* outputDeviceInfo2 = static_cast<OutputDeviceInfo*>(info2);

    return static_cast<char>(outputDeviceInfo1->client == outputDeviceInfo2->client &&
        outputDeviceInfo1->port == outputDeviceInfo2->port);
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputDeviceName(void* info, char** value, int* errorCode)
{
    *errorCode = 0;
    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);

    char* buffer = new char[PROPERTY_VALUE_BUFFER_SIZE];
    strncpy(buffer, outputDeviceInfo->name.c_str(), PROPERTY_VALUE_BUFFER_SIZE - 1);
    buffer[PROPERTY_VALUE_BUFFER_SIZE - 1] = '\0';

    *value = buffer;
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputDeviceManufacturer(void* info, char** value, int* errorCode)
{
    *errorCode = 0;
    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);

    char* buffer = new char[PROPERTY_VALUE_BUFFER_SIZE];
    strncpy(buffer, outputDeviceInfo->manufacturer.c_str(), PROPERTY_VALUE_BUFFER_SIZE - 1);
    buffer[PROPERTY_VALUE_BUFFER_SIZE - 1] = '\0';

    *value = buffer;
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputDeviceProduct(void* info, char** value, int* errorCode)
{
    *errorCode = 0;
    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);

    char* buffer = new char[PROPERTY_VALUE_BUFFER_SIZE];
    strncpy(buffer, outputDeviceInfo->model.c_str(), PROPERTY_VALUE_BUFFER_SIZE - 1);
    buffer[PROPERTY_VALUE_BUFFER_SIZE - 1] = '\0';

    *value = buffer;
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputDeviceDriverVersion(void* info, int* value, int* errorCode)
{
    *errorCode = 0;
    *value = 0;
    return OUT_GETPROPERTYRESULT_UNKNOWNPROPERTY;
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputDeviceUniqueId(void* info, int* value, int* errorCode)
{
    *errorCode = 0;
    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);
    *value = (outputDeviceInfo->client << 16) | outputDeviceInfo->port;
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputDeviceDriverOwner(void* info, char** value, int* errorCode)
{
    *errorCode = 0;
    *value = nullptr;
    return OUT_GETPROPERTYRESULT_UNKNOWNPROPERTY;
}

API_EXPORT OUT_OPENRESULT OpenOutputDevice_Linux(void* info, void* sessionHandle, void** handle, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);
    SessionHandle* pSessionHandle = static_cast<SessionHandle*>(sessionHandle);

    OutputDeviceHandle* outputDeviceHandle = new OutputDeviceHandle();
    outputDeviceHandle->info = outputDeviceInfo;
    outputDeviceHandle->seq = pSessionHandle->seq;

    outputDeviceHandle->port = snd_seq_create_simple_port(pSessionHandle->seq, "OUT",
        SND_SEQ_PORT_CAP_READ | SND_SEQ_PORT_CAP_SUBS_READ,
        SND_SEQ_PORT_TYPE_MIDI_GENERIC | SND_SEQ_PORT_TYPE_APPLICATION);

    if (outputDeviceHandle->port < 0)
    {
        delete outputDeviceHandle;
        *errorCode = outputDeviceHandle->port;
        return OUT_OPENRESULT_UNKNOWNERROR;
    }

    // Connect to destination
    int err = snd_seq_connect_to(pSessionHandle->seq, outputDeviceHandle->port,
        outputDeviceInfo->client, outputDeviceInfo->port);
    if (err < 0)
    {
        snd_seq_delete_simple_port(pSessionHandle->seq, outputDeviceHandle->port);
        delete outputDeviceHandle;
        *errorCode = err;
        return OUT_OPENRESULT_UNKNOWNERROR;
    }

    *handle = outputDeviceHandle;
    return OUT_OPENRESULT_OK;
}

API_EXPORT OUT_CLOSERESULT CloseOutputDevice(void* handle, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceHandle* outputDeviceHandle = static_cast<OutputDeviceHandle*>(handle);

    if (outputDeviceHandle->port >= 0)
        snd_seq_delete_simple_port(outputDeviceHandle->seq, outputDeviceHandle->port);

    delete outputDeviceHandle->info;
    delete outputDeviceHandle;

    return OUT_CLOSERESULT_OK;
}

API_EXPORT OUT_SENDSHORTRESULT SendShortEventToOutputDevice(void* handle, int message, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceHandle* outputDeviceHandle = static_cast<OutputDeviceHandle*>(handle);

    snd_seq_event_t ev;
    snd_seq_ev_clear(&ev);
    snd_seq_ev_set_source(&ev, outputDeviceHandle->port);
    snd_seq_ev_set_subs(&ev);
    snd_seq_ev_set_direct(&ev);

    unsigned char statusByte = static_cast<unsigned char>(message & 0xFF);
    unsigned char data1 = static_cast<unsigned char>((message >> 8) & 0xFF);
    unsigned char data2 = static_cast<unsigned char>((message >> 16) & 0xFF);
    unsigned char channel = statusByte & 0x0F;
    unsigned char command = statusByte & 0xF0;

    switch (command)
    {
    case 0x80: // Note Off
        snd_seq_ev_set_noteoff(&ev, channel, data1, data2);
        break;
    case 0x90: // Note On
        snd_seq_ev_set_noteon(&ev, channel, data1, data2);
        break;
    case 0xA0: // Polyphonic Key Pressure
        snd_seq_ev_set_keypress(&ev, channel, data1, data2);
        break;
    case 0xB0: // Control Change
        snd_seq_ev_set_controller(&ev, channel, data1, data2);
        break;
    case 0xC0: // Program Change
        snd_seq_ev_set_pgmchange(&ev, channel, data1);
        break;
    case 0xD0: // Channel Pressure
        snd_seq_ev_set_chanpress(&ev, channel, data1);
        break;
    case 0xE0: // Pitch Bend
    {
        int value = (data2 << 7) | data1;
        value -= 8192;
        snd_seq_ev_set_pitchbend(&ev, channel, value);
        break;
    }
    default:
        // System messages - send as raw data
        ev.type = SND_SEQ_EVENT_NONE;
        break;
    }

    if (ev.type != SND_SEQ_EVENT_NONE)
    {
        int err = snd_seq_event_output_direct(outputDeviceHandle->seq, &ev);
        if (err < 0)
        {
            *errorCode = err;
            return OUT_SENDSHORTRESULT_UNKNOWNERROR;
        }
    }

    return OUT_SENDSHORTRESULT_OK;
}

API_EXPORT OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Linux(void* handle, unsigned char* data, int dataSize, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceHandle* outputDeviceHandle = static_cast<OutputDeviceHandle*>(handle);

    snd_seq_event_t ev;
    snd_seq_ev_clear(&ev);
    snd_seq_ev_set_source(&ev, outputDeviceHandle->port);
    snd_seq_ev_set_subs(&ev);
    snd_seq_ev_set_direct(&ev);
    snd_seq_ev_set_sysex(&ev, dataSize, data);

    int err = snd_seq_event_output_direct(outputDeviceHandle->seq, &ev);
    if (err < 0)
    {
        *errorCode = err;
        return OUT_SENDSYSEXRESULT_UNKNOWNERROR;
    }

    return OUT_SENDSYSEXRESULT_OK;
}

API_EXPORT char IsOutputDevicePropertySupported(OUT_PROPERTY property)
{
    switch (property)
    {
    case OUT_PROPERTY_PRODUCT:
    case OUT_PROPERTY_MANUFACTURER:
    case OUT_PROPERTY_UNIQUEID:
        return 1;
    case OUT_PROPERTY_DRIVERVERSION:
    case OUT_PROPERTY_DRIVEROWNER:
        return 0;
    }

    return 0;
}

/* ================================
 Virtual device
 ================================ */

struct VirtualDeviceInfo
{
    InputDeviceInfo* inputDeviceInfo;
    OutputDeviceInfo* outputDeviceInfo;
    char* name;
    snd_seq_t* seq;
    int inputPort;  // Source port (for sending)
    int outputPort; // Destination port (for receiving)
    pthread_t thread;
    std::atomic<char> threadActive;
    void (*callback)(snd_seq_event_t*, void*);
};

void* VirtualDeviceThreadProc(void* data)
{
    VirtualDeviceInfo* virtualDeviceInfo = static_cast<VirtualDeviceInfo*>(data);

    int npfds = snd_seq_poll_descriptors_count(virtualDeviceInfo->seq, POLLIN);
    struct pollfd* pfds = new struct pollfd[npfds];
    snd_seq_poll_descriptors(virtualDeviceInfo->seq, pfds, npfds, POLLIN);

    while (virtualDeviceInfo->threadActive.load() == 1)
    {
        if (poll(pfds, npfds, 1000) > 0)
        {
            snd_seq_event_t* ev;
            while (snd_seq_event_input(virtualDeviceInfo->seq, &ev) > 0)
            {
                if (ev && ev->dest.port == virtualDeviceInfo->outputPort)
                {
                    if (virtualDeviceInfo->callback)
                    {
                        virtualDeviceInfo->callback(ev, virtualDeviceInfo->inputDeviceInfo);
                    }
                }
                snd_seq_free_event(ev);
            }
        }
    }

    delete[] pfds;
    return nullptr;
}

API_EXPORT VIRTUAL_OPENRESULT OpenVirtualDevice_Linux(char* name, void* sessionHandle, void (*callback)(snd_seq_event_t*, void*), void** info, int* errorCode)
{
    *errorCode = 0;

    SessionHandle* pSessionHandle = static_cast<SessionHandle*>(sessionHandle);

    VirtualDeviceInfo* virtualDeviceInfo = new VirtualDeviceInfo();
    virtualDeviceInfo->name = name;
    virtualDeviceInfo->seq = pSessionHandle->seq;
    virtualDeviceInfo->callback = callback;
    virtualDeviceInfo->threadActive.store(0);

    // Create input port (source) - others read from this
    char inputName[256];
    snprintf(inputName, sizeof(inputName), "%s In", name);
    virtualDeviceInfo->inputPort = snd_seq_create_simple_port(pSessionHandle->seq, inputName,
        SND_SEQ_PORT_CAP_READ | SND_SEQ_PORT_CAP_SUBS_READ,
        SND_SEQ_PORT_TYPE_MIDI_GENERIC | SND_SEQ_PORT_TYPE_APPLICATION);

    if (virtualDeviceInfo->inputPort < 0)
    {
        delete virtualDeviceInfo;
        *errorCode = virtualDeviceInfo->inputPort;
        return VIRTUAL_OPENRESULT_CREATESOURCE_UNKNOWNERROR;
    }

    InputDeviceInfo* inputDeviceInfo = new InputDeviceInfo();
    inputDeviceInfo->client = snd_seq_client_id(pSessionHandle->seq);
    inputDeviceInfo->port = virtualDeviceInfo->inputPort;
    inputDeviceInfo->name = inputName;
    inputDeviceInfo->manufacturer = "Virtual";
    inputDeviceInfo->model = name;
    virtualDeviceInfo->inputDeviceInfo = inputDeviceInfo;

    // Create output port (destination) - others write to this
    char outputName[256];
    snprintf(outputName, sizeof(outputName), "%s Out", name);
    virtualDeviceInfo->outputPort = snd_seq_create_simple_port(pSessionHandle->seq, outputName,
        SND_SEQ_PORT_CAP_WRITE | SND_SEQ_PORT_CAP_SUBS_WRITE,
        SND_SEQ_PORT_TYPE_MIDI_GENERIC | SND_SEQ_PORT_TYPE_APPLICATION);

    if (virtualDeviceInfo->outputPort < 0)
    {
        snd_seq_delete_simple_port(pSessionHandle->seq, virtualDeviceInfo->inputPort);
        delete inputDeviceInfo;
        delete virtualDeviceInfo;
        *errorCode = virtualDeviceInfo->outputPort;
        return VIRTUAL_OPENRESULT_CREATEDESTINATION_UNKNOWNERROR;
    }

    OutputDeviceInfo* outputDeviceInfo = new OutputDeviceInfo();
    outputDeviceInfo->client = snd_seq_client_id(pSessionHandle->seq);
    outputDeviceInfo->port = virtualDeviceInfo->outputPort;
    outputDeviceInfo->name = outputName;
    outputDeviceInfo->manufacturer = "Virtual";
    outputDeviceInfo->model = name;
    virtualDeviceInfo->outputDeviceInfo = outputDeviceInfo;

    // Start event handling thread
    virtualDeviceInfo->threadActive.store(1);
    if (pthread_create(&virtualDeviceInfo->thread, nullptr, VirtualDeviceThreadProc, virtualDeviceInfo) != 0)
    {
        snd_seq_delete_simple_port(pSessionHandle->seq, virtualDeviceInfo->inputPort);
        snd_seq_delete_simple_port(pSessionHandle->seq, virtualDeviceInfo->outputPort);
        delete inputDeviceInfo;
        delete outputDeviceInfo;
        delete virtualDeviceInfo;
        *errorCode = errno;
        return VIRTUAL_OPENRESULT_CREATEDESTINATION_UNKNOWNERROR;
    }

    *info = virtualDeviceInfo;

    return VIRTUAL_OPENRESULT_OK;
}

API_EXPORT VIRTUAL_CLOSERESULT CloseVirtualDevice(void* info, int* errorCode)
{
    *errorCode = 0;

    VirtualDeviceInfo* virtualDeviceInfo = static_cast<VirtualDeviceInfo*>(info);

    if (virtualDeviceInfo->threadActive.load() == 1)
    {
        virtualDeviceInfo->threadActive.store(0);
        pthread_join(virtualDeviceInfo->thread, nullptr);
    }

    snd_seq_delete_simple_port(virtualDeviceInfo->seq, virtualDeviceInfo->inputPort);
    snd_seq_delete_simple_port(virtualDeviceInfo->seq, virtualDeviceInfo->outputPort);

    delete virtualDeviceInfo->inputDeviceInfo;
    delete virtualDeviceInfo->outputDeviceInfo;
    delete virtualDeviceInfo;

    return VIRTUAL_CLOSERESULT_OK;
}

API_EXPORT VIRTUAL_SENDBACKRESULT SendDataBackFromVirtualDevice(snd_seq_event_t* ev, void* readProcRefCon, int* errorCode)
{
    *errorCode = 0;

    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(readProcRefCon);
    VirtualDeviceInfo* virtualDeviceInfo = reinterpret_cast<VirtualDeviceInfo*>(
        reinterpret_cast<char*>(inputDeviceInfo) - offsetof(VirtualDeviceInfo, inputDeviceInfo));

    // Forward event to input port
    snd_seq_event_t evCopy;
    snd_seq_ev_clear(&evCopy);
    evCopy = *ev;
    snd_seq_ev_set_source(&evCopy, virtualDeviceInfo->inputPort);
    snd_seq_ev_set_subs(&evCopy);
    snd_seq_ev_set_direct(&evCopy);

    int err = snd_seq_event_output_direct(virtualDeviceInfo->seq, &evCopy);
    if (err < 0)
    {
        *errorCode = err;
        return VIRTUAL_SENDBACKRESULT_UNKNOWNERROR;
    }

    return VIRTUAL_SENDBACKRESULT_OK;
}

API_EXPORT void* GetInputDeviceInfoFromVirtualDevice(void* info)
{
    VirtualDeviceInfo* virtualDeviceInfo = static_cast<VirtualDeviceInfo*>(info);
    return virtualDeviceInfo->inputDeviceInfo;
}

API_EXPORT void* GetOutputDeviceInfoFromVirtualDevice(void* info)
{
    VirtualDeviceInfo* virtualDeviceInfo = static_cast<VirtualDeviceInfo*>(info);
    return virtualDeviceInfo->outputDeviceInfo;
}