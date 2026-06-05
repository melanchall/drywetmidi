#pragma comment(lib, "winmm.lib")

#ifndef NOMINMAX
#define NOMINMAX
#endif

#ifndef DRV_QUERYDEVICEINTERFACESIZE
#define DRV_QUERYDEVICEINTERFACESIZE 0x80d
#endif

#ifndef DRV_QUERYDEVICEINTERFACE
#define DRV_QUERYDEVICEINTERFACE 0x80C
#endif

#include <windows.h>
#include <mmsystem.h>
#include <mmreg.h>

#include <algorithm>
#include <new>
#include <vector>
#include <unordered_map>
#include <mutex>
#include <atomic>

#include <string>
#include <wil/com.h>
#include <wil/registry.h>
#include <wil/result.h>

#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt/Windows.Devices.Enumeration.h>
#include <winrt/Microsoft.Windows.Devices.Midi2.h>
namespace midi2 = winrt::Microsoft::Windows::Devices::Midi2;
#include <winrt/Microsoft.Windows.Devices.Midi2.Endpoints.BasicLoopback.h>
namespace basicLoopback = winrt::Microsoft::Windows::Devices::Midi2::Endpoints::BasicLoopback;

#include "winmidi/init/Microsoft.Windows.Devices.Midi2.Initialization.hpp"
namespace init = Microsoft::Windows::Devices::Midi2::Initialization;

#include "winmidi/init/WindowsMidiServicesVersion.h"

#include "../Common/NativeApi-Constants.h"

#define API_EXPORT extern "C" __declspec(dllexport)
#define API_CALL __cdecl

// TODO: check all char: maybe use bool?

/* ================================
   Common
================================ */

const wchar_t* ToWide(const char* narrow)
{
    thread_local static wchar_t buffer[2048]; // Per-thread buffer

    if (!narrow || *narrow == '\0')
        return L"";

    // Try UTF-8 first
    int result = MultiByteToWideChar(CP_UTF8, MB_ERR_INVALID_CHARS,
        narrow, -1, buffer, 2048);

    if (result == 0) // UTF-8 failed, use ANSI (locale-aware)
        result = MultiByteToWideChar(CP_ACP, 0, narrow, -1, buffer, 2048);

    if (result == 0)
        return L"";

    return buffer;
}

const wchar_t* ToWide(const char* narrow, const wchar_t* label)
{
    thread_local static wchar_t buffer[2048];

    const wchar_t* converted = ToWide(narrow);
    swprintf_s(buffer, 2048, L"%s: %s", label, converted);

    return buffer;
}

const wchar_t* FormatError(const winrt::hresult_error& e, const wchar_t* label)
{
    thread_local static wchar_t buffer[2048];

    swprintf_s(
        buffer,
        2048,
        L"%s: %s (HRESULT: 0x%08X)",
        label,
        e.message().c_str(),
        static_cast<unsigned int>(e.code()));

    return buffer;
}

const wchar_t* FormatError(const std::exception& e, const wchar_t* label)
{
    return ToWide(e.what(), label);
}

typedef int WMSSERVICECHECKRESULT;

#define WMSSERVICECHECKRESULT_OK 0
#define WMSSERVICECHECKRESULT_ERROR_OPENSCMANAGER 1
#define WMSSERVICECHECKRESULT_ERROR_OPENSERVICE 2
#define WMSSERVICECHECKRESULT_ERROR_QUERYSERVICECONFIG_1 3
#define WMSSERVICECHECKRESULT_ERROR_ALLOCSERVICECONFIG 4
#define WMSSERVICECHECKRESULT_ERROR_QUERYSERVICECONFIG_2 5
#define WMSSERVICECHECKRESULT_ERROR_SERVICEDISABLED 6

API_EXPORT API_TYPE API_CALL GetApiType()
{
    return API_TYPE_WIN;
}

struct __declspec(uuid("2BA15E4E-5417-4A66-85B8-2B2260EFBC84")) MidiSrvTransportPlaceholder : ::IUnknown
{};

struct __declspec(uuid("c3263827-c3b0-bdbd-2500-ce63a3f3f2c3")) MidiClientInitializer : ::IUnknown
{};

char CheckWmsAvailability_Registry()
{
    std::wstring keyPath = L"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Drivers32";

    for (int i = 0; i < 10; i++)
    {
        std::wstring valueName = (i == 0) ? L"midi" : L"midi" + std::to_wstring(i);

        auto val = wil::reg::try_get_value_string(HKEY_LOCAL_MACHINE, keyPath.c_str(), valueName.c_str());

        if (val.has_value() && val.value() == L"wdmaud2.drv")
        {
            return true;
        }
    }

    return false;
}

char CheckWmsAvailability_Com()
{
    wil::com_ptr_nothrow<IUnknown> servicePointer;

    HRESULT hr = CoCreateInstance(
        __uuidof(MidiSrvTransportPlaceholder),
        nullptr,
        CLSCTX_INPROC_SERVER,
        IID_PPV_ARGS(&servicePointer)
    );

    return SUCCEEDED(hr);
}

WMSSERVICECHECKRESULT CheckWmsAvailability_Service()
{
    SC_HANDLE hSCManager = OpenSCManagerW(nullptr, nullptr, SC_MANAGER_CONNECT);
    if (!hSCManager)
        return WMSSERVICECHECKRESULT_ERROR_OPENSCMANAGER;

    auto closeSCM = wil::scope_exit([&] { CloseServiceHandle(hSCManager); });

    SC_HANDLE hService = OpenServiceW(hSCManager, L"midisrv", SERVICE_QUERY_STATUS | SERVICE_QUERY_CONFIG);
    if (!hService)
        return WMSSERVICECHECKRESULT_ERROR_OPENSERVICE;

    auto closeSvc = wil::scope_exit([&] { CloseServiceHandle(hService); });

    DWORD bytesNeeded = 0;
    BOOL ok = QueryServiceConfigW(hService, nullptr, 0, &bytesNeeded);
    if (!ok && (GetLastError() != ERROR_INSUFFICIENT_BUFFER || bytesNeeded == 0))
        return WMSSERVICECHECKRESULT_ERROR_QUERYSERVICECONFIG_1;

    LPQUERY_SERVICE_CONFIGW config = static_cast<LPQUERY_SERVICE_CONFIGW>(LocalAlloc(LPTR, bytesNeeded));
    if (!config)
        return WMSSERVICECHECKRESULT_ERROR_ALLOCSERVICECONFIG;

    auto freeConfig = wil::scope_exit([&] { LocalFree(config); });

    if (!QueryServiceConfigW(hService, config, bytesNeeded, &bytesNeeded))
        return WMSSERVICECHECKRESULT_ERROR_QUERYSERVICECONFIG_2;

    if (config->dwStartType == SERVICE_DISABLED)
        return WMSSERVICECHECKRESULT_ERROR_SERVICEDISABLED;

    return WMSSERVICECHECKRESULT_OK;
}

char CheckWmsAvailability_Sdk()
{
    wil::com_ptr_nothrow<IUnknown> initPointer;

    HRESULT hr = CoCreateInstance(
        __uuidof(MidiClientInitializer),
        nullptr,
        CLSCTX_INPROC_SERVER,
        IID_PPV_ARGS(&initPointer)
    );

    return SUCCEEDED(hr);
}

API_EXPORT void API_CALL GetNativeEnvironmentInfo_Win(
    char* comInitializationResult,
    char* registryCheckResult,
    char* comCheckResult,
    WMSSERVICECHECKRESULT* serviceCheckResult,
    char* sdkCheckResult)
{
    HRESULT hrInit = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    
    *comInitializationResult = SUCCEEDED(hrInit);
    if (!*comInitializationResult)
        return;
    
    *registryCheckResult = CheckWmsAvailability_Registry();
    *comCheckResult = CheckWmsAvailability_Com();
    *serviceCheckResult = CheckWmsAvailability_Service();
    *sdkCheckResult = CheckWmsAvailability_Sdk();

    CoUninitialize();
}

/* ================================
   Configuration
================================ */

typedef void (*NativeApiActivityCallback)(const wchar_t* record);

struct Configuration
{
    NativeApiActivityCallback activityCallback;
    bool useWms{ 0 };
    bool wmsAvailable{0};
    bool basicLoopbackAvailable{0};
    std::shared_ptr<init::MidiDesktopAppSdkInitializer> wmsSdkInitializer{nullptr};
    bool wmsSdkInitialized{0};
};

API_EXPORT CONFIGURATION_GETRESULT API_CALL GetConfiguration_Win(
    bool useWms,
    NativeApiActivityCallback activityCallback,
    Configuration** configuration,
    int* errorCode)
{
    *errorCode = 0;

    Configuration* config = new Configuration();

    config->activityCallback = activityCallback;
    config->useWms = useWms;

    if (useWms)
    {
        try
        {
            char comInitializationResult;
            char registryCheckResult;
            char comCheckResult;
            WMSSERVICECHECKRESULT serviceCheckResult;
            char sdkCheckResult;

            GetNativeEnvironmentInfo_Win(
                &comInitializationResult,
                &registryCheckResult,
                &comCheckResult,
                &serviceCheckResult,
                &sdkCheckResult);

            config->wmsAvailable = static_cast<char>(
                comInitializationResult &&
                registryCheckResult &&
                comCheckResult &&
                (serviceCheckResult == WMSSERVICECHECKRESULT_OK) &&
                sdkCheckResult);

            if (config->wmsAvailable)
            {
                winrt::init_apartment();

                std::shared_ptr<init::MidiDesktopAppSdkInitializer> wmsSdkInitializer = std::make_shared<init::MidiDesktopAppSdkInitializer>();

                if (wmsSdkInitializer == nullptr)
                    return CONFIGURATION_GETRESULT_CANTCREATEWMSSDKINITIALIZER;

                if (!wmsSdkInitializer->InitializeSdkRuntime())
                    return CONFIGURATION_GETRESULT_CANTINITIALIZEWMSSDK;

                if (!wmsSdkInitializer->CheckForMinimumRequiredSdkVersion(
                    WINDOWS_MIDI_SERVICES_NUGET_BUILD_VERSION_MAJOR,
                    WINDOWS_MIDI_SERVICES_NUGET_BUILD_VERSION_MINOR,
                    WINDOWS_MIDI_SERVICES_NUGET_BUILD_VERSION_PATCH))
                    return SESSION_OPENRESULT_OLDWMSSDK;

                if (!wmsSdkInitializer->EnsureServiceAvailable())
                    return CONFIGURATION_GETRESULT_WMSSERVICEUNAVAILABLE;

                config->wmsSdkInitializer = wmsSdkInitializer;
                config->basicLoopbackAvailable = basicLoopback::MidiBasicLoopbackEndpointManager::IsTransportAvailable();

                config->wmsSdkInitialized = true;
            }
        }
        catch (const winrt::hresult_error& e)
        {
            config->activityCallback(FormatError(e, L"Initialize WMS SDK"));
            return CONFIGURATION_GETRESULT_WMSUNKNOWNERROR;
        }
        catch (const std::exception& e)
        {
            config->activityCallback(FormatError(e, L"Initialize WMS SDK"));
            return CONFIGURATION_GETRESULT_WMSUNKNOWNERROR;
        }
        catch (...)
        {
            config->activityCallback(L"Failed to initialize WMS SDK");
            return CONFIGURATION_GETRESULT_WMSUNKNOWNERROR;
        }
    }

    *configuration = config;
    return CONFIGURATION_GETRESULT_OK;
}

API_EXPORT CONFIGURATION_CLEANUPRESULT API_CALL CleanupConfiguration(Configuration* configuration)
{
    try
    {
        if (configuration->wmsAvailable)
        {
            if (configuration->wmsSdkInitializer != nullptr && configuration->wmsSdkInitialized)
            {
                configuration->wmsSdkInitializer->ShutdownSdkRuntime();
                configuration->wmsSdkInitializer.reset();
            }

            winrt::uninit_apartment();
        }
    }
    catch (...)
    {
        return CONFIGURATION_CLEANUPRESULT_WMSUNKNOWNERROR;
    }

    delete configuration;

    return CONFIGURATION_CLEANUPRESULT_OK;
}

API_EXPORT bool API_CALL IsVirtualDeviceApiAvailable(Configuration* configuration)
{
    return configuration->useWms && configuration->wmsAvailable && configuration->wmsSdkInitialized && configuration->basicLoopbackAvailable;
}

API_EXPORT bool API_CALL IsDevicesCachingRequired(Configuration* configuration)
{
    return configuration->useWms && configuration->wmsAvailable && configuration->wmsSdkInitialized;
}

API_EXPORT bool API_CALL IsDevicesWatcherApiAvailable(Configuration* configuration)
{
    return configuration->useWms && configuration->wmsAvailable && configuration->wmsSdkInitialized;
}

API_EXPORT void API_CALL CheckNativeApiActivityCallback(Configuration* configuration)
{
    configuration->activityCallback(L"Native API activity callback works!");
}

API_EXPORT void API_CALL CheckWinRtErrorHandling_Win(Configuration* configuration)
{
    try
    {
        throw winrt::hresult_error(E_FAIL, L"Simulated failure");
    }
    catch (const winrt::hresult_error& e)
    {
        configuration->activityCallback(FormatError(e, L"WinRT error handling"));
    }
}

API_EXPORT void API_CALL CheckStdExceptionHandling_Win(Configuration* configuration)
{
    try
    {
        throw std::runtime_error("Simulated failure");
    }
    catch (const std::exception& e)
    {
        configuration->activityCallback(FormatError(e, L"Standard exception handling"));
    }
}

/* ================================
   High-precision tick generator
================================ */

typedef struct
{
    char dummy;
} TickGeneratorSessionHandle;

typedef struct
{
    UINT timerResolution;
    UINT timerId;
} TickGeneratorInfo;

API_EXPORT TGSESSION_OPENRESULT API_CALL OpenTickGeneratorSession(void** handle, int* errorCode)
{
    *errorCode = 0;

    TickGeneratorSessionHandle* sessionHandle = new TickGeneratorSessionHandle();

    *handle = sessionHandle;

    return TGSESSION_OPENRESULT_OK;
}

API_EXPORT TGSESSION_CLOSERESULT API_CALL CloseTickGeneratorSession(TickGeneratorSessionHandle* sessionHandle, int* errorCode)
{
    *errorCode = 0;

    delete sessionHandle;

    return TGSESSION_CLOSERESULT_OK;
}

API_EXPORT TG_STARTRESULT API_CALL StartHighPrecisionTickGenerator_Win(int interval, TickGeneratorSessionHandle* sessionHandle, LPTIMECALLBACK callback, TickGeneratorInfo** info, int* errorCode)
{
    *errorCode = 0;

    TIMECAPS tc;
    MMRESULT result = timeGetDevCaps(&tc, sizeof(TIMECAPS));
    if (result != TIMERR_NOERROR)
    {
        *errorCode = result;
        return TG_STARTRESULT_CANTGETDEVICECAPABILITIES;
    }

    UINT wTimerRes = std::min(std::max(tc.wPeriodMin, (UINT)interval), tc.wPeriodMax);

    timeBeginPeriod(wTimerRes);
    result = timeSetEvent(interval, wTimerRes, callback, 0, TIME_PERIODIC);
    if (result == 0)
    {
        *errorCode = result;
        return TG_STARTRESULT_CANTSETTIMERCALLBACK;
    }

    TickGeneratorInfo* tickGeneratorInfo = new TickGeneratorInfo();
    tickGeneratorInfo->timerResolution = wTimerRes;
    tickGeneratorInfo->timerId = result;
    *info = tickGeneratorInfo;

    return TG_STARTRESULT_OK;
}

API_EXPORT TG_STOPRESULT API_CALL StopHighPrecisionTickGenerator(TickGeneratorSessionHandle* sessionHandle, TickGeneratorInfo* info, int* errorCode)
{
    *errorCode = 0;

    MMRESULT result = timeEndPeriod(info->timerResolution);
    if (result != TIMERR_NOERROR)
    {
        *errorCode = result;
        return TG_STOPRESULT_CANTENDPERIOD;
    }

    result = timeKillEvent(info->timerId);
    if (result != TIMERR_NOERROR)
    {
        *errorCode = result;
        return TG_STOPRESULT_CANTKILLEVENT;
    }

    delete info;

    return TG_STOPRESULT_OK;
}

/* ================================
   Devices common
================================ */

void EnsureWinMmPortsAvailable()
{
    midiInGetNumDevs();
    midiOutGetNumDevs();
}

struct EndpointInfoBase
{
    std::wstring endpointDeviceId;

    std::wstring parentDeviceId;
    std::wstring parentDeviceName;
    std::wstring parentManufacturer;
    std::wstring parentModel;
};

struct InputEndpointInfo : EndpointInfoBase
{
    int deviceIndex;
    LPMIDIINCAPSW caps;

    std::wstring portDeviceId;
    std::wstring devicePath;
};

API_EXPORT void API_CALL CloneInputEndpointInfo(InputEndpointInfo* source, InputEndpointInfo** info)
{
    InputEndpointInfo* result = new InputEndpointInfo();

    result->deviceIndex = source->deviceIndex;
    result->caps = new MIDIINCAPSW(*source->caps);
    result->portDeviceId = source->portDeviceId;
    result->devicePath = source->devicePath;
    result->endpointDeviceId = source->endpointDeviceId;
    result->parentDeviceId = source->parentDeviceId;
    result->parentDeviceName = source->parentDeviceName;
    result->parentManufacturer = source->parentManufacturer;
    result->parentModel = source->parentModel;

    *info = result;
}

API_EXPORT int API_CALL GetInputEndpointHashCode(InputEndpointInfo* info)
{
    if (info == nullptr)
        return 0;

    if (!info->endpointDeviceId.empty() && !info->portDeviceId.empty())
        return std::hash<std::wstring>()(info->endpointDeviceId) ^ std::hash<std::wstring>()(info->portDeviceId);

    if (!info->devicePath.empty())
        return std::hash<std::wstring>()(info->devicePath) ^ std::hash<std::wstring>()(info->caps->szPname);

    return std::hash<std::wstring>()(info->caps->szPname);
}

API_EXPORT char API_CALL AreInputEndpointsEqual(InputEndpointInfo* info1, InputEndpointInfo* info2)
{
    if (info1 == nullptr || info2 == nullptr)
        return 0;

    if (!info1->endpointDeviceId.empty() && !info1->portDeviceId.empty() && !info2->endpointDeviceId.empty() && !info2->portDeviceId.empty())
        return info1->endpointDeviceId == info2->endpointDeviceId && info1->portDeviceId == info2->portDeviceId;
    
    if (!info1->devicePath.empty() && !info2->devicePath.empty())
        return info1->devicePath == info2->devicePath && wcscmp(info1->caps->szPname, info2->caps->szPname) == 0;

    return wcscmp(info1->caps->szPname, info2->caps->szPname) == 0;
}

API_EXPORT IN_GETCOUNTRESULT API_CALL GetInputEndpointsCount(int* count)
{
    *count = midiInGetNumDevs();
    return IN_GETCOUNTRESULT_OK;
}

IN_GETINFORESULT GetInputEndpointInfo(int deviceIndex, InputEndpointInfo** info, int* errorCode)
{
    *errorCode = 0;

    InputEndpointInfo* inputDeviceInfo = new InputEndpointInfo();

    inputDeviceInfo->deviceIndex = deviceIndex;
    inputDeviceInfo->caps = new MIDIINCAPSW();

    MMRESULT result = midiInGetDevCapsW(deviceIndex, inputDeviceInfo->caps, sizeof(MIDIINCAPSW));
    if (result != MMSYSERR_NOERROR)
    {
        delete inputDeviceInfo->caps;
        delete inputDeviceInfo;

        *errorCode = result;

        switch (result)
        {
            case MMSYSERR_BADDEVICEID: return IN_GETINFORESULT_BADDEVICEID;
            case MMSYSERR_INVALPARAM: return IN_GETINFORESULT_INVALIDSTRUCTURE;
            case MMSYSERR_NODRIVER: return IN_GETINFORESULT_NODRIVER;
            case MMSYSERR_NOMEM: return IN_GETINFORESULT_NOMEMORY;
        }

        return IN_GETINFORESULT_UNKNOWNERROR;
    }

    ULONG size = 0;
    result = midiInMessage((HMIDIIN)(uintptr_t)deviceIndex, DRV_QUERYDEVICEINTERFACESIZE, (DWORD_PTR)&size, 0);
    if (result == MMSYSERR_NOERROR && size > 0)
    {
        std::vector<wchar_t> buffer(size / sizeof(wchar_t));
        result = midiInMessage((HMIDIIN)(uintptr_t)deviceIndex, DRV_QUERYDEVICEINTERFACE, (DWORD_PTR)buffer.data(), size);
        if (result == MMSYSERR_NOERROR)
            inputDeviceInfo->devicePath = std::wstring(buffer.data());
    }

    *info = inputDeviceInfo;

    return IN_GETINFORESULT_OK;
}

IN_GETINFORESULT GetInputEndpointInfo(const int deviceIndex, const winrt::hstring& endpointDeviceId, const winrt::hstring& portDeviceId, InputEndpointInfo** info, int* errorCode)
{
    *errorCode = 0;

    auto result = GetInputEndpointInfo(deviceIndex, info, errorCode);
    if (result == IN_GETINFORESULT_OK && info != nullptr)
    {
        (*info)->endpointDeviceId = endpointDeviceId.c_str();
        (*info)->portDeviceId = portDeviceId.c_str();
    }

    return result;
}

API_EXPORT void API_CALL DeleteInputEndpointInfo(InputEndpointInfo* info)
{
    delete info->caps;
    delete info;
}

struct OutputEndpointInfo : EndpointInfoBase
{
    int deviceIndex;
    LPMIDIOUTCAPSW caps;

    std::wstring portDeviceId;

    std::wstring devicePath;

    char isMicrosoftGsWavetableSynth = 0;
};

API_EXPORT void API_CALL CloneOutputEndpointInfo(OutputEndpointInfo* source, OutputEndpointInfo** info)
{
    OutputEndpointInfo* result = new OutputEndpointInfo();

    result->deviceIndex = source->deviceIndex;
    result->caps = new MIDIOUTCAPSW(*source->caps);
    result->portDeviceId = source->portDeviceId;
    result->devicePath = source->devicePath;
    result->isMicrosoftGsWavetableSynth = source->isMicrosoftGsWavetableSynth;
    result->endpointDeviceId = source->endpointDeviceId;
    result->parentDeviceId = source->parentDeviceId;
    result->parentDeviceName = source->parentDeviceName;
    result->parentManufacturer = source->parentManufacturer;
    result->parentModel = source->parentModel;

    *info = result;
}

API_EXPORT int API_CALL GetOutputEndpointHashCode(OutputEndpointInfo* info)
{
    if (info == nullptr)
        return 0;

    if (!info->endpointDeviceId.empty() && !info->portDeviceId.empty())
        return std::hash<std::wstring>()(info->endpointDeviceId) ^ std::hash<std::wstring>()(info->portDeviceId);

    if (!info->devicePath.empty())
        return std::hash<std::wstring>()(info->devicePath) ^ std::hash<std::wstring>()(info->caps->szPname);

    return std::hash<std::wstring>()(info->caps->szPname);
}

API_EXPORT char API_CALL AreOutputEndpointsEqual(OutputEndpointInfo* info1, OutputEndpointInfo* info2)
{
    if (info1 == nullptr || info2 == nullptr)
        return 0;

    if (info1->isMicrosoftGsWavetableSynth && info2->isMicrosoftGsWavetableSynth)
        return 1;

    if (!info1->endpointDeviceId.empty() && !info1->portDeviceId.empty() && !info2->endpointDeviceId.empty() && !info2->portDeviceId.empty())
        return info1->endpointDeviceId == info2->endpointDeviceId && info1->portDeviceId == info2->portDeviceId;

    if (!info1->devicePath.empty() && !info2->devicePath.empty())
        return info1->devicePath == info2->devicePath && wcscmp(info1->caps->szPname, info2->caps->szPname) == 0;

    return wcscmp(info1->caps->szPname, info2->caps->szPname) == 0;
}

API_EXPORT OUT_GETCOUNTRESULT API_CALL GetOutputEndpointsCount(int* count)
{
    *count = midiOutGetNumDevs();
    return OUT_GETCOUNTRESULT_OK;
}

OUT_GETINFORESULT GetOutputEndpointInfo(int deviceIndex, OutputEndpointInfo** info, int* errorCode)
{
    *errorCode = 0;

    OutputEndpointInfo* outputDeviceInfo = new OutputEndpointInfo();

    outputDeviceInfo->deviceIndex = deviceIndex;
    outputDeviceInfo->caps = new MIDIOUTCAPSW();

    MMRESULT result = midiOutGetDevCapsW(deviceIndex, outputDeviceInfo->caps, sizeof(MIDIOUTCAPSW));
    if (result != MMSYSERR_NOERROR)
    {
        delete outputDeviceInfo->caps;
        delete outputDeviceInfo;

        *errorCode = result;

        switch (result)
        {
            case MMSYSERR_BADDEVICEID: return OUT_GETINFORESULT_BADDEVICEID;
            case MMSYSERR_INVALPARAM: return OUT_GETINFORESULT_INVALIDSTRUCTURE;
            case MMSYSERR_NODRIVER: return OUT_GETINFORESULT_NODRIVER;
            case MMSYSERR_NOMEM: return OUT_GETINFORESULT_NOMEMORY;
        }

        return OUT_GETINFORESULT_UNKNOWNERROR;
    }

    ULONG size = 0;
    result = midiOutMessage((HMIDIOUT)(uintptr_t)deviceIndex, DRV_QUERYDEVICEINTERFACESIZE, (DWORD_PTR)&size, 0);
    if (result == MMSYSERR_NOERROR && size > 0)
    {
        std::vector<wchar_t> buffer(size / sizeof(wchar_t));
        result = midiOutMessage((HMIDIOUT)(uintptr_t)deviceIndex, DRV_QUERYDEVICEINTERFACE, (DWORD_PTR)buffer.data(), size);
        if (result == MMSYSERR_NOERROR)
            outputDeviceInfo->devicePath = std::wstring(buffer.data());
    }

    *info = outputDeviceInfo;

    return OUT_GETINFORESULT_OK;
}

OUT_GETINFORESULT GetOutputEndpointInfo(const int deviceIndex, const winrt::hstring& endpointDeviceId, const winrt::hstring& portDeviceId, OutputEndpointInfo** info, int* errorCode)
{
    *errorCode = 0;

    auto result = GetOutputEndpointInfo(deviceIndex, info, errorCode);
    if (result == OUT_GETINFORESULT_OK && info != nullptr)
    {
        (*info)->endpointDeviceId = endpointDeviceId.c_str();
        (*info)->portDeviceId = portDeviceId.c_str();
    }

    return result;
}

API_EXPORT void API_CALL DeleteOutputEndpointInfo(OutputEndpointInfo* info)
{
    delete info->caps;
    delete info;
}

int FindPortIndex(const std::wstring& endpointDeviceId, const std::wstring& portDeviceId, const midi2::Midi1PortFlow& flow)
{
    if (endpointDeviceId.empty() || portDeviceId.empty())
        return -1;

    const winrt::hstring endpointDeviceIdH{ endpointDeviceId };

    auto endpointInformation = midi2::MidiEndpointDeviceInformation::CreateFromEndpointDeviceId(endpointDeviceIdH);
    if (endpointInformation == nullptr)
        return -1;

    auto ports = endpointInformation.FindAllAssociatedMidi1PortsForThisEndpoint(flow);
    for (auto const& port : ports)
    {
        if (port.PortDeviceId().c_str() == portDeviceId)
            return port.PortNumber();
    }

    return -1;
}

// TODO: WMS API
API_EXPORT const wchar_t* API_CALL GetDeviceManufacturer(WORD manufacturerId)
{
    // https://docs.microsoft.com/en-us/windows/win32/multimedia/manufacturer-identifiers
    switch (manufacturerId)
    {
        case MM_GRAVIS: return L"Advanced Gravis Computer Technology, Ltd.";
        case MM_ANTEX: return L"Antex Electronics Corporation";
        case MM_APPS: return L"APPS Software";
        case MM_ARTISOFT: return L"Artisoft, Inc.";
        case MM_AST: return L"AST Research, Inc.";
        case MM_ATI: return L"ATI Technologies, Inc.";
        case MM_AUDIOFILE: return L"Audio, Inc.";
        case MM_APT: return L"Audio Processing Technology";
        case MM_AUDIOPT: return L"Audio Processing Technology";
        case MM_AURAVISION: return L"Auravision Corporation";
        case MM_AZTECH: return L"Aztech Labs, Inc.";
        case MM_CANOPUS: return L"Canopus, Co., Ltd.";
        case MM_COMPUSIC: return L"Compusic";
        case MM_CAT: return L"Computer Aided Technology, Inc.";
        case MM_COMPUTER_FRIENDS: return L"Computer Friends, Inc.";
        case MM_CONTROLRES: return L"Control Resources Corporation";
        case MM_CREATIVE: return L"Creative Labs, Inc.";
        case MM_DIALOGIC: return L"Dialogic Corporation";
        case MM_DOLBY: return L"Dolby Laboratories, Inc.";
        case MM_DSP_GROUP: return L"DSP Group, Inc.";
        case MM_DSP_SOLUTIONS: return L"DSP Solutions, Inc.";
        case MM_ECHO: return L"Echo Speech Corporation";
        case MM_ESS: return L"ESS Technology, Inc.";
        case MM_EVEREX: return L"Everex Systems, Inc.";
        case MM_EXAN: return L"EXAN, Ltd.";
        case MM_FUJITSU: return L"Fujitsu, Ltd.";
        case MM_IOMAGIC: return L"I/O Magic Corporation";
        case MM_ICL_PS: return L"ICL Personal Systems";
        case MM_OLIVETTI: return L"Ing. C. Olivetti & C., S.p.A.";
        case MM_ICS: return L"Integrated Circuit Systems, Inc.";
        case MM_INTEL: return L"Intel Corporation";
        case MM_INTERACTIVE: return L"InterActive, Inc.";
        case MM_IBM: return L"International Business Machines";
        case MM_ITERATEDSYS: return L"Iterated Systems, Inc.";
        case MM_LOGITECH: return L"Logitech, Inc.";
        case MM_LYRRUS: return L"Lyrrus, Inc.";
        case MM_MATSUSHITA: return L"Matsushita Electric Corporation of America";
        case MM_MEDIAVISION: return L"Media Vision, Inc.";
        case MM_METHEUS: return L"Metheus Corporation";
        case MM_MELABS: return L"microEngineering Labs";
        case MM_MICROSOFT: return L"Microsoft Corporation";
        case MM_MOSCOM: return L"MOSCOM Corporation";
        case MM_MOTOROLA: return L"Motorola, Inc.";
        case MM_NMS: return L"Natural MicroSystems Corporation";
        case MM_NCR: return L"NCR Corporation";
        case MM_NEC: return L"NEC Corporation";
        case MM_NEWMEDIA: return L"New Media Corporation";
        case MM_OKI: return L"OKI";
        case MM_OPTI: return L"OPTi, Inc.";
        case MM_ROLAND: return L"Roland Corporation";
        case MM_SCALACS: return L"SCALACS";
        case MM_EPSON: return L"Seiko Epson Corporation, Inc.";
        case MM_SIERRA: return L"Sierra Semiconductor Corporation";
        case MM_SILICONSOFT: return L"Silicon Software, Inc.";
        case MM_SONICFOUNDRY: return L"Sonic Foundry";
        case MM_SPEECHCOMP: return L"Speech Compression";
        case MM_SUPERMAC: return L"Supermac Technology, Inc.";
        case MM_TANDY: return L"Tandy Corporation";
        case MM_KORG: return L"Toshihiko Okuhura, Korg, Inc.";
        case MM_TRUEVISION: return L"Truevision, Inc.";
        case MM_TURTLE_BEACH: return L"Turtle Beach Systems";
        case MM_VAL: return L"Video Associates Labs, Inc.";
        case MM_VIDEOLOGIC: return L"VideoLogic, Inc.";
        case MM_VITEC: return L"Visual Information Technologies, Inc.";
        case MM_VOCALTEC: return L"VocalTec, Inc.";
        case MM_VOYETRA: return L"Voyetra Technologies";
        case MM_WANGLABS: return L"Wang Laboratories";
        case MM_WILLOWPOND: return L"Willow Pond Corporation";
        case MM_WINNOV: return L"Winnov, LP";
        case MM_XEBEC: return L"Xebec Multimedia Solutions Limited";
        case MM_YAMAHA: return L"Yamaha Corporation of America";
    }

    return L"Unknown";
}

// TODO: WMS API
API_EXPORT const wchar_t* API_CALL GetDeviceProduct(WORD productId)
{
    // https://docs.microsoft.com/en-us/windows/win32/multimedia/microsoft-corporation-product-identifiers
    switch (productId)
    {
        case MM_ADLIB: return L"Adlib-compatible synthesizer";
        case MM_MSFT_ACM_G711: return L"G.711 codec";
        case MM_MSFT_ACM_GSM610: return L"GSM 610 codec";
        case MM_MSFT_ACM_IMAADPCM: return L"IMA ADPCM codec";
        case MM_PC_JOYSTICK: return L"Joystick adapter";
        case MM_MIDI_MAPPER: return L"MIDI mapper";
        case MM_MPU401_MIDIIN: return L"MPU 401-compatible MIDI input port";
        case MM_MPU401_MIDIOUT: return L"MPU 401-compatible MIDI output port";
        case MM_MSFT_ACM_MSADPCM: return L"MS ADPCM codec";
        case MM_MSFT_WSS_FMSYNTH_STEREO: return L"MS audio board stereo FM synthesizer";
        case MM_MSFT_WSS_AUX: return L"MS audio board aux port";
        case MM_MSFT_WSS_MIXER: return L"MS audio board mixer driver";
        case MM_MSFT_WSS_WAVEIN: return L"MS audio board waveform input";
        case MM_MSFT_WSS_WAVEOUT: return L"MS audio board waveform output";
        case MM_MSFT_MSACM: return L"MS audio compression manager";
        case MM_MSFT_ACM_MSFILTER: return L"MS filter";
        case MM_MSFT_WSS_OEM_AUX: return L"MS OEM audio aux port";
        case MM_MSFT_WSS_OEM_MIXER: return L"MS OEM audio board mixer driver";
        case MM_MSFT_WSS_OEM_FMSYNTH_STEREO: return L"MS OEM audio board stereo FM synthesizer";
        case MM_MSFT_WSS_OEM_WAVEIN: return L"MS OEM audio board waveform input";
        case MM_MSFT_WSS_OEM_WAVEOUT: return L"MS OEM audio board waveform output";
        case MM_MSFT_GENERIC_AUX_CD: return L"MS vanilla driver aux (CD)";
        case MM_MSFT_GENERIC_AUX_LINE: return L"MS vanilla driver aux (line in)";
        case MM_MSFT_GENERIC_AUX_MIC: return L"MS vanilla driver aux (mic)";
        case MM_MSFT_GENERIC_MIDIOUT: return L"MS vanilla driver MIDI external out";
        case MM_MSFT_GENERIC_MIDIIN: return L"MS vanilla driver MIDI in";
        case MM_MSFT_GENERIC_MIDISYNTH: return L"MS vanilla driver MIDI synthesizer";
        case MM_MSFT_GENERIC_WAVEIN: return L"MS vanilla driver waveform input";
        case MM_MSFT_GENERIC_WAVEOUT: return L"MS vanilla driver wavefrom output";
        case MM_PCSPEAKER_WAVEOUT: return L"PC speaker waveform output";
        case MM_MSFT_ACM_PCM: return L"PCM converter";
        case MM_SNDBLST_SYNTH: return L"Sound Blaster internal synthesizer";
        case MM_SNDBLST_MIDIIN: return L"Sound Blaster MIDI input port";
        case MM_SNDBLST_MIDIOUT: return L"Sound Blaster MIDI output port";
        case MM_SNDBLST_WAVEIN: return L"Sound Blaster waveform input";
        case MM_SNDBLST_WAVEOUT: return L"Sound Blaster waveform output";
        case MM_WAVE_MAPPER: return L"Wave mapper";
    }

    // add from https://docs.microsoft.com/en-us/windows/win32/multimedia/product-identifiers
    return L"Unknown";
}

API_EXPORT DEVCOMMON_GETPARENTDEVICEINFORESULT API_CALL GetParentDeviceInfo_Win(
    EndpointInfoBase* deviceInfo,
    Configuration* configuration,
    const wchar_t** id,
    const wchar_t** name,
    const wchar_t** manufacturer,
    const wchar_t** model,
    int* errorCode)
{
    *errorCode = 0;

    if (deviceInfo == nullptr || deviceInfo->endpointDeviceId.empty())
        return DEVCOMMON_GETPARENTDEVICEINFORESULT_NOINFO;

    if (!deviceInfo->parentDeviceId.empty())
    {
        *id = deviceInfo->parentDeviceId.c_str();
        *name = deviceInfo->parentDeviceName.c_str();
        *manufacturer = deviceInfo->parentManufacturer.c_str();
        *model = deviceInfo->parentModel.c_str();

        return DEVCOMMON_GETPARENTDEVICEINFORESULT_OK;
    }

    winrt::Windows::Devices::Enumeration::DeviceInformation parentInformation = nullptr;

    try
    {
        auto endpointInformation = midi2::MidiEndpointDeviceInformation::CreateFromEndpointDeviceId(winrt::hstring{ deviceInfo->endpointDeviceId });
        if (endpointInformation == nullptr)
            return DEVCOMMON_GETPARENTDEVICEINFORESULT_FAILEDTOGETINFO;

        parentInformation = endpointInformation.GetParentDeviceInformation();
        if (parentInformation == nullptr)
            return DEVCOMMON_GETPARENTDEVICEINFORESULT_NOINFO;

        deviceInfo->parentDeviceId = parentInformation.Id().c_str();
        deviceInfo->parentDeviceName = parentInformation.Name().c_str();

        // TODO: dangerous to return pointers to internal strings - need to copy them instead?
        *id = deviceInfo->parentDeviceId.c_str();
        *name = deviceInfo->parentDeviceName.c_str();
    }
    catch (const winrt::hresult_error& e)
    {
        configuration->activityCallback(FormatError(e, L"Get basic parent device properties"));
        return DEVCOMMON_GETPARENTDEVICEINFORESULT_UNKNOWNWMSERROR;
    }
    catch (const std::exception& e)
    {
        configuration->activityCallback(FormatError(e, L"Get basic parent device properties"));
        return DEVCOMMON_GETPARENTDEVICEINFORESULT_UNKNOWNWMSERROR;
    }
    catch (...)
    {
        configuration->activityCallback(L"Failed to get basic parent device properties");
        return DEVCOMMON_GETPARENTDEVICEINFORESULT_UNKNOWNWMSERROR;
    }

    if (parentInformation != nullptr)
    {
        try
        {
            auto manufacturerPropertyName = L"System.Devices.DeviceManufacturer";
            auto modelPropertyName = L"System.Devices.ModelName";

            auto properties = parentInformation.Properties();

            auto manufacturerH = properties.HasKey(manufacturerPropertyName)
                ? winrt::unbox_value_or<winrt::hstring>(properties.Lookup(manufacturerPropertyName), L"")
                : L"";

            auto modelH = properties.HasKey(modelPropertyName)
                ? winrt::unbox_value_or<winrt::hstring>(properties.Lookup(modelPropertyName), L"")
                : L"";

            deviceInfo->parentManufacturer = manufacturerH.c_str();
            deviceInfo->parentModel = modelH.c_str();
        }
        catch (const winrt::hresult_error& e)
        {
            configuration->activityCallback(FormatError(e, L"Get additional parent device properties"));
        }
        catch (const std::exception& e)
        {
            configuration->activityCallback(FormatError(e, L"Get additional parent device properties"));
        }
        catch (...)
        {
            configuration->activityCallback(L"Failed to get additional parent device properties");
        }

        *manufacturer = deviceInfo->parentManufacturer.c_str();
        *model = deviceInfo->parentModel.c_str();
    }

    return DEVCOMMON_GETPARENTDEVICEINFORESULT_OK;
}

/* ================================
   Session
================================ */

typedef void (*InputEndpointCallback)(void* info, SESSION_CALLBACKOPERATION operation);
typedef void (*OutputEndpointCallback)(void* info, SESSION_CALLBACKOPERATION operation);

struct EndpointDevicesInfo
{
    std::vector<InputEndpointInfo*> inputDevicesInfo;
    std::vector<OutputEndpointInfo*> outputDevicesInfo;
};

struct SessionHandle
{
    const wchar_t* name;

    Configuration* configuration;

    InputEndpointCallback inputEndpointCallback;
    OutputEndpointCallback outputEndpointCallback;

    midi2::MidiEndpointDeviceWatcher watcher{nullptr};
    winrt::event_token revokeOnWatcherDeviceRemoved;
    winrt::event_token revokeOnWatcherDeviceAdded;
    winrt::event_token revokeOnWatcherEnumerationCompleted;
    std::atomic<char> initialEnumerationCompleted{0};

    std::mutex endpointDevicesLock;
    std::unordered_map<std::wstring, EndpointDevicesInfo> endpointDevicesById;
};

void DeleteInputEndpointInfos(std::vector<InputEndpointInfo*>& deviceInfos)
{
    for (auto* inputDeviceInfo : deviceInfos)
    {
        DeleteInputEndpointInfo(inputDeviceInfo);
    }

    deviceInfos.clear();
}

void DeleteOutputEndpointInfos(std::vector<OutputEndpointInfo*>& deviceInfos)
{
    for (auto* outputDeviceInfo : deviceInfos)
    {
        DeleteOutputEndpointInfo(outputDeviceInfo);
    }

    deviceInfos.clear();
}

API_EXPORT SESSION_OPENRESULT API_CALL OpenSession_Win(
    const wchar_t* name,
    Configuration* configuration,
    InputEndpointCallback inputEndpointCallback,
    OutputEndpointCallback outputEndpointCallback,
    SessionHandle** handle,
    int* errorCode)
{
    *errorCode = 0;

    SessionHandle* sessionHandle = new SessionHandle();
    sessionHandle->name = name;
    sessionHandle->configuration = configuration;
    sessionHandle->inputEndpointCallback = inputEndpointCallback;
    sessionHandle->outputEndpointCallback = outputEndpointCallback;

    try
    {
        if (configuration->useWms && configuration->wmsAvailable && configuration->wmsSdkInitialized)
        {
            sessionHandle->watcher = midi2::MidiEndpointDeviceWatcher::Create(midi2::MidiEndpointDeviceInformationFilters::AllStandardEndpoints);

            auto OnWatcherDeviceAdded = [sessionHandle](midi2::MidiEndpointDeviceWatcher const&, midi2::MidiEndpointDeviceInformationAddedEventArgs const& args)
            {
                if (sessionHandle->initialEnumerationCompleted.load() == 0)
                    return;

                std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

                std::vector<InputEndpointInfo*> inputDevicesInfo;
                std::vector<OutputEndpointInfo*> outputDevicesInfo;
                EndpointDevicesInfo endpointDevicesInfo;

                auto cleanupAllDeviceInfos = [&]()
                {
                    DeleteInputEndpointInfos(inputDevicesInfo);
                    DeleteInputEndpointInfos(endpointDevicesInfo.inputDevicesInfo);
                    DeleteOutputEndpointInfos(outputDevicesInfo);
                    DeleteOutputEndpointInfos(endpointDevicesInfo.outputDevicesInfo);
                };

                try
                {
                    EnsureWinMmPortsAvailable();

                    auto endpointId = args.AddedDevice().EndpointDeviceId();
                    auto endpointInformation = midi2::MidiEndpointDeviceInformation::CreateFromEndpointDeviceId(endpointId);
                    if (endpointInformation == nullptr)
                    {
                        // TODO
                        return;
                    }
                    
                    auto groupTerminalBlocks = endpointInformation.GetGroupTerminalBlocks();

                    auto sourcesCount = 0;
                    auto destinationsCount = 0;

                    for (auto const& gtb : groupTerminalBlocks)
                    {
                        switch (gtb.Direction())
                        {
                            case midi2::MidiGroupTerminalBlockDirection::BlockInput:
                                destinationsCount++;
                                break;
                            case midi2::MidiGroupTerminalBlockDirection::BlockOutput:
                                sourcesCount++;
                                break;
                            case midi2::MidiGroupTerminalBlockDirection::Bidirectional:
                                sourcesCount++;
                                destinationsCount++;
                                break;
                        }
                    }

                    std::wstring endpointKey = endpointId.c_str();

                    auto ok = false;
                    int attempts = 0;

                    while (!ok && attempts++ < 20)
                    {
                        EnsureWinMmPortsAvailable();

                        ok = true;

                        cleanupAllDeviceInfos();

                        auto endpointInformation = midi2::MidiEndpointDeviceInformation::CreateFromEndpointDeviceId(endpointId);
                        if (endpointInformation == nullptr)
                        {
                            // TODO
                            return;
                        }

                        if (sourcesCount > 0)
                        {
                            auto inputPorts = endpointInformation.FindAllAssociatedMidi1PortsForThisEndpoint(midi2::Midi1PortFlow::MidiMessageSource);
                            ok = inputPorts.Size() >= sourcesCount;
                            if (!ok)
                                continue;

                            for (auto const& port : inputPorts)
                            {
                                InputEndpointInfo* inputDeviceInfo = nullptr;
                                int errorCode;

                                auto getInputEndpointInfoResult = GetInputEndpointInfo(port.PortNumber(), endpointId, port.PortDeviceId(), &inputDeviceInfo, &errorCode);
                                if (getInputEndpointInfoResult != IN_GETINFORESULT_OK)
                                {
                                    ok = false;
                                    break;
                                }

                                inputDevicesInfo.push_back(inputDeviceInfo);

                                InputEndpointInfo* persistentInputEndpointInfo;
                                getInputEndpointInfoResult = GetInputEndpointInfo(port.PortNumber(), endpointId, port.PortDeviceId(), &persistentInputEndpointInfo, &errorCode);
                                if (getInputEndpointInfoResult != IN_GETINFORESULT_OK)
                                {
                                    ok = false;
                                    break;
                                }
                                
                                endpointDevicesInfo.inputDevicesInfo.push_back(persistentInputEndpointInfo);
                            }
                        }

                        if (destinationsCount > 0)
                        {
                            auto outputPorts = endpointInformation.FindAllAssociatedMidi1PortsForThisEndpoint(midi2::Midi1PortFlow::MidiMessageDestination);
                            ok = outputPorts.Size() >= destinationsCount;
                            if (!ok)
                                continue;

                            for (auto const& port : outputPorts)
                            {
                                OutputEndpointInfo* outputDeviceInfo = nullptr;
                                int errorCode;

                                auto getOutputEndpointInfoResult = GetOutputEndpointInfo(port.PortNumber(), endpointId, port.PortDeviceId(), &outputDeviceInfo, &errorCode);
                                if (getOutputEndpointInfoResult != OUT_GETINFORESULT_OK)
                                {
                                    ok = false;
                                    break;
                                }

                                outputDevicesInfo.push_back(outputDeviceInfo);

                                OutputEndpointInfo* persistentOutputEndpointInfo;
                                getOutputEndpointInfoResult = GetOutputEndpointInfo(port.PortNumber(), endpointId, port.PortDeviceId(), &persistentOutputEndpointInfo, &errorCode);
                                if (getOutputEndpointInfoResult != OUT_GETINFORESULT_OK)
                                {
                                    ok = false;
                                    break;
                                }
                                
                                endpointDevicesInfo.outputDevicesInfo.push_back(persistentOutputEndpointInfo);
                            }
                        }

                        if (!ok)
                            Sleep(500);
                    }

                    if (!ok)
                    {
                        cleanupAllDeviceInfos();
                        return;
                    }

                    sessionHandle->endpointDevicesById[endpointKey] = endpointDevicesInfo;

                    for (auto* inputDeviceInfo : inputDevicesInfo)
                    {
                        sessionHandle->inputEndpointCallback(inputDeviceInfo, SESSION_CALLBACKOPERATION_ENDPOINTADDED);
                    }

                    for (auto* outputDeviceInfo : outputDevicesInfo)
                    {
                        sessionHandle->outputEndpointCallback(outputDeviceInfo, SESSION_CALLBACKOPERATION_ENDPOINTADDED);
                    }
                }
                catch (const winrt::hresult_error& e)
                {
                    sessionHandle->configuration->activityCallback(FormatError(e, L"Process endpoint added"));
                }
                catch (const std::exception& e)
                {
                    sessionHandle->configuration->activityCallback(FormatError(e, L"Process endpoint added"));
                }
                catch (...)
                {
                    sessionHandle->configuration->activityCallback(L"Failed to process endpoint added");
                }
            };

            auto OnWatcherDeviceRemoved = [sessionHandle](midi2::MidiEndpointDeviceWatcher const&, midi2::MidiEndpointDeviceInformationRemovedEventArgs const& args)
            {
                if (sessionHandle->initialEnumerationCompleted.load() == 0)
                    return;

                std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

                try
                {
                    auto endpointId = args.EndpointDeviceId();
                    std::wstring endpointKey = endpointId.c_str();

                    EndpointDevicesInfo endpointDevicesInfo;

                    auto it = sessionHandle->endpointDevicesById.find(endpointKey);
                    if (it == sessionHandle->endpointDevicesById.end())
                        return;

                    endpointDevicesInfo = it->second;
                    sessionHandle->endpointDevicesById.erase(it);

                    for (auto* inputDeviceInfo : endpointDevicesInfo.inputDevicesInfo)
                    {
                        sessionHandle->inputEndpointCallback(inputDeviceInfo, SESSION_CALLBACKOPERATION_ENDPOINTREMOVED);
                    }

                    for (auto* outputDeviceInfo : endpointDevicesInfo.outputDevicesInfo)
                    {
                        sessionHandle->outputEndpointCallback(outputDeviceInfo, SESSION_CALLBACKOPERATION_ENDPOINTREMOVED);
                    }
                }
                catch (const winrt::hresult_error& e)
                {
                    sessionHandle->configuration->activityCallback(FormatError(e, L"Process endpoint removed"));
                }
                catch (const std::exception& e)
                {
                    sessionHandle->configuration->activityCallback(FormatError(e, L"Process endpoint removed"));
                }
                catch (...)
                {
                    sessionHandle->configuration->activityCallback(L"Failed to process endpoint removed");
                }
            };

            auto OnWatcherEnumerationCompleted = [sessionHandle](midi2::MidiEndpointDeviceWatcher const&, winrt::Windows::Foundation::IInspectable const&)
            {
                sessionHandle->initialEnumerationCompleted.store(1);
            };

            sessionHandle->revokeOnWatcherEnumerationCompleted = sessionHandle->watcher.EnumerationCompleted(OnWatcherEnumerationCompleted);
            sessionHandle->revokeOnWatcherDeviceRemoved = sessionHandle->watcher.Removed(OnWatcherDeviceRemoved);
            sessionHandle->revokeOnWatcherDeviceAdded = sessionHandle->watcher.Added(OnWatcherDeviceAdded);

            sessionHandle->watcher.Start();
        }
    }
    catch (const winrt::hresult_error& e)
    {
        configuration->activityCallback(FormatError(e, L"Setup devices watcher"));
        delete sessionHandle;
        return SESSION_OPENRESULT_WMSUNKNOWNERROR;
    }
    catch (const std::exception& e)
    {
        configuration->activityCallback(FormatError(e, L"Setup devices watcher"));
        delete sessionHandle;
        return SESSION_OPENRESULT_WMSUNKNOWNERROR;
    }
    catch (...)
    {
        configuration->activityCallback(L"Failed to setup devices watcher");
        delete sessionHandle;
        return SESSION_OPENRESULT_WMSUNKNOWNERROR;
    }

    *handle = sessionHandle;
    return SESSION_OPENRESULT_OK;
}

API_EXPORT SESSION_CLOSERESULT API_CALL CloseSession(SessionHandle* sessionHandle)
{
    if (sessionHandle->watcher != nullptr)
    {
        if (sessionHandle->watcher.Status() != winrt::Windows::Devices::Enumeration::DeviceWatcherStatus::Stopped)
            sessionHandle->watcher.Stop();

        if (sessionHandle->revokeOnWatcherEnumerationCompleted)
            sessionHandle->watcher.EnumerationCompleted(sessionHandle->revokeOnWatcherEnumerationCompleted);

        if (sessionHandle->revokeOnWatcherDeviceRemoved)
            sessionHandle->watcher.Removed(sessionHandle->revokeOnWatcherDeviceRemoved);

        if (sessionHandle->revokeOnWatcherDeviceAdded)
            sessionHandle->watcher.Added(sessionHandle->revokeOnWatcherDeviceAdded);

        sessionHandle->watcher = nullptr;
    }

    {
        std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

        for (auto& pair : sessionHandle->endpointDevicesById)
        {
            DeleteInputEndpointInfos(pair.second.inputDevicesInfo);
            DeleteOutputEndpointInfos(pair.second.outputDevicesInfo);
        }

        sessionHandle->endpointDevicesById.clear();
    }

    delete sessionHandle;
    return SESSION_CLOSERESULT_OK;
}

/* ================================
   Input device
================================ */

typedef struct
{
    InputEndpointInfo* info;
    HMIDIIN handle;
    LPMIDIHDR* sysExHeaders;
    int sysExBufferCount;
    int sysExBufferSize;
    CRITICAL_SECTION lock;
    LONG isClosing;
} InputEndpointHandle;

IN_GETALLINFORESULT ConvertToGetAllInInfoResult(IN_GETINFORESULT getInfoResult)
{
    switch (getInfoResult)
    {
        case IN_GETINFORESULT_BADDEVICEID: return IN_GETALLINFORESULT_BADDEVICEID;
        case IN_GETINFORESULT_INVALIDSTRUCTURE: return IN_GETALLINFORESULT_INVALIDSTRUCTURE;
        case IN_GETINFORESULT_NODRIVER: return IN_GETALLINFORESULT_NODRIVER;
        case IN_GETINFORESULT_NOMEMORY: return IN_GETALLINFORESULT_NOMEMORY;
        default: return IN_GETALLINFORESULT_UNKNOWNERRORONGETINFO;
    }
}

API_EXPORT IN_GETALLINFORESULT API_CALL GetInputEndpointsInfo(Configuration* configuration, SessionHandle* sessionHandle, InputEndpointInfo*** devicesInfo, int* devicesCount, int* errorCode)
{
    *errorCode = 0;

    EnsureWinMmPortsAvailable();

    if (configuration->useWms && configuration->wmsAvailable && configuration->wmsSdkInitialized)
    {
        std::vector<InputEndpointInfo*> inputDevicesInfo;

        auto cleanupInputDevices = [&inputDevicesInfo]()
        {
            for (auto& info : inputDevicesInfo)
            {
                DeleteInputEndpointInfo(info);
            }
        };

        try
        {
            auto endpoints = midi2::MidiEndpointDeviceInformation::FindAll(
                midi2::MidiEndpointDeviceInformationSortOrder::Name,
                midi2::MidiEndpointDeviceInformationFilters::AllStandardEndpoints);

            for (auto const& endpoint : endpoints)
            {
                auto ports = endpoint.FindAllAssociatedMidi1PortsForThisEndpoint(midi2::Midi1PortFlow::MidiMessageSource);

                for (auto const& port : ports)
                {
                    InputEndpointInfo* inputDeviceInfo;

                    auto getInputEndpointInfoResult = GetInputEndpointInfo(port.PortNumber(), endpoint.EndpointDeviceId(), port.PortDeviceId(), &inputDeviceInfo, errorCode);
                    if (getInputEndpointInfoResult != IN_GETINFORESULT_OK)
                    {
                        cleanupInputDevices();
                        return ConvertToGetAllInInfoResult(getInputEndpointInfoResult);
                    }

                    inputDevicesInfo.push_back(inputDeviceInfo);
                }
            }

            auto count = inputDevicesInfo.size();
            auto result = new InputEndpointInfo*[count];

            for (auto i = 0; i < count; i++)
            {
                result[i] = inputDevicesInfo[i];
            }

            *devicesInfo = result;
            *devicesCount = static_cast<int>(count);

            return IN_GETALLINFORESULT_OK;
        }
        catch (const winrt::hresult_error& e)
        {
            cleanupInputDevices();

            configuration->activityCallback(FormatError(e, L"Get all input endpoints via WMS"));
            return IN_GETALLINFORESULT_UNKNOWNWMSERROR;
        }
        catch (const std::exception& e)
        {
            cleanupInputDevices();

            configuration->activityCallback(FormatError(e, L"Get all input endpoints via WMS"));
            return IN_GETALLINFORESULT_UNKNOWNWMSERROR;
        }
        catch (...)
        {
            cleanupInputDevices();

            configuration->activityCallback(L"Failed to get all input endpoints via WMS");
            return IN_GETALLINFORESULT_UNKNOWNWMSERROR;
        }
    }

    // WinMM approach

    GetInputEndpointsCount(devicesCount);

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

            return ConvertToGetAllInInfoResult(getInputEndpointInfoResult);
        }

        result[i] = inputDeviceInfo;
    }

    *devicesInfo = result;

    return IN_GETALLINFORESULT_OK;
}

API_EXPORT void API_CALL FreeInputEndpointsInfo(InputEndpointInfo** devicesInfo, int devicesCount)
{
    delete[] devicesInfo;
}

// TODO: WMS API
API_EXPORT IN_GETPROPERTYRESULT API_CALL GetInputEndpointName(InputEndpointInfo* info, const wchar_t** value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->szPname;
    return IN_GETPROPERTYRESULT_OK;
}

// TODO: WMS API
API_EXPORT IN_GETPROPERTYRESULT API_CALL GetInputEndpointManufacturer(InputEndpointInfo* info, const wchar_t** value, int* errorCode)
{
    *errorCode = 0;

    *value = GetDeviceManufacturer(info->caps->wMid);
    return IN_GETPROPERTYRESULT_OK;
}

// TODO: WMS API
API_EXPORT IN_GETPROPERTYRESULT API_CALL GetInputEndpointProduct(InputEndpointInfo* info, const wchar_t** value, int* errorCode)
{
    *errorCode = 0;

    *value = GetDeviceProduct(info->caps->wPid);
    return IN_GETPROPERTYRESULT_OK;
}

// TODO: WMS API
API_EXPORT IN_GETPROPERTYRESULT API_CALL GetInputEndpointDriverVersion(InputEndpointInfo* info, int* value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->vDriverVersion;
    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_RENEWSYSEXBUFFERRESULT API_CALL RenewInputEndpointSysExBuffer(void* handle, void* headerPointer, int* errorCode)
{
    *errorCode = 0;

    InputEndpointHandle* inputDeviceHandle = static_cast<InputEndpointHandle*>(handle);
    LPMIDIHDR header = static_cast<LPMIDIHDR>(headerPointer);

    if (header == nullptr)
        return IN_RENEWSYSEXBUFFERRESULT_INVALIDHEADER;

    EnterCriticalSection(&inputDeviceHandle->lock);

    if (inputDeviceHandle->isClosing)
    {
        LeaveCriticalSection(&inputDeviceHandle->lock);
        return IN_RENEWSYSEXBUFFERRESULT_CLOSING;
    }

    bool found = false;
    for (int i = 0; i < inputDeviceHandle->sysExBufferCount; i++)
    {
        if (inputDeviceHandle->sysExHeaders[i] == header)
        {
            found = true;
            break;
        }
    }

    if (!found)
    {
        LeaveCriticalSection(&inputDeviceHandle->lock);
        return IN_RENEWSYSEXBUFFERRESULT_INVALIDHEADER;
    }

    if ((header->dwFlags & MHDR_DONE) == 0)
    {
        LeaveCriticalSection(&inputDeviceHandle->lock);
        return IN_RENEWSYSEXBUFFERRESULT_BUFFERNOTDONE;
    }

    header->dwFlags &= MHDR_PREPARED;
    header->dwBytesRecorded = 0;

    const int maxRetries = 5;
    const int retryDelayMs = 10;
    MMRESULT result;

    for (int retry = 0; retry < maxRetries; retry++)
    {
        if (inputDeviceHandle->isClosing)
        {
            LeaveCriticalSection(&inputDeviceHandle->lock);
            return IN_RENEWSYSEXBUFFERRESULT_CLOSING;
        }

        result = midiInAddBuffer(inputDeviceHandle->handle, header, sizeof(MIDIHDR));

        if (result == MMSYSERR_NOERROR)
        {
            LeaveCriticalSection(&inputDeviceHandle->lock);
            return IN_RENEWSYSEXBUFFERRESULT_OK;
        }

        if (result != MIDIERR_STILLPLAYING)
            break;

        Sleep(retryDelayMs);
    }

    LeaveCriticalSection(&inputDeviceHandle->lock);

    *errorCode = result;

    switch (result)
    {
        case MIDIERR_STILLPLAYING: return IN_RENEWSYSEXBUFFERRESULT_STILLPLAYING;
        case MIDIERR_UNPREPARED: return IN_RENEWSYSEXBUFFERRESULT_UNPREPARED;
        case MMSYSERR_INVALHANDLE: return IN_RENEWSYSEXBUFFERRESULT_INVALIDHANDLE;
        case MMSYSERR_INVALPARAM: return IN_RENEWSYSEXBUFFERRESULT_INVALIDSTRUCTURE;
        case MMSYSERR_NOMEM: return IN_RENEWSYSEXBUFFERRESULT_NOMEMORY;
    }

    return IN_RENEWSYSEXBUFFERRESULT_UNKNOWNERROR;
}

IN_PREPARESYSEXBUFFERRESULT PrepareSysExBuffer(HMIDIIN deviceHandle, int bufferSize, LPMIDIHDR* outHeader, int* errorCode)
{
    *errorCode = 0;
    *outHeader = nullptr;

    LPMIDIHDR header = new MIDIHDR();
    LPSTR buffer = new char[bufferSize];

    header->lpData = buffer;
    header->dwBufferLength = bufferSize;
    header->dwFlags = 0;
    header->dwBytesRecorded = 0;

    MMRESULT result = midiInPrepareHeader(deviceHandle, header, sizeof(MIDIHDR));
    if (result != MMSYSERR_NOERROR)
    {
        *errorCode = result;

        delete[] buffer;
        delete header;
        return IN_PREPARESYSEXBUFFERRESULT_PREPAREFAILED;
    }

    result = midiInAddBuffer(deviceHandle, header, sizeof(MIDIHDR));
    if (result != MMSYSERR_NOERROR)
    {
        *errorCode = result;

        midiInUnprepareHeader(deviceHandle, header, sizeof(MIDIHDR));
        delete[] buffer;
        delete header;
        return IN_PREPARESYSEXBUFFERRESULT_ADDBUFFERFAILED;
    }

    *outHeader = header;
    return IN_PREPARESYSEXBUFFERRESULT_OK;
}

API_EXPORT IN_OPENRESULT API_CALL OpenInputEndpoint_Win(InputEndpointInfo* info, SessionHandle* sessionHandle, DWORD_PTR callback, int sysExBufferSize, int sysExBufferCount, void** handle, int* errorCode)
{
    *errorCode = 0;

    EnsureWinMmPortsAvailable();

    InputEndpointInfo* inputDeviceInfo = info;
    InputEndpointHandle* inputDeviceHandle = new InputEndpointHandle();
    inputDeviceHandle->info = inputDeviceInfo;
    inputDeviceHandle->sysExBufferSize = sysExBufferSize;
    inputDeviceHandle->sysExBufferCount = sysExBufferCount;

    inputDeviceHandle->sysExHeaders = new LPMIDIHDR[sysExBufferCount];
    for (int i = 0; i < sysExBufferCount; i++)
    {
        inputDeviceHandle->sysExHeaders[i] = nullptr;
    }

    InitializeCriticalSection(&inputDeviceHandle->lock);
    inputDeviceHandle->isClosing = 0;

    auto deviceIndex = FindPortIndex(inputDeviceInfo->endpointDeviceId, inputDeviceInfo->portDeviceId, midi2::Midi1PortFlow::MidiMessageSource);
    if (deviceIndex < 0)
        deviceIndex = inputDeviceInfo->deviceIndex;

    MMRESULT result = midiInOpen(&inputDeviceHandle->handle, deviceIndex, callback, 0, CALLBACK_FUNCTION);
    if (result != MMSYSERR_NOERROR)
    {
        delete[] inputDeviceHandle->sysExHeaders;
        DeleteCriticalSection(&inputDeviceHandle->lock);
        delete inputDeviceHandle;

        *errorCode = result;

        switch (result)
        {
            case MMSYSERR_ALLOCATED: return IN_OPENRESULT_ALLOCATED;
            case MMSYSERR_BADDEVICEID: return IN_OPENRESULT_BADDEVICEID;
            case MMSYSERR_INVALFLAG: return IN_OPENRESULT_INVALIDFLAG;
            case MMSYSERR_INVALPARAM: return IN_OPENRESULT_INVALIDSTRUCTURE;
            case MMSYSERR_NOMEM: return IN_OPENRESULT_NOMEMORY;
        }

        return IN_OPENRESULT_UNKNOWNERROR;
    }

    auto preparedBuffersCount = 0;

    for (int i = 0; i < sysExBufferCount; i++)
    {
        int prepareErrorCode;
        IN_PREPARESYSEXBUFFERRESULT prepareResult = PrepareSysExBuffer(inputDeviceHandle->handle, sysExBufferSize, &inputDeviceHandle->sysExHeaders[i], &prepareErrorCode);

        if (prepareResult != IN_PREPARESYSEXBUFFERRESULT_OK)
        {
            auto errorMessage = L"Failed to prepare SysEx buffer " +
                std::to_wstring(i) +
                L" for input endpoint (" +
                std::to_wstring(prepareResult) +
                L", " +
                std::to_wstring(prepareErrorCode) +
                L")";
            sessionHandle->configuration->activityCallback(errorMessage.c_str());
            inputDeviceHandle->sysExHeaders[i] = nullptr;
            continue;
        }

        preparedBuffersCount++;
    }

    if (preparedBuffersCount == 0)
    {
        midiInClose(inputDeviceHandle->handle);
        delete[] inputDeviceHandle->sysExHeaders;
        DeleteCriticalSection(&inputDeviceHandle->lock);
        delete inputDeviceHandle;
        return IN_OPENRESULT_FAILEDPREPARESYSEXBUFFERS;
    }

    *handle = inputDeviceHandle;

    return IN_OPENRESULT_OK;
}

API_EXPORT IN_CLOSERESULT API_CALL CloseInputEndpoint(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputEndpointHandle* inputDeviceHandle = static_cast<InputEndpointHandle*>(handle);

    EnterCriticalSection(&inputDeviceHandle->lock);
    inputDeviceHandle->isClosing = 1;

    auto cleanupSysExHeaders = [&inputDeviceHandle]()
    {
        for (int i = 0; i < inputDeviceHandle->sysExBufferCount; i++)
        {
            if (inputDeviceHandle->sysExHeaders[i] == nullptr)
                continue;

            LPMIDIHDR header = inputDeviceHandle->sysExHeaders[i];
            midiInUnprepareHeader(inputDeviceHandle->handle, header, sizeof(MIDIHDR));

            delete[] header->lpData;
            delete header;

            inputDeviceHandle->sysExHeaders[i] = nullptr;
        }

        delete[] inputDeviceHandle->sysExHeaders;
    };

    MMRESULT result = midiInReset(inputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        LeaveCriticalSection(&inputDeviceHandle->lock);
        DeleteCriticalSection(&inputDeviceHandle->lock);

        *errorCode = result;

        cleanupSysExHeaders();
        midiInClose(inputDeviceHandle->handle);
        delete inputDeviceHandle;

        switch (result)
        {
            case MMSYSERR_INVALHANDLE: return IN_CLOSERESULT_RESET_INVALIDHANDLE;
        }

        return IN_CLOSERESULT_RESET_UNKNOWNERROR;
    }

    cleanupSysExHeaders();

    result = midiInClose(inputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        LeaveCriticalSection(&inputDeviceHandle->lock);
        DeleteCriticalSection(&inputDeviceHandle->lock);

        *errorCode = result;

        delete inputDeviceHandle;

        switch (result)
        {
            case MIDIERR_STILLPLAYING: return IN_CLOSERESULT_CLOSE_STILLPLAYING;
            case MMSYSERR_INVALHANDLE: return IN_CLOSERESULT_CLOSE_INVALIDHANDLE;
            case MMSYSERR_NOMEM: return IN_CLOSERESULT_CLOSE_NOMEMORY;
        }

        return IN_CLOSERESULT_CLOSE_UNKNOWNERROR;
    }

    LeaveCriticalSection(&inputDeviceHandle->lock);
    DeleteCriticalSection(&inputDeviceHandle->lock);

    delete inputDeviceHandle;

    return IN_CLOSERESULT_OK;
}

API_EXPORT IN_CONNECTRESULT API_CALL ConnectToInputEndpoint(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputEndpointHandle* inputDeviceHandle = static_cast<InputEndpointHandle*>(handle);

    MMRESULT result = midiInStart(inputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        *errorCode = result;

        switch (result)
        {
            case MMSYSERR_INVALHANDLE: return IN_CONNECTRESULT_INVALIDHANDLE;
        }

        return IN_CONNECTRESULT_UNKNOWNERROR;
    }

    return IN_CONNECTRESULT_OK;
}

API_EXPORT IN_DISCONNECTRESULT API_CALL DisconnectFromInputEndpoint(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputEndpointHandle* inputDeviceHandle = static_cast<InputEndpointHandle*>(handle);

    MMRESULT result = midiInStop(inputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        *errorCode = result;

        switch (result)
        {
            case MMSYSERR_INVALHANDLE: return IN_DISCONNECTRESULT_INVALIDHANDLE;
        }

        return IN_DISCONNECTRESULT_UNKNOWNERROR;
    }

    return IN_DISCONNECTRESULT_OK;
}

API_EXPORT IN_GETSYSEXDATARESULT API_CALL GetInputEndpointSysExBufferData(LPMIDIHDR header, LPSTR* data, int* size)
{
    *data = header->lpData;
    *size = header->dwBytesRecorded;

    return IN_GETSYSEXDATARESULT_OK;
}

// TODO: WMS API
API_EXPORT char API_CALL IsInputEndpointPropertySupported(IN_PROPERTY property)
{
    switch (property)
    {
        case IN_PROPERTY_PRODUCT:
        case IN_PROPERTY_MANUFACTURER:
        case IN_PROPERTY_DRIVERVERSION:
            return 1;
    }

    return 0;
}

/* ================================
   Output device
================================ */

typedef struct
{
    OutputEndpointInfo* info;
    HMIDIOUT handle;
} OutputEndpointHandle;

OUT_GETALLINFORESULT ConvertToGetAllOutInfoResult(OUT_GETINFORESULT getInfoResult)
{
    switch (getInfoResult)
    {
        case OUT_GETINFORESULT_BADDEVICEID: return OUT_GETALLINFORESULT_BADDEVICEID;
        case OUT_GETINFORESULT_INVALIDSTRUCTURE: return OUT_GETALLINFORESULT_INVALIDSTRUCTURE;
        case OUT_GETINFORESULT_NODRIVER: return OUT_GETALLINFORESULT_NODRIVER;
        case OUT_GETINFORESULT_NOMEMORY: return OUT_GETALLINFORESULT_NOMEMORY;
        default: return OUT_GETALLINFORESULT_UNKNOWNERRORONGETINFO;
    }
}

API_EXPORT OUT_GETALLINFORESULT API_CALL GetOutputEndpointsInfo(Configuration* configuration, SessionHandle* sessionHandle, OutputEndpointInfo*** devicesInfo, int* devicesCount, int* errorCode)
{
    *errorCode = 0;

    EnsureWinMmPortsAvailable();

    if (configuration->useWms && configuration->wmsAvailable && configuration->wmsSdkInitialized)
    {
        std::vector<OutputEndpointInfo*> outputDevicesInfo;

        auto cleanupOutputDevices = [&outputDevicesInfo]()
        {
            for (auto& info : outputDevicesInfo)
            {
                DeleteOutputEndpointInfo(info);
            }
        };

        int initialCount = 0;
        GetOutputEndpointsCount(&initialCount);

        for (int i = 0; i < initialCount; i++)
        {
            OutputEndpointInfo* outputDeviceInfo;

            auto getOutputEndpointInfoResult = GetOutputEndpointInfo(i, &outputDeviceInfo, errorCode);
            if (getOutputEndpointInfoResult != OUT_GETINFORESULT_OK)
            {
                cleanupOutputDevices();
                return ConvertToGetAllOutInfoResult(getOutputEndpointInfoResult);
            }
            
            if (wcscmp(outputDeviceInfo->caps->szPname, L"Microsoft GS Wavetable Synth") == 0)
            {
                outputDeviceInfo->isMicrosoftGsWavetableSynth = 1;
                outputDevicesInfo.push_back(outputDeviceInfo);
                break;
            }

            DeleteOutputEndpointInfo(outputDeviceInfo);
        }

        try
        {
            auto endpoints = midi2::MidiEndpointDeviceInformation::FindAll(
                midi2::MidiEndpointDeviceInformationSortOrder::Name,
                midi2::MidiEndpointDeviceInformationFilters::AllStandardEndpoints);

            for (auto const& endpoint : endpoints)
            {
                auto ports = endpoint.FindAllAssociatedMidi1PortsForThisEndpoint(midi2::Midi1PortFlow::MidiMessageDestination);

                for (auto const& port : ports)
                {
                    OutputEndpointInfo* outputDeviceInfo;

                    auto getOutputEndpointInfoResult = GetOutputEndpointInfo(port.PortNumber(), endpoint.EndpointDeviceId(), port.PortDeviceId(), &outputDeviceInfo, errorCode);
                    if (getOutputEndpointInfoResult != OUT_GETINFORESULT_OK)
                    {
                        cleanupOutputDevices();
                        return ConvertToGetAllOutInfoResult(getOutputEndpointInfoResult);
                    }

                    outputDevicesInfo.push_back(outputDeviceInfo);
                }
            }

            auto count = outputDevicesInfo.size();
            OutputEndpointInfo** result = new OutputEndpointInfo * [count];

            for (size_t i = 0; i < count; i++)
            {
                result[i] = outputDevicesInfo[i];
            }

            *devicesInfo = result;
            *devicesCount = static_cast<int>(count);

            return OUT_GETALLINFORESULT_OK;
        }
        catch (const winrt::hresult_error& e)
        {
            cleanupOutputDevices();

            configuration->activityCallback(FormatError(e, L"Get all output endpoints via WMS"));
            return OUT_GETALLINFORESULT_UNKNOWNWMSERROR;
        }
        catch (const std::exception& e)
        {
            cleanupOutputDevices();

            configuration->activityCallback(FormatError(e, L"Get all output endpoints via WMS"));
            return OUT_GETALLINFORESULT_UNKNOWNWMSERROR;
        }
        catch (...)
        {
            cleanupOutputDevices();

            configuration->activityCallback(L"Failed to get all output endpoints via WMS");
            return OUT_GETALLINFORESULT_UNKNOWNWMSERROR;
        }
    }

    // WinMM approach

    GetOutputEndpointsCount(devicesCount);

    OutputEndpointInfo** result = new OutputEndpointInfo * [*devicesCount];

    for (int i = 0; i < *devicesCount; i++)
    {
        OutputEndpointInfo* outputDeviceInfo;
        int errorCode;

        auto getOutputEndpointInfoResult = GetOutputEndpointInfo(i, &outputDeviceInfo, &errorCode);
        if (getOutputEndpointInfoResult != OUT_GETINFORESULT_OK)
        {
            for (int j = 0; j < i; j++)
            {
                DeleteOutputEndpointInfo(result[j]);
            }

            delete[] result;

            return ConvertToGetAllOutInfoResult(getOutputEndpointInfoResult);
        }

        if (wcscmp(outputDeviceInfo->caps->szPname, L"Microsoft GS Wavetable Synth") == 0)
            outputDeviceInfo->isMicrosoftGsWavetableSynth = 1;

        result[i] = outputDeviceInfo;
    }

    *devicesInfo = result;

    return OUT_GETALLINFORESULT_OK;
}

API_EXPORT void API_CALL FreeOutputEndpointsInfo(OutputEndpointInfo** devicesInfo, int devicesCount)
{
    delete[] devicesInfo;
}

API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputEndpointName(OutputEndpointInfo* info, const wchar_t** value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->szPname;
    return OUT_GETPROPERTYRESULT_OK;
}

// TODO: WMS API
API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputEndpointManufacturer(OutputEndpointInfo* info, const wchar_t** value, int* errorCode)
{
    *errorCode = 0;

    *value = GetDeviceManufacturer(info->caps->wMid);
    return OUT_GETPROPERTYRESULT_OK;
}

// TODO: WMS API
API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputEndpointProduct(OutputEndpointInfo* info, const wchar_t** value, int* errorCode)
{
    *errorCode = 0;

    *value = GetDeviceProduct(info->caps->wPid);
    return OUT_GETPROPERTYRESULT_OK;
}

// TODO: WMS API
API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputEndpointDriverVersion(OutputEndpointInfo* info, int* value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->vDriverVersion;
    return OUT_GETPROPERTYRESULT_OK;
}

// TODO: WMS API
API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputEndpointTechnology(OutputEndpointInfo* info, OUT_TECHNOLOGY* value, int* errorCode)
{
    *errorCode = 0;

    *value = OUT_TECHNOLOGY_UNKNOWN;

    switch (info->caps->wTechnology)
    {
        case MOD_MIDIPORT:
            *value = OUT_TECHNOLOGY_MIDIPORT;
            break;
        case MOD_SYNTH:
            *value = OUT_TECHNOLOGY_SYNTH;
            break;
        case MOD_SQSYNTH:
            *value = OUT_TECHNOLOGY_SQSYNTH;
            break;
        case MOD_FMSYNTH:
            *value = OUT_TECHNOLOGY_FMSYNTH;
            break;
        case MOD_MAPPER:
            *value = OUT_TECHNOLOGY_MAPPER;
            break;
        case MOD_WAVETABLE:
            *value = OUT_TECHNOLOGY_WAVETABLE;
            break;
        case MOD_SWSYNTH:
            *value = OUT_TECHNOLOGY_SWSYNTH;
            break;
    }

    return OUT_GETPROPERTYRESULT_OK;
}

// TODO: WMS API
API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputEndpointVoicesNumber(OutputEndpointInfo* info, int* value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->wVoices;
    return OUT_GETPROPERTYRESULT_OK;
}

// TODO: WMS API
API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputEndpointNotesNumber(OutputEndpointInfo* info, int* value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->wNotes;
    return OUT_GETPROPERTYRESULT_OK;
}

// TODO: WMS API
API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputEndpointChannelsMask(OutputEndpointInfo* info, int* value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->wChannelMask;
    return OUT_GETPROPERTYRESULT_OK;
}

// TODO: WMS API
API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputEndpointOptions(OutputEndpointInfo* info, OUT_OPTION* value, int* errorCode)
{
    *errorCode = 0;

    int result = OUT_OPTION_UNKNOWN;

    DWORD support = info->caps->dwSupport;
    if ((support & MIDICAPS_CACHE) != 0)
        result = result | OUT_OPTION_CACHE;
    if ((support & MIDICAPS_LRVOLUME) != 0)
        result = result | OUT_OPTION_LRVOLUME;
    if ((support & MIDICAPS_STREAM) != 0)
        result = result | OUT_OPTION_STREAM;
    if ((support & MIDICAPS_VOLUME) != 0)
        result = result | OUT_OPTION_VOLUME;

    *value = result;

    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_OPENRESULT API_CALL OpenOutputEndpoint_Win(OutputEndpointInfo* info, void* sessionHandle, DWORD_PTR callback, void** handle, int* errorCode)
{
    *errorCode = 0;

    EnsureWinMmPortsAvailable();

    OutputEndpointHandle* outputDeviceHandle = new OutputEndpointHandle();
    outputDeviceHandle->info = info;

    auto deviceIndex = FindPortIndex(info->endpointDeviceId, info->portDeviceId, midi2::Midi1PortFlow::MidiMessageDestination);
    if (deviceIndex < 0)
        deviceIndex = info->deviceIndex;

    HMIDIOUT outHandle;
    MMRESULT result = midiOutOpen(&outHandle, deviceIndex, callback, 0, CALLBACK_FUNCTION);
    if (result != MMSYSERR_NOERROR)
    {
        delete outputDeviceHandle;

        *errorCode = result;

        switch (result)
        {
            case MMSYSERR_ALLOCATED: return OUT_OPENRESULT_ALLOCATED;
            case MMSYSERR_BADDEVICEID: return OUT_OPENRESULT_BADDEVICEID;
            case MMSYSERR_INVALFLAG: return OUT_OPENRESULT_INVALIDFLAG;
            case MMSYSERR_INVALPARAM: return OUT_OPENRESULT_INVALIDSTRUCTURE;
            case MMSYSERR_NOMEM: return OUT_OPENRESULT_NOMEMORY;
        }

        return OUT_OPENRESULT_UNKNOWNERROR;
    }

    outputDeviceHandle->handle = outHandle;

    *handle = outputDeviceHandle;

    return OUT_OPENRESULT_OK;
}

API_EXPORT OUT_CLOSERESULT API_CALL CloseOutputEndpoint(void* handle, int* errorCode)
{
    *errorCode = 0;

    OutputEndpointHandle* outputDeviceHandle = static_cast<OutputEndpointHandle*>(handle);

    MMRESULT result = midiOutReset(outputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        *errorCode = result;

        midiOutClose(outputDeviceHandle->handle);
        delete outputDeviceHandle;

        switch (result)
        {
            case MMSYSERR_INVALHANDLE: return OUT_CLOSERESULT_RESET_INVALIDHANDLE;
        }

        return OUT_CLOSERESULT_RESET_UNKNOWNERROR;
    }

    result = midiOutClose(outputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        *errorCode = result;

        delete outputDeviceHandle;

        switch (result)
        {
            case MIDIERR_STILLPLAYING: return OUT_CLOSERESULT_CLOSE_STILLPLAYING;
            case MMSYSERR_INVALHANDLE: return OUT_CLOSERESULT_CLOSE_INVALIDHANDLE;
            case MMSYSERR_NOMEM: return OUT_CLOSERESULT_CLOSE_NOMEMORY;
        }

        return OUT_CLOSERESULT_CLOSE_UNKNOWNERROR;
    }

    delete outputDeviceHandle;

    return OUT_CLOSERESULT_OK;
}

API_EXPORT OUT_SENDSHORTRESULT API_CALL SendShortEventToOutputEndpoint(void* handle, int message, int* errorCode)
{
    *errorCode = 0;

    OutputEndpointHandle* outputDeviceHandle = static_cast<OutputEndpointHandle*>(handle);

    MMRESULT result = midiOutShortMsg(outputDeviceHandle->handle, (DWORD)message);
    if (result != MMSYSERR_NOERROR)
    {
        *errorCode = result;

        switch (result)
        {
            case MIDIERR_BADOPENMODE: return OUT_SENDSHORTRESULT_BADOPENMODE;
            case MIDIERR_NOTREADY: return OUT_SENDSHORTRESULT_NOTREADY;
            case MMSYSERR_INVALHANDLE: return OUT_SENDSHORTRESULT_INVALIDHANDLE;
        }

        return OUT_SENDSHORTRESULT_UNKNOWNERROR;
    }

    return OUT_SENDSHORTRESULT_OK;
}

API_EXPORT OUT_SENDSYSEXRESULT API_CALL SendSysExEventToOutputEndpoint_Win(void* handle, LPSTR data, int size, int* errorCode)
{
    *errorCode = 0;

    OutputEndpointHandle* outputDeviceHandle = static_cast<OutputEndpointHandle*>(handle);

    LPMIDIHDR header = new MIDIHDR();
    header->lpData = data;
    header->dwBufferLength = size;
    header->dwBytesRecorded = size;
    header->dwFlags = 0;

    MMRESULT result = midiOutPrepareHeader(outputDeviceHandle->handle, header, sizeof(MIDIHDR));
    if (result != MMSYSERR_NOERROR)
    {
        delete header;

        *errorCode = result;

        switch (result)
        {
            case MMSYSERR_INVALHANDLE: return OUT_SENDSYSEXRESULT_PREPAREBUFFER_INVALIDHANDLE;
            case MMSYSERR_INVALPARAM: return OUT_SENDSYSEXRESULT_PREPAREBUFFER_INVALIDADDRESS;
            case MMSYSERR_NOMEM: return OUT_SENDSYSEXRESULT_PREPAREBUFFER_NOMEMORY;
        }

        return OUT_SENDSYSEXRESULT_PREPAREBUFFER_UNKNOWNERROR;
    }

    result = midiOutLongMsg(outputDeviceHandle->handle, header, sizeof(MIDIHDR));
    if (result != MMSYSERR_NOERROR)
    {
        midiOutUnprepareHeader(outputDeviceHandle->handle, header, sizeof(MIDIHDR));
        delete header;

        *errorCode = result;

        switch (result)
        {
            case MIDIERR_NOTREADY: return OUT_SENDSYSEXRESULT_NOTREADY;
            case MIDIERR_UNPREPARED: return OUT_SENDSYSEXRESULT_UNPREPARED;
            case MMSYSERR_INVALHANDLE: return OUT_SENDSYSEXRESULT_INVALIDHANDLE;
            case MMSYSERR_INVALPARAM: return OUT_SENDSYSEXRESULT_INVALIDSTRUCTURE;
        }

        return OUT_SENDSYSEXRESULT_UNKNOWNERROR;
    }

    return OUT_SENDSYSEXRESULT_OK;
}

API_EXPORT OUT_GETSYSEXDATARESULT API_CALL GetOutputEndpointSysExBufferData(void* handle, LPMIDIHDR header, LPSTR* data, int* size, int* errorCode)
{
    *errorCode = 0;

    OutputEndpointHandle* outputDeviceHandle = static_cast<OutputEndpointHandle*>(handle);

    MMRESULT result = midiOutUnprepareHeader(outputDeviceHandle->handle, header, sizeof(MIDIHDR));
    if (result != MMSYSERR_NOERROR)
    {
        *errorCode = result;

        switch (result)
        {
            case MIDIERR_STILLPLAYING: return OUT_GETSYSEXDATARESULT_STILLPLAYING;
            case MMSYSERR_INVALPARAM: return OUT_GETSYSEXDATARESULT_INVALIDSTRUCTURE;
            case MMSYSERR_INVALHANDLE: return OUT_GETSYSEXDATARESULT_INVALIDHANDLE;
        }

        return OUT_GETSYSEXDATARESULT_UNKNOWNERROR;
    }

    *data = header->lpData;
    *size = header->dwBytesRecorded;

    delete header;
    return OUT_GETSYSEXDATARESULT_OK;
}

API_EXPORT char API_CALL IsOutputEndpointPropertySupported(OUT_PROPERTY property)
{
    switch (property)
    {
        case OUT_PROPERTY_PRODUCT:
        case OUT_PROPERTY_MANUFACTURER:
        case OUT_PROPERTY_DRIVERVERSION:
        case OUT_PROPERTY_TECHNOLOGY:
        case OUT_PROPERTY_VOICESNUMBER:
        case OUT_PROPERTY_NOTESNUMBER:
        case OUT_PROPERTY_CHANNELS:
        case OUT_PROPERTY_OPTIONS:
            return 1;
    }

    return 0;
}

/* ================================
 Virtual device
 ================================ */

struct VirtualDeviceInfo
{
    InputEndpointInfo* inputDeviceInfo = nullptr;
    OutputEndpointInfo* outputDeviceInfo = nullptr;
    const wchar_t* name;
    winrt::guid associationId;
    std::wstring endpointDeviceId;
};

const midi2::MidiEndpointAssociatedPortDeviceInformation GetSinglePort(midi2::MidiEndpointDeviceInformation endpointInformation, midi2::Midi1PortFlow flow)
{
    auto ports = endpointInformation.FindAllAssociatedMidi1PortsForThisEndpoint(flow);
    return ports.GetAt(0);
}

API_EXPORT VIRTUAL_OPENRESULT API_CALL OpenVirtualDevice_Win(
    const wchar_t* name,
    Configuration* configuration,
    SessionHandle* sessionHandle,
    VirtualDeviceInfo** info,
    int* errorCode)
{
    *errorCode = 0;

    VirtualDeviceInfo* virtualDeviceInfo = new VirtualDeviceInfo();
    virtualDeviceInfo->name = name;

    if (!configuration->useWms || !configuration->wmsSdkInitialized || !configuration->wmsAvailable)
    {
        delete virtualDeviceInfo;
        return VIRTUAL_OPENRESULT_WMSUNAVAILABLE;
    }

    if (!configuration->basicLoopbackAvailable)
    {
        delete virtualDeviceInfo;
        return VIRTUAL_OPENRESULT_WMSBASICLOOPBACKUNAVAILABLE;
    }

    auto cleanupVirtualDevice = [&virtualDeviceInfo]()
    {
        if (virtualDeviceInfo->inputDeviceInfo != nullptr)
            DeleteInputEndpointInfo(virtualDeviceInfo->inputDeviceInfo);

        if (virtualDeviceInfo->outputDeviceInfo != nullptr)
            DeleteOutputEndpointInfo(virtualDeviceInfo->outputDeviceInfo);

        delete virtualDeviceInfo;
    };

    try
    {
        winrt::hstring uniqueId = winrt::to_hstring(winrt::Windows::Foundation::GuidHelper::CreateNewGuid());

        virtualDeviceInfo->associationId = winrt::Windows::Foundation::GuidHelper::CreateNewGuid();

        basicLoopback::MidiBasicLoopbackEndpointDefinition definition{};
        definition.Name = winrt::to_hstring(name);
        definition.UniqueId = uniqueId;
        definition.IsMuted = false;

        basicLoopback::MidiBasicLoopbackEndpointCreationConfig config(
            virtualDeviceInfo->associationId,
            definition);

        auto result = basicLoopback::MidiBasicLoopbackEndpointManager::CreateTransientLoopbackEndpoint(config);

        if (!result.Success())
        {
            *errorCode = static_cast<int>(result.ErrorCode());

            delete virtualDeviceInfo;

            switch (result.ErrorCode())
            {
                case basicLoopback::MidiBasicLoopbackEndpointCreationResultErrorCode::InvalidOrMissingName:
                    return VIRTUAL_OPENRESULT_INVALIDNAME;
                case basicLoopback::MidiBasicLoopbackEndpointCreationResultErrorCode::InvalidOrMissingUniqueId:
                    return VIRTUAL_OPENRESULT_INVALIDUNIQUEID;
                case basicLoopback::MidiBasicLoopbackEndpointCreationResultErrorCode::NameInUse:
                    return VIRTUAL_OPENRESULT_NAMEINUSE;
                case basicLoopback::MidiBasicLoopbackEndpointCreationResultErrorCode::UniqueIdInUse:
                    return VIRTUAL_OPENRESULT_UNIQUEIDINUSE;
            }

            return VIRTUAL_OPENRESULT_FAILED;
        }

        virtualDeviceInfo->endpointDeviceId = result.EndpointDeviceId().c_str();

        EnsureWinMmPortsAvailable();

        auto endpointId = result.EndpointDeviceId();
        auto endpointInformation = midi2::MidiEndpointDeviceInformation::CreateFromEndpointDeviceId(endpointId);

        auto inputPort = GetSinglePort(endpointInformation, midi2::Midi1PortFlow::MidiMessageSource);

        InputEndpointInfo* inputDeviceInfo = nullptr;
        IN_GETINFORESULT inputResult = GetInputEndpointInfo(inputPort.PortNumber(), endpointId, inputPort.PortDeviceId(), &inputDeviceInfo, errorCode);
        if (inputResult != IN_GETINFORESULT_OK)
        {
            delete virtualDeviceInfo;
            return VIRTUAL_OPENRESULT_FAILEDGETINPUTDEVICEINFO;
        }

        virtualDeviceInfo->inputDeviceInfo = inputDeviceInfo;

        auto outputPort = GetSinglePort(endpointInformation, midi2::Midi1PortFlow::MidiMessageDestination);

        OutputEndpointInfo* outputDeviceInfo = nullptr;
        OUT_GETINFORESULT outputResult = GetOutputEndpointInfo(outputPort.PortNumber(), endpointId, outputPort.PortDeviceId(), &outputDeviceInfo, errorCode);
        if (outputResult != OUT_GETINFORESULT_OK)
        {
            DeleteInputEndpointInfo(virtualDeviceInfo->inputDeviceInfo);
            delete virtualDeviceInfo;
            return VIRTUAL_OPENRESULT_FAILEDGETOUTPUTDEVICEINFO;
        }

        virtualDeviceInfo->outputDeviceInfo = outputDeviceInfo;
    }
    catch (const winrt::hresult_error& e)
    {
        cleanupVirtualDevice();
        configuration->activityCallback(FormatError(e, L"Create virtual device"));
        return VIRTUAL_OPENRESULT_WMSERROR;
    }
    catch (const std::exception& e)
    {
        cleanupVirtualDevice();
        configuration->activityCallback(FormatError(e, L"Create virtual device"));
        return VIRTUAL_OPENRESULT_WMSERROR;
    }
    catch (...)
    {
        cleanupVirtualDevice();
        configuration->activityCallback(L"Failed to create virtual device");
        return VIRTUAL_OPENRESULT_WMSERROR;
    }

    *info = virtualDeviceInfo;
    return VIRTUAL_OPENRESULT_OK;
}

API_EXPORT VIRTUAL_CLOSERESULT API_CALL CloseVirtualDevice(VirtualDeviceInfo* info, int* errorCode)
{
    *errorCode = 0;

    try
    {
        auto removed = basicLoopback::MidiBasicLoopbackEndpointManager::RemoveTransientLoopbackEndpoint(
            basicLoopback::MidiBasicLoopbackEndpointRemovalConfig{ info->associationId });

        if (!removed)
        {
            delete info;
            return VIRTUAL_CLOSERESULT_FAILED;
        }
    }
    catch (...)
    {
        delete info;
        return VIRTUAL_CLOSERESULT_WMSERROR;
    }

    delete info;
    return VIRTUAL_CLOSERESULT_OK;
}

API_EXPORT InputEndpointInfo* API_CALL GetInputEndpointInfoFromVirtualDevice(VirtualDeviceInfo* info)
{
    return info->inputDeviceInfo;
}

API_EXPORT OutputEndpointInfo* API_CALL GetOutputEndpointInfoFromVirtualDevice(VirtualDeviceInfo* info)
{
    return info->outputDeviceInfo;
}

API_EXPORT VIRTUAL_MUTERESULT API_CALL MuteVirtualDevice(
    VirtualDeviceInfo* info,
    Configuration* configuration)
{
    try
    {
        auto muted = basicLoopback::MidiBasicLoopbackEndpointManager::MuteLoopback(info->associationId);
        if (!muted)
            return VIRTUAL_MUTERESULT_FAILED;
    }
    catch (const winrt::hresult_error& e)
    {
        configuration->activityCallback(FormatError(e, L"Mute virtual device"));
        return VIRTUAL_MUTERESULT_WMSERROR;
    }
    catch (const std::exception& e)
    {
        configuration->activityCallback(FormatError(e, L"Mute virtual device"));
        return VIRTUAL_MUTERESULT_WMSERROR;
    }
    catch (...)
    {
        configuration->activityCallback(L"Failed to mute virtual device");
        return VIRTUAL_MUTERESULT_WMSERROR;
    }

    return VIRTUAL_MUTERESULT_OK;
}

API_EXPORT VIRTUAL_UNMUTERESULT API_CALL UnmuteVirtualDevice(
    VirtualDeviceInfo* info,
    Configuration* configuration)
{
    try
    {
        auto unmuted = basicLoopback::MidiBasicLoopbackEndpointManager::UnmuteLoopback(info->associationId);
        if (!unmuted)
            return VIRTUAL_UNMUTERESULT_FAILED;
    }
    catch (const winrt::hresult_error& e)
    {
        configuration->activityCallback(FormatError(e, L"Unmute virtual device"));
        return VIRTUAL_UNMUTERESULT_WMSERROR;
    }
    catch (const std::exception& e)
    {
        configuration->activityCallback(FormatError(e, L"Unmute virtual device"));
        return VIRTUAL_UNMUTERESULT_WMSERROR;
    }
    catch (...)
    {
        configuration->activityCallback(L"Failed to unmute virtual device");
        return VIRTUAL_UNMUTERESULT_WMSERROR;
    }

    return VIRTUAL_UNMUTERESULT_OK;
}