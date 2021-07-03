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

int GetInputDeviceInfo(int deviceIndex, void** info)
{
    InputDeviceInfo* inputDeviceInfo = malloc(sizeof(InputDeviceInfo));
    
    MIDIEndpointRef endpointRef = MIDIGetSource(deviceIndex);
    inputDeviceInfo->endpointRef = endpointRef;
    
    /*CFStringRef nameRef;
    OSStatus status = MIDIObjectGetStringProperty(endpointRef, kMIDIPropertyDisplayName, &nameRef);
    if (status == noErr)
	{
        inputDeviceInfo->name = malloc(256 * sizeof(char));
        CFStringGetCString(nameRef, inputDeviceInfo->name, 256, kCFStringEncodingUTF8);
	}*/
	
	inputDeviceInfo->name = GetDevicePropertyValue(endpointRef, kMIDIPropertyDisplayName);
    
    *info = inputDeviceInfo;
    
    return 0;
}

char* GetInputDeviceName(void* info)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    return inputDeviceInfo->name;
}

char* GetInputDeviceManufacturer(void* info)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    
    CFStringRef manufacturerRef;
    MIDIObjectGetStringProperty(inputDeviceInfo->endpointRef, kMIDIPropertyManufacturer, &manufacturerRef);
    
    return CFStringGetCStringPtr(manufacturerRef, kCFStringEncodingUTF8);
}

char* GetInputDeviceProduct(void* info)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    
    CFStringRef modelRef;
    MIDIObjectGetStringProperty(inputDeviceInfo->endpointRef, kMIDIPropertyModel, &modelRef);
    
    return CFStringGetCStringPtr(modelRef, kCFStringEncodingUTF8);
}

int GetInputDeviceDriverVersion(void* info)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    
    SInt32 driverVersion;
    MIDIObjectGetIntegerProperty(inputDeviceInfo->endpointRef, kMIDIPropertyDriverVersion, &driverVersion);
    
    return driverVersion;
}

/* ================================
 Output device
 ================================ */

typedef struct
{
    MIDIEndpointRef endpointRef;
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
    
    *info = outputDeviceInfo;
    
    return 0;
}

char* GetOutputDeviceName(void* info)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    
    CFStringRef nameRef;
    MIDIObjectGetStringProperty(outputDeviceInfo->endpointRef, kMIDIPropertyDisplayName, &nameRef);
    
    return CFStringGetCStringPtr(nameRef, kCFStringEncodingUTF8);
}

char* GetOutputDeviceManufacturer(void* info)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    
    CFStringRef manufacturerRef;
    MIDIObjectGetStringProperty(outputDeviceInfo->endpointRef, kMIDIPropertyManufacturer, &manufacturerRef);
    
    return CFStringGetCStringPtr(manufacturerRef, kCFStringEncodingUTF8);
}

char* GetOutputDeviceProduct(void* info)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    
    CFStringRef modelRef;
    MIDIObjectGetStringProperty(outputDeviceInfo->endpointRef, kMIDIPropertyModel, &modelRef);
    
    return CFStringGetCStringPtr(modelRef, kCFStringEncodingUTF8);
}

int GetOutputDeviceDriverVersion(void* info)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    
    SInt32 driverVersion;
    MIDIObjectGetIntegerProperty(outputDeviceInfo->endpointRef, kMIDIPropertyDriverVersion, &driverVersion);
    
    return driverVersion;
}