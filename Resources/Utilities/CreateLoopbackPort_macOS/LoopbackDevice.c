#include <CoreFoundation/CoreFoundation.h>
#include <CoreMIDI/CoreMIDI.h>
#include <stdio.h>
#include <unistd.h>

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

void ReadProc(const MIDIPacketList *pktlist, void *readProcRefCon, void *srcConnRefCon)
{
    PortInfo* portInfo = (PortInfo*)readProcRefCon;
    MIDIReceived(portInfo->srcRef, pktlist);
}

int main(int argc, char *argv[])
{
    printf("Creating client...\n");
    
    MIDIClientRef clientRef;
    OSStatus result = MIDIClientCreate(CFSTR("LoopbackClient"), NULL, NULL, &clientRef);
    if (result != noErr)
        return LPBCREATE_FAILEDCREATECLIENT;
    
    for (int i = 1; i < argc; i++)
    {
        printf("Creating port '%s'...\n", argv[i]);
        
        PortInfo *portInfo = malloc(sizeof(PortInfo));
        CFStringRef nameRef = CFStringCreateWithCString(NULL, argv[i], kCFStringEncodingUTF8);
        
        printf("    creating source...\n");
        
        result = MIDISourceCreate(clientRef, nameRef, &portInfo->srcRef);
        if (result != noErr)
            return LPBCREATE_FAILEDCREATESOURCE;
        
        MIDIObjectSetStringProperty(portInfo->srcRef, kMIDIPropertyModel, CFSTR("InputProduct"));
        MIDIObjectSetStringProperty(portInfo->srcRef, kMIDIPropertyManufacturer, CFSTR("InputManufacturer"));
        MIDIObjectSetIntegerProperty(portInfo->srcRef, kMIDIPropertyDriverVersion, 100);
        MIDIObjectSetStringProperty(portInfo->srcRef, kMIDIPropertyDriverOwner, CFSTR("InputDriverOwner"));
    
        printf("    creating destination...\n");
    
        result = MIDIDestinationCreate(clientRef, nameRef, ReadProc, portInfo, &portInfo->destRef);
        if (result != noErr)
            return LPBCREATE_FAILEDCREATEDESTINATION;
        
        MIDIObjectSetStringProperty(portInfo->destRef, kMIDIPropertyModel, CFSTR("OutputProduct"));
        MIDIObjectSetStringProperty(portInfo->destRef, kMIDIPropertyManufacturer, CFSTR("OutputManufacturer"));
        MIDIObjectSetIntegerProperty(portInfo->destRef, kMIDIPropertyDriverVersion, 200);
        MIDIObjectSetStringProperty(portInfo->destRef, kMIDIPropertyDriverOwner, CFSTR("OutputDriverOwner"));
        
        printf("OK\n");
    }
    
    printf("Waiting for data...\n");
    
    while (true)
    {
        usleep(1000);
    }
   
    return LPBCREATE_OK;
}