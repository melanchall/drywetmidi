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
    
    MIDIEndpointRef destEndpoint;
    result = MIDIDestinationCreate(client, nameRef, callback, portInfo, &destEndpoint);
    if (result != 0)
    {
        return LPBCREATE_FAILEDCREATEDESTINATION;
    }
    portInfo->destRef = destEndpoint;
    
    *info = portInfo;
    return LPBCREATE_OK;
}

void SendDataBack(MIDIPacketList *pktlist, void *info)
{
    if (info != NULL)
	{
        PortInfo* portInfo = (PortInfo*)info;
		MIDIReceived(portInfo->srcRef, pktlist);
	}
}
