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
    pthread_t thread;
    void (*callback)(void);
    char active;
    int interval;
} TickGeneratorInfo;

void* RunLoopThreadRoutine(void* data)
{
    TickGeneratorInfo* tickGeneratorInfo = (TickGeneratorInfo*)data;
    
    while (tickGeneratorInfo->active == 1)
    {
        usleep(tickGeneratorInfo->interval * 1000);
        tickGeneratorInfo->callback();
    }
	
	free(data);
    
    return NULL;
}

TG_STARTRESULT StartHighPrecisionTickGenerator_Apple(int interval, void (*callback)(void), TickGeneratorInfo** info)
{
    TickGeneratorInfo* tickGeneratorInfo = malloc(sizeof(TickGeneratorInfo));
    
    tickGeneratorInfo->active = 1;
    tickGeneratorInfo->interval = interval;
    tickGeneratorInfo->callback = callback;
    
    *info = tickGeneratorInfo;
    
    pthread_create(&tickGeneratorInfo->thread, NULL, RunLoopThreadRoutine, tickGeneratorInfo);
    
    return TG_STARTRESULT_OK;
}

TG_STOPRESULT StopHighPrecisionTickGenerator(void* info)
{
    TickGeneratorInfo* tickGeneratorInfo = (TickGeneratorInfo*)info;
    
    tickGeneratorInfo->active = 0;
    
    //pthread_join(tickGeneratorInfo->thread, NULL);
    
    //free(info);
    
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
	
	//CFStringRef portNameRef = CFStringCreateWithCString(kCFAllocatorDefault, inputDeviceInfo->name, kCFStringEncodingUTF8);
	CFStringRef portNameRef = CFSTR("IN");
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
	
	*message = (packet.data[2] << 16) | (packet.data[1] << 8) | packet.data[0];
    
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
    
    //CFStringRef portNameRef = CFStringCreateWithCString(kCFAllocatorDefault, outputDeviceInfo->name, kCFStringEncodingUTF8);
	CFStringRef portNameRef = CFSTR("OUT");
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

int SendShortEventToOutputDevice(void* handle, int message)
{
    MIDIPacket packet;
    
	packet.length = 3;
    packet.data[0] = (Byte)(message & 0xFF);
	packet.data[1] = (Byte)((message >> 8) & 0xFF);
	packet.data[2] = (Byte)(message >> 16);
    
    MIDIPacketList packetList;
    packetList.numPackets = 1;
    packetList.packet[0] = packet;
    
    OutputDeviceHandle* outputDeviceHandle = (OutputDeviceHandle*)handle;
    
    OSStatus res = MIDISend(outputDeviceHandle->portRef, outputDeviceHandle->info->endpointRef, &packetList);
    if (res != noErr)
        return 100;
    
    return 0;
}