#include <CoreFoundation/CoreFoundation.h>
#include <CoreMIDI/CoreMIDI.h>
#include <pthread.h>
#include <mach/mach_time.h>

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
	
	while (tickGeneratorInfo->active == 0) { }
    
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
	
	outputDeviceInfo->name = GetDevicePropertyValue(endpointRef, kMIDIPropertyDisplayName);
	outputDeviceInfo->manufacturer = GetDevicePropertyValue(endpointRef, kMIDIPropertyManufacturer);
	outputDeviceInfo->product = GetDevicePropertyValue(endpointRef, kMIDIPropertyModel);
    
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
    OSStatus result = MIDIOutputPortCreate(pSessionHandle->clientRef, portNameRef, &outputDeviceHandle->portRef);
	if (result != noErr)
	{
		switch (result)
	    {
	        case kMIDIInvalidClient: return OUT_OPENRESULT_INVALIDCLIENT;
			case kMIDIInvalidPort: return OUT_OPENRESULT_INVALIDPORT;
			case kMIDIServerStartErr: return OUT_OPENRESULT_SERVERSTARTERROR;
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
    MIDIPacketList *packetList = (MIDIPacketList*)buffer;
    MIDIPacket *packet = MIDIPacketListInit(packetList);
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
    MIDIPacketList *packetList = (MIDIPacketList*)buffer;
    MIDIPacket *packet = MIDIPacketListInit(packetList);
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