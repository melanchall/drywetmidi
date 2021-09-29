#include <CoreFoundation/CoreFoundation.h>
#include <CoreMIDI/CoreMIDI.h>
#include <pthread.h>
#include <mach/mach_time.h>
#include <pthread.h>

#include "NativeApi-Constants.h"

#define PROPERTY_VALUE_BUFFER_SIZE 256
#define SMALL_BUFFER_ERROR 10000

/* ================================
   Common
================================ */

API_TYPE GetApiType()
{
    return API_TYPE_MAC;
}

char CanCompareDevices()
{
	return 1;
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

TG_STARTRESULT StartHighPrecisionTickGenerator_Mac(int interval, void (*callback)(void), TickGeneratorInfo** info)
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
    MIDIEndpointRef endpointRef;
} InputDeviceInfo;

typedef struct
{
    MIDIEndpointRef endpointRef;
} OutputDeviceInfo;

OSStatus GetDevicePropertyValue(MIDIEndpointRef endpointRef, CFStringRef propertyID, char** value)
{
    CFStringRef stringRef = NULL;
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
   Session
 ================================ */

typedef void (*InputDeviceCallback)(void* info, char operation);
typedef void (*OutputDeviceCallback)(void* info, char operation);

typedef struct
{
    char* name;
    MIDIClientRef clientRef;
	pthread_t thread;
	char clientCreated;
	OSStatus clientCreationStatus;
	InputDeviceCallback inputDeviceCallback;
	OutputDeviceCallback outputDeviceCallback;
} SessionHandle;
 
void HandleSource(MIDIEndpointRef source, InputDeviceCallback inputDeviceCallback, char operation)
{
	InputDeviceInfo* inputDeviceInfo = malloc(sizeof(InputDeviceInfo));
    inputDeviceInfo->endpointRef = source;

    inputDeviceCallback(inputDeviceInfo, operation);
}

void HandleDestination(MIDIEndpointRef destination, OutputDeviceCallback outputDeviceCallback, char operation)
{
	OutputDeviceInfo* outputDeviceInfo = malloc(sizeof(OutputDeviceInfo));
    outputDeviceInfo->endpointRef = destination;

    outputDeviceCallback(outputDeviceInfo, operation);
}

void HandleEntitySources(MIDIEntityRef entity, InputDeviceCallback inputDeviceCallback, char operation)
{
    ItemCount _sourcesCount = MIDIEntityGetNumberOfSources(entity);
    
    for (int i = 0; i < _sourcesCount; i++)
    {
        MIDIEndpointRef source = MIDIEntityGetSource(entity, i);
        HandleSource(source, inputDeviceCallback, operation);
    }
}

void HandleEntityDestinations(MIDIEntityRef entity, OutputDeviceCallback outputDeviceCallback, char operation)
{
    ItemCount _destinationsCount = MIDIEntityGetNumberOfDestinations(entity);
    
    for (int i = 0; i < _destinationsCount; i++)
    {
        MIDIEndpointRef destination = MIDIEntityGetDestination(entity, i);
        HandleDestination(destination, outputDeviceCallback, operation);
    }
}

void HandleEntity(MIDIEntityRef entity, InputDeviceCallback inputDeviceCallback, OutputDeviceCallback outputDeviceCallback, char operation)
{
    HandleEntitySources(entity, inputDeviceCallback, operation);
    HandleEntityDestinations(entity, outputDeviceCallback, operation);
}

void HandleDevice(MIDIDeviceRef device, InputDeviceCallback inputDeviceCallback, OutputDeviceCallback outputDeviceCallback, char operation)
{
    ItemCount entitiesCount = MIDIDeviceGetNumberOfEntities(device);
    
    for (int i = 0; i < entitiesCount; i++)
    {
        MIDIEntityRef entity = MIDIDeviceGetEntity(device, i);
        HandleEntity(entity, inputDeviceCallback, outputDeviceCallback, operation);
    }
}

void HandleNotification(const MIDINotification* message, SessionHandle* sessionHandle)
{
    switch (message->messageID)
    {
        case kMIDIMsgObjectAdded:
        case kMIDIMsgObjectRemoved:
        {
            char operation = message->messageID == kMIDIMsgObjectAdded ? 1 : 0;
            
            MIDIObjectAddRemoveNotification* n = (MIDIObjectAddRemoveNotification*)message;
            
            switch (n->childType)
            {
                case kMIDIObjectType_Device:
                {
                    HandleDevice(n->child, sessionHandle->inputDeviceCallback, sessionHandle->outputDeviceCallback, operation);
                    break;
                }
                case kMIDIObjectType_Entity:
                {
                    HandleEntity(n->child, sessionHandle->inputDeviceCallback, sessionHandle->outputDeviceCallback, operation);
                    break;
                }
                case kMIDIObjectType_Source:
                {
                    HandleSource(n->child, sessionHandle->inputDeviceCallback, operation);                    
                    break;
                }
                case kMIDIObjectType_Destination:
                {
                    HandleDestination(n->child, sessionHandle->outputDeviceCallback, operation);                    
                    break;
                }
            }
            
            break;
        }
    }
}

void NotifyProc(const MIDINotification* message, void* refCon)
{
	SessionHandle* sessionHandle = (SessionHandle*)refCon;
    HandleNotification(message, sessionHandle);
}

void* ThreadProc(void* data)
{
    SessionHandle* sessionHandle = (SessionHandle*)data;
    
	CFStringRef nameRef = CFStringCreateWithCString(kCFAllocatorDefault, sessionHandle->name, kCFStringEncodingUTF8);
    sessionHandle->clientCreationStatus = MIDIClientCreate(nameRef, NotifyProc, data, &sessionHandle->clientRef);
    sessionHandle->clientCreated = 1;
    
    CFRunLoopRun();
    
    return NULL;
}

SESSION_OPENRESULT OpenSession_Mac(char* name, InputDeviceCallback inputDeviceCallback, OutputDeviceCallback outputDeviceCallback, void** handle)
{
    SessionHandle* sessionHandle = malloc(sizeof(SessionHandle));
	
    sessionHandle->name = name;
	sessionHandle->inputDeviceCallback = inputDeviceCallback;
	sessionHandle->outputDeviceCallback = outputDeviceCallback;
    sessionHandle->clientCreated = 0;
	sessionHandle->name = name;
	
    pthread_create(&sessionHandle->thread, NULL, ThreadProc, sessionHandle);
    
    while (sessionHandle->clientCreated == 0) {}

    if (sessionHandle->clientCreationStatus != noErr)
    {
        switch (sessionHandle->clientCreationStatus)
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

/* ================================
   Input device
 ================================ */

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

    *info = inputDeviceInfo;

    return IN_GETINFORESULT_OK;
}

int GetInputDeviceHashCode(void* info)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    return inputDeviceInfo->endpointRef;
}

char AreInputDevicesEqual(void* info1, void* info2)
{
	InputDeviceInfo* inputDeviceInfo1 = (InputDeviceInfo*)info1;
	InputDeviceInfo* inputDeviceInfo2 = (InputDeviceInfo*)info2;
	
	return (char)(inputDeviceInfo1->endpointRef == inputDeviceInfo2->endpointRef);
}

IN_GETPROPERTYRESULT GetInputDeviceStringPropertyValue(InputDeviceInfo* inputDeviceInfo, CFStringRef propertyID, char** value)
{
	OSStatus status = GetDevicePropertyValue(inputDeviceInfo->endpointRef, propertyID, value);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownEndpoint: return IN_GETPROPERTYRESULT_UNKNOWNENDPOINT;
            case SMALL_BUFFER_ERROR: return IN_GETPROPERTYRESULT_TOOLONG;
            case kMIDIUnknownProperty: return IN_GETPROPERTYRESULT_UNKNOWNPROPERTY;
            default: return IN_GETPROPERTYRESULT_UNKNOWNERROR;
        }
    }
	
	return IN_GETPROPERTYRESULT_OK;
}

IN_GETPROPERTYRESULT GetInputDeviceIntPropertyValue(InputDeviceInfo* inputDeviceInfo, CFStringRef propertyID, int* value)
{
	OSStatus status = MIDIObjectGetIntegerProperty(inputDeviceInfo->endpointRef, propertyID, value);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownEndpoint: return IN_GETPROPERTYRESULT_UNKNOWNENDPOINT;
            case kMIDIUnknownProperty: return IN_GETPROPERTYRESULT_UNKNOWNPROPERTY;
            default: return IN_GETPROPERTYRESULT_UNKNOWNERROR;
        }
    }
	
	return IN_GETPROPERTYRESULT_OK;
}

IN_GETPROPERTYRESULT GetInputDeviceName(void* info, char** value)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
	return GetInputDeviceStringPropertyValue(inputDeviceInfo, kMIDIPropertyDisplayName, value);
}

IN_GETPROPERTYRESULT GetInputDeviceManufacturer(void* info, char** value)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
	return GetInputDeviceStringPropertyValue(inputDeviceInfo, kMIDIPropertyManufacturer, value);
}

IN_GETPROPERTYRESULT GetInputDeviceProduct(void* info, char** value)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    return GetInputDeviceStringPropertyValue(inputDeviceInfo, kMIDIPropertyModel, value);
}

IN_GETPROPERTYRESULT GetInputDeviceDriverVersion(void* info, int* value)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
	return GetInputDeviceIntPropertyValue(inputDeviceInfo, kMIDIPropertyDriverVersion, value);
}

IN_GETPROPERTYRESULT GetInputDeviceUniqueId(void* info, int* value)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
	return GetInputDeviceIntPropertyValue(inputDeviceInfo, kMIDIPropertyUniqueID, value);
}

IN_GETPROPERTYRESULT GetInputDeviceDriverOwner(void* info, char** value)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
	return GetInputDeviceStringPropertyValue(inputDeviceInfo, kMIDIPropertyDriverOwner, value);
}

IN_OPENRESULT OpenInputDevice_Mac(void* info, void* sessionHandle, MIDIReadProc callback, void** handle)
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

char IsInputDevicePropertySupported(IN_PROPERTY property)
{
	switch (property)
	{
		case IN_PROPERTY_PRODUCT:
		case IN_PROPERTY_MANUFACTURER:
		case IN_PROPERTY_DRIVERVERSION:
		case IN_PROPERTY_UNIQUEID:
		case IN_PROPERTY_DRIVEROWNER:
			return 1;
	}
	
	return 0;
}

/* ================================
   Output device
 ================================ */

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

    *info = outputDeviceInfo;

    return OUT_GETINFORESULT_OK;
}

int GetOutputDeviceHashCode(void* info)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    return outputDeviceInfo->endpointRef;
}

char AreOutputDevicesEqual(void* info1, void* info2)
{
	OutputDeviceInfo* outputDeviceInfo1 = (OutputDeviceInfo*)info1;
	OutputDeviceInfo* outputDeviceInfo2 = (OutputDeviceInfo*)info2;
	
	return (char)(outputDeviceInfo1->endpointRef == outputDeviceInfo2->endpointRef);
}

OUT_GETPROPERTYRESULT GetOutputDeviceStringPropertyValue(OutputDeviceInfo* outputDeviceInfo, CFStringRef propertyID, char** value)
{
	OSStatus status = GetDevicePropertyValue(outputDeviceInfo->endpointRef, propertyID, value);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownEndpoint: return OUT_GETPROPERTYRESULT_UNKNOWNENDPOINT;
            case SMALL_BUFFER_ERROR: return OUT_GETPROPERTYRESULT_TOOLONG;
            case kMIDIUnknownProperty: return OUT_GETPROPERTYRESULT_UNKNOWNPROPERTY;
            default: return OUT_GETPROPERTYRESULT_UNKNOWNERROR;
        }
    }
	
	return OUT_GETPROPERTYRESULT_OK;
}

OUT_GETPROPERTYRESULT GetOutputDeviceIntPropertyValue(OutputDeviceInfo* outputDeviceInfo, CFStringRef propertyID, int* value)
{
	OSStatus status = MIDIObjectGetIntegerProperty(outputDeviceInfo->endpointRef, propertyID, value);
    if (status != noErr)
    {
        switch (status)
        {
            case kMIDIUnknownEndpoint: return OUT_GETPROPERTYRESULT_UNKNOWNENDPOINT;
            case kMIDIUnknownProperty: return OUT_GETPROPERTYRESULT_UNKNOWNPROPERTY;
            default: return OUT_GETPROPERTYRESULT_UNKNOWNERROR;
        }
    }
	
	return OUT_GETPROPERTYRESULT_OK;
}

OUT_GETPROPERTYRESULT GetOutputDeviceName(void* info, char** value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
	return GetOutputDeviceStringPropertyValue(outputDeviceInfo, kMIDIPropertyDisplayName, value);
}

OUT_GETPROPERTYRESULT GetOutputDeviceManufacturer(void* info, char** value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
	return GetOutputDeviceStringPropertyValue(outputDeviceInfo, kMIDIPropertyManufacturer, value);
}

OUT_GETPROPERTYRESULT GetOutputDeviceProduct(void* info, char** value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
	return GetOutputDeviceStringPropertyValue(outputDeviceInfo, kMIDIPropertyModel, value);
}

OUT_GETPROPERTYRESULT GetOutputDeviceDriverVersion(void* info, int* value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
	return GetOutputDeviceIntPropertyValue(outputDeviceInfo, kMIDIPropertyDriverVersion, value);
}

OUT_GETPROPERTYRESULT GetOutputDeviceUniqueId(void* info, int* value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
	return GetOutputDeviceIntPropertyValue(outputDeviceInfo, kMIDIPropertyUniqueID, value);
}

OUT_GETPROPERTYRESULT GetOutputDeviceDriverOwner(void* info, char** value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
	return GetOutputDeviceStringPropertyValue(outputDeviceInfo, kMIDIPropertyDriverOwner, value);
}

OUT_OPENRESULT OpenOutputDevice_Mac(void* info, void* sessionHandle, void** handle)
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

OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Mac(void* handle, Byte* data, ByteCount dataSize)
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

char IsOutputDevicePropertySupported(OUT_PROPERTY property)
{
	switch (property)
	{
		case OUT_PROPERTY_PRODUCT:
		case OUT_PROPERTY_MANUFACTURER:
		case OUT_PROPERTY_DRIVERVERSION:
		case OUT_PROPERTY_UNIQUEID:
		case OUT_PROPERTY_DRIVEROWNER:
			return 1;
	}
	
	return 0;
}

/* ================================
 Virtual device
 ================================ */

typedef struct
{
    InputDeviceInfo* inputDeviceInfo;
	OutputDeviceInfo* outputDeviceInfo;
	char* name;
} VirtualDeviceInfo;

VIRTUAL_OPENRESULT OpenVirtualDevice_Mac(char* name, void* sessionHandle, MIDIReadProc callback, void** info)
{	
	SessionHandle* pSessionHandle = (SessionHandle*)sessionHandle;
	
	VirtualDeviceInfo* virtualDeviceInfo = malloc(sizeof(VirtualDeviceInfo));
	virtualDeviceInfo->name = name;
	
	CFStringRef nameRef = CFStringCreateWithCString(NULL, name, kCFStringEncodingUTF8);
	
	MIDIEndpointRef sourceRef;
	OSStatus status = MIDISourceCreate(pSessionHandle->clientRef, nameRef, &sourceRef);
	if (status != noErr)
	{
		switch (status)
        {
            case kMIDIServerStartErr: return VIRTUAL_OPENRESULT_CREATESOURCE_SERVERSTARTERROR;
            case kMIDIWrongThread: return VIRTUAL_OPENRESULT_CREATESOURCE_WRONGTHREAD;
            case kMIDINotPermitted: return VIRTUAL_OPENRESULT_CREATESOURCE_NOTPERMITTED;
            case kMIDIUnknownError: return VIRTUAL_OPENRESULT_CREATESOURCE_UNKNOWNERROR;
        }
	}
	
	InputDeviceInfo* inputDeviceInfo = malloc(sizeof(InputDeviceInfo));
	inputDeviceInfo->endpointRef = sourceRef;
	virtualDeviceInfo->inputDeviceInfo = inputDeviceInfo;
	
	MIDIEndpointRef destinationRef;
	status = MIDIDestinationCreate(pSessionHandle->clientRef, nameRef, callback, inputDeviceInfo, &destinationRef);
	if (status != noErr)
	{
		switch (status)
        {
            case kMIDIServerStartErr: return VIRTUAL_OPENRESULT_CREATEDESTINATION_SERVERSTARTERROR;
            case kMIDIWrongThread: return VIRTUAL_OPENRESULT_CREATEDESTINATION_WRONGTHREAD;
            case kMIDINotPermitted: return VIRTUAL_OPENRESULT_CREATEDESTINATION_NOTPERMITTED;
            case kMIDIUnknownError: return VIRTUAL_OPENRESULT_CREATEDESTINATION_UNKNOWNERROR;
        }
	}
	
	OutputDeviceInfo* outputDeviceInfo = malloc(sizeof(OutputDeviceInfo));
	outputDeviceInfo->endpointRef = destinationRef;
	virtualDeviceInfo->outputDeviceInfo = outputDeviceInfo;
	
	*info = virtualDeviceInfo;
	
	return VIRTUAL_OPENRESULT_OK;
}

VIRTUAL_CLOSERESULT CloseVirtualDevice(void* info)
{
	VirtualDeviceInfo* virtualDeviceInfo = (VirtualDeviceInfo*)info;
	
	OSStatus status = MIDIEndpointDispose(virtualDeviceInfo->inputDeviceInfo->endpointRef);
	if (status != noErr)
	{
		switch (status)
        {
			case kMIDIUnknownEndpoint: return VIRTUAL_CLOSERESULT_DISPOSESOURCE_UNKNOWNENDPOINT;
            case kMIDINotPermitted: return VIRTUAL_CLOSERESULT_DISPOSESOURCE_NOTPERMITTED;
            case kMIDIUnknownError: return VIRTUAL_CLOSERESULT_DISPOSESOURCE_UNKNOWNERROR;
        }
	}
	
	status = MIDIEndpointDispose(virtualDeviceInfo->outputDeviceInfo->endpointRef);
	if (status != noErr)
	{
		switch (status)
        {
			case kMIDIUnknownEndpoint: return VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_UNKNOWNENDPOINT;
            case kMIDINotPermitted: return VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_NOTPERMITTED;
            case kMIDIUnknownError: return VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_UNKNOWNERROR;
        }
	}
	
	free(virtualDeviceInfo);
	
	return VIRTUAL_CLOSERESULT_OK;
}

VIRTUAL_SENDBACKRESULT SendDataBackFromVirtualDevice(const MIDIPacketList *pktlist, void *readProcRefCon)
{
	InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)readProcRefCon;
	
    OSStatus status = MIDIReceived(inputDeviceInfo->endpointRef, pktlist);
	if (status != noErr)
	{
		switch (status)
        {
			case kMIDIUnknownEndpoint: return VIRTUAL_SENDBACKRESULT_UNKNOWNENDPOINT;
            case kMIDINotPermitted: return VIRTUAL_SENDBACKRESULT_NOTPERMITTED;
            case kMIDIUnknownError: return VIRTUAL_SENDBACKRESULT_UNKNOWNERROR;
			case kMIDIWrongEndpointType: return VIRTUAL_SENDBACKRESULT_WRONGENDPOINT;
			case kMIDIMessageSendErr: return VIRTUAL_SENDBACKRESULT_MESSAGESENDERROR;
			case kMIDIServerStartErr: return VIRTUAL_SENDBACKRESULT_SERVERSTARTERROR;
			case kMIDIWrongThread: return VIRTUAL_SENDBACKRESULT_WRONGTHREAD;
        }
	}
	
	return VIRTUAL_SENDBACKRESULT_OK;
}

void* GetInputDeviceInfoFromVirtualDevice(void* info)
{
	VirtualDeviceInfo* virtualDeviceInfo = (VirtualDeviceInfo*)info;
	return virtualDeviceInfo->inputDeviceInfo;
}

void* GetOutputDeviceInfoFromVirtualDevice(void* info)
{
	VirtualDeviceInfo* virtualDeviceInfo = (VirtualDeviceInfo*)info;
	return virtualDeviceInfo->outputDeviceInfo;
}