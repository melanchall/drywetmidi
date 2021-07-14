#include <CoreFoundation/CoreFoundation.h>
#include <CoreMIDI/CoreMIDI.h>

int SendData(char* portName, unsigned char* data, int length, int* indices, int indicesLength)
{
    MIDIClientRef clientRef;
    MIDIClientCreate(CFSTR("CLIENT"), NULL, NULL, &clientRef);
    
    MIDIPortRef portRef;
    MIDIOutputPortCreate(clientRef, CFSTR("OUT"), &portRef);
    
    CFStringRef portNameRef = CFStringCreateWithCString(kCFAllocatorDefault, portName, kCFStringEncodingUTF8);
    
    //
    
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
    
    Byte buffer[length + sizeof(MIDIPacketList)];
    MIDIPacketList *packetList = (MIDIPacketList*)buffer;
    MIDIPacket *packet = MIDIPacketListInit(packetList);
    
    for (int i = 0; i < indicesLength; i++)
    {
        ByteCount packetSize = (i == indicesLength - 1 ? length : indices[i + 1]) - indices[i];
        packet = MIDIPacketListAdd(packetList, sizeof(buffer), packet, 0, packetSize, &data[indices[i]]);
    }
    
    //
    
    MIDISend(portRef, endpointRef, packetList);
    
    //
    
    return 0;
}