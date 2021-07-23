#include <CoreFoundation/CoreFoundation.h>
#include <CoreMIDI/CoreMIDI.h>
#include <pthread.h>
#include <mach/mach_time.h>

#include "NativeApi-Constants.h"

#define PROPERTY_VALUE_BUFFER_SIZE 256
#define SMALL_BUFFER_ERROR 10000

API_TYPE GetApiType()
{
    return API_TYPE_APPLE;
}

/* ================================
 High-precision tick generator
 ================================ */

typedef struct
{
    pthread_t thread;
    void (*callback)(void);
    char active;
    int interval;
} TickGeneratorInfo;

uint64_t GetTimeInMilliseconds()
{
    struct mach_timebase_info convfact;
    mach_timebase_info(&convfact);
    uint64_t tick = mach_absolute_time();
    return (tick * convfact.numer) / (convfact.denom * 1000000);
}

void* RunLoopThreadRoutine(void* data)
{
    TickGeneratorInfo* tickGeneratorInfo = (TickGeneratorInfo*)data;

    uint64_t lastMs = GetTimeInMilliseconds();

    tickGeneratorInfo->active = 1;

    while (tickGeneratorInfo->active == 1)
    {
        uint64_t ms = GetTimeInMilliseconds();
        if (ms - lastMs < tickGeneratorInfo->interval)
            continue;

        lastMs = ms;
        tickGeneratorInfo->callback();
    }

    free(data);

    return NULL;
}

TG_STARTRESULT StartHighPrecisionTickGenerator_Apple(int interval, void (*callback)(void), TickGeneratorInfo** info)
{
    TickGeneratorInfo* tickGeneratorInfo = malloc(sizeof(TickGeneratorInfo));

    tickGeneratorInfo->active = 0;
    tickGeneratorInfo->interval = interval;
    tickGeneratorInfo->callback = callback;

    *info = tickGeneratorInfo;

    pthread_create(&tickGeneratorInfo->thread, NULL, RunLoopThreadRoutine, tickGeneratorInfo);

    while (tickGeneratorInfo->active == 0) {}

    return TG_STARTRESULT_OK;
}

TG_STOPRESULT StopHighPrecisionTickGenerator(void* info)
{
    TickGeneratorInfo* tickGeneratorInfo = (TickGeneratorInfo*)info;

    tickGeneratorInfo->active = 0;

    return TG_STOPRESULT_OK;
}

/* ================================
 Devices common
 ================================ */

typedef struct
{
    char* name;
    MIDIClientRef clientRef;
} SessionHandle;

SESSION_OPENRESULT OpenSession(char* name, void** handle)
{
    SessionHandle* sessionHandle = malloc(sizeof(SessionHandle));
    sessionHandle->name = name;

    CFStringRef nameRef = CFStringCreateWithCString(kCFAllocatorDefault, name, kCFStringEncodingUTF8);
    OSStatus status = MIDIClientCreate(nameRef, NULL, NULL, &sessionHandle->clientRef);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIServerStartErr: return SESSION_OPENRESULT_SERVERSTARTERROR;
            case kMIDIWrongThread: return SESSION_OPENRESULT_WRONGTHREAD;
            case kMIDINotPermitted: return SESSION_OPENRESULT_NOTPERMITTED;
            case kMIDIUnknownError: return SESSION_OPENRESULT_UNKNOWNERROR;
        }
    }

    *handle = sessionHandle;

    return SESSION_OPENRESULT_OK;
}

SESSION_CLOSERESULT CloseSession(void* handle)
{
    SessionHandle* sessionHandle = (SessionHandle*)handle;
    free(sessionHandle);
    return SESSION_CLOSERESULT_OK;
}

OSStatus GetDevicePropertyValue(MIDIEndpointRef endpointRef, CFStringRef propertyID, char** value)
{
    CFStringRef stringRef;
    OSStatus status = MIDIObjectGetStringProperty(endpointRef, propertyID, &stringRef);
    if (status == noErr)
    {
        char* buffer = malloc(PROPERTY_VALUE_BUFFER_SIZE * sizeof(char));
        if (!CFStringGetCString(stringRef, buffer, PROPERTY_VALUE_BUFFER_SIZE, kCFStringEncodingUTF8))
            return SMALL_BUFFER_ERROR;

        *value = buffer;
    }

    return status;
}

OSStatus GetDeviceDriverVersion(MIDIEndpointRef endpointRef, int* value)
{
    SInt32 driverVersion;
    OSStatus status = MIDIObjectGetIntegerProperty(endpointRef, kMIDIPropertyDriverVersion, &driverVersion);

    *value = driverVersion;

    return status;
}

/* ================================
 Input device
 ================================ */

typedef struct
{
    MIDIEndpointRef endpointRef;
    char* name;
    char* manufacturer;
    char* product;
    int driverVersion;
} InputDeviceInfo;

typedef struct
{
    InputDeviceInfo* info;
    MIDIPortRef portRef;
} InputDeviceHandle;

int GetInputDevicesCount()
{
    return (int)MIDIGetNumberOfSources();
}

IN_GETINFORESULT GetInputDeviceInfo(int deviceIndex, void** info)
{
    InputDeviceInfo* inputDeviceInfo = malloc(sizeof(InputDeviceInfo));

    MIDIEndpointRef endpointRef = MIDIGetSource(deviceIndex);
    inputDeviceInfo->endpointRef = endpointRef;

    OSStatus status = GetDevicePropertyValue(endpointRef, kMIDIPropertyDisplayName, &inputDeviceInfo->name);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownEndpoint: return IN_GETINFORESULT_NAME_UNKNOWNENDPOINT;
            case SMALL_BUFFER_ERROR: return IN_GETINFORESULT_NAME_TOOLONG;
            case kMIDIUnknownProperty:
                inputDeviceInfo->name = NULL;
                break;
            default: return IN_GETINFORESULT_UNKNOWNERROR;
        }
    }

    status = GetDevicePropertyValue(endpointRef, kMIDIPropertyManufacturer, &inputDeviceInfo->manufacturer);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownEndpoint: return IN_GETINFORESULT_MANUFACTURER_UNKNOWNENDPOINT;
            case SMALL_BUFFER_ERROR: return IN_GETINFORESULT_MANUFACTURER_TOOLONG;
            case kMIDIUnknownProperty:
                inputDeviceInfo->manufacturer = NULL;
                break;
            default: return IN_GETINFORESULT_UNKNOWNERROR;
        }
    }

    status = GetDevicePropertyValue(endpointRef, kMIDIPropertyModel, &inputDeviceInfo->product);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownEndpoint: return IN_GETINFORESULT_PRODUCT_UNKNOWNENDPOINT;
            case SMALL_BUFFER_ERROR: return IN_GETINFORESULT_PRODUCT_TOOLONG;
            case kMIDIUnknownProperty:
                inputDeviceInfo->product = NULL;
                break;
            default: return IN_GETINFORESULT_UNKNOWNERROR;
        }
    }

    status = GetDeviceDriverVersion(endpointRef, &inputDeviceInfo->driverVersion);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownEndpoint: return IN_GETINFORESULT_DRIVERVERSION_UNKNOWNENDPOINT;
            case kMIDIUnknownProperty:
                inputDeviceInfo->driverVersion = 0;
                break;
            default: return IN_GETINFORESULT_UNKNOWNERROR;
        }
    }

    *info = inputDeviceInfo;

    return IN_GETINFORESULT_OK;
}

char* GetInputDeviceName(void* info)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    return inputDeviceInfo->name;
}

char* GetInputDeviceManufacturer(void* info)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    return inputDeviceInfo->manufacturer;
}

char* GetInputDeviceProduct(void* info)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    return inputDeviceInfo->product;
}

int GetInputDeviceDriverVersion(void* info)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    return inputDeviceInfo->driverVersion;
}

IN_OPENRESULT OpenInputDevice_Apple(void* info, void* sessionHandle, MIDIReadProc callback, void** handle)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    SessionHandle* pSessionHandle = (SessionHandle*)sessionHandle;

    InputDeviceHandle* inputDeviceHandle = malloc(sizeof(InputDeviceHandle));
    inputDeviceHandle->info = inputDeviceInfo;

    *handle = inputDeviceHandle;

    CFStringRef portNameRef = CFSTR("IN");
    OSStatus status = MIDIInputPortCreate(pSessionHandle->clientRef, portNameRef, callback, NULL, &inputDeviceHandle->portRef);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIInvalidClient: return IN_OPENRESULT_INVALIDCLIENT;
            case kMIDIWrongThread: return IN_OPENRESULT_WRONGTHREAD;
            case kMIDINotPermitted: return IN_OPENRESULT_NOTPERMITTED;
            case kMIDIUnknownError: return IN_OPENRESULT_UNKNOWNERROR;
        }
    }

    return IN_OPENRESULT_OK;
}

IN_CLOSERESULT CloseInputDevice(void* handle)
{
    InputDeviceHandle* inputDeviceHandle = (InputDeviceHandle*)handle;

    free(inputDeviceHandle->info);
    free(inputDeviceHandle);

    return IN_CLOSERESULT_OK;
}

IN_CONNECTRESULT ConnectToInputDevice(void* handle)
{
    InputDeviceHandle* inputDeviceHandle = (InputDeviceHandle*)handle;

    OSStatus status = MIDIPortConnectSource(inputDeviceHandle->portRef, inputDeviceHandle->info->endpointRef, NULL);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownError: return IN_CONNECTRESULT_UNKNOWNERROR;
            case kMIDIInvalidPort: return IN_CONNECTRESULT_INVALIDPORT;
            case kMIDIWrongThread: return IN_CONNECTRESULT_WRONGTHREAD;
            case kMIDINotPermitted: return IN_CONNECTRESULT_NOTPERMITTED;
            case kMIDIUnknownEndpoint: return IN_CONNECTRESULT_UNKNOWNENDPOINT;
            case kMIDIWrongEndpointType: return IN_CONNECTRESULT_WRONGENDPOINT;
        }
    }

    return IN_CONNECTRESULT_OK;
}

IN_DISCONNECTRESULT DisconnectFromInputDevice(void* handle)
{
    InputDeviceHandle* inputDeviceHandle = (InputDeviceHandle*)handle;

    OSStatus status = MIDIPortDisconnectSource(inputDeviceHandle->portRef, inputDeviceHandle->info->endpointRef);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownError: return IN_DISCONNECTRESULT_UNKNOWNERROR;
            case kMIDIInvalidPort: return IN_DISCONNECTRESULT_INVALIDPORT;
            case kMIDIWrongThread: return IN_DISCONNECTRESULT_WRONGTHREAD;
            case kMIDINotPermitted: return IN_DISCONNECTRESULT_NOTPERMITTED;
            case kMIDIUnknownEndpoint: return IN_DISCONNECTRESULT_UNKNOWNENDPOINT;
            case kMIDIWrongEndpointType: return IN_DISCONNECTRESULT_WRONGENDPOINT;
            case kMIDINoConnection: return IN_DISCONNECTRESULT_NOCONNECTION;
        }
    }

    return IN_DISCONNECTRESULT_OK;
}

IN_GETEVENTDATARESULT GetEventDataFromInputDevice(MIDIPacketList* packetList, int packetIndex, Byte** data, int* length)
{
    if (packetIndex == 0)
    {
        *data = packetList->packet[0].data;
        *length = packetList->packet[0].length;
        return IN_GETEVENTDATARESULT_OK;
    }

    MIDIPacket* packetPtr = &packetList->packet[0];

    for (int i = 0; i < packetIndex; i++)
    {
        packetPtr = MIDIPacketNext(packetPtr);
    }

    *data = packetPtr->data;
    *length = packetPtr->length;

    return IN_GETEVENTDATARESULT_OK;
}

/* ================================
 Output device
 ================================ */

typedef struct
{
    MIDIEndpointRef endpointRef;
    char* name;
    char* manufacturer;
    char* product;
    int driverVersion;
} OutputDeviceInfo;

typedef struct
{
    OutputDeviceInfo* info;
    MIDIPortRef portRef;
} OutputDeviceHandle;

int GetOutputDevicesCount()
{
    return (int)MIDIGetNumberOfDestinations();
}

OUT_GETINFORESULT GetOutputDeviceInfo(int deviceIndex, void** info)
{
    OutputDeviceInfo* outputDeviceInfo = malloc(sizeof(OutputDeviceInfo));

    MIDIEndpointRef endpointRef = MIDIGetDestination(deviceIndex);
    outputDeviceInfo->endpointRef = endpointRef;

    OSStatus status = GetDevicePropertyValue(endpointRef, kMIDIPropertyDisplayName, &outputDeviceInfo->name);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownEndpoint: return OUT_GETINFORESULT_NAME_UNKNOWNENDPOINT;
            case SMALL_BUFFER_ERROR: return OUT_GETINFORESULT_NAME_TOOLONG;
            case kMIDIUnknownProperty:
                outputDeviceInfo->name = NULL;
                break;
            default: return OUT_GETINFORESULT_UNKNOWNERROR;
        }
    }

    status = GetDevicePropertyValue(endpointRef, kMIDIPropertyManufacturer, &outputDeviceInfo->manufacturer);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownEndpoint: return OUT_GETINFORESULT_MANUFACTURER_UNKNOWNENDPOINT;
            case SMALL_BUFFER_ERROR: return OUT_GETINFORESULT_MANUFACTURER_TOOLONG;
            case kMIDIUnknownProperty:
                outputDeviceInfo->manufacturer = NULL;
                break;
            default: return OUT_GETINFORESULT_UNKNOWNERROR;
        }
    }

    status = GetDevicePropertyValue(endpointRef, kMIDIPropertyModel, &outputDeviceInfo->product);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownEndpoint: return OUT_GETINFORESULT_PRODUCT_UNKNOWNENDPOINT;
            case SMALL_BUFFER_ERROR: return OUT_GETINFORESULT_PRODUCT_TOOLONG;
            case kMIDIUnknownProperty:
                outputDeviceInfo->product = NULL;
                break;
            default: return OUT_GETINFORESULT_UNKNOWNERROR;
        }
    }

    status = GetDeviceDriverVersion(endpointRef, &outputDeviceInfo->driverVersion);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownEndpoint: return OUT_GETINFORESULT_DRIVERVERSION_UNKNOWNENDPOINT;
            case kMIDIUnknownProperty:
                outputDeviceInfo->driverVersion = 0;
                break;
            default: return OUT_GETINFORESULT_UNKNOWNERROR;
        }
    }

    *info = outputDeviceInfo;

    return OUT_GETINFORESULT_OK;
}

char* GetOutputDeviceName(void* info)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    return outputDeviceInfo->name;
}

char* GetOutputDeviceManufacturer(void* info)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    return outputDeviceInfo->manufacturer;
}

char* GetOutputDeviceProduct(void* info)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    return outputDeviceInfo->product;
}

int GetOutputDeviceDriverVersion(void* info)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    return outputDeviceInfo->driverVersion;
}

OUT_OPENRESULT OpenOutputDevice_Apple(void* info, void* sessionHandle, void** handle)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    SessionHandle* pSessionHandle = (SessionHandle*)sessionHandle;

    OutputDeviceHandle* outputDeviceHandle = malloc(sizeof(OutputDeviceHandle));
    outputDeviceHandle->info = outputDeviceInfo;

    *handle = outputDeviceHandle;

    CFStringRef portNameRef = CFSTR("OUT");
    OSStatus result = MIDIOutputPortCreate(pSessionHandle->clientRef, portNameRef, &outputDeviceHandle->portRef);
    if (result != noErr)
    {
        switch (result)
        {
            case kMIDIInvalidClient: return OUT_OPENRESULT_INVALIDCLIENT;
            case kMIDIWrongThread: return OUT_OPENRESULT_WRONGTHREAD;
            case kMIDINotPermitted: return OUT_OPENRESULT_NOTPERMITTED;
            case kMIDIUnknownError: return OUT_OPENRESULT_UNKNOWNERROR;
        }
    }

    return OUT_OPENRESULT_OK;
}

OUT_CLOSERESULT CloseOutputDevice(void* handle)
{
    OutputDeviceHandle* outputDeviceHandle = (OutputDeviceHandle*)handle;

    free(outputDeviceHandle->info);
    free(outputDeviceHandle);

    return OUT_CLOSERESULT_OK;
}

OUT_SENDSHORTRESULT SendShortEventToOutputDevice(void* handle, int message)
{
    OutputDeviceHandle* outputDeviceHandle = (OutputDeviceHandle*)handle;

    Byte data[3];
    Byte statusByte = (Byte)(message & 0xFF);
    data[0] = statusByte;
    ByteCount dataSize = 1;

    if (statusByte < 0xF8 && statusByte != 0xF6)
    {
        data[1] = (Byte)((message >> 8) & 0xFF);
        dataSize++;

        Byte channelStatus = (Byte)(statusByte >> 4);
        if (channelStatus == 0x8 || channelStatus == 0x9 || channelStatus == 0xA || channelStatus == 0xB || channelStatus == 0xE || statusByte == 0xF2)
        {
            data[2] = (Byte)(message >> 16);
            dataSize++;
        }
    }

    Byte buffer[dataSize + (sizeof(MIDIPacketList))];
    MIDIPacketList* packetList = (MIDIPacketList*)buffer;
    MIDIPacket* packet = MIDIPacketListInit(packetList);
    MIDIPacketListAdd(packetList, sizeof(buffer), packet, 0, dataSize, &data[0]);

    OSStatus result = MIDISend(outputDeviceHandle->portRef, outputDeviceHandle->info->endpointRef, packetList);
    if (result != noErr)
    {
        switch (result)
        {
            case kMIDIInvalidClient: return OUT_SENDSHORTRESULT_INVALIDCLIENT;
            case kMIDIInvalidPort: return OUT_SENDSHORTRESULT_INVALIDPORT;
            case kMIDIWrongEndpointType: return OUT_SENDSHORTRESULT_WRONGENDPOINT;
            case kMIDIUnknownEndpoint: return OUT_SENDSHORTRESULT_UNKNOWNENDPOINT;
            case kMIDIMessageSendErr: return OUT_SENDSHORTRESULT_COMMUNICATIONERROR;
            case kMIDIServerStartErr: return OUT_SENDSHORTRESULT_SERVERSTARTERROR;
            case kMIDIWrongThread: return OUT_SENDSHORTRESULT_WRONGTHREAD;
            case kMIDINotPermitted: return OUT_SENDSHORTRESULT_NOTPERMITTED;
            case kMIDIUnknownError: return OUT_SENDSHORTRESULT_UNKNOWNERROR;
        }
    }

    return OUT_SENDSHORTRESULT_OK;
}

OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Apple(void* handle, Byte* data, ByteCount dataSize)
{
    OutputDeviceHandle* outputDeviceHandle = (OutputDeviceHandle*)handle;

    Byte buffer[dataSize + (sizeof(MIDIPacketList))];
    MIDIPacketList* packetList = (MIDIPacketList*)buffer;
    MIDIPacket* packet = MIDIPacketListInit(packetList);
    MIDIPacketListAdd(packetList, sizeof(buffer), packet, 0, dataSize, &data[0]);

    OSStatus result = MIDISend(outputDeviceHandle->portRef, outputDeviceHandle->info->endpointRef, packetList);
    if (result != noErr)
    {
        switch (result)
        {
            case kMIDIInvalidClient: return OUT_SENDSYSEXRESULT_INVALIDCLIENT;
            case kMIDIInvalidPort: return OUT_SENDSYSEXRESULT_INVALIDPORT;
            case kMIDIWrongEndpointType: return OUT_SENDSYSEXRESULT_WRONGENDPOINT;
            case kMIDIUnknownEndpoint: return OUT_SENDSYSEXRESULT_UNKNOWNENDPOINT;
            case kMIDIMessageSendErr: return OUT_SENDSYSEXRESULT_COMMUNICATIONERROR;
            case kMIDIServerStartErr: return OUT_SENDSYSEXRESULT_SERVERSTARTERROR;
            case kMIDIWrongThread: return OUT_SENDSYSEXRESULT_WRONGTHREAD;
            case kMIDINotPermitted: return OUT_SENDSYSEXRESULT_NOTPERMITTED;
            case kMIDIUnknownError: return OUT_SENDSYSEXRESULT_UNKNOWNERROR;
        }
    }

    return OUT_SENDSYSEXRESULT_OK;
}