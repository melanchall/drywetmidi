#include <CoreFoundation/CoreFoundation.h>
#include <CoreMIDI/CoreMIDI.h>
#include <mach/mach_time.h>

typedef int OPENRESULT;

#define OPENRESULT_OK 0

#define OPENRESULT_FAILEDCREATECLIENT 1
#define OPENRESULT_FAILEDCREATEPORT 2
#define OPENRESULT_FAILEDFINDPORT 3

typedef int CLOSERESULT;

#define CLOSERESULT_OK 0

typedef int SENDRESULT;

#define SENDRESULT_OK 0

#define SENDRESULT_FAILEDSEND 1

typedef struct
{
    MIDIClientRef clientRef;
	MIDIPortRef portRef;
    MIDIEndpointRef endpointRef;
} SenderHandle;

OPENRESULT OpenSender(char* portName, void** handle)
{
    MIDIClientRef clientRef;
    OSStatus status = MIDIClientCreate(CFSTR("CLIENT"), NULL, NULL, &clientRef);
	if (status != noErr)
		return OPENRESULT_FAILEDCREATECLIENT;
    
    MIDIPortRef portRef;
    status = MIDIOutputPortCreate(clientRef, CFSTR("OUT"), &portRef);
	if (status != noErr)
		return OPENRESULT_FAILEDCREATEPORT;
    
    CFStringRef portNameRef = CFStringCreateWithCString(kCFAllocatorDefault, portName, kCFStringEncodingUTF8);
	
	MIDIEndpointRef endpointRef;
	unsigned char portFound = 0;
    ItemCount destinationsCount = MIDIGetNumberOfDestinations();
    
    for (int i = 0; i < destinationsCount; i++)
    {
        endpointRef = MIDIGetDestination(i);
        
        CFStringRef name;
        MIDIObjectGetStringProperty(endpointRef, kMIDIPropertyDisplayName, &name);
        
        if (CFStringCompare(name, portNameRef, 0) == kCFCompareEqualTo)
        {
			portFound = 1;
            break;
        }
    }
	
	if (portFound == 0)
		return OPENRESULT_FAILEDFINDPORT;
	
	//
	
	SenderHandle* senderHandle = malloc(sizeof(SenderHandle));
	
	senderHandle->clientRef = clientRef;
	senderHandle->portRef = portRef;
	senderHandle->endpointRef = endpointRef;
	
	*handle = senderHandle;
	return OPENRESULT_OK;
}

CLOSERESULT CloseSender(void* handle)
{
	SenderHandle* senderHandle = (SenderHandle*)handle;
	free(senderHandle);
	return CLOSERESULT_OK;
}

SENDRESULT SendData(void* handle, Byte* data, int length, int* indices, int indicesLength)
{
    SenderHandle* senderHandle = (SenderHandle*)handle;
    
    //
	
	Byte buffer[length + sizeof(MIDIPacketList)];
	MIDIPacketList *packetList = (MIDIPacketList*)buffer;
	MIDIPacket *packet = MIDIPacketListInit(packetList);
	
	for (int i = 0; i < indicesLength; i++)
	{
		ByteCount packetSize = (i == indicesLength - 1 ? length : indices[i + 1]) - indices[i];
		packet = MIDIPacketListAdd(packetList, sizeof(buffer), packet, mach_absolute_time() + i, packetSize, &data[indices[i]]);
	}
	
	OSStatus status = MIDISend(senderHandle->portRef, senderHandle->endpointRef, packetList);
	if (status != noErr)
		return SENDRESULT_FAILEDSEND;
	
    return SENDRESULT_OK;
}