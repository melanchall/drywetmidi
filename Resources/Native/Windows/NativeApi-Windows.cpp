#pragma comment(lib, "winmm.lib")

#ifndef NOMINMAX
#define NOMINMAX
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

/* ================================
   Common
================================ */

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

API_EXPORT char API_CALL CanCompareDevices()
{
    return 0;
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

typedef struct
{
    int deviceIndex;
    LPMIDIINCAPSA caps;
} InputDeviceInfo;

API_EXPORT IN_GETCOUNTRESULT API_CALL GetInputDevicesCount(int* count)
{
    *count = midiInGetNumDevs();
    return IN_GETCOUNTRESULT_OK;
}

API_EXPORT IN_GETINFORESULT API_CALL GetInputDeviceInfo(int deviceIndex, InputDeviceInfo** info, int* errorCode)
{
    *errorCode = 0;

    InputDeviceInfo* inputDeviceInfo = new InputDeviceInfo();

    inputDeviceInfo->deviceIndex = deviceIndex;
    inputDeviceInfo->caps = new MIDIINCAPSA();

    MMRESULT result = midiInGetDevCapsA(deviceIndex, inputDeviceInfo->caps, sizeof(MIDIINCAPSA));
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

    *info = inputDeviceInfo;

    return IN_GETINFORESULT_OK;
}

API_EXPORT void API_CALL DeleteInputDeviceInfo(InputDeviceInfo* info)
{
    delete info->caps;
    delete info;
}

typedef struct
{
    int deviceIndex;
    LPMIDIOUTCAPSA caps;
} OutputDeviceInfo;

API_EXPORT OUT_GETCOUNTRESULT API_CALL GetOutputDevicesCount(int* count)
{
    *count = midiOutGetNumDevs();
    return OUT_GETCOUNTRESULT_OK;
}

API_EXPORT OUT_GETINFORESULT API_CALL GetOutputDeviceInfo(int deviceIndex, OutputDeviceInfo** info, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceInfo* outputDeviceInfo = new OutputDeviceInfo();

    outputDeviceInfo->deviceIndex = deviceIndex;
    outputDeviceInfo->caps = new MIDIOUTCAPSA();

    MMRESULT result = midiOutGetDevCapsA(deviceIndex, outputDeviceInfo->caps, sizeof(MIDIOUTCAPSA));
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

    *info = outputDeviceInfo;

    return OUT_GETINFORESULT_OK;
}

API_EXPORT void API_CALL DeleteOutputDeviceInfo(OutputDeviceInfo* info)
{
    delete info->caps;
    delete info;
}

API_EXPORT const char* API_CALL GetDeviceManufacturer(WORD manufacturerId)
{
    // https://docs.microsoft.com/en-us/windows/win32/multimedia/manufacturer-identifiers
    switch (manufacturerId)
    {
        case MM_GRAVIS: return "Advanced Gravis Computer Technology, Ltd.";
        case MM_ANTEX: return "Antex Electronics Corporation";
        case MM_APPS: return "APPS Software";
        case MM_ARTISOFT: return "Artisoft, Inc.";
        case MM_AST: return "AST Research, Inc.";
        case MM_ATI: return "ATI Technologies, Inc.";
        case MM_AUDIOFILE: return "Audio, Inc.";
        case MM_APT: return "Audio Processing Technology";
        case MM_AUDIOPT: return "Audio Processing Technology";
        case MM_AURAVISION: return "Auravision Corporation";
        case MM_AZTECH: return "Aztech Labs, Inc.";
        case MM_CANOPUS: return "Canopus, Co., Ltd.";
        case MM_COMPUSIC: return "Compusic";
        case MM_CAT: return "Computer Aided Technology, Inc.";
        case MM_COMPUTER_FRIENDS: return "Computer Friends, Inc.";
        case MM_CONTROLRES: return "Control Resources Corporation";
        case MM_CREATIVE: return "Creative Labs, Inc.";
        case MM_DIALOGIC: return "Dialogic Corporation";
        case MM_DOLBY: return "Dolby Laboratories, Inc.";
        case MM_DSP_GROUP: return "DSP Group, Inc.";
        case MM_DSP_SOLUTIONS: return "DSP Solutions, Inc.";
        case MM_ECHO: return "Echo Speech Corporation";
        case MM_ESS: return "ESS Technology, Inc.";
        case MM_EVEREX: return "Everex Systems, Inc.";
        case MM_EXAN: return "EXAN, Ltd.";
        case MM_FUJITSU: return "Fujitsu, Ltd.";
        case MM_IOMAGIC: return "I/O Magic Corporation";
        case MM_ICL_PS: return "ICL Personal Systems";
        case MM_OLIVETTI: return "Ing. C. Olivetti & C., S.p.A.";
        case MM_ICS: return "Integrated Circuit Systems, Inc.";
        case MM_INTEL: return "Intel Corporation";
        case MM_INTERACTIVE: return "InterActive, Inc.";
        case MM_IBM: return "International Business Machines";
        case MM_ITERATEDSYS: return "Iterated Systems, Inc.";
        case MM_LOGITECH: return "Logitech, Inc.";
        case MM_LYRRUS: return "Lyrrus, Inc.";
        case MM_MATSUSHITA: return "Matsushita Electric Corporation of America";
        case MM_MEDIAVISION: return "Media Vision, Inc.";
        case MM_METHEUS: return "Metheus Corporation";
        case MM_MELABS: return "microEngineering Labs";
        case MM_MICROSOFT: return "Microsoft Corporation";
        case MM_MOSCOM: return "MOSCOM Corporation";
        case MM_MOTOROLA: return "Motorola, Inc.";
        case MM_NMS: return "Natural MicroSystems Corporation";
        case MM_NCR: return "NCR Corporation";
        case MM_NEC: return "NEC Corporation";
        case MM_NEWMEDIA: return "New Media Corporation";
        case MM_OKI: return "OKI";
        case MM_OPTI: return "OPTi, Inc.";
        case MM_ROLAND: return "Roland Corporation";
        case MM_SCALACS: return "SCALACS";
        case MM_EPSON: return "Seiko Epson Corporation, Inc.";
        case MM_SIERRA: return "Sierra Semiconductor Corporation";
        case MM_SILICONSOFT: return "Silicon Software, Inc.";
        case MM_SONICFOUNDRY: return "Sonic Foundry";
        case MM_SPEECHCOMP: return "Speech Compression";
        case MM_SUPERMAC: return "Supermac Technology, Inc.";
        case MM_TANDY: return "Tandy Corporation";
        case MM_KORG: return "Toshihiko Okuhura, Korg, Inc.";
        case MM_TRUEVISION: return "Truevision, Inc.";
        case MM_TURTLE_BEACH: return "Turtle Beach Systems";
        case MM_VAL: return "Video Associates Labs, Inc.";
        case MM_VIDEOLOGIC: return "VideoLogic, Inc.";
        case MM_VITEC: return "Visual Information Technologies, Inc.";
        case MM_VOCALTEC: return "VocalTec, Inc.";
        case MM_VOYETRA: return "Voyetra Technologies";
        case MM_WANGLABS: return "Wang Laboratories";
        case MM_WILLOWPOND: return "Willow Pond Corporation";
        case MM_WINNOV: return "Winnov, LP";
        case MM_XEBEC: return "Xebec Multimedia Solutions Limited";
        case MM_YAMAHA: return "Yamaha Corporation of America";
    }

    return "Unknown";
}

API_EXPORT const char* API_CALL GetDeviceProduct(WORD productId)
{
    // https://docs.microsoft.com/en-us/windows/win32/multimedia/microsoft-corporation-product-identifiers
    switch (productId)
    {
        case MM_ADLIB: return "Adlib-compatible synthesizer";
        case MM_MSFT_ACM_G711: return "G.711 codec";
        case MM_MSFT_ACM_GSM610: return "GSM 610 codec";
        case MM_MSFT_ACM_IMAADPCM: return "IMA ADPCM codec";
        case MM_PC_JOYSTICK: return "Joystick adapter";
        case MM_MIDI_MAPPER: return "MIDI mapper";
        case MM_MPU401_MIDIIN: return "MPU 401-compatible MIDI input port";
        case MM_MPU401_MIDIOUT: return "MPU 401-compatible MIDI output port";
        case MM_MSFT_ACM_MSADPCM: return "MS ADPCM codec";
        case MM_MSFT_WSS_FMSYNTH_STEREO: return "MS audio board stereo FM synthesizer";
        case MM_MSFT_WSS_AUX: return "MS audio board aux port";
        case MM_MSFT_WSS_MIXER: return "MS audio board mixer driver";
        case MM_MSFT_WSS_WAVEIN: return "MS audio board waveform input";
        case MM_MSFT_WSS_WAVEOUT: return "MS audio board waveform output";
        case MM_MSFT_MSACM: return "MS audio compression manager";
        case MM_MSFT_ACM_MSFILTER: return "MS filter";
        case MM_MSFT_WSS_OEM_AUX: return "MS OEM audio aux port";
        case MM_MSFT_WSS_OEM_MIXER: return "MS OEM audio board mixer driver";
        case MM_MSFT_WSS_OEM_FMSYNTH_STEREO: return "MS OEM audio board stereo FM synthesizer";
        case MM_MSFT_WSS_OEM_WAVEIN: return "MS OEM audio board waveform input";
        case MM_MSFT_WSS_OEM_WAVEOUT: return "MS OEM audio board waveform output";
        case MM_MSFT_GENERIC_AUX_CD: return "MS vanilla driver aux (CD)";
        case MM_MSFT_GENERIC_AUX_LINE: return "MS vanilla driver aux (line in)";
        case MM_MSFT_GENERIC_AUX_MIC: return "MS vanilla driver aux (mic)";
        case MM_MSFT_GENERIC_MIDIOUT: return "MS vanilla driver MIDI external out";
        case MM_MSFT_GENERIC_MIDIIN: return "MS vanilla driver MIDI in";
        case MM_MSFT_GENERIC_MIDISYNTH: return "MS vanilla driver MIDI synthesizer";
        case MM_MSFT_GENERIC_WAVEIN: return "MS vanilla driver waveform input";
        case MM_MSFT_GENERIC_WAVEOUT: return "MS vanilla driver wavefrom output";
        case MM_PCSPEAKER_WAVEOUT: return "PC speaker waveform output";
        case MM_MSFT_ACM_PCM: return "PCM converter";
        case MM_SNDBLST_SYNTH: return "Sound Blaster internal synthesizer";
        case MM_SNDBLST_MIDIIN: return "Sound Blaster MIDI input port";
        case MM_SNDBLST_MIDIOUT: return "Sound Blaster MIDI output port";
        case MM_SNDBLST_WAVEIN: return "Sound Blaster waveform input";
        case MM_SNDBLST_WAVEOUT: return "Sound Blaster waveform output";
        case MM_WAVE_MAPPER: return "Wave mapper";
    }

    // add from https://docs.microsoft.com/en-us/windows/win32/multimedia/product-identifiers
    return "Unknown";
}

void EnsureWinMmPortsAvailable()
{
    midiInGetNumDevs();
    midiOutGetNumDevs();
}

/* ================================
   Session
================================ */

typedef void (*InputDeviceCallback)(void* info, char operation);
typedef void (*OutputDeviceCallback)(void* info, char operation);

struct EndpointDevicesInfo
{
    std::vector<InputDeviceInfo*> inputDevicesInfo;
    std::vector<OutputDeviceInfo*> outputDevicesInfo;
};

struct SessionHandle
{
    char* name;

    char wmsAvailable;
    char basicLoopbackAvailable;
    std::shared_ptr<init::MidiDesktopAppSdkInitializer> wmsSdkInitializer;
    char wmsSdkInitialized;

    midi2::MidiEndpointDeviceWatcher watcher{nullptr};
    std::atomic<char> devicesWatcherEnabled{0};
    InputDeviceCallback inputDeviceCallback;
    OutputDeviceCallback outputDeviceCallback;
    winrt::event_token revokeOnWatcherDeviceRemoved;
    winrt::event_token revokeOnWatcherDeviceAdded;
    winrt::event_token revokeOnWatcherEnumerationCompleted;
    std::atomic<char> initialEnumerationCompleted{0};

    std::mutex endpointDevicesLock;
    std::unordered_map<std::wstring, EndpointDevicesInfo> endpointDevicesById;
};

void DeleteInputDeviceInfos(std::vector<InputDeviceInfo*>& deviceInfos)
{
    for (auto* inputDeviceInfo : deviceInfos)
    {
        DeleteInputDeviceInfo(inputDeviceInfo);
    }

    deviceInfos.clear();
}

void DeleteOutputDeviceInfos(std::vector<OutputDeviceInfo*>& deviceInfos)
{
    for (auto* outputDeviceInfo : deviceInfos)
    {
        DeleteOutputDeviceInfo(outputDeviceInfo);
    }

    deviceInfos.clear();
}

API_EXPORT SESSION_OPENRESULT API_CALL OpenSession(char* name, InputDeviceCallback inputDeviceCallback, OutputDeviceCallback outputDeviceCallback, void** handle, int* errorCode)
{
    *errorCode = 0;

    SessionHandle* sessionHandle = new SessionHandle();
    sessionHandle->name = name;
    sessionHandle->wmsSdkInitialized = 0;
    sessionHandle->inputDeviceCallback = inputDeviceCallback;
    sessionHandle->outputDeviceCallback = outputDeviceCallback;

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

        sessionHandle->wmsAvailable = static_cast<char>(
            comInitializationResult &&
            registryCheckResult &&
            comCheckResult &&
            (serviceCheckResult == WMSSERVICECHECKRESULT_OK) &&
            sdkCheckResult);

        if (sessionHandle->wmsAvailable)
        {
            winrt::init_apartment();

            std::shared_ptr<init::MidiDesktopAppSdkInitializer> wmsSdkInitializer = std::make_shared<init::MidiDesktopAppSdkInitializer>();

            if (wmsSdkInitializer == nullptr)
                return SESSION_OPENRESULT_CANTCREATEWMSSDKINITIALIZER;

            if (!wmsSdkInitializer->InitializeSdkRuntime())
                return SESSION_OPENRESULT_CANTINITIALIZEWMSSDK;

            if (!wmsSdkInitializer->CheckForMinimumRequiredSdkVersion(
                WINDOWS_MIDI_SERVICES_NUGET_BUILD_VERSION_MAJOR,
                WINDOWS_MIDI_SERVICES_NUGET_BUILD_VERSION_MINOR,
                WINDOWS_MIDI_SERVICES_NUGET_BUILD_VERSION_PATCH))
                return SESSION_OPENRESULT_OLDWMSSDK;

            if (!wmsSdkInitializer->EnsureServiceAvailable())
                return SESSION_OPENRESULT_WMSSERVICEUNAVAILABLE;

            sessionHandle->wmsSdkInitializer = wmsSdkInitializer;
            sessionHandle->basicLoopbackAvailable = basicLoopback::MidiBasicLoopbackEndpointManager::IsTransportAvailable();

            sessionHandle->wmsSdkInitialized = 1;

            sessionHandle->watcher = midi2::MidiEndpointDeviceWatcher::Create(midi2::MidiEndpointDeviceInformationFilters::AllStandardEndpoints);

            auto OnWatcherDeviceAdded = [sessionHandle](midi2::MidiEndpointDeviceWatcher const&, midi2::MidiEndpointDeviceInformationAddedEventArgs const& args)
            {
                if (sessionHandle->initialEnumerationCompleted.load() == 0 ||
                    sessionHandle->devicesWatcherEnabled.load() == 0)
                    return;

                std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

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

                    auto hasSources = false;
                    auto hasDestinations = false;

                    for (auto const& gtb : groupTerminalBlocks)
                    {
                        switch (gtb.Direction())
                        {
                            case midi2::MidiGroupTerminalBlockDirection::BlockInput:
                                hasDestinations = true;
                                break;
                            case midi2::MidiGroupTerminalBlockDirection::BlockOutput:
                                hasSources = true;
                                break;
                            case midi2::MidiGroupTerminalBlockDirection::Bidirectional:
                                hasSources = true;
                                hasDestinations = true;
                                break;
                        }
                    }

                    std::vector<InputDeviceInfo*> inputDevicesInfo;
                    std::vector<OutputDeviceInfo*> outputDevicesInfo;

                    std::wstring endpointKey = endpointId.c_str();
                    EndpointDevicesInfo endpointDevicesInfo;

                    auto ok = false;
                    int attempts = 0;

                    while (!ok && attempts++ < 10)
                    {
                        ok = true;

                        DeleteInputDeviceInfos(inputDevicesInfo);
                        DeleteInputDeviceInfos(endpointDevicesInfo.inputDevicesInfo);
                        DeleteOutputDeviceInfos(outputDevicesInfo);
                        DeleteOutputDeviceInfos(endpointDevicesInfo.outputDevicesInfo);

                        auto endpointInformation = midi2::MidiEndpointDeviceInformation::CreateFromEndpointDeviceId(endpointId);
                        if (endpointInformation == nullptr)
                        {
                            // TODO
                            return;
                        }

                        if (hasSources)
                        {
                            auto inputPorts = endpointInformation.FindAllAssociatedMidi1PortsForThisEndpoint(midi2::Midi1PortFlow::MidiMessageSource);
                            ok = inputPorts.Size() > 0;

                            for (auto const& port : inputPorts)
                            {
                                InputDeviceInfo* inputDeviceInfo = nullptr;
                                int errorCode;

                                auto getInputDeviceInfoResult = GetInputDeviceInfo(port.PortNumber(), &inputDeviceInfo, &errorCode);
                                if (getInputDeviceInfoResult != IN_GETINFORESULT_OK)
                                {
                                    ok = false;
                                    break;
                                }

                                inputDevicesInfo.push_back(inputDeviceInfo);

                                InputDeviceInfo* persistentInputDeviceInfo;
                                GetInputDeviceInfo(port.PortNumber(), &persistentInputDeviceInfo, &errorCode);
                                endpointDevicesInfo.inputDevicesInfo.push_back(persistentInputDeviceInfo);
                            }
                        }

                        if (hasDestinations)
                        {
                            auto outputPorts = endpointInformation.FindAllAssociatedMidi1PortsForThisEndpoint(midi2::Midi1PortFlow::MidiMessageDestination);
                            ok = outputPorts.Size() > 0;

                            for (auto const& port : outputPorts)
                            {
                                OutputDeviceInfo* outputDeviceInfo = nullptr;
                                int errorCode;

                                auto getOutputDeviceInfoResult = GetOutputDeviceInfo(port.PortNumber(), &outputDeviceInfo, &errorCode);
                                if (getOutputDeviceInfoResult != OUT_GETINFORESULT_OK)
                                {
                                    ok = false;
                                    break;
                                }

                                outputDevicesInfo.push_back(outputDeviceInfo);

                                OutputDeviceInfo* persistentOutputDeviceInfo;
                                GetOutputDeviceInfo(port.PortNumber(), &persistentOutputDeviceInfo, &errorCode);
                                endpointDevicesInfo.outputDevicesInfo.push_back(persistentOutputDeviceInfo);
                            }
                        }

                        if (!ok)
                            Sleep(1000);
                    }

                    if (!ok)
                    {
                        // TODO
                        return;
                    }

                    sessionHandle->endpointDevicesById[endpointKey] = endpointDevicesInfo;

                    for (auto* inputDeviceInfo : inputDevicesInfo)
                    {
                        sessionHandle->inputDeviceCallback(inputDeviceInfo, 1);
                    }

                    for (auto* outputDeviceInfo : outputDevicesInfo)
                    {
                        sessionHandle->outputDeviceCallback(outputDeviceInfo, 1);
                    }
                }
                catch (...)
                {
                    // TODO
                }
            };

            auto OnWatcherDeviceRemoved = [sessionHandle](midi2::MidiEndpointDeviceWatcher const&, midi2::MidiEndpointDeviceInformationRemovedEventArgs const& args)
            {
                if (sessionHandle->initialEnumerationCompleted.load() == 0 ||
                    sessionHandle->devicesWatcherEnabled.load() == 0)
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
                        sessionHandle->inputDeviceCallback(inputDeviceInfo, 0);
                    }

                    for (auto* outputDeviceInfo : endpointDevicesInfo.outputDevicesInfo)
                    {
                        sessionHandle->outputDeviceCallback(outputDeviceInfo, 0);
                    }
                }
                catch (...)
                {
                    // TODO
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
    catch (...)
    {
        // TODO
        return SESSION_OPENRESULT_WMSUNKNOWNERROR;
    }

    *handle = sessionHandle;
    return SESSION_OPENRESULT_OK;
}

API_EXPORT SESSION_CLOSERESULT API_CALL CloseSession(void* handle)
{
    SessionHandle* sessionHandle = static_cast<SessionHandle*>(handle);

    try
    {
        if (sessionHandle->wmsAvailable)
        {
            if (sessionHandle->wmsSdkInitializer != nullptr)
            {
                sessionHandle->wmsSdkInitializer->ShutdownSdkRuntime();
                sessionHandle->wmsSdkInitializer.reset();
            }

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

            winrt::uninit_apartment();
        }
    }
    catch (...)
    {
        // TODO
    }

    {
        std::lock_guard<std::mutex> lock(sessionHandle->endpointDevicesLock);

        for (auto& pair : sessionHandle->endpointDevicesById)
        {
            DeleteInputDeviceInfos(pair.second.inputDevicesInfo);
            DeleteOutputDeviceInfos(pair.second.outputDevicesInfo);
        }

        sessionHandle->endpointDevicesById.clear();
    }

    delete sessionHandle;
    return SESSION_CLOSERESULT_OK;
}

/* ================================
   Devices watcher
================================ */

API_EXPORT void API_CALL EnableDevicesWatcher(SessionHandle* sessionHandle)
{
    sessionHandle->devicesWatcherEnabled.store(1);
}

API_EXPORT void API_CALL DisableDevicesWatcher(SessionHandle* sessionHandle)
{
    sessionHandle->devicesWatcherEnabled.store(0);
}

/* ================================
   Input device
================================ */

typedef struct
{
    InputDeviceInfo* info;
    HMIDIIN handle;
    LPMIDIHDR* sysExHeaders;
    int sysExBufferCount;
    int sysExBufferSize;
    CRITICAL_SECTION lock;
    LONG isClosing;
} InputDeviceHandle;

API_EXPORT int API_CALL GetInputDeviceHashCode(InputDeviceInfo* info)
{
    return 0;
}

API_EXPORT IN_GETPROPERTYRESULT API_CALL GetInputDeviceName(InputDeviceInfo* info, const char** value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->szPname;
    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_GETPROPERTYRESULT API_CALL GetInputDeviceManufacturer(InputDeviceInfo* info, const char** value, int* errorCode)
{
    *errorCode = 0;

    *value = GetDeviceManufacturer(info->caps->wMid);
    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_GETPROPERTYRESULT API_CALL GetInputDeviceProduct(InputDeviceInfo* info, const char** value, int* errorCode)
{
    *errorCode = 0;

    *value = GetDeviceProduct(info->caps->wPid);
    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_GETPROPERTYRESULT API_CALL GetInputDeviceDriverVersion(InputDeviceInfo* info, int* value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->vDriverVersion;
    return IN_GETPROPERTYRESULT_OK;
}

API_EXPORT IN_RENEWSYSEXBUFFERRESULT API_CALL RenewInputDeviceSysExBuffer(void* handle, void* headerPointer, int* errorCode)
{
    *errorCode = 0;

    InputDeviceHandle* inputDeviceHandle = static_cast<InputDeviceHandle*>(handle);
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

API_EXPORT IN_OPENRESULT API_CALL OpenInputDevice_Win(InputDeviceInfo* info, void* sessionHandle, DWORD_PTR callback, int sysExBufferSize, int sysExBufferCount, void** handle, int* errorCode)
{
    *errorCode = 0;

    InputDeviceInfo* inputDeviceInfo = info;
    InputDeviceHandle* inputDeviceHandle = new InputDeviceHandle();
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

    *handle = inputDeviceHandle;

    MMRESULT result = midiInOpen(&inputDeviceHandle->handle, inputDeviceInfo->deviceIndex, callback, 0, CALLBACK_FUNCTION);
    if (result != MMSYSERR_NOERROR)
    {
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

    for (int i = 0; i < sysExBufferCount; i++)
    {
        int prepareErrorCode;
        IN_PREPARESYSEXBUFFERRESULT prepareResult = PrepareSysExBuffer(inputDeviceHandle->handle, sysExBufferSize, &inputDeviceHandle->sysExHeaders[i], &prepareErrorCode);

        if (result != IN_PREPARESYSEXBUFFERRESULT_OK)
        {
            // TODO
        }
    }

    return IN_OPENRESULT_OK;
}

API_EXPORT IN_CLOSERESULT API_CALL CloseInputDevice(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputDeviceHandle* inputDeviceHandle = static_cast<InputDeviceHandle*>(handle);

    EnterCriticalSection(&inputDeviceHandle->lock);
    inputDeviceHandle->isClosing = 1;

    MMRESULT result = midiInReset(inputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        LeaveCriticalSection(&inputDeviceHandle->lock);
        *errorCode = result;

        switch (result)
        {
            case MMSYSERR_INVALHANDLE: return IN_CLOSERESULT_RESET_INVALIDHANDLE;
        }

        return IN_CLOSERESULT_RESET_UNKNOWNERROR;
    }

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

    result = midiInClose(inputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        LeaveCriticalSection(&inputDeviceHandle->lock);
        *errorCode = result;

        switch (result)
        {
            case MIDIERR_STILLPLAYING: return IN_CLOSERESULT_CLOSE_STILLPLAYING;
            case MMSYSERR_INVALHANDLE: return IN_CLOSERESULT_CLOSE_INVALIDHANDLE;
            case MMSYSERR_NOMEM: return IN_CLOSERESULT_CLOSE_NOMEMORY;
        }

        return IN_CLOSERESULT_CLOSE_UNKNOWNERROR;
    }

    // if (inputDeviceHandle->info)
    // {
    //     DeleteInputDeviceInfo(inputDeviceHandle->info);
    // }

    LeaveCriticalSection(&inputDeviceHandle->lock);
    DeleteCriticalSection(&inputDeviceHandle->lock);

    delete inputDeviceHandle;

    return IN_CLOSERESULT_OK;
}

API_EXPORT IN_CONNECTRESULT API_CALL ConnectToInputDevice(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputDeviceHandle* inputDeviceHandle = static_cast<InputDeviceHandle*>(handle);

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

API_EXPORT IN_DISCONNECTRESULT API_CALL DisconnectFromInputDevice(void* handle, int* errorCode)
{
    *errorCode = 0;

    InputDeviceHandle* inputDeviceHandle = static_cast<InputDeviceHandle*>(handle);

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

API_EXPORT IN_GETSYSEXDATARESULT API_CALL GetInputDeviceSysExBufferData(LPMIDIHDR header, LPSTR* data, int* size)
{
    *data = header->lpData;
    *size = header->dwBytesRecorded;

    return IN_GETSYSEXDATARESULT_OK;
}

API_EXPORT char API_CALL IsInputDevicePropertySupported(IN_PROPERTY property)
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
    OutputDeviceInfo* info;
    HMIDIOUT handle;
} OutputDeviceHandle;

API_EXPORT int API_CALL GetOutputDeviceHashCode(OutputDeviceInfo* info)
{
    return 0;
}

API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputDeviceName(OutputDeviceInfo* info, const char** value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->szPname;
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputDeviceManufacturer(OutputDeviceInfo* info, const char** value, int* errorCode)
{
    *errorCode = 0;

    *value = GetDeviceManufacturer(info->caps->wMid);
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputDeviceProduct(OutputDeviceInfo* info, const char** value, int* errorCode)
{
    *errorCode = 0;

    *value = GetDeviceProduct(info->caps->wPid);
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputDeviceDriverVersion(OutputDeviceInfo* info, int* value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->vDriverVersion;
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputDeviceTechnology(OutputDeviceInfo* info, OUT_TECHNOLOGY* value, int* errorCode)
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

API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputDeviceVoicesNumber(OutputDeviceInfo* info, int* value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->wVoices;
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputDeviceNotesNumber(OutputDeviceInfo* info, int* value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->wNotes;
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputDeviceChannelsMask(OutputDeviceInfo* info, int* value, int* errorCode)
{
    *errorCode = 0;

    *value = info->caps->wChannelMask;
    return OUT_GETPROPERTYRESULT_OK;
}

API_EXPORT OUT_GETPROPERTYRESULT API_CALL GetOutputDeviceOptions(OutputDeviceInfo* info, OUT_OPTION* value, int* errorCode)
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

API_EXPORT OUT_OPENRESULT API_CALL OpenOutputDevice_Win(OutputDeviceInfo* info, void* sessionHandle, DWORD_PTR callback, void** handle, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceHandle* outputDeviceHandle = new OutputDeviceHandle();
    outputDeviceHandle->info = info;

    HMIDIOUT outHandle;
    MMRESULT result = midiOutOpen(&outHandle, info->deviceIndex, callback, 0, CALLBACK_FUNCTION);
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

API_EXPORT OUT_CLOSERESULT API_CALL CloseOutputDevice(void* handle, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceHandle* outputDeviceHandle = static_cast<OutputDeviceHandle*>(handle);

    MMRESULT result = midiOutReset(outputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        *errorCode = result;

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

        switch (result)
        {
            case MIDIERR_STILLPLAYING: return OUT_CLOSERESULT_CLOSE_STILLPLAYING;
            case MMSYSERR_INVALHANDLE: return OUT_CLOSERESULT_CLOSE_INVALIDHANDLE;
            case MMSYSERR_NOMEM: return OUT_CLOSERESULT_CLOSE_NOMEMORY;
        }

        return OUT_CLOSERESULT_CLOSE_UNKNOWNERROR;
    }

    // if (outputDeviceHandle->info)
    // {
    //     DeleteOutputDeviceInfo(outputDeviceHandle->info);
    // }

    delete outputDeviceHandle;

    return OUT_CLOSERESULT_OK;
}

API_EXPORT OUT_SENDSHORTRESULT API_CALL SendShortEventToOutputDevice(void* handle, int message, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceHandle* outputDeviceHandle = static_cast<OutputDeviceHandle*>(handle);

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

API_EXPORT OUT_SENDSYSEXRESULT API_CALL SendSysExEventToOutputDevice_Win(void* handle, LPSTR data, int size, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceHandle* outputDeviceHandle = static_cast<OutputDeviceHandle*>(handle);

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

API_EXPORT OUT_GETSYSEXDATARESULT API_CALL GetOutputDeviceSysExBufferData(void* handle, LPMIDIHDR header, LPSTR* data, int* size, int* errorCode)
{
    *errorCode = 0;

    OutputDeviceHandle* outputDeviceHandle = static_cast<OutputDeviceHandle*>(handle);

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

API_EXPORT char API_CALL IsOutputDevicePropertySupported(OUT_PROPERTY property)
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
    InputDeviceInfo* inputDeviceInfo;
    OutputDeviceInfo* outputDeviceInfo;
    char* name;
    winrt::guid associationId;
};

API_EXPORT char API_CALL IsVirtualDeviceApiAvailable(SessionHandle* sessionHandle)
{
    if (!sessionHandle->wmsSdkInitialized || !sessionHandle->wmsAvailable)
        return 0;

    return sessionHandle->basicLoopbackAvailable;
}

int GetSinglePortIndex(midi2::MidiEndpointDeviceInformation endpointInformation, midi2::Midi1PortFlow flow)
{
    auto ports = endpointInformation.FindAllAssociatedMidi1PortsForThisEndpoint(flow);
    midi2::MidiEndpointAssociatedPortDeviceInformation port = ports.GetAt(0);
    return port.PortNumber();
}

API_EXPORT VIRTUAL_OPENRESULT API_CALL OpenVirtualDevice_Win(char* name, SessionHandle* sessionHandle, VirtualDeviceInfo** info, int* errorCode)
{
    *errorCode = 0;

    VirtualDeviceInfo* virtualDeviceInfo = new VirtualDeviceInfo();
    virtualDeviceInfo->name = name;

    if (!sessionHandle->wmsSdkInitialized || !sessionHandle->wmsAvailable)
    {
        delete virtualDeviceInfo;
        return VIRTUAL_OPENRESULT_WMSUNAVAILABLE;
    }

    if (!sessionHandle->basicLoopbackAvailable)
    {
        delete virtualDeviceInfo;
        return VIRTUAL_OPENRESULT_WMSBASICLOOPBACKUNAVAILABLE;
    }

    try
    {
        winrt::hstring uniqueId = winrt::to_hstring(winrt::Windows::Foundation::GuidHelper::CreateNewGuid());

        try
        {
            while (basicLoopback::MidiBasicLoopbackEndpointManager::DoesLoopbackExist(uniqueId));
            {
                uniqueId = winrt::to_hstring(winrt::Windows::Foundation::GuidHelper::CreateNewGuid());
            }
        }
        catch (...)
        {
            // TODO
        }

        virtualDeviceInfo->associationId = winrt::Windows::Foundation::GuidHelper::CreateNewGuid();

        basicLoopback::MidiBasicLoopbackEndpointDefinition definition{};
        definition.Name = winrt::to_hstring(name);
        definition.UniqueId = uniqueId;
        definition.IsMuted = false;

        basicLoopback::MidiBasicLoopbackEndpointCreationConfig config(
            virtualDeviceInfo->associationId,
            definition);

        basicLoopback::MidiBasicLoopbackEndpointCreationResult result =
            basicLoopback::MidiBasicLoopbackEndpointManager::CreateTransientLoopbackEndpoint(config);

        if (!result.Success())
        {
            *errorCode = static_cast<int>(result.ErrorCode());

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

            delete virtualDeviceInfo;
            return VIRTUAL_OPENRESULT_FAILED;
        }

        EnsureWinMmPortsAvailable();

        auto endpointId = result.EndpointDeviceId();
        auto endpointInformation = midi2::MidiEndpointDeviceInformation::CreateFromEndpointDeviceId(endpointId);

        auto inputDeviceIndex = GetSinglePortIndex(endpointInformation, midi2::Midi1PortFlow::MidiMessageSource);

        InputDeviceInfo* inputDeviceInfo = nullptr;
        IN_GETINFORESULT inputResult = GetInputDeviceInfo(inputDeviceIndex, &inputDeviceInfo, errorCode);
        if (inputResult != IN_GETINFORESULT_OK)
            return VIRTUAL_OPENRESULT_FAILEDGETINPUTDEVICEINFO;

        virtualDeviceInfo->inputDeviceInfo = inputDeviceInfo;

        auto outputDeviceIndex = GetSinglePortIndex(endpointInformation, midi2::Midi1PortFlow::MidiMessageDestination);

        OutputDeviceInfo* outputDeviceInfo = nullptr;
        OUT_GETINFORESULT outputResult = GetOutputDeviceInfo(outputDeviceIndex, &outputDeviceInfo, errorCode);
        if (outputResult != OUT_GETINFORESULT_OK)
            return VIRTUAL_OPENRESULT_FAILEDGETOUTPUTDEVICEINFO;

        virtualDeviceInfo->outputDeviceInfo = outputDeviceInfo;
    }
    catch (...)
    {
        delete virtualDeviceInfo;
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
            return VIRTUAL_CLOSERESULT_FAILED;
    }
    catch (...)
    {
        return VIRTUAL_CLOSERESULT_WMSERROR;
    }

    // TODO: analyze why this causes crash
    // delete virtualDeviceInfo->inputDeviceInfo;
    // delete virtualDeviceInfo->outputDeviceInfo;
    delete info;

    return VIRTUAL_CLOSERESULT_OK;
}

API_EXPORT void* API_CALL GetInputDeviceInfoFromVirtualDevice(VirtualDeviceInfo* info)
{
    return info->inputDeviceInfo;
}

API_EXPORT void* API_CALL GetOutputDeviceInfoFromVirtualDevice(VirtualDeviceInfo* info)
{
    return info->outputDeviceInfo;
}

API_EXPORT VIRTUAL_MUTERESULT API_CALL MuteVirtualDevice(VirtualDeviceInfo* info)
{
    try
    {
        auto muted = basicLoopback::MidiBasicLoopbackEndpointManager::MuteLoopback(info->associationId);
        if (!muted)
            return VIRTUAL_MUTERESULT_FAILED;
    }
    catch (...)
    {
        return VIRTUAL_MUTERESULT_WMSERROR;
    }

    return VIRTUAL_MUTERESULT_OK;
}

API_EXPORT VIRTUAL_UNMUTERESULT API_CALL UnmuteVirtualDevice(VirtualDeviceInfo* info)
{
    try
    {
        auto unmuted = basicLoopback::MidiBasicLoopbackEndpointManager::UnmuteLoopback(info->associationId);
        if (!unmuted)
            return VIRTUAL_UNMUTERESULT_FAILED;
    }
    catch (...)
    {
        return VIRTUAL_UNMUTERESULT_WMSERROR;
    }

    return VIRTUAL_UNMUTERESULT_OK;
}