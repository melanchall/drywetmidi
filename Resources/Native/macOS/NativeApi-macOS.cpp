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
#include <chrono>
#include <thread>

#include "../Common/NativeApi-Constants.h"

#pragma clang diagnostic ignored "-Wswitch"

#define API_EXPORT extern "C" __attribute__((visibility("default")))

/* ================================
   Common
================================ */

API_EXPORT API_TYPE GetApiType()
{
    return API_TYPE_MAC;
}

API_EXPORT void FreeBuffer(const char* buffer)
{
    delete[] buffer;
}

const char* CloneCString(const char* value)
{
    if (value == nullptr)
        return nullptr;

    auto length = std::strlen(value);
    char* copy = new char[length + 1];
    std::memcpy(copy, value, length + 1);
    return copy;
}

/* ================================
   Configuration
================================ */

typedef void (*NativeApiActivityCallback)(const char* record);

struct Configuration
{
    NativeApiActivityCallback activityCallback;
};

API_EXPORT CONFIGURATION_GETRESULT GetConfiguration_Mac(
    NativeApiActivityCallback activityCallback,
    Configuration** configuration,
    int* errorCode)
{
    *errorCode = 0;

    Configuration* config = new Configuration();

    config->activityCallback = activityCallback;

    *configuration = config;
    config->activityCallback("Configuration initialized for macOS");

    return CONFIGURATION_GETRESULT_OK;
}

API_EXPORT CONFIGURATION_CLEANUPRESULT CleanupConfiguration(Configuration* configuration)
{
    delete configuration;

    return CONFIGURATION_CLEANUPRESULT_OK;
}

API_EXPORT bool IsVirtualDeviceApiAvailable(Configuration* configuration)
{
    return true;
}

API_EXPORT bool IsDevicesCachingRequired(Configuration* configuration)
{
    return false;
}

API_EXPORT bool IsDevicesWatcherApiAvailable(Configuration* configuration)
{
    return true;
}

API_EXPORT void CheckNativeApiActivityCallback(Configuration* configuration)
{
    configuration->activityCallback("Native API activity callback works!");
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
    CFRunLoopTimerRef timerRef = nullptr;
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

    sessionHandle->timerRef = timerRef;

    CFRunLoopRef runLoopRef = CFRunLoopGetCurrent();
    CFRunLoopAddTimer(runLoopRef, timerRef, kCFRunLoopDefaultMode);
    CFRelease(timerRef);
    
    // Set realtime priority
    // (thanks to https://stackoverflow.com/a/44310370/2975589)

    mach_timebase_info_data_t timebase;
    kern_return_t kr = mach_timebase_info(&timebase);
    if (kr != KERN_SUCCESS)
    {
        CFRunLoopRemoveTimer(runLoopRef, timerRef, kCFRunLoopDefaultMode);
        sessionHandle->timerRef = nullptr;
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
        CFRunLoopRemoveTimer(runLoopRef, timerRef, kCFRunLoopDefaultMode);
        sessionHandle->timerRef = nullptr;
        sessionHandle->threadStartError = kr;
        sessionHandle->threadStartResult = TGSESSION_OPENRESULT_FAILEDTOSETREALTIMEPRIORITY;
        return nullptr;
    }

    //

    sessionHandle->runLoopRef = (CFRunLoopRef)CFRetain(runLoopRef);
    sessionHandle->active.store(1);

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

    if (sessionHandle->sessionClosed.exchange(1) == 1)
        return SESSION_CLOSERESULT_OK;

    if (sessionHandle->runLoopRef == nullptr)
    {
        delete sessionHandle;
        return SESSION_CLOSERESULT_OK;
    }

    if (sessionHandle->timerRef != nullptr)
    {
        CFRunLoopRemoveTimer(sessionHandle->runLoopRef, sessionHandle->timerRef, kCFRunLoopDefaultMode);
        sessionHandle->timerRef = nullptr;
    }

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
    CFRelease(tickGeneratorInfo->timerRef);

    delete tickGeneratorInfo;
    return TG_STOPRESULT_OK;
}

/* ================================
   Devices common
 ================================ */

struct EndpointInfoBase
{
    MIDIEndpointRef endpointRef;

    const char* deviceId = nullptr;
    const char* deviceName = nullptr;
    const char* deviceManufacturer = nullptr;
    const char* deviceModel = nullptr;
    const char* deviceDriverVersion = nullptr;
};

void FreeParentDeviceInfoStrings(EndpointInfoBase* info)
{
    if (info == nullptr)
        return;

    delete[] info->deviceId;
    delete[] info->deviceName;
    delete[] info->deviceManufacturer;
    delete[] info->deviceModel;
    delete[] info->deviceDriverVersion;

    info->deviceName = nullptr;
    info->deviceManufacturer = nullptr;
    info->deviceModel = nullptr;
    info->deviceId = nullptr;
    info->deviceDriverVersion = nullptr;
}

struct InputEndpointInfo : EndpointInfoBase
{
};

API_EXPORT void CloneInputEndpointInfo(InputEndpointInfo* source, InputEndpointInfo** info)
{
    InputEndpointInfo* result = new InputEndpointInfo();

    result->endpointRef = source->endpointRef;
    result->deviceId = CloneCString(source->deviceId);
    result->deviceName = CloneCString(source->deviceName);
    result->deviceManufacturer = CloneCString(source->deviceManufacturer);
    result->deviceModel = CloneCString(source->deviceModel);
    result->deviceDriverVersion = CloneCString(source->deviceDriverVersion);

    *info = result;
}

API_EXPORT void DeleteInputEndpointInfo(InputEndpointInfo* info)
{
    FreeParentDeviceInfoStrings(info);
    delete info;
}

struct OutputEndpointInfo : EndpointInfoBase
{
};

API_EXPORT void CloneOutputEndpointInfo(OutputEndpointInfo* source, OutputEndpointInfo** info)
{
    OutputEndpointInfo* result = new OutputEndpointInfo();

    result->endpointRef = source->endpointRef;
    result->deviceId = CloneCString(source->deviceId);
    result->deviceName = CloneCString(source->deviceName);
    result->deviceManufacturer = CloneCString(source->deviceManufacturer);
    result->deviceModel = CloneCString(source->deviceModel);
    result->deviceDriverVersion = CloneCString(source->deviceDriverVersion);

    *info = result;
}

API_EXPORT void DeleteOutputEndpointInfo(OutputEndpointInfo* info)
{
    FreeParentDeviceInfoStrings(info);
    delete info;
}

GETSTRINGPROPERTYRESULT GetStringPropertyValue(MIDIObjectRef obj, CFStringRef property, const char** value, int* errorCode)
{
    *errorCode = 0;

    CFStringRef stringRef = nullptr;
    OSStatus status = MIDIObjectGetStringProperty(obj, property, &stringRef);
    if (status == kMIDIUnknownProperty)
    {
        *errorCode = status;
        return GETSTRINGPROPERTYRESULT_PROPERTYUNAVAILABLE;
    }

    if (status != noErr || stringRef == nullptr)
    {
        *errorCode = status;
        return GETSTRINGPROPERTYRESULT_FAILEDGETVALUE;
    }

    CFIndex length = CFStringGetLength(stringRef);
    CFIndex maxSize = CFStringGetMaximumSizeForEncoding(length, kCFStringEncodingUTF8) + 1;
        
    char* buffer = new char[maxSize];

    if (!CFStringGetCString(stringRef, buffer, maxSize, kCFStringEncodingUTF8))
    {
        delete[] buffer;
        CFRelease(stringRef);
        return GETSTRINGPROPERTYRESULT_FAILEDFILLVALUEBUFFER;
    }

    *value = buffer;
    CFRelease(stringRef);

    return GETSTRINGPROPERTYRESULT_OK;
}

API_EXPORT DEVICE_GETDEVICEINFORESULT GetDeviceInformation(
    EndpointInfoBase* deviceInfo,
    Configuration* configuration,
    const char** id,
    const char** name,
    const char** manufacturer,
    const char** model,
    const char** driverVersion,
    int* errorCode)
{
    *errorCode = 0;

    if (deviceInfo->deviceId != nullptr)
    {
        *id = deviceInfo->deviceId;
        *name = deviceInfo->deviceName;
        *manufacturer = deviceInfo->deviceManufacturer;
        *model = deviceInfo->deviceModel;
        *driverVersion = deviceInfo->deviceDriverVersion;

        return DEVICE_GETDEVICEINFORESULT_OK;
    }

    MIDIEntityRef entity = 0;
    OSStatus status = MIDIEndpointGetEntity(deviceInfo->endpointRef, &entity);
    if (status != noErr || entity == 0)
    {
        *errorCode = status;
        return DEVICE_GETDEVICEINFORESULT_FAILEDGETENTITY;
    }

    MIDIDeviceRef device = 0;
    status = MIDIEntityGetDevice(entity, &device);
    if (status != noErr || device == 0)
    {
        *errorCode = status;
        return DEVICE_GETDEVICEINFORESULT_FAILEDGETDEVICE;
    }

    // ID

    int rawId = 0;
    auto getIdResult = MIDIObjectGetIntegerProperty(device, kMIDIPropertyUniqueID, &rawId);
    if (getIdResult != noErr)
    {
        *errorCode = getIdResult;
        return DEVICE_GETDEVICEINFORESULT_FAILEDGETID;
    }

    char* buffer = new char[16];
    snprintf(buffer, 16, "%d", rawId);
    *id = buffer;
    deviceInfo->deviceId = *id;

    // Name

    auto getNameResult = GetStringPropertyValue(device, kMIDIPropertyDisplayName, name, errorCode);
    if (getNameResult != GETSTRINGPROPERTYRESULT_OK)
    {
        getNameResult = GetStringPropertyValue(device, kMIDIPropertyName, name, errorCode);
        if (getNameResult != GETSTRINGPROPERTYRESULT_OK)
        {
            *errorCode = getNameResult;

            switch (getNameResult)
            {
                case GETSTRINGPROPERTYRESULT_PROPERTYUNAVAILABLE: return DEVICE_GETDEVICEINFORESULT_NAME_UNAVAILABLE;
                case GETSTRINGPROPERTYRESULT_FAILEDGETVALUE: return DEVICE_GETDEVICEINFORESULT_NAME_FAILEDGETVALUE;
                case GETSTRINGPROPERTYRESULT_FAILEDFILLVALUEBUFFER: return DEVICE_GETDEVICEINFORESULT_NAME_FAILEDFILLVALUEBUFFER;
            }
        }
    }

    deviceInfo->deviceName = *name;

    // Manufacturer

    if (deviceInfo->deviceManufacturer == nullptr)
    {
        auto getManufacturerResult = GetStringPropertyValue(device, kMIDIPropertyManufacturer, manufacturer, errorCode);
        if (getManufacturerResult != GETSTRINGPROPERTYRESULT_OK)
        {
            char buffer[256];
            snprintf(buffer, sizeof(buffer), "Failed to get device manufacturer (%d)", getManufacturerResult);
            configuration->activityCallback(buffer);
        }
        else
            deviceInfo->deviceManufacturer = *manufacturer;
    }

    // Model

    if (deviceInfo->deviceModel == nullptr)
    {
        auto getModelResult = GetStringPropertyValue(device, kMIDIPropertyModel, model, errorCode);
        if (getModelResult != GETSTRINGPROPERTYRESULT_OK)
        {
            char buffer[256];
            snprintf(buffer, sizeof(buffer), "Failed to get device model (%d)", getModelResult);
            configuration->activityCallback(buffer);
        }
        else
            deviceInfo->deviceModel = *model;
    }

    // Driver version

    if (deviceInfo->deviceDriverVersion == nullptr)
    {
        int rawDriverVersion = 0;
        auto getDriverVersionResult = MIDIObjectGetIntegerProperty(device, kMIDIPropertyDriverVersion, &rawDriverVersion);
        if (getDriverVersionResult != noErr)
        {
            char buffer[256];
            snprintf(buffer, sizeof(buffer), "Failed to get device driver version (%d)", getDriverVersionResult);
            configuration->activityCallback(buffer);
        }
        else
        {
            char* buffer = new char[16];
            snprintf(buffer, 16, "%d", rawDriverVersion);
            *driverVersion = buffer;
            deviceInfo->deviceDriverVersion = *driverVersion;
        }
    }

    //

    return DEVICE_GETDEVICEINFORESULT_OK;
}

/* ================================
   Session
 ================================ */

typedef void (*InputEndpointCallback)(void* info, SESSION_CALLBACKOPERATION operation);
typedef void (*OutputEndpointCallback)(void* info, SESSION_CALLBACKOPERATION operation);

struct SessionHandle
{
    const char* name;
    MIDIClientRef clientRef;
    CFRunLoopRef runLoopRef;
    pthread_t thread;
    std::atomic<char> clientCreated;
    std::atomic<char> sessionClosed;
    std::atomic<char> threadExited;
    OSStatus clientCreationStatus;
    InputEndpointCallback inputDeviceCallback;
    OutputEndpointCallback outputDeviceCallback;
};
 
void HandleSource(MIDIEndpointRef source, SessionHandle* sessionHandle, SESSION_CALLBACKOPERATION operation)
{
    if (sessionHandle->sessionClosed.load() == 1)
        return;

    if (sessionHandle->inputDeviceCallback == nullptr)
        return;

    InputEndpointInfo* inputDeviceInfo = new InputEndpointInfo();
    inputDeviceInfo->endpointRef = source;
    sessionHandle->inputDeviceCallback(inputDeviceInfo, operation);
}

void HandleDestination(MIDIEndpointRef destination, SessionHandle* sessionHandle, SESSION_CALLBACKOPERATION operation)
{
    if (sessionHandle->sessionClosed.load() == 1)
        return;

    if (sessionHandle->outputDeviceCallback == nullptr)
        return;

    OutputEndpointInfo* outputDeviceInfo = new OutputEndpointInfo();
    outputDeviceInfo->endpointRef = destination;
    sessionHandle->outputDeviceCallback(outputDeviceInfo, operation);
}

void HandleEntitySources(MIDIEntityRef entity, SessionHandle* sessionHandle, SESSION_CALLBACKOPERATION operation)
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

void HandleEntityDestinations(MIDIEntityRef entity, SessionHandle* sessionHandle, SESSION_CALLBACKOPERATION operation)
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

void HandleEntity(MIDIEntityRef entity, SessionHandle* sessionHandle, SESSION_CALLBACKOPERATION operation)
{
    if (sessionHandle->sessionClosed.load() == 1)
        return;

    HandleEntitySources(entity, sessionHandle, operation);
    HandleEntityDestinations(entity, sessionHandle, operation);
}

void HandleDevice(MIDIDeviceRef device, SessionHandle* sessionHandle, SESSION_CALLBACKOPERATION operation)
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
            SESSION_CALLBACKOPERATION operation = message->messageID == kMIDIMsgObjectAdded
                ? SESSION_CALLBACKOPERATION_ENDPOINTADDED
                : SESSION_CALLBACKOPERATION_ENDPOINTREMOVED;
            
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
    sessionHandle->runLoopRef = (CFRunLoopRef)CFRetain(CFRunLoopGetCurrent());
    
    CFStringRef nameRef = CFStringCreateWithCString(kCFAllocatorDefault, sessionHandle->name, kCFStringEncodingUTF8);
    if (!nameRef)
    {
        CFRelease(sessionHandle->runLoopRef);
        sessionHandle->runLoopRef = nullptr;
        sessionHandle->clientCreationStatus = kMIDIUnknownError;
        sessionHandle->clientCreated.store(1);
        return nullptr;
    }

    sessionHandle->clientCreationStatus = MIDIClientCreate(nameRef, NotifyProc, data, &sessionHandle->clientRef);
    CFRelease(nameRef);
    
    sessionHandle->clientCreated.store(1);
    
    CFRunLoopRun();

    if (sessionHandle->runLoopRef != nullptr)
    {
        CFRelease(sessionHandle->runLoopRef);
        sessionHandle->runLoopRef = nullptr;
    }

    sessionHandle->threadExited.store(1);

    return nullptr;
}

API_EXPORT SESSION_OPENRESULT OpenSession_Mac(const char* name, Configuration* configuration, InputEndpointCallback inputDeviceCallback, OutputEndpointCallback outputDeviceCallback, SessionHandle** handle, int* errorCode)
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
        delete sessionHandle;
        return SESSION_OPENRESULT_THREADSTARTERROR;
    }

    auto startTime = std::chrono::steady_clock::now();
    const auto timeoutMs = 5000;

    while (sessionHandle->clientCreated.load() == 0)
    {
        std::this_thread::sleep_for(std::chrono::milliseconds(1));

        auto elapsed = std::chrono::duration_cast<std::chrono::milliseconds>(
            std::chrono::steady_clock::now() - startTime
        ).count();

        if (elapsed >= timeoutMs)
        {
            pthread_cancel(sessionHandle->thread);
            return SESSION_OPENRESULT_CLIENTCREATIONTIMEOUT;
        }
    }

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

API_EXPORT SESSION_CLOSERESULT CloseSession(SessionHandle* sessionHandle)
{
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

struct InputEndpointHandle
{
    InputEndpointInfo* info;
    MIDIPortRef portRef;
};

IN_GETINFORESULT GetInputEndpointInfo(int deviceIndex, InputEndpointInfo** info, int* errorCode)
{
    *errorCode = 0;

    InputEndpointInfo* inputDeviceInfo = new InputEndpointInfo();

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

API_EXPORT IN_GETALLINFORESULT GetInputEndpointsInfo(Configuration* configuration, SessionHandle* sessionHandle, InputEndpointInfo*** devicesInfo, int* devicesCount, int* errorCode)
{
    *errorCode = 0;
    *devicesCount = static_cast<int>(MIDIGetNumberOfSources());

    InputEndpointInfo** result = new InputEndpointInfo*[*devicesCount];

    for (int i = 0; i < *devicesCount; i++)
    {
        InputEndpointInfo* inputDeviceInfo;

        auto getInputEndpointInfoResult = GetInputEndpointInfo(i, &inputDeviceInfo, errorCode);
        if (getInputEndpointInfoResult != IN_GETINFORESULT_OK)
        {
            for (int j = 0; j < i; j++)
            {
                DeleteInputEndpointInfo(result[j]);
            }

            delete[] result;

            return IN_GETALLINFORESULT_UNKNOWNERRORONGETINFO;
        }

        result[i] = inputDeviceInfo;
    }

    *devicesInfo = result;

    return IN_GETALLINFORESULT_OK;
}

API_EXPORT void FreeInputEndpointsInfo(InputEndpointInfo** devicesInfo, int devicesCount)
{
    delete[] devicesInfo;
}

API_EXPORT IN_GETCOUNTRESULT GetInputEndpointsCount(int* count)
{
    *count = static_cast<int>(MIDIGetNumberOfSources());
    return IN_GETCOUNTRESULT_OK;
}

API_EXPORT IN_GETPROPERTYRESULT GetInputEndpointName(InputEndpointInfo* info, const char** value, int* errorCode)
{
    auto result = GetStringPropertyValue(info->endpointRef, kMIDIPropertyDisplayName, value, errorCode);
    if (result != GETSTRINGPROPERTYRESULT_OK)
        result = GetStringPropertyValue(info->endpointRef, kMIDIPropertyName, value, errorCode);

    switch (result)
    {
        case GETSTRINGPROPERTYRESULT_PROPERTYUNAVAILABLE: return IN_GETPROPERTYRESULT_PROPERTYUNAVAILABLE;
        case GETSTRINGPROPERTYRESULT_FAILEDGETVALUE: return IN_GETPROPERTYRESULT_FAILEDGETVALUE;
        case GETSTRINGPROPERTYRESULT_FAILEDFILLVALUEBUFFER: return IN_GETPROPERTYRESULT_FAILEDFILLVALUEBUFFER;
    }

    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_GETPROPERTYRESULT GetInputEndpointId_Mac(InputEndpointInfo* info, int* value, int* errorCode)
{
    *errorCode = 0;

    OSStatus status = MIDIObjectGetIntegerProperty(info->endpointRef, kMIDIPropertyUniqueID, value);
    if (status != noErr)
    {
        *errorCode = status;
        return IN_GETPROPERTYRESULT_FAILEDGETVALUE;
    }

    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_OPENRESULT OpenInputEndpoint_Mac(InputEndpointInfo* info, void* sessionHandle, MIDIReadProc callback, void** handle, int* errorCode)
{
    *errorCode = 0;

    SessionHandle* pSessionHandle = static_cast<SessionHandle*>(sessionHandle);

    InputEndpointHandle* inputDeviceHandle = new InputEndpointHandle();
    inputDeviceHandle->info = info;

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

API_EXPORT IN_CLOSERESULT CloseInputEndpoint(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputEndpointHandle* inputDeviceHandle = static_cast<InputEndpointHandle*>(handle);
    MIDIPortDispose(inputDeviceHandle->portRef);

    delete inputDeviceHandle;

    return IN_CLOSERESULT_OK;
}

API_EXPORT IN_CONNECTRESULT ConnectToInputEndpoint(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputEndpointHandle* inputDeviceHandle = static_cast<InputEndpointHandle*>(handle);

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

API_EXPORT IN_DISCONNECTRESULT DisconnectFromInputEndpoint(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputEndpointHandle* inputDeviceHandle = static_cast<InputEndpointHandle*>(handle);

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

API_EXPORT IN_GETEVENTDATARESULT GetEventDataFromInputEndpoint(MIDIPacketList* packetList, int packetIndex, Byte** data, int* length, int* packetsCount)
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

/* ================================
   Output device
 ================================ */

struct OutputEndpointHandle
{
    OutputEndpointInfo* info;
    MIDIPortRef portRef;
};

OUT_GETINFORESULT GetOutputEndpointInfo(int deviceIndex, OutputEndpointInfo** info, int* errorCode)
{
    *errorCode = 0;

    OutputEndpointInfo* outputDeviceInfo = new OutputEndpointInfo();

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

API_EXPORT OUT_GETALLINFORESULT GetOutputEndpointsInfo(Configuration* configuration, SessionHandle* sessionHandle, OutputEndpointInfo*** devicesInfo, int* devicesCount, int* errorCode)
{
    *errorCode = 0;
    *devicesCount = static_cast<int>(MIDIGetNumberOfDestinations());

    OutputEndpointInfo** result = new OutputEndpointInfo*[*devicesCount];

    for (int i = 0; i < *devicesCount; i++)
    {
        OutputEndpointInfo* outputDeviceInfo;

        auto getOutputEndpointInfoResult = GetOutputEndpointInfo(i, &outputDeviceInfo, errorCode);
        if (getOutputEndpointInfoResult != OUT_GETINFORESULT_OK)
        {
            for (int j = 0; j < i; j++)
            {
                DeleteOutputEndpointInfo(result[j]);
            }

            delete[] result;

            return OUT_GETALLINFORESULT_UNKNOWNERRORONGETINFO;
        }

        result[i] = outputDeviceInfo;
    }

    *devicesInfo = result;

    return OUT_GETALLINFORESULT_OK;
}

API_EXPORT void FreeOutputEndpointsInfo(OutputEndpointInfo** devicesInfo, int devicesCount)
{
    delete[] devicesInfo;
}

API_EXPORT OUT_GETCOUNTRESULT GetOutputEndpointsCount(int* count)
{
    *count = static_cast<int>(MIDIGetNumberOfDestinations());
    return OUT_GETCOUNTRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputEndpointName(OutputEndpointInfo* info, const char** value, int* errorCode)
{
    auto result = GetStringPropertyValue(info->endpointRef, kMIDIPropertyDisplayName, value, errorCode);
    if (result != GETSTRINGPROPERTYRESULT_OK)
        result = GetStringPropertyValue(info->endpointRef, kMIDIPropertyName, value, errorCode);

    switch (result)
    {
        case GETSTRINGPROPERTYRESULT_PROPERTYUNAVAILABLE: return OUT_GETPROPERTYRESULT_PROPERTYUNAVAILABLE;
        case GETSTRINGPROPERTYRESULT_FAILEDGETVALUE: return OUT_GETPROPERTYRESULT_FAILEDGETVALUE;
        case GETSTRINGPROPERTYRESULT_FAILEDFILLVALUEBUFFER: return OUT_GETPROPERTYRESULT_FAILEDFILLVALUEBUFFER;
    }

    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT GetOutputEndpointId_Mac(OutputEndpointInfo* info, int* value, int* errorCode)
{
    *errorCode = 0;

    OSStatus status = MIDIObjectGetIntegerProperty(info->endpointRef, kMIDIPropertyUniqueID, value);
    if (status != noErr)
    {
        *errorCode = status;
        return OUT_GETPROPERTYRESULT_FAILEDGETVALUE;
    }

    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_OPENRESULT OpenOutputEndpoint_Mac(OutputEndpointInfo* info, void* sessionHandle, void** handle, int* errorCode)
{
    *errorCode = 0;

    SessionHandle* pSessionHandle = static_cast<SessionHandle*>(sessionHandle);

    OutputEndpointHandle* outputDeviceHandle = new OutputEndpointHandle();
    outputDeviceHandle->info = info;

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

API_EXPORT OUT_CLOSERESULT CloseOutputEndpoint(void* handle, int* errorCode)
{
    *errorCode = 0;

    OutputEndpointHandle* outputDeviceHandle = static_cast<OutputEndpointHandle*>(handle);
    MIDIPortDispose(outputDeviceHandle->portRef);

    // delete outputDeviceHandle->info;
    delete outputDeviceHandle;

    return OUT_CLOSERESULT_OK;
}

API_EXPORT OUT_SENDSHORTRESULT SendShortEventToOutputEndpoint(void* handle, int message, int* errorCode)
{
    *errorCode = 0;

    OutputEndpointHandle* outputDeviceHandle = static_cast<OutputEndpointHandle*>(handle);

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

API_EXPORT OUT_SENDSYSEXRESULT SendSysExEventToOutputEndpoint_Mac(void* handle, Byte* data, ByteCount dataSize, int* errorCode)
{
    *errorCode = 0;

    OutputEndpointHandle* outputDeviceHandle = static_cast<OutputEndpointHandle*>(handle);

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

/* ================================
 Virtual device
 ================================ */

struct VirtualDeviceInfo
{
    InputEndpointInfo* inputDeviceInfo = nullptr;
    OutputEndpointInfo* outputDeviceInfo = nullptr;
    const char* name;
    bool isMuted = false;
};

API_EXPORT VIRTUAL_OPENRESULT OpenVirtualDevice_Mac(
    const char* name,
    Configuration* configuration,
    SessionHandle* sessionHandle,
    MIDIReadProc callback,
    VirtualDeviceInfo** info,
    int* errorCode)
{
    *errorCode = 0;

    VirtualDeviceInfo* virtualDeviceInfo = new VirtualDeviceInfo();
    virtualDeviceInfo->name = name;
    
    CFStringRef nameRef = CFStringCreateWithCString(nullptr, name, kCFStringEncodingUTF8);
    if (!nameRef)
    {
        delete virtualDeviceInfo;
        return VIRTUAL_OPENRESULT_CREATESOURCE_FAILEDPROCESSNAME;
    }
    
    MIDIEndpointRef sourceRef;
    OSStatus status = MIDISourceCreate(sessionHandle->clientRef, nameRef, &sourceRef);
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
    
    InputEndpointInfo* inputDeviceInfo = new InputEndpointInfo();
    inputDeviceInfo->endpointRef = sourceRef;
    virtualDeviceInfo->inputDeviceInfo = inputDeviceInfo;
    
    CFStringRef nameRef2 = CFStringCreateWithCString(nullptr, name, kCFStringEncodingUTF8);
    if (!nameRef2)
    {
        MIDIEndpointDispose(sourceRef);
        delete inputDeviceInfo;
        delete virtualDeviceInfo;

        return VIRTUAL_OPENRESULT_CREATEDESTINATION_FAILEDPROCESSNAME;
    }
    
    MIDIEndpointRef destinationRef;
    status = MIDIDestinationCreate(sessionHandle->clientRef, nameRef2, callback, virtualDeviceInfo, &destinationRef);
    CFRelease(nameRef2);
    
    if (status != noErr)
    {
        MIDIEndpointDispose(sourceRef);
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
    
    OutputEndpointInfo* outputDeviceInfo = new OutputEndpointInfo();
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

        delete info;

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

        delete info;

        switch (status)
        {
            case kMIDIUnknownEndpoint: return VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_UNKNOWNENDPOINT;
            case kMIDINotPermitted: return VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_NOTPERMITTED;
        }
        
        return VIRTUAL_CLOSERESULT_DISPOSEDESTINATION_UNKNOWNERROR;
    }
    
    delete info;
    
    return VIRTUAL_CLOSERESULT_OK;
}

API_EXPORT VIRTUAL_SENDBACKRESULT SendDataBackFromVirtualDevice(const MIDIPacketList *pktlist, void *readProcRefCon, int* errorCode)
{
    *errorCode = 0;

    VirtualDeviceInfo* virtualDeviceInfo = static_cast<VirtualDeviceInfo*>(readProcRefCon);
    if (virtualDeviceInfo->isMuted)
        return VIRTUAL_SENDBACKRESULT_OK;
    
    OSStatus status = MIDIReceived(virtualDeviceInfo->inputDeviceInfo->endpointRef, pktlist);
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

API_EXPORT InputEndpointInfo* GetInputEndpointInfoFromVirtualDevice(VirtualDeviceInfo* info)
{
    return info->inputDeviceInfo;
}

API_EXPORT OutputEndpointInfo* GetOutputEndpointInfoFromVirtualDevice(VirtualDeviceInfo* info)
{
    return info->outputDeviceInfo;
}

API_EXPORT VIRTUAL_MUTERESULT MuteVirtualDevice(
    VirtualDeviceInfo* info,
    Configuration* configuration)
{
    info->isMuted = true;

    return VIRTUAL_MUTERESULT_OK;
}

API_EXPORT VIRTUAL_UNMUTERESULT UnmuteVirtualDevice(
    VirtualDeviceInfo* info,
    Configuration* configuration)
{
    info->isMuted = false;

    return VIRTUAL_UNMUTERESULT_OK;
}