#pragma comment(lib, "winmm.lib")
#pragma comment(lib, "setupapi.lib")

#ifndef NOMINMAX
#define NOMINMAX
#endif

#ifndef DRV_QUERYDEVICEINTERFACESIZE
#define DRV_QUERYDEVICEINTERFACESIZE 0x80d
#endif

#ifndef DRV_QUERYDEVICEINTERFACE
#define DRV_QUERYDEVICEINTERFACE 0x80C
#endif

#include <initguid.h>
#include <windows.h>
#include <mmsystem.h>
#include <mmreg.h>
#include <setupapi.h>
#include <devpkey.h>

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
#include <winrt/Windows.Devices.Midi2.h>
namespace midi2 = winrt::Windows::Devices::Midi2;
#include <winrt/Windows.Devices.Midi2.Enumeration.h>
#include <winrt/Windows.Devices.Midi2.Enumeration.Legacy.h>
#include <winrt/Windows.Devices.Midi2.Transports.BasicLoopback.h>
namespace basicLoopback = winrt::Windows::Devices::Midi2::Transports::BasicLoopback;

//#include "winmidi/init/Windows.Devices.Midi2.Initialization.hpp"
//namespace init = Windows::Devices::Midi2::Initialization;

//#include "winmidi/init/WindowsMidiServicesVersion.h"

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

API_EXPORT API_TYPE API_CALL GetApiType()
{
    return API_TYPE_WIN;
}

API_EXPORT void API_CALL GetNativeEnvironmentInfo_Win(
    bool* wmsAvailable)
{
    winrt::init_apartment();
    
    try
    {
        *wmsAvailable = midi2::MidiApi::EnsureServiceAvailable();
    }
    catch (...)
    {
        *wmsAvailable = false;
    }
}

int GetPortNumber(const midi2::Enumeration::Legacy::MidiLegacyPortDeviceInformation info)
{
    if (info.Flow() == midi2::Enumeration::Midi1PortFlow::MidiMessageSource)
        return info.Number() - 1;
    else if (info.Flow() == midi2::Enumeration::Midi1PortFlow::MidiMessageDestination)
        return info.Number();

    // TODO
    return -1;
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
            winrt::init_apartment();
            config->wmsAvailable = midi2::MidiApi::EnsureServiceAvailable();

            if (config->wmsAvailable)
            {
                config->basicLoopbackAvailable = basicLoopback::MidiBasicLoopbackManager::IsTransportAvailable();
            }
        }
        catch (const winrt::hresult_error& e)
        {
            config->activityCallback(FormatError(e, L"Failed to initialize WMS SDK"));
            return CONFIGURATION_GETRESULT_WMSUNKNOWNERROR;
        }
        catch (const std::exception& e)
        {
            config->activityCallback(FormatError(e, L"Failed to initialize WMS SDK"));
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
    return configuration->useWms && configuration->wmsAvailable && configuration->basicLoopbackAvailable;
}

API_EXPORT bool API_CALL IsDevicesWatcherApiAvailable(Configuration* configuration)
{
    return configuration->useWms && configuration->wmsAvailable;
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
    std::wstring endpointId;

    std::wstring endpointDeviceId;

    std::wstring portDeviceId;
    std::wstring devicePath;

    std::wstring deviceId;
    std::wstring deviceName;
    std::wstring deviceManufacturer;
    std::wstring deviceModel;
    std::wstring deviceDriverInformation;
};

struct InputEndpointInfo : EndpointInfoBase
{
    int deviceIndex;
    LPMIDIINCAPSW caps;
};

API_EXPORT void API_CALL CloneInputEndpointInfo(InputEndpointInfo* source, InputEndpointInfo** info)
{
    InputEndpointInfo* result = new InputEndpointInfo();

    result->deviceIndex = source->deviceIndex;
    result->caps = new MIDIINCAPSW(*source->caps);
    result->portDeviceId = source->portDeviceId;
    result->devicePath = source->devicePath;
    result->endpointDeviceId = source->endpointDeviceId;
    result->deviceId = source->deviceId;
    result->deviceName = source->deviceName;
    result->deviceManufacturer = source->deviceManufacturer;
    result->deviceModel = source->deviceModel;
    result->deviceDriverInformation = source->deviceDriverInformation;

    *info = result;
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

    bool isMicrosoftGsWavetableSynth{false};
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
    result->deviceId = source->deviceId;
    result->deviceName = source->deviceName;
    result->deviceManufacturer = source->deviceManufacturer;
    result->deviceModel = source->deviceModel;
    result->deviceDriverInformation = source->deviceDriverInformation;

    *info = result;
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

int FindPortIndex(const std::wstring& endpointDeviceId, const std::wstring& portDeviceId, const midi2::Enumeration::Midi1PortFlow& flow)
{
    if (endpointDeviceId.empty() || portDeviceId.empty())
        return -1;

    const winrt::hstring endpointDeviceIdH{ endpointDeviceId };

    auto endpointInformation = midi2::Enumeration::MidiEndpointDeviceInformation::CreateFromEndpointDeviceId(endpointDeviceIdH);
    if (endpointInformation == nullptr)
        return -1;

    auto ports = midi2::Enumeration::Legacy::MidiLegacyPortDeviceInformation::FindAllForAssociatedEndpoint(endpointInformation.EndpointDeviceId(), flow);
    for (auto const& port : ports)
    {
        if (port.PortDeviceId().c_str() == portDeviceId)
            return GetPortNumber(port);
    }

    return -1;
}

std::wstring ReadStringProp(HDEVINFO hDev, SP_DEVINFO_DATA& devData, const DEVPROPKEY& key)
{
    DEVPROPTYPE propType;
    DWORD bufSize = 0;
    SetupDiGetDevicePropertyW(hDev, &devData, &key, &propType, nullptr, 0, &bufSize, 0);
    if (bufSize == 0)
        return {};

    std::vector<BYTE> buf(bufSize);
    if (!SetupDiGetDevicePropertyW(hDev, &devData, &key, &propType, buf.data(), bufSize, nullptr, 0))
        return {};

    if (propType != DEVPROP_TYPE_STRING)
        return {};
    
    return std::wstring(reinterpret_cast<const wchar_t*>(buf.data()));
}

API_EXPORT DEVICE_GETDEVICEINFORESULT API_CALL GetDeviceInformation(
    EndpointInfoBase* deviceInfo,
    Configuration* configuration,
    const wchar_t** id,
    const wchar_t** name,
    const wchar_t** manufacturer,
    const wchar_t** model,
    const wchar_t** driverVersion,
    int* errorCode)
{
    *errorCode = 0;

    if (!deviceInfo->deviceId.empty())
    {
        *id = deviceInfo->deviceId.c_str();
        *name = deviceInfo->deviceName.c_str();
        *manufacturer = deviceInfo->deviceManufacturer.c_str();
        *model = deviceInfo->deviceModel.c_str();
        *driverVersion = deviceInfo->deviceDriverInformation.c_str();

        return DEVICE_GETDEVICEINFORESULT_OK;
    }

    if (!deviceInfo->endpointDeviceId.empty())
    {
        midi2::Enumeration::MidiParentDeviceInformation parentInformation = nullptr;
        midi2::Enumeration::MidiEndpointDeviceInformation endpointInformation = nullptr;

        try
        {
            endpointInformation = midi2::Enumeration::MidiEndpointDeviceInformation::CreateFromEndpointDeviceId(winrt::hstring{ deviceInfo->endpointDeviceId });
            if (endpointInformation == nullptr)
                return DEVICE_GETDEVICEINFORESULT_FAILEDGETENDPOINTINFO;

            parentInformation = endpointInformation.GetParentDeviceInformation();
            if (parentInformation == nullptr)
                return DEVICE_GETDEVICEINFORESULT_FAILEDGETPARENTDEVICEINFO;

            deviceInfo->deviceId = parentInformation.Id().c_str();
            deviceInfo->deviceName = parentInformation.Name().c_str();
            deviceInfo->deviceDriverInformation = parentInformation.DriverVersion().c_str();

            *id = deviceInfo->deviceId.c_str();
            *name = deviceInfo->deviceName.c_str();
            *driverVersion = deviceInfo->deviceDriverInformation.c_str();
        }
        catch (const winrt::hresult_error& e)
        {
            configuration->activityCallback(FormatError(e, L"Failed to get basic parent device properties"));
            return DEVICE_GETDEVICEINFORESULT_UNKNOWNWMSERROR;
        }
        catch (const std::exception& e)
        {
            configuration->activityCallback(FormatError(e, L"Failed to get basic parent device properties"));
            return DEVICE_GETDEVICEINFORESULT_UNKNOWNWMSERROR;
        }
        catch (...)
        {
            configuration->activityCallback(L"Failed to get basic parent device properties");
            return DEVICE_GETDEVICEINFORESULT_UNKNOWNWMSERROR;
        }

        if (parentInformation != nullptr && endpointInformation != nullptr)
        {
            try
            {
                auto manufacturerPropertyName = L"System.Devices.DeviceManufacturer";
                auto modelPropertyName = L"System.Devices.ModelName";

                auto parentDeviceInfo = winrt::Windows::Devices::Enumeration::DeviceInformation::CreateFromIdAsync(
                    endpointInformation.ParentDeviceInstanceId(),
                    {
                        manufacturerPropertyName,
                        modelPropertyName
                    },
                    winrt::Windows::Devices::Enumeration::DeviceInformationKind::Device).get();

                auto properties = parentDeviceInfo.Properties();

                if (properties.HasKey(manufacturerPropertyName))
                {
                    auto unboxed = winrt::unbox_value_or<winrt::hstring>(properties.Lookup(manufacturerPropertyName), L"");
                    deviceInfo->deviceManufacturer = unboxed.c_str();
                    *manufacturer = deviceInfo->deviceManufacturer.c_str();
                }
                else
                    configuration->activityCallback(L"Device manufacturer property is not set");

                if (properties.HasKey(modelPropertyName))
                {
                    auto unboxed = winrt::unbox_value_or<winrt::hstring>(properties.Lookup(modelPropertyName), L"");
                    deviceInfo->deviceModel = unboxed.c_str();
                    *model = deviceInfo->deviceModel.c_str();
                }
                else
                    configuration->activityCallback(L"Device model property is not set");
            }
            catch (const winrt::hresult_error& e)
            {
                configuration->activityCallback(FormatError(e, L"Failed to get additional parent device properties"));
            }
            catch (const std::exception& e)
            {
                configuration->activityCallback(FormatError(e, L"Failed to get additional parent device properties"));
            }
            catch (...)
            {
                configuration->activityCallback(L"Failed to get additional parent device properties");
            }
        }
    }
    else if (!deviceInfo->devicePath.empty())
    {
        // TODO: check returns

        HDEVINFO hDev = SetupDiCreateDeviceInfoListExW(nullptr, nullptr, nullptr, nullptr);
        if (hDev == INVALID_HANDLE_VALUE)
            return DEVICE_GETDEVICEINFORESULT_FAILEDPREPAREDEVICEINFO;

        SP_DEVICE_INTERFACE_DATA ifaceData{};
        ifaceData.cbSize = sizeof(ifaceData);

        if (!SetupDiOpenDeviceInterfaceW(hDev, deviceInfo->devicePath.c_str(), 0, &ifaceData))
        {
            SetupDiDestroyDeviceInfoList(hDev);
            return DEVICE_GETDEVICEINFORESULT_FAILEDGETDEVICEINFO;
        }

        SP_DEVINFO_DATA devData{};
        devData.cbSize = sizeof(devData);
        DWORD reqSize = 0;
        SetupDiGetDeviceInterfaceDetailW(hDev, &ifaceData, nullptr, 0, &reqSize, &devData);

        deviceInfo->deviceName = ReadStringProp(hDev, devData, DEVPKEY_Device_FriendlyName);
        if (deviceInfo->deviceName.empty())
            deviceInfo->deviceName = ReadStringProp(hDev, devData, DEVPKEY_Device_DeviceDesc);

        *name = deviceInfo->deviceName.c_str();

        deviceInfo->deviceManufacturer = ReadStringProp(hDev, devData, DEVPKEY_Device_Manufacturer);
        *manufacturer = deviceInfo->deviceManufacturer.c_str();

        std::wstring parentInstanceId = ReadStringProp(hDev, devData, DEVPKEY_Device_Parent);
        SetupDiDestroyDeviceInfoList(hDev);

        // TODO: another error
        if (parentInstanceId.empty())
            return DEVICE_GETDEVICEINFORESULT_FAILEDGETPARENTDEVICEINFO;

        deviceInfo->deviceId = parentInstanceId;
        *id = deviceInfo->deviceId.c_str();

        HDEVINFO hParent = SetupDiCreateDeviceInfoListExW(nullptr, nullptr, nullptr, nullptr);
        if (hParent == INVALID_HANDLE_VALUE)
            return DEVICE_GETDEVICEINFORESULT_FAILEDPREPAREPARENTDEVICEINFO;

        SP_DEVINFO_DATA parentData{};
        parentData.cbSize = sizeof(parentData);

        if (!SetupDiOpenDeviceInfoW(hParent, parentInstanceId.c_str(), nullptr, 0, &parentData))
        {
            SetupDiDestroyDeviceInfoList(hParent);
            return DEVICE_GETDEVICEINFORESULT_FAILEDGETPARENTDEVICEINFO;
        }

        if (deviceInfo->deviceName.empty())
            deviceInfo->deviceName = ReadStringProp(hParent, parentData, DEVPKEY_Device_BusReportedDeviceDesc);
        if (deviceInfo->deviceName.empty())
            deviceInfo->deviceName = ReadStringProp(hParent, parentData, DEVPKEY_Device_DeviceDesc);

        *name = deviceInfo->deviceName.c_str();

        deviceInfo->deviceModel = ReadStringProp(hParent, parentData, DEVPKEY_Device_BusReportedDeviceDesc);
        *model = deviceInfo->deviceModel.c_str();
        
        deviceInfo->deviceDriverInformation = ReadStringProp(hParent, parentData, DEVPKEY_Device_DriverVersion);
        *driverVersion = deviceInfo->deviceDriverInformation.c_str();

        SetupDiDestroyDeviceInfoList(hParent);
    }

    return DEVICE_GETDEVICEINFORESULT_OK;
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

    midi2::Enumeration::MidiEndpointDeviceWatcher watcher{nullptr};
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
        if (configuration->useWms && configuration->wmsAvailable)
        {
            sessionHandle->watcher = midi2::Enumeration::MidiEndpointDeviceWatcher::Create(midi2::Enumeration::MidiEndpointDeviceInformationFilters::AllStandardEndpoints);

            auto OnWatcherDeviceAdded = [sessionHandle](midi2::Enumeration::MidiEndpointDeviceWatcher const&, midi2::Enumeration::MidiEndpointDeviceInformationAddedEventArgs const& args)
            {
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
                    auto endpointInformation = midi2::Enumeration::MidiEndpointDeviceInformation::CreateFromEndpointDeviceId(endpointId);
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
                            case midi2::Enumeration::MidiGroupTerminalBlockDirection::BlockInput:
                                destinationsCount++;
                                break;
                            case midi2::Enumeration::MidiGroupTerminalBlockDirection::BlockOutput:
                                sourcesCount++;
                                break;
                            case midi2::Enumeration::MidiGroupTerminalBlockDirection::Bidirectional:
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

                        auto endpointInformation = midi2::Enumeration::MidiEndpointDeviceInformation::CreateFromEndpointDeviceId(endpointId);
                        if (endpointInformation == nullptr)
                        {
                            // TODO
                            return;
                        }

                        if (sourcesCount > 0)
                        {
                            auto inputPorts = midi2::Enumeration::Legacy::MidiLegacyPortDeviceInformation::FindAllForAssociatedEndpoint(endpointInformation.EndpointDeviceId(), midi2::Enumeration::Midi1PortFlow::MidiMessageSource);
                            ok = inputPorts.Size() >= sourcesCount;
                            if (!ok)
                                continue;

                            for (auto const& port : inputPorts)
                            {
                                InputEndpointInfo* inputDeviceInfo = nullptr;
                                int errorCode;

                                auto getInputEndpointInfoResult = GetInputEndpointInfo(GetPortNumber(port), endpointId, port.PortDeviceId(), &inputDeviceInfo, &errorCode);
                                if (getInputEndpointInfoResult != IN_GETINFORESULT_OK)
                                {
                                    ok = false;
                                    break;
                                }

                                inputDevicesInfo.push_back(inputDeviceInfo);

                                InputEndpointInfo* persistentInputEndpointInfo;
                                getInputEndpointInfoResult = GetInputEndpointInfo(GetPortNumber(port), endpointId, port.PortDeviceId(), &persistentInputEndpointInfo, &errorCode);
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
                            auto outputPorts = midi2::Enumeration::Legacy::MidiLegacyPortDeviceInformation::FindAllForAssociatedEndpoint(endpointInformation.EndpointDeviceId(), midi2::Enumeration::Midi1PortFlow::MidiMessageDestination);
                            ok = outputPorts.Size() >= destinationsCount;
                            if (!ok)
                                continue;

                            for (auto const& port : outputPorts)
                            {
                                OutputEndpointInfo* outputDeviceInfo = nullptr;
                                int errorCode;

                                auto getOutputEndpointInfoResult = GetOutputEndpointInfo(GetPortNumber(port), endpointId, port.PortDeviceId(), &outputDeviceInfo, &errorCode);
                                if (getOutputEndpointInfoResult != OUT_GETINFORESULT_OK)
                                {
                                    ok = false;
                                    break;
                                }

                                outputDevicesInfo.push_back(outputDeviceInfo);

                                OutputEndpointInfo* persistentOutputEndpointInfo;
                                getOutputEndpointInfoResult = GetOutputEndpointInfo(GetPortNumber(port), endpointId, port.PortDeviceId(), &persistentOutputEndpointInfo, &errorCode);
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

                    if (sessionHandle->initialEnumerationCompleted.load() == 0)
                        return;

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
                    sessionHandle->configuration->activityCallback(FormatError(e, L"Failed to process endpoint added"));
                }
                catch (const std::exception& e)
                {
                    sessionHandle->configuration->activityCallback(FormatError(e, L"Failed to process endpoint added"));
                }
                catch (...)
                {
                    sessionHandle->configuration->activityCallback(L"Failed to process endpoint added");
                }
            };

            auto OnWatcherDeviceRemoved = [sessionHandle](midi2::Enumeration::MidiEndpointDeviceWatcher const&, midi2::Enumeration::MidiEndpointDeviceInformationRemovedEventArgs const& args)
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
                    sessionHandle->configuration->activityCallback(FormatError(e, L"Failed to process endpoint removed"));
                }
                catch (const std::exception& e)
                {
                    sessionHandle->configuration->activityCallback(FormatError(e, L"Failed to process endpoint removed"));
                }
                catch (...)
                {
                    sessionHandle->configuration->activityCallback(L"Failed to process endpoint removed");
                }
            };

            auto OnWatcherEnumerationCompleted = [sessionHandle](midi2::Enumeration::MidiEndpointDeviceWatcher const&, winrt::Windows::Foundation::IInspectable const&)
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
        configuration->activityCallback(FormatError(e, L"Failed to setup devices watcher"));
        delete sessionHandle;
        return SESSION_OPENRESULT_WMSUNKNOWNERROR;
    }
    catch (const std::exception& e)
    {
        configuration->activityCallback(FormatError(e, L"Failed to setup devices watcher"));
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

    if (configuration->useWms && configuration->wmsAvailable)
    {
        while (sessionHandle->initialEnumerationCompleted.load() == 0)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
        }

        std::vector<InputEndpointInfo*> inputDevicesInfo;
        std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

        auto cleanupInputDevices = [&inputDevicesInfo]()
        {
            for (auto& info : inputDevicesInfo)
            {
                DeleteInputEndpointInfo(info);
            }
        };

        try
        {
            for (const auto& pair : sessionHandle->endpointDevicesById)
            {
                const EndpointDevicesInfo& endpointDevicesInfo = pair.second;

                for (auto* inputDeviceInfo : endpointDevicesInfo.inputDevicesInfo)
                {
                    InputEndpointInfo* infoCopy = nullptr;
                    CloneInputEndpointInfo(inputDeviceInfo, &infoCopy);
                    inputDevicesInfo.push_back(infoCopy);
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

            configuration->activityCallback(FormatError(e, L"Failed to get all input endpoints via WMS"));
            return IN_GETALLINFORESULT_UNKNOWNWMSERROR;
        }
        catch (const std::exception& e)
        {
            cleanupInputDevices();

            configuration->activityCallback(FormatError(e, L"Failed to get all input endpoints via WMS"));
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

API_EXPORT IN_GETPROPERTYRESULT API_CALL GetInputEndpointId_Win(InputEndpointInfo* info, const wchar_t** value, int* errorCode)
{
    *errorCode = 0;
    *value = L"";

    if (info == nullptr)
        return IN_GETPROPERTYRESULT_OK;

    if (!info->endpointDeviceId.empty() && !info->portDeviceId.empty())
        info->endpointId = info->endpointDeviceId + L"_" + info->portDeviceId;
    else if (!info->devicePath.empty())
        info->endpointId = info->devicePath + L"_" + info->caps->szPname;
    else
        info->endpointId = info->caps->szPname;

    *value = info->endpointId.c_str();

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

    auto deviceIndex = FindPortIndex(inputDeviceInfo->endpointDeviceId, inputDeviceInfo->portDeviceId, midi2::Enumeration::Midi1PortFlow::MidiMessageSource);
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

    if (configuration->useWms && configuration->wmsAvailable)
    {
        while (sessionHandle->initialEnumerationCompleted.load() == 0)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
        }

        std::vector<OutputEndpointInfo*> outputDevicesInfo;
        std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

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
                outputDeviceInfo->isMicrosoftGsWavetableSynth = true;
                outputDevicesInfo.push_back(outputDeviceInfo);
                break;
            }

            DeleteOutputEndpointInfo(outputDeviceInfo);
        }

        try
        {
            for (const auto& pair : sessionHandle->endpointDevicesById)
            {
                const EndpointDevicesInfo& endpointDevicesInfo = pair.second;

                for (auto* outputDeviceInfo : endpointDevicesInfo.outputDevicesInfo)
                {
                    OutputEndpointInfo* infoCopy = nullptr;
                    CloneOutputEndpointInfo(outputDeviceInfo, &infoCopy);
                    outputDevicesInfo.push_back(infoCopy);
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

            configuration->activityCallback(FormatError(e, L"Failed to get all output endpoints via WMS"));
            return OUT_GETALLINFORESULT_UNKNOWNWMSERROR;
        }
        catch (const std::exception& e)
        {
            cleanupOutputDevices();

            configuration->activityCallback(FormatError(e, L"Failed to get all output endpoints via WMS"));
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
            outputDeviceInfo->isMicrosoftGsWavetableSynth = true;

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

    // TODO: WMS
    *value = info->caps->szPname;
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputEndpointId_Win(OutputEndpointInfo* info, const wchar_t** value, int* errorCode)
{
    *errorCode = 0;
    *value = L"";

    if (info == nullptr)
        return OUT_GETPROPERTYRESULT_OK;

    if (!info->endpointDeviceId.empty() && !info->portDeviceId.empty())
        info->endpointId = info->endpointDeviceId + L"_" + info->portDeviceId;
    else if (!info->devicePath.empty())
        info->endpointId = info->devicePath + L"_" + info->caps->szPname;
    else if (info->isMicrosoftGsWavetableSynth)
        info->endpointId = L"Microsoft_GS_Wavetable_Synth";
    else
        info->endpointId = info->caps->szPname;

    *value = info->endpointId.c_str();

    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_OPENRESULT API_CALL OpenOutputEndpoint_Win(OutputEndpointInfo* info, void* sessionHandle, DWORD_PTR callback, void** handle, int* errorCode)
{
    *errorCode = 0;

    EnsureWinMmPortsAvailable();

    OutputEndpointHandle* outputDeviceHandle = new OutputEndpointHandle();
    outputDeviceHandle->info = info;

    auto deviceIndex = FindPortIndex(info->endpointDeviceId, info->portDeviceId, midi2::Enumeration::Midi1PortFlow::MidiMessageDestination);
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

const midi2::Enumeration::Legacy::MidiLegacyPortDeviceInformation GetSinglePort(midi2::Enumeration::MidiEndpointDeviceInformation endpointInformation, midi2::Enumeration::Midi1PortFlow flow)
{
    auto ports = midi2::Enumeration::Legacy::MidiLegacyPortDeviceInformation::FindAllForAssociatedEndpoint(endpointInformation.EndpointDeviceId(), flow);

    // TODO: check there are at least 1 port and not more than 1 port, and return error if not
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

    if (!configuration->useWms || !configuration->wmsAvailable)
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

        basicLoopback::MidiBasicLoopbackEndpointDefinition definition;
        definition.Name(winrt::to_hstring(name));
        definition.UniqueId(uniqueId);

        basicLoopback::MidiBasicLoopbackCreationConfig config(
            virtualDeviceInfo->associationId,
            definition);

        auto result = basicLoopback::MidiBasicLoopbackManager::CreateTransientLoopback(config);

        if (!result.Success())
        {
            *errorCode = static_cast<int>(result.ErrorCode());

            delete virtualDeviceInfo;

            switch (result.ErrorCode())
            {
                case basicLoopback::MidiBasicLoopbackErrorCode::ClientApiException:
                    return VIRTUAL_OPENRESULT_CLIENTAPIEXCEPTION;
                case basicLoopback::MidiBasicLoopbackErrorCode::InvalidOrMissingUniqueId:
                    return VIRTUAL_OPENRESULT_INVALIDUNIQUEID;
                case basicLoopback::MidiBasicLoopbackErrorCode::DuplicateEndpointName:
                    return VIRTUAL_OPENRESULT_DUPLICATEENDPOINTNAME;
                case basicLoopback::MidiBasicLoopbackErrorCode::DuplicateUniqueId:
                    return VIRTUAL_OPENRESULT_DUPLICATEUNIQUEID;
                case basicLoopback::MidiBasicLoopbackErrorCode::EndpointCreationFailed:
                    return VIRTUAL_OPENRESULT_ENDPOINTCREATIONFAILED;
                case basicLoopback::MidiBasicLoopbackErrorCode::EndpointNotFound:
                    return VIRTUAL_OPENRESULT_ENDPOINTNOTFOUND;
                case basicLoopback::MidiBasicLoopbackErrorCode::InvalidArgument:
                    return VIRTUAL_OPENRESULT_INVALIDARGUMENT;
                case basicLoopback::MidiBasicLoopbackErrorCode::InvalidJson:
                    return VIRTUAL_OPENRESULT_INVALIDJSON;
                case basicLoopback::MidiBasicLoopbackErrorCode::InvalidOrMissingAssociationId:
                    return VIRTUAL_OPENRESULT_INVALIDORMISSINGASSOCIATIONID;
                case basicLoopback::MidiBasicLoopbackErrorCode::InvalidOrMissingEndpointName:
                    return VIRTUAL_OPENRESULT_INVALIDORMISSINGENDPOINTNAME;
                case basicLoopback::MidiBasicLoopbackErrorCode::UnrecognizedCommand:
                    return VIRTUAL_OPENRESULT_UNRECOGNIZEDCOMMAND;
            }

            return VIRTUAL_OPENRESULT_FAILED;
        }

        virtualDeviceInfo->endpointDeviceId = result.CreatedLoopbackEntry().EndpointDeviceId().c_str();

        EnsureWinMmPortsAvailable();

        auto endpointId = result.CreatedLoopbackEntry().EndpointDeviceId();
        auto endpointInformation = midi2::Enumeration::MidiEndpointDeviceInformation::CreateFromEndpointDeviceId(endpointId);

        auto inputPort = GetSinglePort(endpointInformation, midi2::Enumeration::Midi1PortFlow::MidiMessageSource);

        InputEndpointInfo* inputDeviceInfo = nullptr;
        IN_GETINFORESULT inputResult = GetInputEndpointInfo(GetPortNumber(inputPort), endpointId, inputPort.PortDeviceId(), &inputDeviceInfo, errorCode);
        if (inputResult != IN_GETINFORESULT_OK)
        {
            delete virtualDeviceInfo;
            return VIRTUAL_OPENRESULT_FAILEDGETINPUTDEVICEINFO;
        }

        virtualDeviceInfo->inputDeviceInfo = inputDeviceInfo;

        auto outputPort = GetSinglePort(endpointInformation, midi2::Enumeration::Midi1PortFlow::MidiMessageDestination);

        OutputEndpointInfo* outputDeviceInfo = nullptr;
        OUT_GETINFORESULT outputResult = GetOutputEndpointInfo(GetPortNumber(outputPort), endpointId, outputPort.PortDeviceId(), &outputDeviceInfo, errorCode);
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
        configuration->activityCallback(FormatError(e, L"Failed to create virtual device"));
        return VIRTUAL_OPENRESULT_WMSERROR;
    }
    catch (const std::exception& e)
    {
        cleanupVirtualDevice();
        configuration->activityCallback(FormatError(e, L"Failed to create virtual device"));
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
        auto removed = basicLoopback::MidiBasicLoopbackManager::RemoveTransientLoopback(
            basicLoopback::MidiBasicLoopbackRemovalConfig{ info->associationId });

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
        auto muted = basicLoopback::MidiBasicLoopbackManager::MuteLoopback(info->associationId);
        if (!muted)
            return VIRTUAL_MUTERESULT_FAILED;
    }
    catch (const winrt::hresult_error& e)
    {
        configuration->activityCallback(FormatError(e, L"Failed to mute virtual device"));
        return VIRTUAL_MUTERESULT_WMSERROR;
    }
    catch (const std::exception& e)
    {
        configuration->activityCallback(FormatError(e, L"Failed to mute virtual device"));
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
        auto unmuted = basicLoopback::MidiBasicLoopbackManager::UnmuteLoopback(info->associationId);
        if (!unmuted)
            return VIRTUAL_UNMUTERESULT_FAILED;
    }
    catch (const winrt::hresult_error& e)
    {
        configuration->activityCallback(FormatError(e, L"Failed to unmute virtual device"));
        return VIRTUAL_UNMUTERESULT_WMSERROR;
    }
    catch (const std::exception& e)
    {
        configuration->activityCallback(FormatError(e, L"Failed to unmute virtual device"));
        return VIRTUAL_UNMUTERESULT_WMSERROR;
    }
    catch (...)
    {
        configuration->activityCallback(L"Failed to unmute virtual device");
        return VIRTUAL_UNMUTERESULT_WMSERROR;
    }

    return VIRTUAL_UNMUTERESULT_OK;
}