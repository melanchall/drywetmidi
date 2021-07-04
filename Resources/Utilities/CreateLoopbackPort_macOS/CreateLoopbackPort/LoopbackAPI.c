#include <CoreFoundation/CoreFoundation.h>
#include <CoreMIDI/CoreMIDI.h>

typedef int LPBCREATE_RESULT;

#define LPBCREATE_OK 0
#define LPBCREATE_FAILEDCREATECLIENT 1
#define LPBCREATE_FAILEDCREATESOURCE 2
#define LPBCREATE_FAILEDCREATEDESTINATION 3

typedef struct
{
    MIDIEndpointRef destRef;
    MIDIEndpointRef srcRef;
} PortInfo;

typedef struct
{
	char* name;
	MIDIClientRef clientRef;
} SessionHandle;

int OpenSession(char* name, void** handle)
{
	SessionHandle* sessionHandle = malloc(sizeof(SessionHandle));
	sessionHandle->name = name;
	
	CFStringRef nameRef = CFStringCreateWithCString(kCFAllocatorDefault, name, kCFStringEncodingUTF8);
	MIDIClientCreate(nameRef, NULL, NULL, &sessionHandle->clientRef);

	*handle = sessionHandle;

	return 0;
}

LPBCREATE_RESULT CreateLoopbackPort(SessionHandle* sessionHandle, char* portName, MIDIReadProc callback, PortInfo** info)
{
    PortInfo* portInfo = malloc(sizeof(PortInfo));
    
    CFStringRef nameRef = CFStringCreateWithCString(NULL, portName, kCFStringEncodingUTF8);
    
    MIDIEndpointRef srcEndpoint;
    OSStatus result = MIDISourceCreate(sessionHandle->clientRef, nameRef, &srcEndpoint);
    if (result != 0)
    {
        return LPBCREATE_FAILEDCREATESOURCE;
    }
    portInfo->srcRef = srcEndpoint;
    
    MIDIEndpointRef destEndpoint;
    result = MIDIDestinationCreate(sessionHandle->clientRef, nameRef, callback, portInfo, &destEndpoint);
    if (result != 0)
    {
        return LPBCREATE_FAILEDCREATEDESTINATION;
    }
    portInfo->destRef = destEndpoint;
    
    *info = portInfo;
    return LPBCREATE_OK;
}

int SendDataBack(MIDIPacketList *pktlist, void *info)
{
    if (info != NULL)
	{
        PortInfo* portInfo = (PortInfo*)info;
		OSStatus status = MIDIReceived(portInfo->srcRef, pktlist);
		if (status != noErr)
			return 100;
		
		return 0;
	}
	
	return 1000;
}
