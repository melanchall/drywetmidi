#include <CoreFoundation/CoreFoundation.h>
#include <CoreMIDI/CoreMIDI.h>
#include <pthread.h>
#include <mach/mach_time.h>
#include <mach/mach.h>

#include <atomic>
#include <vector>
#include <new>
#include <cstdint>
#include <cstring>

#include "../Common/NativeApi-Constants.h"

#define PROPERTY_VALUE_BUFFER_SIZE 256
#define SMALL_BUFFER_ERROR 10000

#pragma clang diagnostic ignored "-Wswitch"

#define API_EXPORT extern "C" __attribute__((visibility("default")))

/* ================================
   Common
================================ */

API_EXPORT API_TYPE GetApiType()
{
    return API_TYPE_MAC;
}

API_EXPORT char CanCompareDevices()
{
    return 1;
}

/* ================================
   High-precision tick generator
 ================================ */

struct TickGeneratorSessionHandle
{
    pthread_t thread;
    std::atomic<char> active;
    std::atomic<char> sessionClosed;
    std::atomic<char> threadExited;
    CFRunLoopRef runLoopRef;
    TGSESSION_OPENRESULT threadStartResult;
    int threadStartError;
};

struct TickGeneratorInfo
{
    void (*callback)(void);
    CFRunLoopTimerRef timerRef;
};

void SessionCallback(CFRunLoopTimerRef timer, void *info)
{
}

void* TickGeneratorSessionThreadRoutine(void* data)
{
    TickGeneratorSessionHandle* sessionHandle = static_cast<TickGeneratorSessionHandle*>(data);

    CFRunLoopTimerContext context = { 0, nullptr, nullptr, nullptr, nullptr };
    CFRunLoopTimerRef timerRef = CFRunLoopTimerCreate(
        nullptr,
        CFAbsoluteTimeGetCurrent() + 60,
        60,
        0,
        0,
        SessionCallback,
        &context);

    CFRunLoopRef runLoopRef = CFRunLoopGetCurrent();
    CFRunLoopAddTimer(runLoopRef, timerRef, kCFRunLoopDefaultMode);
    
    // Set realtime priority
    // (thanks to https://stackoverflow.com/a/44310370/2975589)

    mach_timebase_info_data_t timebase;
    kern_return_t kr = mach_timebase_info(&timebase);
    if (kr != KERN_SUCCESS)
    {
        sessionHandle->threadStartError = kr;
        sessionHandle->threadStartResult = TGSESSION_OPENRESULT_FAILEDTOGETTIMEBASEINFO;
        return nullptr;
    }

    struct thread_time_constraint_policy constraintPolicy;

    constraintPolicy.period = 0; // Period over which we demand scheduling.
    constraintPolicy.computation = 1000 * 1000 * timebase.denom / timebase.numer; // Minimum time in a period where we must be running.
    constraintPolicy.constraint = 2000 * 1000 * timebase.denom / timebase.numer; // Maximum time between start and end of our computation in the period.
    constraintPolicy.preemptible = FALSE;

    thread_port_t threadId = pthread_mach_thread_np(pthread_self());
    kr = thread_policy_set(threadId, THREAD_TIME_CONSTRAINT_POLICY, (thread_policy_t)&constraintPolicy, THREAD_TIME_CONSTRAINT_POLICY_COUNT);
    if (kr != KERN_SUCCESS)
    {
        sessionHandle->threadStartError = kr;
        sessionHandle->threadStartResult = TGSESSION_OPENRESULT_FAILEDTOSETREALTIMEPRIORITY;
        return nullptr;
    }

    //

    sessionHandle->active.store(1);
    sessionHandle->runLoopRef = (CFRunLoopRef)CFRetain(runLoopRef);

    CFRunLoopRun();

    if (sessionHandle->runLoopRef != nullptr)
    {
        CFRelease(sessionHandle->runLoopRef);
        sessionHandle->runLoopRef = nullptr;
    }

    sessionHandle->threadExited.store(1);

    return nullptr;
}

API_EXPORT TGSESSION_OPENRESULT OpenTickGeneratorSession(void** handle, int* errorCode)
{
    *errorCode = 0;

    TickGeneratorSessionHandle* sessionHandle = new TickGeneratorSessionHandle();

    sessionHandle->threadStartResult = TGSESSION_OPENRESULT_OK;
    sessionHandle->active.store(0);
    sessionHandle->sessionClosed.store(0);
    sessionHandle->threadExited.store(0);

    int pthreadCreateResult;
    if ((pthreadCreateResult = pthread_create(&sessionHandle->thread, nullptr, TickGeneratorSessionThreadRoutine, sessionHandle)) != 0)
    {
        delete sessionHandle;
        *errorCode = pthreadCreateResult;
        return TGSESSION_OPENRESULT_THREADSTARTERROR;
    }
    
    while (sessionHandle->active.load() == 0)
    {
        if (sessionHandle->threadStartResult != TGSESSION_OPENRESULT_OK)
        {
            TGSESSION_OPENRESULT res = sessionHandle->threadStartResult;
            *errorCode = sessionHandle->threadStartError;
            delete sessionHandle;
            return res;
        }

        struct timespec ts = {0, 1000000}; // 1ms
        nanosleep(&ts, nullptr);
    }
    
    *handle = sessionHandle;

    return TGSESSION_OPENRESULT_OK;
}

API_EXPORT TGSESSION_CLOSERESULT CloseTickGeneratorSession(void* handle, int* errorCode)
{
    *errorCode = 0;

    TickGeneratorSessionHandle* sessionHandle = static_cast<TickGeneratorSessionHandle*>(handle);

    if (sessionHandle->sessionClosed.exchange(1) == 1 || sessionHandle->runLoopRef == nullptr)
        return SESSION_CLOSERESULT_OK;

    if (sessionHandle->runLoopRef != nullptr)
        CFRunLoopStop(sessionHandle->runLoopRef);

    const int maxWaitMs = 500;
    const int pollIntervalMs = 10;
    int waitedMs = 0;

    while (sessionHandle->threadExited.load() == 0 && waitedMs < maxWaitMs)
    {
        struct timespec ts = { 0, pollIntervalMs * 1000000 };
        nanosleep(&ts, nullptr);
        waitedMs += pollIntervalMs;
    }

    TGSESSION_CLOSERESULT result = sessionHandle->threadExited.load() == 1
        ? TGSESSION_CLOSERESULT_OK
        : TGSESSION_CLOSERESULT_THREADEXITTIMEOUT;

    delete sessionHandle;

    return TGSESSION_CLOSERESULT_OK;
}

void TimerCallback(CFRunLoopTimerRef timer, void *info)
{
    TickGeneratorInfo* tickGeneratorInfo = static_cast<TickGeneratorInfo*>(info);
    tickGeneratorInfo->callback();
}

API_EXPORT TG_STARTRESULT StartHighPrecisionTickGenerator_Mac(int interval, void* sessionHandle, void (*callback)(void), TickGeneratorInfo** info, int* errorCode)
{
    *errorCode = 0;

    TickGeneratorSessionHandle* pSessionHandle = static_cast<TickGeneratorSessionHandle*>(sessionHandle);
    TickGeneratorInfo* tickGeneratorInfo = new TickGeneratorInfo();

    tickGeneratorInfo->callback = callback;
    
    double seconds = static_cast<double>(interval) / 1000.0;
    
    CFRunLoopTimerContext context = { 0, tickGeneratorInfo, nullptr, nullptr, nullptr };
    CFRunLoopTimerRef timerRef = CFRunLoopTimerCreate(
        nullptr,
        CFAbsoluteTimeGetCurrent() + seconds,
        seconds,
        0,
        0,
        TimerCallback,
        &context);

    tickGeneratorInfo->timerRef = timerRef;
    CFRunLoopAddTimer(pSessionHandle->runLoopRef, timerRef, kCFRunLoopDefaultMode);

    *info = tickGeneratorInfo;

    return TG_STARTRESULT_OK;
}

API_EXPORT TG_STOPRESULT StopHighPrecisionTickGenerator(TickGeneratorSessionHandle* sessionHandle, TickGeneratorInfo* tickGeneratorInfo, int* errorCode)
{
    *errorCode = 0;

    CFRunLoopRemoveTimer(sessionHandle->runLoopRef, tickGeneratorInfo->timerRef, kCFRunLoopDefaultMode);
    delete tickGeneratorInfo;
    return TG_STOPRESULT_OK;
}

/* ================================
   Devices common
 ================================ */

struct InputDeviceInfo
{
    MIDIEndpointRef endpointRef;
};

struct OutputDeviceInfo
{
    MIDIEndpointRef endpointRef;
};

OSStatus GetDevicePropertyValue(MIDIEndpointRef endpointRef, CFStringRef propertyID, char** value)
{
    CFStringRef stringRef = nullptr;
    OSStatus status = MIDIObjectGetStringProperty(endpointRef, propertyID, &stringRef);
    if (status == noErr && stringRef != nullptr)
    {
        char* buffer = new char[PROPERTY_VALUE_BUFFER_SIZE];
        if (!CFStringGetCString(stringRef, buffer, PROPERTY_VALUE_BUFFER_SIZE, kCFStringEncodingUTF8))
        {
            delete [] buffer;
            CFRelease(stringRef);
            return SMALL_BUFFER_ERROR;
        }

        *value = buffer;
        CFRelease(stringRef);
    }

    return status;
}

OSStatus GetDeviceDriverVersion(MIDIEndpointRef endpointRef, int* value)
{
    SInt32 driverVersion;
    OSStatus status = MIDIObjectGetIntegerProperty(endpointRef, kMIDIPropertyDriverVersion, &driverVersion);

    *value = driverVersion;

    return status;
}

/* ================================
   Session
 ================================ */

typedef void (*InputDeviceCallback)(void* info, char operation);
typedef void (*OutputDeviceCallback)(void* info, char operation);

struct SessionHandle
{
    char* name;
    MIDIClientRef clientRef;
    CFRunLoopRef runLoopRef;
    pthread_t thread;
    std::atomic<char> clientCreated;
    std::atomic<char> sessionClosed;
    std::atomic<char> threadExited;
    OSStatus clientCreationStatus;
    InputDeviceCallback inputDeviceCallback;
    OutputDeviceCallback outputDeviceCallback;
};
 
void HandleSource(MIDIEndpointRef source, SessionHandle* sessionHandle, char operation)
{
    if (sessionHandle->sessionClosed.load() == 1)
        return;

    InputDeviceInfo* inputDeviceInfo = new InputDeviceInfo();
    inputDeviceInfo->endpointRef = source;

    if (sessionHandle->inputDeviceCallback != nullptr)
        sessionHandle->inputDeviceCallback(inputDeviceInfo, operation);
}

void HandleDestination(MIDIEndpointRef destination, SessionHandle* sessionHandle, char operation)
{
    if (sessionHandle->sessionClosed.load() == 1)
        return;

    OutputDeviceInfo* outputDeviceInfo = new OutputDeviceInfo();
    outputDeviceInfo->endpointRef = destination;

    if (sessionHandle->outputDeviceCallback != nullptr)
        sessionHandle->outputDeviceCallback(outputDeviceInfo, operation);
}

void HandleEntitySources(MIDIEntityRef entity, SessionHandle* sessionHandle, char operation)
{
    if (sessionHandle->sessionClosed.load() == 1)
        return;

    ItemCount _sourcesCount = MIDIEntityGetNumberOfSources(entity);
    
    for (int i = 0; i < _sourcesCount; i++)
    {
        MIDIEndpointRef source = MIDIEntityGetSource(entity, i);
        HandleSource(source, sessionHandle, operation);
    }
}

void HandleEntityDestinations(MIDIEntityRef entity, SessionHandle* sessionHandle, char operation)
{
    if (sessionHandle->sessionClosed.load() == 1)
        return;

    ItemCount _destinationsCount = MIDIEntityGetNumberOfDestinations(entity);
    
    for (int i = 0; i < _destinationsCount; i++)
    {
        MIDIEndpointRef destination = MIDIEntityGetDestination(entity, i);
        HandleDestination(destination, sessionHandle, operation);
    }
}

void HandleEntity(MIDIEntityRef entity, SessionHandle* sessionHandle, char operation)
{
    if (sessionHandle->sessionClosed.load() == 1)
        return;

    HandleEntitySources(entity, sessionHandle, operation);
    HandleEntityDestinations(entity, sessionHandle, operation);
}

void HandleDevice(MIDIDeviceRef device, SessionHandle* sessionHandle, char operation)
{
    if (sessionHandle->sessionClosed.load() == 1)
        return;

    ItemCount entitiesCount = MIDIDeviceGetNumberOfEntities(device);
    
    for (int i = 0; i < entitiesCount; i++)
    {
        MIDIEntityRef entity = MIDIDeviceGetEntity(device, i);
        HandleEntity(entity, sessionHandle, operation);
    }
}

void HandleNotification(const MIDINotification* message, SessionHandle* sessionHandle)
{
    if (sessionHandle->sessionClosed.load() == 1)
        return;

    switch (message->messageID)
    {
        case kMIDIMsgObjectAdded:
        case kMIDIMsgObjectRemoved:
        {
            char operation = message->messageID == kMIDIMsgObjectAdded ? 1 : 0;
            
            MIDIObjectAddRemoveNotification* n = (MIDIObjectAddRemoveNotification*)message;
            
            switch (n->childType)
            {
                case kMIDIObjectType_Device:
                {
                    HandleDevice(n->child, sessionHandle, operation);
                    break;
                }
                case kMIDIObjectType_Entity:
                {
                    HandleEntity(n->child, sessionHandle, operation);
                    break;
                }
                case kMIDIObjectType_Source:
                {
                    HandleSource(n->child, sessionHandle, operation);                    
                    break;
                }
                case kMIDIObjectType_Destination:
                {
                    HandleDestination(n->child, sessionHandle, operation);                    
                    break;
                }
            }
            
            break;
        }
    }
}

void NotifyProc(const MIDINotification* message, void* refCon)
{
    SessionHandle* sessionHandle = static_cast<SessionHandle*>(refCon);
    if (sessionHandle->sessionClosed.load() == 1)
        return;

    HandleNotification(message, sessionHandle);
}

void* ThreadProc(void* data)
{
    SessionHandle* sessionHandle = static_cast<SessionHandle*>(data);
    
    CFStringRef nameRef = CFStringCreateWithCString(kCFAllocatorDefault, sessionHandle->name, kCFStringEncodingUTF8);
    sessionHandle->clientCreationStatus = MIDIClientCreate(nameRef, NotifyProc, data, &sessionHandle->clientRef);
    sessionHandle->clientCreated.store(1);
    sessionHandle->runLoopRef = (CFRunLoopRef)CFRetain(CFRunLoopGetCurrent());

    if (nameRef)
        CFRelease(nameRef);
    
    CFRunLoopRun();

    if (sessionHandle->runLoopRef != nullptr)
    {
        CFRelease(sessionHandle->runLoopRef);
        sessionHandle->runLoopRef = nullptr;
    }

    sessionHandle->threadExited.store(1);

    return nullptr;
}

API_EXPORT SESSION_OPENRESULT OpenSession_Mac(char* name, InputDeviceCallback inputDeviceCallback, OutputDeviceCallback outputDeviceCallback, void** handle, int* errorCode)
{
    *errorCode = 0;

    SessionHandle* sessionHandle = new SessionHandle();
    
    sessionHandle->name = name;
    sessionHandle->inputDeviceCallback = inputDeviceCallback;
    sessionHandle->outputDeviceCallback = outputDeviceCallback;
    sessionHandle->clientCreated.store(0);
    sessionHandle->sessionClosed.store(0);
    sessionHandle->threadExited.store(0);
    
    int pthreadCreateResult;
    if ((pthreadCreateResult = pthread_create(&sessionHandle->thread, nullptr, ThreadProc, sessionHandle)) != 0)
    {
        *errorCode = pthreadCreateResult;
        return SESSION_OPENRESULT_THREADSTARTERROR;
    }
    
    while (sessionHandle->clientCreated.load() == 0) {}

    if (sessionHandle->clientCreationStatus != noErr)
    {
        OSStatus clientCreationStatus = sessionHandle->clientCreationStatus;
        *errorCode = clientCreationStatus;

        delete sessionHandle;

        switch (clientCreationStatus)
        {
            case kMIDIServerStartErr: return SESSION_OPENRESULT_SERVERSTARTERROR;
            case kMIDIWrongThread: return SESSION_OPENRESULT_WRONGTHREAD;
            case kMIDINotPermitted: return SESSION_OPENRESULT_NOTPERMITTED;
        }
        
        return SESSION_OPENRESULT_UNKNOWNERROR;
    }

    *handle = sessionHandle;

    return SESSION_OPENRESULT_OK;
}

API_EXPORT SESSION_CLOSERESULT CloseSession(void* handle)
{
    SessionHandle* sessionHandle = static_cast<SessionHandle*>(handle);

    if (sessionHandle->sessionClosed.exchange(1) == 1 || sessionHandle->runLoopRef == nullptr)
        return SESSION_CLOSERESULT_OK;
    
    sessionHandle->inputDeviceCallback = nullptr;
    sessionHandle->outputDeviceCallback = nullptr;

    if (sessionHandle->runLoopRef != nullptr)
        CFRunLoopStop(sessionHandle->runLoopRef);

    const int maxWaitMs = 500;
    const int pollIntervalMs = 10;
    int waitedMs = 0;

    while (sessionHandle->threadExited.load() == 0 && waitedMs < maxWaitMs)
    {
        struct timespec ts = { 0, pollIntervalMs * 1000000 };
        nanosleep(&ts, nullptr);
        waitedMs += pollIntervalMs;
    }

    SESSION_CLOSERESULT result = sessionHandle->threadExited.load() == 1
        ? SESSION_CLOSERESULT_OK
        : SESSION_CLOSERESULT_THREADEXITTIMEOUT;

    delete sessionHandle;
    return result;
}

/* ================================
   Input device
 ================================ */

struct InputDeviceHandle
{
    InputDeviceInfo* info;
    MIDIPortRef portRef;
};

API_EXPORT IN_GETCOUNTRESULT GetInputDevicesCount(int* count)
{
    *count = static_cast<int>(MIDIGetNumberOfSources());
    return IN_GETCOUNTRESULT_OK;
}

API_EXPORT IN_GETINFORESULT GetInputDeviceInfo(int deviceIndex, void** info, int* errorCode)
{
    *errorCode = 0;

    InputDeviceInfo* inputDeviceInfo = new InputDeviceInfo();

    MIDIEndpointRef endpointRef = MIDIGetSource(deviceIndex);
    if (endpointRef == 0)
    {
        delete inputDeviceInfo;
        return IN_GETINFORESULT_UNKNOWNERROR;
    }

    inputDeviceInfo->endpointRef = endpointRef;

    *info = inputDeviceInfo;

    return IN_GETINFORESULT_OK;
}

API_EXPORT int GetInputDeviceHashCode(void* info)
{
    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);
    return static_cast<int>(inputDeviceInfo->endpointRef);
}

API_EXPORT char AreInputDevicesEqual(void* info1, void* info2)
{
    InputDeviceInfo* inputDeviceInfo1 = static_cast<InputDeviceInfo*>(info1);
    InputDeviceInfo* inputDeviceInfo2 = static_cast<InputDeviceInfo*>(info2);
    
    return static_cast<char>(inputDeviceInfo1->endpointRef == inputDeviceInfo2->endpointRef);
}

IN_GETPROPERTYRESULT GetInputDeviceStringPropertyValue(InputDeviceInfo* inputDeviceInfo, CFStringRef propertyID, char** value, int* errorCode)
{
    *errorCode = 0;

    OSStatus status = GetDevicePropertyValue(inputDeviceInfo->endpointRef, propertyID, value);
    if (status != noErr)
    {
        *errorCode = status;

        switch (status)
        {
            case kMIDIUnknownEndpoint: return IN_GETPROPERTYRESULT_UNKNOWNENDPOINT;
            case SMALL_BUFFER_ERROR: return IN_GETPROPERTYRESULT_TOOLONG;
            case kMIDIUnknownProperty: return IN_GETPROPERTYRESULT_UNKNOWNPROPERTY;
        }
        
        return IN_GETPROPERTYRESULT_UNKNOWNERROR;
    }
    
    return IN_GETPROPERTYRESULT_OK;
}

IN_GETPROPERTYRESULT GetInputDeviceIntPropertyValue(InputDeviceInfo* inputDeviceInfo, CFStringRef propertyID, int* value, int* errorCode)
{
    *errorCode = 0;

    OSStatus status = MIDIObjectGetIntegerProperty(inputDeviceInfo->endpointRef, propertyID, value);
    if (status != noErr)
    {
        *errorCode = status;

        switch (status)
        {
            case kMIDIUnknownEndpoint: return IN_GETPROPERTYRESULT_UNKNOWNENDPOINT;
            case kMIDIUnknownProperty: return IN_GETPROPERTYRESULT_UNKNOWNPROPERTY;
        }
        
        return IN_GETPROPERTYRESULT_UNKNOWNERROR;
    }
    
    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_GETPROPERTYRESULT GetInputDeviceName(void* info, char** value, int* errorCode)
{
    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);
    return GetInputDeviceStringPropertyValue(inputDeviceInfo, kMIDIPropertyDisplayName, value, errorCode);
}

API_EXPORT IN_GETPROPERTYRESULT GetInputDeviceManufacturer(void* info, char** value, int* errorCode)
{
    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);
    return GetInputDeviceStringPropertyValue(inputDeviceInfo, kMIDIPropertyManufacturer, value, errorCode);
}

API_EXPORT IN_GETPROPERTYRESULT GetInputDeviceProduct(void* info, char** value, int* errorCode)
{
    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);
    return GetInputDeviceStringPropertyValue(inputDeviceInfo, kMIDIPropertyModel, value, errorCode);
}

API_EXPORT IN_GETPROPERTYRESULT GetInputDeviceDriverVersion(void* info, int* value, int* errorCode)
{
    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);
    return GetInputDeviceIntPropertyValue(inputDeviceInfo, kMIDIPropertyDriverVersion, value, errorCode);
}

API_EXPORT IN_GETPROPERTYRESULT GetInputDeviceUniqueId(void* info, int* value, int* errorCode)
{
    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);
    return GetInputDeviceIntPropertyValue(inputDeviceInfo, kMIDIPropertyUniqueID, value, errorCode);
}

API_EXPORT IN_GETPROPERTYRESULT GetInputDeviceDriverOwner(void* info, char** value, int* errorCode)
{
    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);
    return GetInputDeviceStringPropertyValue(inputDeviceInfo, kMIDIPropertyDriverOwner, value, errorCode);
}

API_EXPORT IN_OPENRESULT OpenInputDevice_Mac(void* info, void* sessionHandle, MIDIReadProc callback, void** handle, int* errorCode)
{
    *errorCode = 0;

    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(info);
    SessionHandle* pSessionHandle = static_cast<SessionHandle*>(sessionHandle);

    InputDeviceHandle* inputDeviceHandle = new InputDeviceHandle();
    inputDeviceHandle->info = inputDeviceInfo;

    *handle = inputDeviceHandle;

    CFStringRef portNameRef = CFSTR("IN");
    OSStatus status = MIDIInputPortCreate(pSessionHandle->clientRef, portNameRef, callback, nullptr, &inputDeviceHandle->portRef);
    if (status != noErr)
    {
        delete inputDeviceHandle;

        *errorCode = status;

        switch (status)
        {
            case kMIDIInvalidClient: return IN_OPENRESULT_INVALIDCLIENT;
            case kMIDIWrongThread: return IN_OPENRESULT_WRONGTHREAD;
            case kMIDINotPermitted: return IN_OPENRESULT_NOTPERMITTED;
        }
        
        return IN_OPENRESULT_UNKNOWNERROR;
    }

    return IN_OPENRESULT_OK;
}

API_EXPORT IN_CLOSERESULT CloseInputDevice(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputDeviceHandle* inputDeviceHandle = static_cast<InputDeviceHandle*>(handle);

    delete inputDeviceHandle->info;
    delete inputDeviceHandle;

    return IN_CLOSERESULT_OK;
}

API_EXPORT IN_CONNECTRESULT ConnectToInputDevice(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputDeviceHandle* inputDeviceHandle = static_cast<InputDeviceHandle*>(handle);

    OSStatus status = MIDIPortConnectSource(inputDeviceHandle->portRef, inputDeviceHandle->info->endpointRef, nullptr);
    if (status != noErr)
    {
        *errorCode = status;

        switch (status)
        {
            case kMIDIInvalidPort: return IN_CONNECTRESULT_INVALIDPORT;
            case kMIDIWrongThread: return IN_CONNECTRESULT_WRONGTHREAD;
            case kMIDINotPermitted: return IN_CONNECTRESULT_NOTPERMITTED;
            case kMIDIUnknownEndpoint: return IN_CONNECTRESULT_UNKNOWNENDPOINT;
            case kMIDIWrongEndpointType: return IN_CONNECTRESULT_WRONGENDPOINT;
        }
        
        return IN_CONNECTRESULT_UNKNOWNERROR;
    }

    return IN_CONNECTRESULT_OK;
}

API_EXPORT IN_DISCONNECTRESULT DisconnectFromInputDevice(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputDeviceHandle* inputDeviceHandle = static_cast<InputDeviceHandle*>(handle);

    OSStatus status = MIDIPortDisconnectSource(inputDeviceHandle->portRef, inputDeviceHandle->info->endpointRef);
    if (status != noErr)
    {
        *errorCode = status;

        switch (status)
        {
            case kMIDIInvalidPort: return IN_DISCONNECTRESULT_INVALIDPORT;
            case kMIDIWrongThread: return IN_DISCONNECTRESULT_WRONGTHREAD;
            case kMIDINotPermitted: return IN_DISCONNECTRESULT_NOTPERMITTED;
            case kMIDIUnknownEndpoint: return IN_DISCONNECTRESULT_UNKNOWNENDPOINT;
            case kMIDIWrongEndpointType: return IN_DISCONNECTRESULT_WRONGENDPOINT;
            case kMIDINoConnection: return IN_DISCONNECTRESULT_NOCONNECTION;
        }
        
        return IN_DISCONNECTRESULT_UNKNOWNERROR;
    }

    return IN_DISCONNECTRESULT_OK;
}

API_EXPORT IN_GETEVENTDATARESULT GetEventDataFromInputDevice(MIDIPacketList* packetList, int packetIndex, Byte** data, int* length, int* packetsCount)
{
    *packetsCount = packetList->numPackets;
    
    if (packetIndex == 0)
    {
        *data = packetList->packet[0].data;
        *length = packetList->packet[0].length;
        return IN_GETEVENTDATARESULT_OK;
    }

    MIDIPacket* packetPtr = packetList->packet;

    for (int i = 0; i < packetIndex; i++)
    {
        packetPtr = MIDIPacketNext(packetPtr);
    }

    *data = packetPtr->data;
    *length = packetPtr->length;

    return IN_GETEVENTDATARESULT_OK;
}

API_EXPORT char IsInputDevicePropertySupported(IN_PROPERTY property)
{
    switch (property)
    {
        case IN_PROPERTY_PRODUCT:
        case IN_PROPERTY_MANUFACTURER:
        case IN_PROPERTY_DRIVERVERSION:
        case IN_PROPERTY_UNIQUEID:
        case IN_PROPERTY_DRIVEROWNER:
            return 1;
    }
    
    return 0;
}

/* ================================
   Output device
 ================================ */

struct OutputDeviceHandle
{
    OutputDeviceInfo* info;
    MIDIPortRef portRef;
};

API_EXPORT OUT_GETCOUNTRESULT GetOutputDevicesCount(int* count)
{
    *count = static_cast<int>(MIDIGetNumberOfDestinations());
    return OUT_GETCOUNTRESULT_OK;
}

API_EXPORT OUT_GETINFORESULT GetOutputDeviceInfo(int deviceIndex, void** info, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceInfo* outputDeviceInfo = new OutputDeviceInfo();

    MIDIEndpointRef endpointRef = MIDIGetDestination(deviceIndex);
    if (endpointRef == 0)
    {
        delete outputDeviceInfo;
        return OUT_GETINFORESULT_UNKNOWNERROR;
    }

    outputDeviceInfo->endpointRef = endpointRef;

    *info = outputDeviceInfo;

    return OUT_GETINFORESULT_OK;
}

API_EXPORT int GetOutputDeviceHashCode(void* info)
{
    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);
    return static_cast<int>(outputDeviceInfo->endpointRef);
}

API_EXPORT char AreOutputDevicesEqual(void* info1, void* info2)
{
    OutputDeviceInfo* outputDeviceInfo1 = static_cast<OutputDeviceInfo*>(info1);
    OutputDeviceInfo* outputDeviceInfo2 = static_cast<OutputDeviceInfo*>(info2);
    
    return static_cast<char>(outputDeviceInfo1->endpointRef == outputDeviceInfo2->endpointRef);
}

OUT_GETPROPERTYRESULT GetOutputDeviceStringPropertyValue(OutputDeviceInfo* outputDeviceInfo, CFStringRef propertyID, char** value, int* errorCode)
{
    *errorCode = 0;

    OSStatus status = GetDevicePropertyValue(outputDeviceInfo->endpointRef, propertyID, value);
    if (status != noErr)
    {
        *errorCode = status;

        switch (status)
        {
            case kMIDIUnknownEndpoint: return OUT_GETPROPERTYRESULT_UNKNOWNENDPOINT;
            case SMALL_BUFFER_ERROR: return OUT_GETPROPERTYRESULT_TOOLONG;
            case kMIDIUnknownProperty: return OUT_GETPROPERTYRESULT_UNKNOWNPROPERTY;
        }
        
        return OUT_GETPROPERTYRESULT_UNKNOWNERROR;
    }
    
    return OUT_GETPROPERTYRESULT_OK;
}

OUT_GETPROPERTYRESULT GetOutputDeviceIntPropertyValue(OutputDeviceInfo* outputDeviceInfo, CFStringRef propertyID, int* value, int* errorCode)
{
    *errorCode = 0;

    OSStatus status = MIDIObjectGetIntegerProperty(outputDeviceInfo->endpointRef, propertyID, value);
    if (status != noErr)
    {
        *errorCode = status;

        switch (status)
        {
            case kMIDIUnknownEndpoint: return OUT_GETPROPERTYRESULT_UNKNOWNENDPOINT;
            case kMIDIUnknownProperty: return OUT_GETPROPERTYRESULT_UNKNOWNPROPERTY;
        }
        
        return OUT_GETPROPERTYRESULT_UNKNOWNERROR;
    }
    
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputDeviceName(void* info, char** value, int* errorCode)
{
    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);
    return GetOutputDeviceStringPropertyValue(outputDeviceInfo, kMIDIPropertyDisplayName, value, errorCode);
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputDeviceManufacturer(void* info, char** value, int* errorCode)
{
    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);
    return GetOutputDeviceStringPropertyValue(outputDeviceInfo, kMIDIPropertyManufacturer, value, errorCode);
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputDeviceProduct(void* info, char** value, int* errorCode)
{
    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);
    return GetOutputDeviceStringPropertyValue(outputDeviceInfo, kMIDIPropertyModel, value, errorCode);
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputDeviceDriverVersion(void* info, int* value, int* errorCode)
{
    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);
    return GetOutputDeviceIntPropertyValue(outputDeviceInfo, kMIDIPropertyDriverVersion, value, errorCode);
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputDeviceUniqueId(void* info, int* value, int* errorCode)
{
    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);
    return GetOutputDeviceIntPropertyValue(outputDeviceInfo, kMIDIPropertyUniqueID, value, errorCode);
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputDeviceDriverOwner(void* info, char** value, int* errorCode)
{
    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);
    return GetOutputDeviceStringPropertyValue(outputDeviceInfo, kMIDIPropertyDriverOwner, value, errorCode);
}

API_EXPORT OUT_OPENRESULT OpenOutputDevice_Mac(void* info, void* sessionHandle, void** handle, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceInfo* outputDeviceInfo = static_cast<OutputDeviceInfo*>(info);
    SessionHandle* pSessionHandle = static_cast<SessionHandle*>(sessionHandle);

    OutputDeviceHandle* outputDeviceHandle = new OutputDeviceHandle();
    outputDeviceHandle->info = outputDeviceInfo;

    *handle = outputDeviceHandle;

    CFStringRef portNameRef = CFSTR("OUT");
    OSStatus result = MIDIOutputPortCreate(pSessionHandle->clientRef, portNameRef, &outputDeviceHandle->portRef);
    if (result != noErr)
    {
        delete outputDeviceHandle;

        *errorCode = result;

        switch (result)
        {
            case kMIDIInvalidClient: return OUT_OPENRESULT_INVALIDCLIENT;
            case kMIDIWrongThread: return OUT_OPENRESULT_WRONGTHREAD;
            case kMIDINotPermitted: return OUT_OPENRESULT_NOTPERMITTED;
        }
        
        return OUT_OPENRESULT_UNKNOWNERROR;
    }

    return OUT_OPENRESULT_OK;
}

API_EXPORT OUT_CLOSERESULT CloseOutputDevice(void* handle, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceHandle* outputDeviceHandle = static_cast<OutputDeviceHandle*>(handle);

    delete outputDeviceHandle->info;
    delete outputDeviceHandle;

    return OUT_CLOSERESULT_OK;
}

API_EXPORT OUT_SENDSHORTRESULT SendShortEventToOutputDevice(void* handle, int message, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceHandle* outputDeviceHandle = static_cast<OutputDeviceHandle*>(handle);

    Byte data[3];
    Byte statusByte = static_cast<Byte>(message & 0xFF);
    data[0] = statusByte;
    ByteCount dataSize = 1;

    if (statusByte < 0xF8 && statusByte != 0xF6)
    {
        data[1] = static_cast<Byte>((message >> 8) & 0xFF);
        dataSize++;

        Byte channelStatus = static_cast<Byte>(statusByte >> 4);
        if (channelStatus == 0x8 || channelStatus == 0x9 || channelStatus == 0xA || channelStatus == 0xB || channelStatus == 0xE || statusByte == 0xF2)
        {
            data[2] = static_cast<Byte>(message >> 16);
            dataSize++;
        }
    }

    std::vector<Byte> bufferVec(static_cast<size_t>(dataSize) + sizeof(MIDIPacketList));
    MIDIPacketList* packetList = reinterpret_cast<MIDIPacketList*>(bufferVec.data());
    MIDIPacket* packet = MIDIPacketListInit(packetList);
    MIDIPacketListAdd(packetList, static_cast<ByteCount>(bufferVec.size()), packet, 0, dataSize, &data[0]);

    OSStatus result = MIDISend(outputDeviceHandle->portRef, outputDeviceHandle->info->endpointRef, packetList);
    if (result != noErr)
    {
        *errorCode = result;

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
        }
        
        return OUT_SENDSHORTRESULT_UNKNOWNERROR;
    }

    return OUT_SENDSHORTRESULT_OK;
}

API_EXPORT OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Mac(void* handle, Byte* data, ByteCount dataSize, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceHandle* outputDeviceHandle = static_cast<OutputDeviceHandle*>(handle);

    std::vector<Byte> bufferVec(static_cast<size_t>(dataSize) + sizeof(MIDIPacketList));
    MIDIPacketList* packetList = reinterpret_cast<MIDIPacketList*>(bufferVec.data());
    MIDIPacket* packet = MIDIPacketListInit(packetList);
    MIDIPacketListAdd(packetList, static_cast<ByteCount>(bufferVec.size()), packet, 0, dataSize, &data[0]);

    OSStatus result = MIDISend(outputDeviceHandle->portRef, outputDeviceHandle->info->endpointRef, packetList);
    if (result != noErr)
    {
        *errorCode = result;

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
        }
        
        return OUT_SENDSYSEXRESULT_UNKNOWNERROR;
    }

    return OUT_SENDSYSEXRESULT_OK;
}

API_EXPORT char IsOutputDevicePropertySupported(OUT_PROPERTY property)
{
    switch (property)
    {
        case OUT_PROPERTY_PRODUCT:
        case OUT_PROPERTY_MANUFACTURER:
        case OUT_PROPERTY_DRIVERVERSION:
        case OUT_PROPERTY_UNIQUEID:
        case OUT_PROPERTY_DRIVEROWNER:
            return 1;
    }
    
    return 0;
}

/* ================================
 Virtual device
 ================================ */

struct VirtualDeviceInfo
{
    InputDeviceInfo* inputDeviceInfo;
    OutputDeviceInfo* outputDeviceInfo;
    char* name;
};

API_EXPORT char IsVirtualDeviceApiAvailable(SessionHandle* sessionHandle)
{
    return 1;
}

API_EXPORT VIRTUAL_OPENRESULT OpenVirtualDevice_Mac(char* name, SessionHandle* sessionHandle, MIDIReadProc callback, VirtualDeviceInfo** info, int* errorCode)
{
    *errorCode = 0;

    VirtualDeviceInfo* virtualDeviceInfo = new VirtualDeviceInfo();
    virtualDeviceInfo->name = name;
    
    CFStringRef nameRef = CFStringCreateWithCString(nullptr, name, kCFStringEncodingUTF8);
    
    MIDIEndpointRef sourceRef;
    OSStatus status = MIDISourceCreate(sessionHandle->clientRef, nameRef, &sourceRef);
    if (nameRef)
        CFRelease(nameRef);
    
    if (status != noErr)
    {
        delete virtualDeviceInfo;

        *errorCode = status;

        switch (status)
        {
            case kMIDIServerStartErr: return VIRTUAL_OPENRESULT_CREATESOURCE_SERVERSTARTERROR;
            case kMIDIWrongThread: return VIRTUAL_OPENRESULT_CREATESOURCE_WRONGTHREAD;
            case kMIDINotPermitted: return VIRTUAL_OPENRESULT_CREATESOURCE_NOTPERMITTED;
        }
        
        return VIRTUAL_OPENRESULT_CREATESOURCE_UNKNOWNERROR;
    }
    
    InputDeviceInfo* inputDeviceInfo = new InputDeviceInfo();
    inputDeviceInfo->endpointRef = sourceRef;
    virtualDeviceInfo->inputDeviceInfo = inputDeviceInfo;
    
    CFStringRef nameRef2 = CFStringCreateWithCString(nullptr, name, kCFStringEncodingUTF8);
    MIDIEndpointRef destinationRef;
    status = MIDIDestinationCreate(sessionHandle->clientRef, nameRef2, callback, inputDeviceInfo, &destinationRef);
    if (nameRef2)
        CFRelease(nameRef2);
    
    if (status != noErr)
    {
        delete inputDeviceInfo;
        delete virtualDeviceInfo;

        *errorCode = status;

        switch (status)
        {
            case kMIDIServerStartErr: return VIRTUAL_OPENRESULT_CREATEDESTINATION_SERVERSTARTERROR;
            case kMIDIWrongThread: return VIRTUAL_OPENRESULT_CREATEDESTINATION_WRONGTHREAD;
            case kMIDINotPermitted: return VIRTUAL_OPENRESULT_CREATEDESTINATION_NOTPERMITTED;
        }
        
        return VIRTUAL_OPENRESULT_CREATEDESTINATION_UNKNOWNERROR;
    }
    
    OutputDeviceInfo* outputDeviceInfo = new OutputDeviceInfo();
    outputDeviceInfo->endpointRef = destinationRef;
    virtualDeviceInfo->outputDeviceInfo = outputDeviceInfo;
    
    *info = virtualDeviceInfo;
    
    return VIRTUAL_OPENRESULT_OK;
}

API_EXPORT VIRTUAL_CLOSERESULT CloseVirtualDevice(VirtualDeviceInfo* info, int* errorCode)
{
    *errorCode = 0;

    OSStatus status = MIDIEndpointDispose(info->inputDeviceInfo->endpointRef);
    if (status != noErr)
    {
        *errorCode = status;

        switch (status)
        {
            case kMIDIUnknownEndpoint: return VIRTUAL_CLOSERESULT_DISPOSESOURCE_UNKNOWNENDPOINT;
            case kMIDINotPermitted: return VIRTUAL_CLOSERESULT_DISPOSESOURCE_NOTPERMITTED;
        }
        
        return VIRTUAL_CLOSERESULT_DISPOSESOURCE_UNKNOWNERROR;
    }
    
    status = MIDIEndpointDispose(info->outputDeviceInfo->endpointRef);
    if (status != noErr)
    {
        *errorCode = status;

        switch (status)
        {
            case kMIDIUnknownEndpoint: return VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_UNKNOWNENDPOINT;
            case kMIDINotPermitted: return VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_NOTPERMITTED;
        }
        
        return VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_UNKNOWNERROR;
    }
    
    // TODO: check
    // delete info->inputDeviceInfo;
    // delete info->outputDeviceInfo;
    delete info;
    
    return VIRTUAL_CLOSERESULT_OK;
}

API_EXPORT VIRTUAL_SENDBACKRESULT SendDataBackFromVirtualDevice(const MIDIPacketList *pktlist, void *readProcRefCon, int* errorCode)
{
    *errorCode = 0;

    InputDeviceInfo* inputDeviceInfo = static_cast<InputDeviceInfo*>(readProcRefCon);
    
    OSStatus status = MIDIReceived(inputDeviceInfo->endpointRef, pktlist);
    if (status != noErr)
    {
        *errorCode = status;

        switch (status)
        {
            case kMIDIUnknownEndpoint: return VIRTUAL_SENDBACKRESULT_UNKNOWNENDPOINT;
            case kMIDINotPermitted: return VIRTUAL_SENDBACKRESULT_NOTPERMITTED;
            case kMIDIWrongEndpointType: return VIRTUAL_SENDBACKRESULT_WRONGENDPOINT;
            case kMIDIMessageSendErr: return VIRTUAL_SENDBACKRESULT_MESSAGESENDERROR;
            case kMIDIServerStartErr: return VIRTUAL_SENDBACKRESULT_SERVERSTARTERROR;
            case kMIDIWrongThread: return VIRTUAL_SENDBACKRESULT_WRONGTHREAD;
        }
        
        return VIRTUAL_SENDBACKRESULT_UNKNOWNERROR;
    }
    
    return VIRTUAL_SENDBACKRESULT_OK;
}

API_EXPORT void* GetInputDeviceInfoFromVirtualDevice(VirtualDeviceInfo* info)
{
    return info->inputDeviceInfo;
}

API_EXPORT void* GetOutputDeviceInfoFromVirtualDevice(VirtualDeviceInfo* info)
{
    return info->outputDeviceInfo;
}

API_EXPORT VIRTUAL_MUTERESULT MuteVirtualDevice(VirtualDeviceInfo* info)
{
    // TODO

    return VIRTUAL_MUTERESULT_OK;
}

API_EXPORT VIRTUAL_UNMUTERESULT UnmuteVirtualDevice(VirtualDeviceInfo* info)
{
    // TODO

    return VIRTUAL_UNMUTERESULT_OK;
}