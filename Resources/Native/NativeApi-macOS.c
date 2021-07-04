#include <CoreFoundation/CoreFoundation.h>
#include <CoreMIDI/CoreMIDI.h>
#include <pthread.h>

#include "NativeApi-Constants.h"

API_TYPE GetApiType()
{
    return API_TYPE_APPLE;
}

/* ================================
 High-precision tick generator
 ================================ */

typedef struct
{
    CFRunLoopTimerRef timerRef;
    CFRunLoopRef runLoopRef;
    CFRunLoopMode runLoopMode;
    pthread_t thread;
} TickGeneratorInfo;

void* RunLoopThreadRoutine(void* data)
{
    TickGeneratorInfo* tickGeneratorInfo = (TickGeneratorInfo*)data;
    
    CFRunLoopRef runLoopRef = CFRunLoopGetCurrent();
    CFRunLoopMode runLoopMode = kCFRunLoopDefaultMode;
    CFRunLoopAddTimer(runLoopRef, tickGeneratorInfo->timerRef, runLoopMode);
    
    tickGeneratorInfo->runLoopRef = runLoopRef;
    tickGeneratorInfo->runLoopMode = runLoopMode;
    
    CFRunLoopRun();
    
    return 0;
}

TG_STARTRESULT StartHighPrecisionTickGenerator_Apple(int interval, CFRunLoopTimerCallBack callback, TickGeneratorInfo** info)
{
    double secondsInterval = interval;
    secondsInterval /= 1000;
    
    CFRunLoopTimerContext context = { 0, NULL, NULL, NULL, NULL };
    CFRunLoopTimerRef timerRef = CFRunLoopTimerCreate(kCFAllocatorDefault,
                                                      CFAbsoluteTimeGetCurrent() + secondsInterval,
                                                      secondsInterval,
                                                      0,
                                                      0,
                                                      callback,
                                                      &context);
    
    CFRunLoopTimerSetTolerance(timerRef, secondsInterval / 2);
    
    TickGeneratorInfo* tickGeneratorInfo = malloc(sizeof(TickGeneratorInfo));
    tickGeneratorInfo->timerRef = timerRef;
    
    int result = pthread_create(&tickGeneratorInfo->thread, NULL, RunLoopThreadRoutine, tickGeneratorInfo);
    if (result != 0)
    {
        if (result == EAGAIN)
        {
            return TG_STARTRESULT_NORESOURCES;
        }
        else if (result == EINVAL)
        {
            return TG_STARTRESULT_BADTHREADATTRIBUTE;
        }
        
        return TG_STARTRESULT_UNKNOWNERROR;
    }
    
    *info = tickGeneratorInfo;
    
    return TG_STARTRESULT_OK;
}

TG_STOPRESULT StopHighPrecisionTickGenerator(void* info)
{
    TickGeneratorInfo* tickGeneratorInfo = (TickGeneratorInfo*)info;
    
    CFRunLoopStop(tickGeneratorInfo->runLoopRef);
    CFRunLoopRemoveTimer(tickGeneratorInfo->runLoopRef, tickGeneratorInfo->timerRef, tickGeneratorInfo->runLoopMode);
    
    free(info);
    
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
	MIDIClientCreate(nameRef, NULL, NULL, &sessionHandle->clientRef);

	*handle = sessionHandle;

	return SESSION_OPENRESULT_OK;
}

SESSION_CLOSERESULT CloseSession(void* handle)
{
	SessionHandle* sessionHandle = (SessionHandle*)handle;
	free(sessionHandle);
	return SESSION_CLOSERESULT_OK;
}

char* GetDevicePropertyValue(MIDIEndpointRef endpointRef, CFStringRef propertyID)
{
	CFStringRef stringRef;
    OSStatus status = MIDIObjectGetStringProperty(endpointRef, propertyID, &stringRef);
    if (status == noErr)
	{
        char* result = malloc(256 * sizeof(char));
        CFStringGetCString(stringRef, result, 256, kCFStringEncodingUTF8);
		return result;
	}
	
	return NULL;
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
	
	inputDeviceInfo->name = GetDevicePropertyValue(endpointRef, kMIDIPropertyDisplayName);
	inputDeviceInfo->manufacturer = GetDevicePropertyValue(endpointRef, kMIDIPropertyManufacturer);
	inputDeviceInfo->product = GetDevicePropertyValue(endpointRef, kMIDIPropertyModel);
    
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
    
    SInt32 driverVersion;
    MIDIObjectGetIntegerProperty(inputDeviceInfo->endpointRef, kMIDIPropertyDriverVersion, &driverVersion);
    
    return driverVersion;
}

IN_OPENRESULT OpenInputDevice_Apple(void* info, void* sessionHandle, MIDIReadProc callback, void** handle)
{
	InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
	SessionHandle* pSessionHandle = (SessionHandle*)sessionHandle;

	InputDeviceHandle* inputDeviceHandle = malloc(sizeof(InputDeviceHandle));
	inputDeviceHandle->info = inputDeviceInfo;
	
	*handle = inputDeviceHandle;
	
	CFStringRef portNameRef = CFStringCreateWithCString(kCFAllocatorDefault, inputDeviceInfo->name, kCFStringEncodingUTF8);
	OSStatus status = MIDIInputPortCreate(pSessionHandle->clientRef, portNameRef, callback, NULL, &inputDeviceHandle->portRef);
	if (status != noErr)
		return IN_OPENRESULT_UNKNOWNERROR;

	// ...

	return IN_OPENRESULT_OK;
}

IN_CLOSERESULT CloseInputDevice(void* handle)
{
	InputDeviceHandle* inputDeviceHandle = (InputDeviceHandle*)handle;

	// ...

	free(inputDeviceHandle->info);
	free(inputDeviceHandle);

	return IN_CLOSERESULT_OK;
}

IN_CONNECTRESULT ConnectToInputDevice(void* handle)
{
	InputDeviceHandle* inputDeviceHandle = (InputDeviceHandle*)handle;

	OSStatus status = MIDIPortConnectSource(inputDeviceHandle->portRef, inputDeviceHandle->info->endpointRef, NULL);
	if (status != noErr)
		return IN_CONNECTRESULT_UNKNOWNERROR;

	// ...

	return IN_CONNECTRESULT_OK;
}

IN_DISCONNECTRESULT DisconnectFromInputDevice(void* handle)
{
	InputDeviceHandle* inputDeviceHandle = (InputDeviceHandle*)handle;

	MIDIPortDisconnectSource(inputDeviceHandle->portRef, inputDeviceHandle->info->endpointRef);

	// ...

	return IN_DISCONNECTRESULT_OK;
}

// delete
int GetEventDataFromInputDevice(MIDIPacketList* packetList, char* data)
{
    MIDIPacket packet = packetList->packet[0];
    
    for (int i = 0; i < packet.length; i++)
    {
        data[i] = packet.data[i];
    }
    
    return 0;
}

int GetShortEventFromInputDevice(MIDIPacketList* packetList, int* message)
{
    MIDIPacket packet = packetList->packet[0];
	
	*message = (packet.data[1] << 16) | (packet.data[2] << 8) | packet.data[3];
    
    return 0;
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

int GetOutputDeviceInfo(int deviceIndex, void** info)
{
    OutputDeviceInfo* outputDeviceInfo = malloc(sizeof(OutputDeviceInfo));
    
    MIDIEndpointRef endpointRef = MIDIGetDestination(deviceIndex);
    outputDeviceInfo->endpointRef = endpointRef;
	
	outputDeviceInfo->name = GetDevicePropertyValue(endpointRef, kMIDIPropertyDisplayName);
	outputDeviceInfo->manufacturer = GetDevicePropertyValue(endpointRef, kMIDIPropertyManufacturer);
	outputDeviceInfo->product = GetDevicePropertyValue(endpointRef, kMIDIPropertyModel);
    
    *info = outputDeviceInfo;
    
    return 0;
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
    
    SInt32 driverVersion;
    MIDIObjectGetIntegerProperty(outputDeviceInfo->endpointRef, kMIDIPropertyDriverVersion, &driverVersion);
    
    return driverVersion;
}

OUT_OPENRESULT OpenOutputDevice_Apple(void* info, void* sessionHandle, void** handle)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    SessionHandle* pSessionHandle = (SessionHandle*)sessionHandle;
    
    OutputDeviceHandle* outputDeviceHandle = malloc(sizeof(OutputDeviceHandle));
    outputDeviceHandle->info = outputDeviceInfo;
    
    *handle = outputDeviceHandle;
    
    CFStringRef portNameRef = CFStringCreateWithCString(kCFAllocatorDefault, outputDeviceInfo->name, kCFStringEncodingUTF8);
    MIDIOutputPortCreate(pSessionHandle->clientRef, portNameRef, &outputDeviceHandle->portRef);
    
    // ...
    
    return OUT_OPENRESULT_OK;
}

void CloseOutputDevice(void* handle)
{
	OutputDeviceHandle* outputDeviceHandle = (OutputDeviceHandle*)handle;

	free(outputDeviceHandle->info);
	free(outputDeviceHandle);
}

// delete
int SendEventToOutputDevice(void* handle, char* data, int length)
{
    MIDIPacket packet;
    
    for (int i = 0; i < length; i++)
    {
        packet.data[i] = data[i];
    }
    
    packet.length = length;
    
    MIDIPacketList packetList;
    packetList.numPackets = 1;
    packetList.packet[0] = packet;
    
    OutputDeviceHandle* outputDeviceHandle = (OutputDeviceHandle*)handle;
    
    OSStatus res = MIDISend(outputDeviceHandle->portRef, outputDeviceHandle->info->endpointRef, &packetList);
    if (res != noErr)
        return 100;
    
    return 0;
}

int SendShortEventToOutputDevice(void* handle, int message)
{
    MIDIPacket packet;
    
	packet.length = 4;
    packet.data[0] = 0;
	packet.data[1] = (Byte)(message >> 16);
	packet.data[2] = (Byte)((message >> 8) & 0xFF);
	packet.data[3] = (Byte)(message & 0xFF);
    
    MIDIPacketList packetList;
    packetList.numPackets = 1;
    packetList.packet[0] = packet;
    
    OutputDeviceHandle* outputDeviceHandle = (OutputDeviceHandle*)handle;
    
    OSStatus res = MIDISend(outputDeviceHandle->portRef, outputDeviceHandle->info->endpointRef, &packetList);
    if (res != noErr)
        return 100;
    
    return 0;
}

/* ================================
   Loopback device
================================ */

typedef struct
{
    MIDIEndpointRef destRef;
    MIDIEndpointRef srcRef;
    MIDIClientRef clientRef;
} PortInfo;

LPBCREATE_RESULT CreateLoopbackPort(char* portName, MIDIReadProc callback, PortInfo** info)
{
    PortInfo* portInfo = malloc(sizeof(PortInfo));
    
    CFStringRef nameRef = CFStringCreateWithCString(NULL, portName, kCFStringEncodingUTF8);
    
    MIDIClientRef client;
    OSStatus result = MIDIClientCreate(nameRef, NULL, NULL, &client);
    if (result != 0)
    {
        return LPBCREATE_FAILEDCREATECLIENT;
    }
    portInfo->clientRef = client;
    
    MIDIEndpointRef srcEndpoint;
    result = MIDISourceCreate(client, nameRef, &srcEndpoint);
    if (result != 0)
    {
        return LPBCREATE_FAILEDCREATESOURCE;
    }
    portInfo->srcRef = srcEndpoint;
	
	*info = portInfo;
    
    MIDIEndpointRef destEndpoint;
    result = MIDIDestinationCreate(client, nameRef, callback, *info, &destEndpoint);
    if (result != 0)
    {
        return LPBCREATE_FAILEDCREATEDESTINATION;
    }
    portInfo->destRef = destEndpoint;
    
    return LPBCREATE_OK;
}

int SendDataBack(MIDIPacketList *pktlist, void *info)
{
    if (info != NULL)
	{
        PortInfo* portInfo = (PortInfo*)info;
		MIDIReceived(portInfo->srcRef, pktlist);
		return 0;
	}
	
	return 1;
}