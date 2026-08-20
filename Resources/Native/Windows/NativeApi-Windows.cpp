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
#include <iostream>

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
#include <winrt/Windows.Devices.Midi2.Utilities.Messages.h>
namespace messages = winrt::Windows::Devices::Midi2::Utilities::Messages;

#include <winrt/Windows.Devices.Midi2.ClientPlugins.h>
using namespace winrt::Windows::Devices::Midi2::ClientPlugins;

#include <winrt/Windows.Storage.Streams.h>
#include <winrt/Windows.Devices.Midi2.Utilities.SysExTransfer.h>
namespace sysex = winrt::Windows::Devices::Midi2::Utilities::SysExTransfer;

#include "../Common/NativeApi-Constants.h"

#define API_EXPORT extern "C" __declspec(dllexport)
#define API_CALL __cdecl

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

API_EXPORT OS_TYPE API_CALL GetOsType()
{
    return OS_TYPE_WIN;
}

API_EXPORT void API_CALL GetNativeEnvironmentInfo_Win(
    bool* wmsAvailable)
{
    *wmsAvailable = false;

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
    return info.Number();
}

/* ================================
   Configuration
================================ */

typedef void (*NativeApiActivityCallback)(const wchar_t* record);

struct Configuration
{
    NativeApiActivityCallback activityCallback;
    bool useWms{ false };
    bool wmsAvailable{ false };
    bool wmsInitialized{ false };
    bool basicLoopbackAvailable{ 0 };
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
            GetNativeEnvironmentInfo_Win(&config->wmsAvailable);

            if (config->wmsAvailable)
                config->wmsInitialized = true;

            try
            {
                config->basicLoopbackAvailable = basicLoopback::MidiBasicLoopbackManager::IsTransportAvailable();
            }
            catch (...)
            {
                config->basicLoopbackAvailable = false;
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
    delete configuration;

    return CONFIGURATION_CLEANUPRESULT_OK;
}

API_EXPORT CONFIGURATION_API_TYPE API_CALL GetApiType(Configuration* configuration)
{
    if (configuration->wmsInitialized)
        return CONFIGURATION_API_TYPE_WMS;

    return CONFIGURATION_API_TYPE_WINMM;
}

API_EXPORT bool API_CALL IsVirtualDeviceApiAvailable(Configuration* configuration)
{
    return configuration->wmsInitialized && configuration->basicLoopbackAvailable;
}

API_EXPORT bool API_CALL IsDevicesWatcherApiAvailable(Configuration* configuration)
{
    return configuration->wmsInitialized;
}

API_EXPORT bool API_CALL IsWmsInitialized(Configuration* configuration)
{
    return configuration->wmsInitialized;
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
    midi2::Enumeration::MidiGroupTerminalBlock gtb{nullptr};
    midi2::MidiGroup group{nullptr};

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
    if (source->caps)
        result->caps = new MIDIINCAPSW(*source->caps);
    result->devicePath = source->devicePath;
    result->endpointDeviceId = source->endpointDeviceId;
    result->gtb = source->gtb;
    result->group = source->group;
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

    InputEndpointInfo* inputEndpointInfo = new InputEndpointInfo();

    inputEndpointInfo->deviceIndex = deviceIndex;
    inputEndpointInfo->caps = new MIDIINCAPSW();

    MMRESULT result = midiInGetDevCapsW(deviceIndex, inputEndpointInfo->caps, sizeof(MIDIINCAPSW));
    if (result != MMSYSERR_NOERROR)
    {
        delete inputEndpointInfo->caps;
        delete inputEndpointInfo;

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
            inputEndpointInfo->devicePath = std::wstring(buffer.data());
    }

    *info = inputEndpointInfo;

    return IN_GETINFORESULT_OK;
}

IN_GETINFORESULT GetInputEndpointInfo(const winrt::hstring& endpointDeviceId, const midi2::Enumeration::MidiGroupTerminalBlock& gtb, const midi2::MidiGroup& group, InputEndpointInfo** info, int* errorCode)
{
    *errorCode = 0;

    InputEndpointInfo* inputEndpointInfo = new InputEndpointInfo();
    inputEndpointInfo->endpointDeviceId = endpointDeviceId;
    inputEndpointInfo->gtb = gtb;
    inputEndpointInfo->group = group;
    *info = inputEndpointInfo;

    return IN_GETINFORESULT_OK;
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

    bool isMicrosoftGsWavetableSynth{ false };
};

API_EXPORT void API_CALL CloneOutputEndpointInfo(OutputEndpointInfo* source, OutputEndpointInfo** info)
{
    OutputEndpointInfo* result = new OutputEndpointInfo();

    result->deviceIndex = source->deviceIndex;
    if (source->caps)
        result->caps = new MIDIOUTCAPSW(*source->caps);
    result->devicePath = source->devicePath;
    result->isMicrosoftGsWavetableSynth = source->isMicrosoftGsWavetableSynth;
    result->endpointDeviceId = source->endpointDeviceId;
    result->gtb = source->gtb;
    result->group = source->group;
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

    OutputEndpointInfo* outputEndpointInfo = new OutputEndpointInfo();

    outputEndpointInfo->deviceIndex = deviceIndex;
    outputEndpointInfo->caps = new MIDIOUTCAPSW();

    MMRESULT result = midiOutGetDevCapsW(deviceIndex, outputEndpointInfo->caps, sizeof(MIDIOUTCAPSW));
    if (result != MMSYSERR_NOERROR)
    {
        delete outputEndpointInfo->caps;
        delete outputEndpointInfo;

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
            outputEndpointInfo->devicePath = std::wstring(buffer.data());
    }

    *info = outputEndpointInfo;

    return OUT_GETINFORESULT_OK;
}

OUT_GETINFORESULT GetOutputEndpointInfo(const winrt::hstring& endpointDeviceId, const midi2::Enumeration::MidiGroupTerminalBlock& gtb, const midi2::MidiGroup& group, OutputEndpointInfo** info, int* errorCode)
{
    *errorCode = 0;

    OutputEndpointInfo* outputEndpointInfo = new OutputEndpointInfo();
    outputEndpointInfo->endpointDeviceId = endpointDeviceId;
    outputEndpointInfo->gtb = gtb;
    outputEndpointInfo->group = group;
    *info = outputEndpointInfo;

    return OUT_GETINFORESULT_OK;
}

API_EXPORT void API_CALL DeleteOutputEndpointInfo(OutputEndpointInfo* info)
{
    delete info->caps;
    delete info;
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
    std::vector<InputEndpointInfo*> inputEndpointsInfo{};
    std::vector<OutputEndpointInfo*> outputEndpointsInfo{};
};

struct SessionHandle
{
    const wchar_t* name{};

    Configuration* configuration;

    InputEndpointCallback inputEndpointCallback;
    OutputEndpointCallback outputEndpointCallback;

    midi2::Enumeration::MidiEndpointDeviceWatcher watcher{ nullptr };
    winrt::event_token revokeOnWatcherDeviceRemoved;
    winrt::event_token revokeOnWatcherDeviceAdded;
    winrt::event_token revokeOnWatcherEnumerationCompleted;
    std::atomic<char> initialEnumerationCompleted{ 0 };

    midi2::MidiSession session{ nullptr };
    std::mutex endpointDevicesLock;
    std::unordered_map<std::wstring, EndpointDevicesInfo> endpointDevicesById;
    winrt::Windows::Foundation::Collections::IMap<winrt::hstring, midi2::MidiEndpointConnection> endpointConnectionsById =
        winrt::single_threaded_map<winrt::hstring, midi2::MidiEndpointConnection>();
    std::unordered_map<winrt::guid, int> endpointConnectionsUsersCounts;
};

struct EndpointHandleBase
{
    SessionHandle* sessionHandle{ nullptr };
    midi2::MidiEndpointConnection connection{ nullptr };
};

void DeleteInputEndpointInfos(std::vector<InputEndpointInfo*>& deviceInfos)
{
    for (auto* inputEndpointInfo : deviceInfos)
    {
        DeleteInputEndpointInfo(inputEndpointInfo);
    }

    deviceInfos.clear();
}

void DeleteOutputEndpointInfos(std::vector<OutputEndpointInfo*>& deviceInfos)
{
    for (auto* outputEndpointInfo : deviceInfos)
    {
        DeleteOutputEndpointInfo(outputEndpointInfo);
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
        if (configuration->wmsInitialized)
        {
            sessionHandle->session = midi2::MidiSession::Create(name);

            sessionHandle->watcher = midi2::Enumeration::MidiEndpointDeviceWatcher::Create(midi2::Enumeration::MidiEndpointDeviceInformationFilters::AllStandardEndpoints);

            auto OnWatcherDeviceAdded = [sessionHandle](midi2::Enumeration::MidiEndpointDeviceWatcher const&, midi2::Enumeration::MidiEndpointDeviceInformationAddedEventArgs const& args)
            {
                std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

                std::vector<InputEndpointInfo*> inputEndpointsInfo;
                std::vector<OutputEndpointInfo*> outputEndpointsInfo;
                EndpointDevicesInfo endpointDevicesInfo;

                try
                {
                    EnsureWinMmPortsAvailable();

                    auto endpointInformation = args.AddedDevice();
                    auto endpointId = endpointInformation.EndpointDeviceId();

                    auto addOutputEndpoints = [&](const midi2::Enumeration::MidiGroupTerminalBlock& gtb)
                    {
                        for (auto i = 0; i < gtb.GroupCount(); i++)
                        {
                            auto group = midi2::MidiGroup(gtb.FirstGroup().Index() + i);

                            OutputEndpointInfo* outputEndpointInfo = nullptr;
                            int errorCode;

                            auto getOutputEndpointInfoResult = GetOutputEndpointInfo(endpointId, gtb, group, &outputEndpointInfo, &errorCode);
                            if (getOutputEndpointInfoResult != OUT_GETINFORESULT_OK)
                            {
                                // TODO
                                return;
                            }

                            outputEndpointsInfo.push_back(outputEndpointInfo);

                            OutputEndpointInfo* persistentOutputEndpointInfo;
                            getOutputEndpointInfoResult = GetOutputEndpointInfo(endpointId, gtb, group, &persistentOutputEndpointInfo, &errorCode);
                            if (getOutputEndpointInfoResult != OUT_GETINFORESULT_OK)
                            {
                                // TODO
                                return;
                            }

                            endpointDevicesInfo.outputEndpointsInfo.push_back(persistentOutputEndpointInfo);
                        }
                    };

                    auto addInputEndpoints = [&](const midi2::Enumeration::MidiGroupTerminalBlock& gtb)
                    {
                        for (auto i = 0; i < gtb.GroupCount(); i++)
                        {
                            auto group = midi2::MidiGroup(gtb.FirstGroup().Index() + i);

                            InputEndpointInfo* inputEndpointInfo = nullptr;
                            int errorCode;

                            auto getInputEndpointInfoResult = GetInputEndpointInfo(endpointId, gtb, group, &inputEndpointInfo, &errorCode);
                            if (getInputEndpointInfoResult != IN_GETINFORESULT_OK)
                            {
                                // TODO
                                return;
                            }

                            inputEndpointsInfo.push_back(inputEndpointInfo);

                            InputEndpointInfo* persistentInputEndpointInfo;
                            getInputEndpointInfoResult = GetInputEndpointInfo(endpointId, gtb, group, &persistentInputEndpointInfo, &errorCode);
                            if (getInputEndpointInfoResult != IN_GETINFORESULT_OK)
                            {
                                // TODO
                                return;
                            }

                            endpointDevicesInfo.inputEndpointsInfo.push_back(persistentInputEndpointInfo);
                        }
                    };

                    auto groupTerminalBlocks = endpointInformation.GetGroupTerminalBlocks();

                    for (auto const& gtb : groupTerminalBlocks)
                    {
                        auto direction = gtb.Direction();
                        if (direction == midi2::Enumeration::MidiGroupTerminalBlockDirection::BlockInput)
                        {
                            addOutputEndpoints(gtb);
                        }
                        else if (direction == midi2::Enumeration::MidiGroupTerminalBlockDirection::BlockOutput)
                        {
                            addInputEndpoints(gtb);
                        }
                        else if (direction == midi2::Enumeration::MidiGroupTerminalBlockDirection::Bidirectional)
                        {
                            addOutputEndpoints(gtb);
                            addInputEndpoints(gtb);
                        }
                    }

                    std::wstring endpointKey = endpointId.c_str();

                    sessionHandle->endpointDevicesById[endpointKey] = std::move(endpointDevicesInfo);

                    if (sessionHandle->initialEnumerationCompleted.load() == 0)
                        return;

                    for (auto* inputEndpointInfo : inputEndpointsInfo)
                    {
                        sessionHandle->inputEndpointCallback(inputEndpointInfo, SESSION_CALLBACKOPERATION_ENDPOINTADDED);
                    }

                    for (auto* outputEndpointInfo : outputEndpointsInfo)
                    {
                        sessionHandle->outputEndpointCallback(outputEndpointInfo, SESSION_CALLBACKOPERATION_ENDPOINTADDED);
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

                    for (auto* inputEndpointInfo : endpointDevicesInfo.inputEndpointsInfo)
                    {
                        sessionHandle->inputEndpointCallback(inputEndpointInfo, SESSION_CALLBACKOPERATION_ENDPOINTREMOVED);
                    }

                    for (auto* outputEndpointInfo : endpointDevicesInfo.outputEndpointsInfo)
                    {
                        sessionHandle->outputEndpointCallback(outputEndpointInfo, SESSION_CALLBACKOPERATION_ENDPOINTREMOVED);
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
        try
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
        catch (...)
        {
        }
    }

    {
        std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

        for (auto& pair : sessionHandle->endpointDevicesById)
        {
            DeleteInputEndpointInfos(pair.second.inputEndpointsInfo);
            DeleteOutputEndpointInfos(pair.second.outputEndpointsInfo);
        }

        sessionHandle->endpointDevicesById.clear();
    }

    if (sessionHandle->session != nullptr)
    {
        std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

        try
        {
            for (auto const& pair : sessionHandle->endpointConnectionsById)
            {
                sessionHandle->session.DisconnectEndpointConnection(pair.Value().ConnectionId());
            }

            sessionHandle->endpointConnectionsById.Clear();
            sessionHandle->session.Close();
        }
        catch (...)
        {
        }
    }

    delete sessionHandle;
    return SESSION_CLOSERESULT_OK;
}

typedef int GETCONNECTIONRESULT;

#define GETCONNECTIONRESULT_OK 0
#define GETCONNECTIONRESULT_OPENFAILED 1
#define GETCONNECTIONRESULT_UNKNOWNERROR 2

GETCONNECTIONRESULT GetConnection(SessionHandle* sessionHandle, EndpointInfoBase* info, midi2::MidiEndpointConnection* connection)
{
    try
    {
        if (!sessionHandle->endpointConnectionsById.HasKey(info->endpointDeviceId))
        {
            *connection = sessionHandle->session.CreateEndpointConnection(info->endpointDeviceId);

            sessionHandle->endpointConnectionsById.Insert(info->endpointDeviceId, *connection);

            if (!connection->Open())
                return GETCONNECTIONRESULT_OPENFAILED;
        }

        *connection = sessionHandle->endpointConnectionsById.Lookup(info->endpointDeviceId);
        return GETCONNECTIONRESULT_OK;
    }
    catch (const winrt::hresult_error& e)
    {
        return GETCONNECTIONRESULT_UNKNOWNERROR;
    }
    catch (const std::exception& e)
    {
        return GETCONNECTIONRESULT_UNKNOWNERROR;
    }
    catch (...)
    {
        return GETCONNECTIONRESULT_UNKNOWNERROR;
    }
}

/* ================================
   Input device
================================ */

struct InputEndpointHandle : EndpointHandleBase
{
    InputEndpointInfo* info;
    HMIDIIN handle;
    LPMIDIHDR* sysExHeaders;
    int sysExBufferCount;
    int sysExBufferSize;
    LONG isClosing;
    MidiGroupEndpointListener groupListener{nullptr};
    winrt::event_token revokeOnGroupListener;
};

typedef void (*BytesReceivedCallback)(const uint8_t* bytes, int size);

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

    if (configuration->wmsInitialized)
    {
        while (sessionHandle->initialEnumerationCompleted.load() == 0)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
        }

        std::vector<InputEndpointInfo*> inputEndpointsInfo;
        std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

        auto cleanupInputDevices = [&inputEndpointsInfo]()
        {
            for (auto& info : inputEndpointsInfo)
            {
                DeleteInputEndpointInfo(info);
            }
        };

        try
        {
            for (const auto& pair : sessionHandle->endpointDevicesById)
            {
                const EndpointDevicesInfo& endpointDevicesInfo = pair.second;

                for (auto* inputEndpointInfo : endpointDevicesInfo.inputEndpointsInfo)
                {
                    InputEndpointInfo* infoCopy = nullptr;
                    CloneInputEndpointInfo(inputEndpointInfo, &infoCopy);
                    inputEndpointsInfo.push_back(infoCopy);
                }
            }

            auto count = inputEndpointsInfo.size();
            auto result = new InputEndpointInfo * [count];

            for (auto i = 0; i < count; i++)
            {
                result[i] = inputEndpointsInfo[i];
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

    InputEndpointInfo** result = new InputEndpointInfo * [*devicesCount];

    for (int i = 0; i < *devicesCount; i++)
    {
        InputEndpointInfo* inputEndpointInfo;

        auto getInputEndpointInfoResult = GetInputEndpointInfo(i, &inputEndpointInfo, errorCode);
        if (getInputEndpointInfoResult != IN_GETINFORESULT_OK)
        {
            for (int j = 0; j < i; j++)
            {
                DeleteInputEndpointInfo(result[j]);
            }

            delete[] result;

            return ConvertToGetAllInInfoResult(getInputEndpointInfoResult);
        }

        result[i] = inputEndpointInfo;
    }

    *devicesInfo = result;

    return IN_GETALLINFORESULT_OK;
}

API_EXPORT void API_CALL FreeInputEndpointsInfo(InputEndpointInfo** devicesInfo, int devicesCount)
{
    delete[] devicesInfo;
}

API_EXPORT IN_GETPROPERTYRESULT API_CALL GetInputEndpointName(InputEndpointInfo* info, const wchar_t** value, int* errorCode)
{
    *errorCode = 0;

    if (!info->endpointDeviceId.empty() && info->gtb != nullptr)
    {
        *value = info->gtb.Name().c_str();
        return IN_GETPROPERTYRESULT_OK;
    }

    *value = info->caps->szPname;
    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_GETPROPERTYRESULT API_CALL GetInputEndpointId_Win(InputEndpointInfo* info, const wchar_t** value, int* errorCode)
{
    *errorCode = 0;
    *value = L"";

    if (info == nullptr)
        return IN_GETPROPERTYRESULT_OK;

    if (!info->endpointDeviceId.empty() && info->gtb != nullptr && info->group != nullptr)
        info->endpointId = info->endpointDeviceId + L"_" + std::to_wstring(info->gtb.Number()) + L"_" + std::to_wstring(info->group.Index());
    else if (!info->devicePath.empty())
        info->endpointId = info->devicePath + L"_" + info->caps->szPname;
    else
        info->endpointId = info->caps->szPname;

    *value = info->endpointId.c_str();

    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_RENEWSYSEXBUFFERRESULT API_CALL RenewInputEndpointSysExBuffer(InputEndpointHandle* inputEndpointHandle, void* headerPointer, int* errorCode)
{
    *errorCode = 0;

    LPMIDIHDR header = static_cast<LPMIDIHDR>(headerPointer);

    if (header == nullptr)
        return IN_RENEWSYSEXBUFFERRESULT_INVALIDHEADER;

    std::lock_guard<std::mutex> lock(inputEndpointHandle->sessionHandle->endpointDevicesLock);

    if (inputEndpointHandle->isClosing)
        return IN_RENEWSYSEXBUFFERRESULT_CLOSING;

    bool found = false;
    for (int i = 0; i < inputEndpointHandle->sysExBufferCount; i++)
    {
        if (inputEndpointHandle->sysExHeaders[i] == header)
        {
            found = true;
            break;
        }
    }

    if (!found)
        return IN_RENEWSYSEXBUFFERRESULT_INVALIDHEADER;

    if ((header->dwFlags & MHDR_DONE) == 0)
        return IN_RENEWSYSEXBUFFERRESULT_BUFFERNOTDONE;

    header->dwFlags &= MHDR_PREPARED;
    header->dwBytesRecorded = 0;

    const int maxRetries = 5;
    const int retryDelayMs = 10;
    MMRESULT result;

    for (int retry = 0; retry < maxRetries; retry++)
    {
        if (inputEndpointHandle->isClosing)
            return IN_RENEWSYSEXBUFFERRESULT_CLOSING;

        result = midiInAddBuffer(inputEndpointHandle->handle, header, sizeof(MIDIHDR));

        if (result == MMSYSERR_NOERROR)
            return IN_RENEWSYSEXBUFFERRESULT_OK;

        if (result != MIDIERR_STILLPLAYING)
            break;

        Sleep(retryDelayMs);
    }

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

API_EXPORT IN_OPENRESULT API_CALL OpenInputEndpoint_Win(InputEndpointInfo* info, SessionHandle* sessionHandle, DWORD_PTR callback, BytesReceivedCallback bytesReceivedCallback, int sysExBufferSize, int sysExBufferCount, InputEndpointHandle** handle, int* errorCode)
{
    *errorCode = 0;

    EnsureWinMmPortsAvailable();

    InputEndpointInfo* inputEndpointInfo = info;
    InputEndpointHandle* inputEndpointHandle = new InputEndpointHandle();
    inputEndpointHandle->info = inputEndpointInfo;
    inputEndpointHandle->sessionHandle = sessionHandle;

    if (!info->endpointDeviceId.empty() && info->gtb != nullptr && info->group != nullptr)
    {
        std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

        try
        {
            inputEndpointHandle->groupListener = MidiGroupEndpointListener();
            inputEndpointHandle->groupListener.IncludedGroups().Append(midi2::MidiGroup(static_cast<uint8_t>(info->group.Index())));
            inputEndpointHandle->groupListener.PreventCallingFurtherListeners(false);
            inputEndpointHandle->groupListener.PreventFiringMainMessageReceivedEvent(true);
            inputEndpointHandle->groupListener.PluginName(L"Group listener " + std::to_wstring(info->group.Index()));
            inputEndpointHandle->groupListener.IsEnabled(false);

            auto MessageReceivedHandler = [bytesReceivedCallback](midi2::IMidiMessageReceivedEventSource const& sender, midi2::MidiMessageReceivedEventArgs const& args)
            {
                auto ump = args.GetMessagePacket();
                auto words = messages::MidiMessageConverter::ConvertSingleGroupCompleteMessageUmpWordsToMidi1Bytes(ump.GetAllWords());

                int byteCount = static_cast<int>(words.Size());
                uint8_t* bytes = new uint8_t[byteCount];
                std::vector<uint8_t> temp(byteCount);
                words.GetMany(0, temp);
                std::memcpy(bytes, temp.data(), byteCount);

                bytesReceivedCallback(bytes, byteCount);

                delete[] bytes;
            };

            inputEndpointHandle->revokeOnGroupListener = inputEndpointHandle->groupListener.MessageReceived(MessageReceivedHandler);

            midi2::MidiEndpointConnection connection{nullptr};
            auto getConnectionResult = GetConnection(sessionHandle, info, &connection);
            if (getConnectionResult != GETCONNECTIONRESULT_OK)
            {
                delete inputEndpointHandle;

                switch (getConnectionResult)
                {
                    case GETCONNECTIONRESULT_OPENFAILED: return IN_OPENRESULT_GETCONNECTION_OPENFAILED;
                    default: return IN_OPENRESULT_GETCONNECTION_UNKNOWNERROR;
                }
            }

            connection.AddMessageProcessingPlugin(inputEndpointHandle->groupListener);

            sessionHandle->endpointConnectionsUsersCounts[connection.ConnectionId()]++;
            inputEndpointHandle->connection = connection;

            *handle = inputEndpointHandle;
            return IN_OPENRESULT_OK;
        }
        catch (const winrt::hresult_error& e)
        {
            return IN_OPENRESULT_UNKNOWNWMSERROR;
        }
        catch (const std::exception& e)
        {
            return IN_OPENRESULT_UNKNOWNWMSERROR;
        }
        catch (...)
        {
            return IN_OPENRESULT_UNKNOWNWMSERROR;
        }
    }

    inputEndpointHandle->sysExBufferSize = sysExBufferSize;
    inputEndpointHandle->sysExBufferCount = sysExBufferCount;

    inputEndpointHandle->sysExHeaders = new LPMIDIHDR[sysExBufferCount];
    for (int i = 0; i < sysExBufferCount; i++)
    {
        inputEndpointHandle->sysExHeaders[i] = nullptr;
    }

    std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

    inputEndpointHandle->isClosing = 0;

    auto deviceIndex = inputEndpointInfo->deviceIndex;

    MMRESULT result = midiInOpen(&inputEndpointHandle->handle, deviceIndex, callback, 0, CALLBACK_FUNCTION);
    if (result != MMSYSERR_NOERROR)
    {
        delete[] inputEndpointHandle->sysExHeaders;
        delete inputEndpointHandle;

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
        IN_PREPARESYSEXBUFFERRESULT prepareResult = PrepareSysExBuffer(inputEndpointHandle->handle, sysExBufferSize, &inputEndpointHandle->sysExHeaders[i], &prepareErrorCode);

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
            inputEndpointHandle->sysExHeaders[i] = nullptr;
            continue;
        }

        preparedBuffersCount++;
    }

    if (preparedBuffersCount == 0)
    {
        midiInClose(inputEndpointHandle->handle);
        delete[] inputEndpointHandle->sysExHeaders;
        delete inputEndpointHandle;
        return IN_OPENRESULT_FAILEDPREPARESYSEXBUFFERS;
    }

    *handle = inputEndpointHandle;

    return IN_OPENRESULT_OK;
}

API_EXPORT IN_CLOSERESULT API_CALL CloseInputEndpoint(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputEndpointHandle* inputEndpointHandle = static_cast<InputEndpointHandle*>(handle);
    auto sessionHandle = inputEndpointHandle->sessionHandle;

    if (inputEndpointHandle->groupListener != nullptr)
    {
        std::lock_guard<std::mutex> lock(inputEndpointHandle->sessionHandle->endpointDevicesLock);

        try
        {
            if (inputEndpointHandle->groupListener != nullptr)
                inputEndpointHandle->groupListener.MessageReceived(inputEndpointHandle->revokeOnGroupListener);

            auto connectionId = inputEndpointHandle->connection.ConnectionId();

            sessionHandle->endpointConnectionsUsersCounts[connectionId]--;
            if (sessionHandle->endpointConnectionsUsersCounts[connectionId] == 0)
            {
                sessionHandle->session.DisconnectEndpointConnection(connectionId);
                sessionHandle->endpointConnectionsById.Remove(inputEndpointHandle->info->endpointDeviceId);
            }
        }
        catch (...)
        {
            return IN_CLOSERESULT_UNKNOWNWMSERROR;
        }

        return IN_CLOSERESULT_OK;
    }

    std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

    inputEndpointHandle->isClosing = 1;

    auto cleanupSysExHeaders = [&inputEndpointHandle]()
    {
        for (int i = 0; i < inputEndpointHandle->sysExBufferCount; i++)
        {
            if (inputEndpointHandle->sysExHeaders[i] == nullptr)
                continue;

            LPMIDIHDR header = inputEndpointHandle->sysExHeaders[i];
            midiInUnprepareHeader(inputEndpointHandle->handle, header, sizeof(MIDIHDR));

            delete[] header->lpData;
            delete header;

            inputEndpointHandle->sysExHeaders[i] = nullptr;
        }

        delete[] inputEndpointHandle->sysExHeaders;
    };

    MMRESULT result = midiInReset(inputEndpointHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        *errorCode = result;

        cleanupSysExHeaders();
        midiInClose(inputEndpointHandle->handle);
        delete inputEndpointHandle;

        switch (result)
        {
            case MMSYSERR_INVALHANDLE: return IN_CLOSERESULT_RESET_INVALIDHANDLE;
        }

        return IN_CLOSERESULT_RESET_UNKNOWNERROR;
    }

    cleanupSysExHeaders();

    result = midiInClose(inputEndpointHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        *errorCode = result;

        delete inputEndpointHandle;

        switch (result)
        {
            case MIDIERR_STILLPLAYING: return IN_CLOSERESULT_CLOSE_STILLPLAYING;
            case MMSYSERR_INVALHANDLE: return IN_CLOSERESULT_CLOSE_INVALIDHANDLE;
            case MMSYSERR_NOMEM: return IN_CLOSERESULT_CLOSE_NOMEMORY;
        }

        return IN_CLOSERESULT_CLOSE_UNKNOWNERROR;
    }

    delete inputEndpointHandle;

    return IN_CLOSERESULT_OK;
}

API_EXPORT IN_CONNECTRESULT API_CALL ConnectToInputEndpoint(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputEndpointHandle* inputEndpointHandle = static_cast<InputEndpointHandle*>(handle);

    if (inputEndpointHandle->groupListener != nullptr)
    {
        inputEndpointHandle->groupListener.IsEnabled(true);
        return IN_CONNECTRESULT_OK;
    }

    MMRESULT result = midiInStart(inputEndpointHandle->handle);
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

    InputEndpointHandle* inputEndpointHandle = static_cast<InputEndpointHandle*>(handle);

    if (inputEndpointHandle->groupListener != nullptr)
    {
        inputEndpointHandle->groupListener.IsEnabled(false);
        return IN_DISCONNECTRESULT_OK;
    }

    MMRESULT result = midiInStop(inputEndpointHandle->handle);
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

struct OutputEndpointHandle : EndpointHandleBase
{
    OutputEndpointInfo* info;
    HMIDIOUT handle;
};

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

API_EXPORT OUT_GETALLINFORESULT API_CALL GetOutputEndpointsInfo_Win(
    Configuration* configuration,
    SessionHandle* sessionHandle,
    bool forceWinMM,
    OutputEndpointInfo*** devicesInfo,
    int* devicesCount,
    int* errorCode)
{
    *errorCode = 0;

    EnsureWinMmPortsAvailable();

    if (!forceWinMM && configuration->wmsInitialized)
    {
        while (sessionHandle->initialEnumerationCompleted.load() == 0)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
        }

        std::vector<OutputEndpointInfo*> outputEndpointsInfo;
        std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

        auto cleanupOutputDevices = [&outputEndpointsInfo]()
        {
            for (auto& info : outputEndpointsInfo)
            {
                DeleteOutputEndpointInfo(info);
            }
        };

        int initialCount = 0;
        GetOutputEndpointsCount(&initialCount);

        for (int i = 0; i < initialCount; i++)
        {
            OutputEndpointInfo* outputEndpointInfo;

            auto getOutputEndpointInfoResult = GetOutputEndpointInfo(i, &outputEndpointInfo, errorCode);
            if (getOutputEndpointInfoResult != OUT_GETINFORESULT_OK)
            {
                cleanupOutputDevices();
                return ConvertToGetAllOutInfoResult(getOutputEndpointInfoResult);
            }

            if (wcscmp(outputEndpointInfo->caps->szPname, L"Microsoft GS Wavetable Synth") == 0)
            {
                outputEndpointInfo->isMicrosoftGsWavetableSynth = true;
                outputEndpointsInfo.push_back(outputEndpointInfo);
                break;
            }

            DeleteOutputEndpointInfo(outputEndpointInfo);
        }

        try
        {
            for (const auto& pair : sessionHandle->endpointDevicesById)
            {
                const EndpointDevicesInfo& endpointDevicesInfo = pair.second;

                for (auto* outputEndpointInfo : endpointDevicesInfo.outputEndpointsInfo)
                {
                    OutputEndpointInfo* infoCopy = nullptr;
                    CloneOutputEndpointInfo(outputEndpointInfo, &infoCopy);
                    outputEndpointsInfo.push_back(infoCopy);
                }
            }

            auto count = outputEndpointsInfo.size();
            OutputEndpointInfo** result = new OutputEndpointInfo * [count];

            for (size_t i = 0; i < count; i++)
            {
                result[i] = outputEndpointsInfo[i];
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
        OutputEndpointInfo* outputEndpointInfo;
        int errorCode;

        auto getOutputEndpointInfoResult = GetOutputEndpointInfo(i, &outputEndpointInfo, &errorCode);
        if (getOutputEndpointInfoResult != OUT_GETINFORESULT_OK)
        {
            for (int j = 0; j < i; j++)
            {
                DeleteOutputEndpointInfo(result[j]);
            }

            delete[] result;

            return ConvertToGetAllOutInfoResult(getOutputEndpointInfoResult);
        }

        if (wcscmp(outputEndpointInfo->caps->szPname, L"Microsoft GS Wavetable Synth") == 0)
            outputEndpointInfo->isMicrosoftGsWavetableSynth = true;

        result[i] = outputEndpointInfo;
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

    if (!info->endpointDeviceId.empty() && info->gtb != nullptr)
    {
        *value = info->gtb.Name().c_str();
        return IN_GETPROPERTYRESULT_OK;
    }

    *value = info->caps->szPname;
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputEndpointId_Win(OutputEndpointInfo* info, const wchar_t** value, int* errorCode)
{
    *errorCode = 0;
    *value = L"";

    if (info == nullptr)
        return OUT_GETPROPERTYRESULT_OK;

    if (!info->endpointDeviceId.empty() && info->gtb != nullptr && info->group != nullptr)
        info->endpointId = info->endpointDeviceId + L"_" + std::to_wstring(info->gtb.Number()) + L"_" + std::to_wstring(info->group.Index());
    else if (!info->devicePath.empty())
        info->endpointId = info->devicePath + L"_" + info->caps->szPname;
    else if (info->isMicrosoftGsWavetableSynth)
        info->endpointId = L"Microsoft_GS_Wavetable_Synth";
    else
        info->endpointId = info->caps->szPname;

    *value = info->endpointId.c_str();

    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_OPENRESULT API_CALL OpenOutputEndpoint_Win(OutputEndpointInfo* info, SessionHandle* sessionHandle, DWORD_PTR callback, OutputEndpointHandle** handle, int* errorCode)
{
    *errorCode = 0;

    EnsureWinMmPortsAvailable();

    OutputEndpointHandle* outputEndpointHandle = new OutputEndpointHandle();
    outputEndpointHandle->info = info;
    outputEndpointHandle->sessionHandle = sessionHandle;

    if (!info->endpointDeviceId.empty() && info->gtb != nullptr && info->group != nullptr)
    {
        std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

        try
        {
            midi2::MidiEndpointConnection connection{ nullptr };
            auto getConnectionResult = GetConnection(sessionHandle, info, &connection);
            if (getConnectionResult != GETCONNECTIONRESULT_OK)
            {
                delete outputEndpointHandle;

                switch (getConnectionResult)
                {
                    case GETCONNECTIONRESULT_OPENFAILED: return OUT_OPENRESULT_GETCONNECTION_OPENFAILED;
                    default: return OUT_OPENRESULT_GETCONNECTION_UNKNOWNERROR;
                }
            }

            sessionHandle->endpointConnectionsUsersCounts[connection.ConnectionId()]++;
            outputEndpointHandle->connection = connection;

            *handle = outputEndpointHandle;
            return OUT_OPENRESULT_OK;
        }
        catch (const winrt::hresult_error& e)
        {
            return OUT_OPENRESULT_UNKNOWNWMSERROR;
        }
        catch (const std::exception& e)
        {
            return OUT_OPENRESULT_UNKNOWNWMSERROR;
        }
        catch (...)
        {
            return OUT_OPENRESULT_UNKNOWNWMSERROR;
        }
    }

    auto deviceIndex = info->deviceIndex;

    HMIDIOUT outHandle;
    MMRESULT result = midiOutOpen(&outHandle, deviceIndex, callback, 0, CALLBACK_FUNCTION);
    if (result != MMSYSERR_NOERROR)
    {
        delete outputEndpointHandle;

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

    outputEndpointHandle->handle = outHandle;

    *handle = outputEndpointHandle;

    return OUT_OPENRESULT_OK;
}

API_EXPORT OUT_CLOSERESULT API_CALL CloseOutputEndpoint(void* handle, int* errorCode)
{
    *errorCode = 0;

    OutputEndpointHandle* outputEndpointHandle = static_cast<OutputEndpointHandle*>(handle);

    if (outputEndpointHandle->connection != nullptr)
    {
        std::lock_guard<std::mutex> lock(outputEndpointHandle->sessionHandle->endpointDevicesLock);

        try
        {
            auto connectionId = outputEndpointHandle->connection.ConnectionId();
            auto sessionHandle = outputEndpointHandle->sessionHandle;

            sessionHandle->endpointConnectionsUsersCounts[connectionId]--;
            if (sessionHandle->endpointConnectionsUsersCounts[connectionId] == 0)
            {
                sessionHandle->session.DisconnectEndpointConnection(connectionId);
                sessionHandle->endpointConnectionsById.Remove(outputEndpointHandle->info->endpointDeviceId);
            }
        }
        catch (...)
        {
            return OUT_CLOSERESULT_UNKNOWNWMSERROR;
        }

        return OUT_CLOSERESULT_OK;
    }

    MMRESULT result = midiOutReset(outputEndpointHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        *errorCode = result;

        midiOutClose(outputEndpointHandle->handle);
        delete outputEndpointHandle;

        switch (result)
        {
            case MMSYSERR_INVALHANDLE: return OUT_CLOSERESULT_RESET_INVALIDHANDLE;
        }

        return OUT_CLOSERESULT_RESET_UNKNOWNERROR;
    }

    result = midiOutClose(outputEndpointHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        *errorCode = result;

        delete outputEndpointHandle;

        switch (result)
        {
            case MIDIERR_STILLPLAYING: return OUT_CLOSERESULT_CLOSE_STILLPLAYING;
            case MMSYSERR_INVALHANDLE: return OUT_CLOSERESULT_CLOSE_INVALIDHANDLE;
            case MMSYSERR_NOMEM: return OUT_CLOSERESULT_CLOSE_NOMEMORY;
        }

        return OUT_CLOSERESULT_CLOSE_UNKNOWNERROR;
    }

    delete outputEndpointHandle;

    return OUT_CLOSERESULT_OK;
}

API_EXPORT OUT_SENDSHORTRESULT API_CALL SendShortEventToOutputEndpoint(void* handle, SessionHandle* sessionHandle, int message, int* errorCode)
{
    *errorCode = 0;

    OutputEndpointHandle* outputEndpointHandle = static_cast<OutputEndpointHandle*>(handle);

    if (outputEndpointHandle->info->group != nullptr)
    {
        std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

        try
        {
            auto connection = sessionHandle->endpointConnectionsById.Lookup(outputEndpointHandle->info->endpointDeviceId);

            auto ump = messages::MidiMessageConverter::ConvertMidi1Message(
                midi2::MidiClock::TimestampConstantSendImmediately(),
                outputEndpointHandle->info->group,
                message & 0xFF,
                (message >> 8) & 0xFF,
                (message >> 16) & 0xFF);

            auto result = connection.SendSingleMessagePacket(ump);
            if (result != midi2::MidiSendMessageResults::Succeeded)
            {
                *errorCode = static_cast<int>(result);
                return OUT_SENDSHORTRESULT_SENDERROR;
            }

            return OUT_SENDSHORTRESULT_OK;
        }
        catch (const winrt::hresult_error& e)
        {
            return OUT_SENDSHORTRESULT_UNKNOWNWMSERROR;
        }
        catch (const std::exception& e)
        {
            return OUT_SENDSHORTRESULT_UNKNOWNWMSERROR;
        }
        catch (...)
        {
            return OUT_SENDSHORTRESULT_UNKNOWNWMSERROR;
        }
    }

    MMRESULT result = midiOutShortMsg(outputEndpointHandle->handle, (DWORD)message);
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

API_EXPORT OUT_SENDSYSEXRESULT API_CALL SendSysExEventToOutputEndpoint_Win(void* handle, SessionHandle* sessionHandle, LPSTR data, int size, int* errorCode)
{
    *errorCode = 0;

    OutputEndpointHandle* outputEndpointHandle = static_cast<OutputEndpointHandle*>(handle);

    if (outputEndpointHandle->info->group != nullptr)
    {
        std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

        try
        {
            auto connection = sessionHandle->endpointConnectionsById.Lookup(outputEndpointHandle->info->endpointDeviceId);

            std::vector<uint8_t> bytes(
                reinterpret_cast<uint8_t*>(data),
                reinterpret_cast<uint8_t*>(data) + size);

            winrt::Windows::Storage::Streams::InMemoryRandomAccessStream memoryStream;
            winrt::Windows::Storage::Streams::DataWriter writer(memoryStream);
            writer.WriteBytes(bytes);
            writer.StoreAsync().get();
            winrt::Windows::Storage::Streams::IInputStream inputStream = memoryStream.GetInputStreamAt(0);

            messages::MidiBytestreamToUmpMessageConverterState converterState;

            auto result = sysex::MidiSystemExclusiveSender::SendBinarySysEx7ByteDataAsync(
                connection,
                outputEndpointHandle->info->group,
                inputStream,
                0,
                0,
                converterState).get();

            if (!result)
            {
                *errorCode = static_cast<int>(result);
                return OUT_SENDSYSEXRESULT_SENDERROR;
            }

            return OUT_SENDSYSEXRESULT_OK;
        }
        catch (const winrt::hresult_error& e)
        {
            return OUT_SENDSYSEXRESULT_UNKNOWNWMSERROR;
        }
        catch (const std::exception& e)
        {
            return OUT_SENDSYSEXRESULT_UNKNOWNWMSERROR;
        }
        catch (...)
        {
            return OUT_SENDSYSEXRESULT_UNKNOWNWMSERROR;
        }
    }

    LPMIDIHDR header = new MIDIHDR();
    header->lpData = data;
    header->dwBufferLength = size;
    header->dwBytesRecorded = size;
    header->dwFlags = 0;

    MMRESULT result = midiOutPrepareHeader(outputEndpointHandle->handle, header, sizeof(MIDIHDR));
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

    result = midiOutLongMsg(outputEndpointHandle->handle, header, sizeof(MIDIHDR));
    if (result != MMSYSERR_NOERROR)
    {
        midiOutUnprepareHeader(outputEndpointHandle->handle, header, sizeof(MIDIHDR));
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

    OutputEndpointHandle* outputEndpointHandle = static_cast<OutputEndpointHandle*>(handle);

    MMRESULT result = midiOutUnprepareHeader(outputEndpointHandle->handle, header, sizeof(MIDIHDR));
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
    InputEndpointInfo* inputEndpointInfo = nullptr;
    OutputEndpointInfo* outputEndpointInfo = nullptr;
    const wchar_t* name;
    winrt::guid associationId;
    std::wstring endpointDeviceId;
};

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

    if (!configuration->wmsInitialized)
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
        if (virtualDeviceInfo->inputEndpointInfo != nullptr)
            DeleteInputEndpointInfo(virtualDeviceInfo->inputEndpointInfo);

        if (virtualDeviceInfo->outputEndpointInfo != nullptr)
            DeleteOutputEndpointInfo(virtualDeviceInfo->outputEndpointInfo);

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
        auto groupTerminalBlocks = endpointInformation.GetGroupTerminalBlocks();

        for (auto const& gtb : groupTerminalBlocks)
        {
            auto const group = gtb.FirstGroup();
            auto direction = gtb.Direction();

            if (direction == midi2::Enumeration::MidiGroupTerminalBlockDirection::BlockInput)
            {
                OutputEndpointInfo* outputEndpointInfo = nullptr;
                int errorCode;

                auto getOutputEndpointInfoResult = GetOutputEndpointInfo(endpointId, gtb, group, &outputEndpointInfo, &errorCode);
                if (getOutputEndpointInfoResult == OUT_GETINFORESULT_OK)
                    virtualDeviceInfo->outputEndpointInfo = outputEndpointInfo;
            }
            else if (direction == midi2::Enumeration::MidiGroupTerminalBlockDirection::BlockOutput)
            {
                InputEndpointInfo* inputEndpointInfo = nullptr;
                int errorCode;

                auto getInputEndpointInfoResult = GetInputEndpointInfo(endpointId, gtb, group, &inputEndpointInfo, &errorCode);
                if (getInputEndpointInfoResult == IN_GETINFORESULT_OK)
                    virtualDeviceInfo->inputEndpointInfo = inputEndpointInfo;
            }
            else if (direction == midi2::Enumeration::MidiGroupTerminalBlockDirection::Bidirectional)
            {
                int errorCode;

                OutputEndpointInfo* outputEndpointInfo = nullptr;

                auto getOutputEndpointInfoResult = GetOutputEndpointInfo(endpointId, gtb, group, &outputEndpointInfo, &errorCode);
                if (getOutputEndpointInfoResult == OUT_GETINFORESULT_OK)
                    virtualDeviceInfo->outputEndpointInfo = outputEndpointInfo;

                InputEndpointInfo* inputEndpointInfo = nullptr;

                auto getInputEndpointInfoResult = GetInputEndpointInfo(endpointId, gtb, group, &inputEndpointInfo, &errorCode);
                if (getInputEndpointInfoResult == IN_GETINFORESULT_OK)
                    virtualDeviceInfo->inputEndpointInfo = inputEndpointInfo;
            }
        }

        if (virtualDeviceInfo->inputEndpointInfo == nullptr)
        {
            // TODO: error
        }

        if (virtualDeviceInfo->outputEndpointInfo == nullptr)
        {
            // TODO: error
        }
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
        auto result = basicLoopback::MidiBasicLoopbackManager::RemoveTransientLoopback(
            basicLoopback::MidiBasicLoopbackRemovalConfig{ info->associationId });

        if (!result.Success())
        {
            *errorCode = static_cast<int>(result.ErrorCode());
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
    return info->inputEndpointInfo;
}

API_EXPORT OutputEndpointInfo* API_CALL GetOutputEndpointInfoFromVirtualDevice(VirtualDeviceInfo* info)
{
    return info->outputEndpointInfo;
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