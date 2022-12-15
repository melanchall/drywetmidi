#include <CoreFoundation/CoreFoundation.h>
#include <CoreMIDI/CoreMIDI.h>
#include <mach/mach_time.h>

typedef struct
{
    MIDIClientRef clientRef;
	MIDIPortRef portRef;
    MIDIEndpointRef endpointRef;
} SenderHandle;

int OpenSender(char* portName, void** handle)
{
    MIDIClientRef clientRef;
    MIDIClientCreate(CFSTR("CLIENT"), NULL, NULL, &clientRef);
    
    MIDIPortRef portRef;
    MIDIOutputPortCreate(clientRef, CFSTR("OUT"), &portRef);
    
    CFStringRef portNameRef = CFStringCreateWithCString(kCFAllocatorDefault, portName, kCFStringEncodingUTF8);
	
	MIDIEndpointRef endpointRef;
    ItemCount destinationsCount = MIDIGetNumberOfDestinations();
    
    for (int i = 0; i < destinationsCount; i++)
    {
        endpointRef = MIDIGetDestination(i);
        
        CFStringRef name;
        MIDIObjectGetStringProperty(endpointRef, kMIDIPropertyDisplayName, &name);
        
        if (CFStringCompare(name, portNameRef, 0) == kCFCompareEqualTo)
        {
            break;
        }
    }
	
	//
	
	SenderHandle* senderHandle = malloc(sizeof(SenderHandle));
	
	senderHandle->clientRef = clientRef;
	senderHandle->portRef = portRef;
	senderHandle->endpointRef = endpointRef;
	
	*handle = senderHandle;
	return 0;
}

int CloseSender(void* handle)
{
	SenderHandle* senderHandle = (SenderHandle*)handle;
	free(senderHandle);
	return 0;
}

int SendData(void* handle, Byte* data, int length, int* indices, int indicesLength)
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
	
	MIDISend(senderHandle->portRef, senderHandle->endpointRef, packetList);
    return 0;
}