#pragma comment(lib, "winmm.lib")

#include <windows.h>
#include <mmsystem.h>
#include <mmreg.h>

#include "NativeApi-Constants.h"

/* ================================
   Common
================================ */

API_TYPE GetApiType()
{
    return API_TYPE_WIN;
}

char CanCompareDevices()
{
	return 0;
}

/* ================================
   High-precision tick generator
================================ */

typedef struct
{
    UINT timerResolution;
    UINT timerId;
} TickGeneratorInfo;

TG_STARTRESULT StartHighPrecisionTickGenerator_Win(int interval, LPTIMECALLBACK callback, TickGeneratorInfo** info)
{
    TIMECAPS tc;
    MMRESULT result = timeGetDevCaps(&tc, sizeof(TIMECAPS));
    if (result != TIMERR_NOERROR)
        return TG_STARTRESULT_CANTGETDEVICECAPABILITIES;

    UINT wTimerRes = min(max(tc.wPeriodMin, interval), tc.wPeriodMax);

    timeBeginPeriod(wTimerRes);
    result = timeSetEvent(interval, wTimerRes, callback, 0, TIME_PERIODIC);
    if (result == 0)
        return TG_STARTRESULT_CANTSETTIMERCALLBACK;

    TickGeneratorInfo* tickGeneratorInfo = malloc(sizeof(TickGeneratorInfo));
    tickGeneratorInfo->timerResolution = wTimerRes;
    tickGeneratorInfo->timerId = result;
    *info = tickGeneratorInfo;

    return TG_STARTRESULT_OK;
}

TG_STOPRESULT StopHighPrecisionTickGenerator(
    TickGeneratorInfo* info)
{
    MMRESULT result = timeEndPeriod(info->timerResolution);
    if (result != TIMERR_NOERROR)
        return TG_STOPRESULT_CANTENDPERIOD;

    result = timeKillEvent(info->timerId);
    if (result != TIMERR_NOERROR)
        return TG_STOPRESULT_CANTKILLEVENT;

    free(info);

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

typedef struct
{
    int deviceIndex;
	LPMIDIOUTCAPSA caps;
} OutputDeviceInfo;

char* GetDeviceManufacturer(WORD manufacturerId)
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

char* GetDeviceProduct(WORD productId)
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

/* ================================
   Session
================================ */

typedef struct
{
    char* name;
} SessionHandle;

SESSION_OPENRESULT OpenSession_Win(char* name, void** handle)
{
    SessionHandle* sessionHandle = malloc(sizeof(SessionHandle));
    sessionHandle->name = name;

    *handle = sessionHandle;

    return SESSION_OPENRESULT_OK;
}

SESSION_CLOSERESULT CloseSession(void* handle)
{
    SessionHandle* sessionHandle = (SessionHandle*)handle;
    free(sessionHandle);
    return SESSION_CLOSERESULT_OK;
}

/* ================================
   Input device
================================ */

typedef struct
{
    InputDeviceInfo* info;
    HMIDIIN handle;
    LPMIDIHDR sysExHeader;
} InputDeviceHandle;

int GetInputDevicesCount()
{
    return midiInGetNumDevs();
}

IN_GETINFORESULT GetInputDeviceInfo(int deviceIndex, void** info)
{
    InputDeviceInfo* inputDeviceInfo = malloc(sizeof(InputDeviceInfo));

    inputDeviceInfo->deviceIndex = deviceIndex;
	inputDeviceInfo->caps = malloc(sizeof(MIDIINCAPSA));

    MMRESULT result = midiInGetDevCapsA(deviceIndex, inputDeviceInfo->caps, sizeof(MIDIINCAPSA));
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MMSYSERR_BADDEVICEID: return IN_GETINFORESULT_BADDEVICEID;
            case MMSYSERR_INVALPARAM: return IN_GETINFORESULT_INVALIDSTRUCTURE;
            case MMSYSERR_NODRIVER: return IN_GETINFORESULT_NODRIVER;
            case MMSYSERR_NOMEM: return IN_GETINFORESULT_NOMEMORY;
        }
    }

    *info = inputDeviceInfo;

    return IN_GETINFORESULT_OK;
}

int GetInputDeviceHashCode(void* info)
{
    return 0;
}

IN_GETPROPERTYRESULT GetInputDeviceName(void* info, char** value)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    *value = inputDeviceInfo->caps->szPname;
	return IN_GETPROPERTYRESULT_OK;
}

IN_GETPROPERTYRESULT GetInputDeviceManufacturer(void* info, char** value)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    *value = GetDeviceManufacturer(inputDeviceInfo->caps->wMid);
	return IN_GETPROPERTYRESULT_OK;
}

IN_GETPROPERTYRESULT GetInputDeviceProduct(void* info, char** value)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    *value = GetDeviceProduct(inputDeviceInfo->caps->wPid);
	return IN_GETPROPERTYRESULT_OK;
}

IN_GETPROPERTYRESULT GetInputDeviceDriverVersion(void* info, int* value)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;
    *value = inputDeviceInfo->caps->vDriverVersion;
	return IN_GETPROPERTYRESULT_OK;
}

IN_PREPARESYSEXBUFFERRESULT PrepareInputDeviceSysExBuffer(void* handle, int size)
{
    InputDeviceHandle* inputDeviceHandle = (InputDeviceHandle*)handle;

    LPMIDIHDR header = malloc(sizeof(MIDIHDR));
    header->lpData = malloc(size * sizeof(char));
    header->dwBufferLength = size;
    header->dwFlags = 0;
    inputDeviceHandle->sysExHeader = header;

    MMRESULT result = midiInPrepareHeader(inputDeviceHandle->handle, header, sizeof(MIDIHDR));
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MMSYSERR_INVALHANDLE: return IN_PREPARESYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDHANDLE;
            case MMSYSERR_INVALPARAM: return IN_PREPARESYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDADDRESS;
            case MMSYSERR_NOMEM: return IN_PREPARESYSEXBUFFERRESULT_PREPAREBUFFER_NOMEMORY;
        }
    }

    result = midiInAddBuffer(inputDeviceHandle->handle, header, sizeof(MIDIHDR));
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MIDIERR_STILLPLAYING: return IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_STILLPLAYING;
            case MIDIERR_UNPREPARED: return IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_UNPREPARED;
            case MMSYSERR_INVALHANDLE: return IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_INVALIDHANDLE;
            case MMSYSERR_INVALPARAM: return IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_INVALIDSTRUCTURE;
            case MMSYSERR_NOMEM: return IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_NOMEMORY;
        }
    }

    return IN_PREPARESYSEXBUFFERRESULT_OK;
}

IN_UNPREPARESYSEXBUFFERRESULT UnprepareInputDeviceSysExBuffer(void* handle)
{
    InputDeviceHandle* inputDeviceHandle = (InputDeviceHandle*)handle;

    if (inputDeviceHandle->sysExHeader == NULL)
        return IN_UNPREPARESYSEXBUFFERRESULT_OK;

    MMRESULT result = midiInUnprepareHeader(inputDeviceHandle->handle, inputDeviceHandle->sysExHeader, sizeof(MIDIHDR));
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MIDIERR_STILLPLAYING: return IN_UNPREPARESYSEXBUFFERRESULT_STILLPLAYING;
            case MMSYSERR_INVALPARAM: return IN_UNPREPARESYSEXBUFFERRESULT_INVALIDSTRUCTURE;
            case MMSYSERR_INVALHANDLE: return IN_UNPREPARESYSEXBUFFERRESULT_INVALIDHANDLE;
        }
    }

    free(inputDeviceHandle->sysExHeader->lpData);
    free(inputDeviceHandle->sysExHeader);

    return IN_UNPREPARESYSEXBUFFERRESULT_OK;
}

IN_RENEWSYSEXBUFFERRESULT RenewInputDeviceSysExBuffer(void* handle, int size)
{
    IN_UNPREPARESYSEXBUFFERRESULT unprepareResult = UnprepareInputDeviceSysExBuffer(handle);
    if (unprepareResult != IN_UNPREPARESYSEXBUFFERRESULT_OK)
    {
        switch (unprepareResult)
        {
            case IN_UNPREPARESYSEXBUFFERRESULT_STILLPLAYING: return IN_RENEWSYSEXBUFFERRESULT_UNPREPAREBUFFER_STILLPLAYING;
            case IN_UNPREPARESYSEXBUFFERRESULT_INVALIDSTRUCTURE: return IN_RENEWSYSEXBUFFERRESULT_UNPREPAREBUFFER_INVALIDSTRUCTURE;
            case IN_UNPREPARESYSEXBUFFERRESULT_INVALIDHANDLE: return IN_RENEWSYSEXBUFFERRESULT_UNPREPAREBUFFER_INVALIDHANDLE;
        }
    }

    IN_PREPARESYSEXBUFFERRESULT prepareResult = PrepareInputDeviceSysExBuffer(handle, size);
    if (prepareResult != IN_PREPARESYSEXBUFFERRESULT_OK)
    {
        switch (prepareResult)
        {
            case IN_PREPARESYSEXBUFFERRESULT_PREPAREBUFFER_NOMEMORY: return IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_NOMEMORY;
            case IN_PREPARESYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDHANDLE: return IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDHANDLE;
            case IN_PREPARESYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDADDRESS: return IN_RENEWSYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDADDRESS;
            case IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_NOMEMORY: return IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_NOMEMORY;
            case IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_STILLPLAYING: return IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_STILLPLAYING;
            case IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_UNPREPARED: return IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_UNPREPARED;
            case IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_INVALIDHANDLE: return IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_INVALIDHANDLE;
            case IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_INVALIDSTRUCTURE: return IN_RENEWSYSEXBUFFERRESULT_ADDBUFFER_INVALIDSTRUCTURE;
        }
    }

    return IN_RENEWSYSEXBUFFERRESULT_OK;
}

IN_OPENRESULT OpenInputDevice_Win(void* info, void* sessionHandle, DWORD_PTR callback, int sysExBufferSize, void** handle)
{
    InputDeviceInfo* inputDeviceInfo = (InputDeviceInfo*)info;

    InputDeviceHandle* inputDeviceHandle = malloc(sizeof(InputDeviceHandle));
    inputDeviceHandle->info = inputDeviceInfo;

    HMIDIIN inHandle;
    MMRESULT result = midiInOpen(&inHandle, inputDeviceInfo->deviceIndex, callback, 0, CALLBACK_FUNCTION);
    switch (result)
    {
        case MMSYSERR_ALLOCATED: return IN_OPENRESULT_ALLOCATED;
        case MMSYSERR_BADDEVICEID: return IN_OPENRESULT_BADDEVICEID;
        case MMSYSERR_INVALFLAG: return IN_OPENRESULT_INVALIDFLAG;
        case MMSYSERR_INVALPARAM: return IN_OPENRESULT_INVALIDSTRUCTURE;
        case MMSYSERR_NOMEM: return IN_OPENRESULT_NOMEMORY;
    }

    inputDeviceHandle->handle = inHandle;

    *handle = inputDeviceHandle;

    IN_PREPARESYSEXBUFFERRESULT prepareBufferResult = PrepareInputDeviceSysExBuffer(*handle, sysExBufferSize);
    if (prepareBufferResult != IN_PREPARESYSEXBUFFERRESULT_OK)
    {
        switch (prepareBufferResult)
        {
            case IN_PREPARESYSEXBUFFERRESULT_PREPAREBUFFER_NOMEMORY: return IN_OPENRESULT_PREPAREBUFFER_NOMEMORY;
            case IN_PREPARESYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDHANDLE: return IN_OPENRESULT_PREPAREBUFFER_INVALIDHANDLE;
            case IN_PREPARESYSEXBUFFERRESULT_PREPAREBUFFER_INVALIDADDRESS: return IN_OPENRESULT_PREPAREBUFFER_INVALIDADDRESS;
            case IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_NOMEMORY: return IN_OPENRESULT_ADDBUFFER_NOMEMORY;
            case IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_STILLPLAYING: return IN_OPENRESULT_ADDBUFFER_STILLPLAYING;
            case IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_UNPREPARED: return IN_OPENRESULT_ADDBUFFER_UNPREPARED;
            case IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_INVALIDHANDLE: return IN_OPENRESULT_ADDBUFFER_INVALIDHANDLE;
            case IN_PREPARESYSEXBUFFERRESULT_ADDBUFFER_INVALIDSTRUCTURE: return IN_OPENRESULT_ADDBUFFER_INVALIDSTRUCTURE;
        }
    }

    return IN_OPENRESULT_OK;
}

IN_CLOSERESULT CloseInputDevice(void* handle)
{
    InputDeviceHandle* inputDeviceHandle = (InputDeviceHandle*)handle;

    MMRESULT result = midiInReset(inputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
        case MMSYSERR_INVALHANDLE: return IN_CLOSERESULT_RESET_INVALIDHANDLE;
        }
    }

    IN_UNPREPARESYSEXBUFFERRESULT unprepareBufferResult = UnprepareInputDeviceSysExBuffer(inputDeviceHandle);
    if (unprepareBufferResult != IN_UNPREPARESYSEXBUFFERRESULT_OK)
    {
        switch (result)
        {
            case IN_UNPREPARESYSEXBUFFERRESULT_STILLPLAYING: return IN_CLOSERESULT_UNPREPAREBUFFER_STILLPLAYING;
            case IN_UNPREPARESYSEXBUFFERRESULT_INVALIDSTRUCTURE: return IN_CLOSERESULT_UNPREPAREBUFFER_INVALIDSTRUCTURE;
            case IN_UNPREPARESYSEXBUFFERRESULT_INVALIDHANDLE: return IN_CLOSERESULT_UNPREPAREBUFFER_INVALIDHANDLE;
        }
    }

    result = midiInClose(inputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MIDIERR_STILLPLAYING: return IN_CLOSERESULT_CLOSE_STILLPLAYING;
            case MMSYSERR_INVALHANDLE: return IN_CLOSERESULT_CLOSE_INVALIDHANDLE;
            case MMSYSERR_NOMEM: return IN_CLOSERESULT_CLOSE_NOMEMORY;
        }
    }

    free(inputDeviceHandle->info);
    free(inputDeviceHandle);

    return IN_CLOSERESULT_OK;
}

IN_CONNECTRESULT ConnectToInputDevice(void* handle)
{
    InputDeviceHandle* inputDeviceHandle = (InputDeviceHandle*)handle;

    MMRESULT result = midiInStart(inputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MMSYSERR_INVALHANDLE: return IN_CONNECTRESULT_INVALIDHANDLE;
        }
    }

    return IN_CONNECTRESULT_OK;
}

IN_DISCONNECTRESULT DisconnectFromInputDevice(void* handle)
{
    InputDeviceHandle* inputDeviceHandle = (InputDeviceHandle*)handle;

    MMRESULT result = midiInStop(inputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MMSYSERR_INVALHANDLE: return IN_DISCONNECTRESULT_INVALIDHANDLE;
        }
    }

    return IN_DISCONNECTRESULT_OK;
}

IN_GETSYSEXDATARESULT GetInputDeviceSysExBufferData(LPMIDIHDR header, LPSTR* data, int* size)
{
    *data = header->lpData;
    *size = header->dwBytesRecorded;

    return IN_GETSYSEXDATARESULT_OK;
}

char IsInputDevicePropertySupported(IN_PROPERTY property)
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

int GetOutputDevicesCount()
{
    return midiOutGetNumDevs();
}

OUT_GETINFORESULT GetOutputDeviceInfo(int deviceIndex, void** info)
{
    OutputDeviceInfo* outputDeviceInfo = malloc(sizeof(OutputDeviceInfo));

    outputDeviceInfo->deviceIndex = deviceIndex;
	outputDeviceInfo->caps = malloc(sizeof(MIDIOUTCAPSA));

    MMRESULT result = midiOutGetDevCapsA(deviceIndex, outputDeviceInfo->caps, sizeof(MIDIOUTCAPSA));
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MMSYSERR_BADDEVICEID: return OUT_GETINFORESULT_BADDEVICEID;
            case MMSYSERR_INVALPARAM: return OUT_GETINFORESULT_INVALIDSTRUCTURE;
            case MMSYSERR_NODRIVER: return OUT_GETINFORESULT_NODRIVER;
            case MMSYSERR_NOMEM: return OUT_GETINFORESULT_NOMEMORY;
        }
    }

    *info = outputDeviceInfo;

    return OUT_GETINFORESULT_OK;
}

int GetOutputDeviceHashCode(void* info)
{
    return 0;
}

OUT_GETPROPERTYRESULT GetOutputDeviceName(void* info, char** value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    *value = outputDeviceInfo->caps->szPname;
	return OUT_GETPROPERTYRESULT_OK;
}

OUT_GETPROPERTYRESULT GetOutputDeviceManufacturer(void* info, char** value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    *value = GetDeviceManufacturer(outputDeviceInfo->caps->wMid);
	return OUT_GETPROPERTYRESULT_OK;
}

OUT_GETPROPERTYRESULT GetOutputDeviceProduct(void* info, char** value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    *value = GetDeviceProduct(outputDeviceInfo->caps->wPid);
	return OUT_GETPROPERTYRESULT_OK;
}

OUT_GETPROPERTYRESULT GetOutputDeviceDriverVersion(void* info, int* value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    *value = outputDeviceInfo->caps->vDriverVersion;
	return OUT_GETPROPERTYRESULT_OK;
}

OUT_GETPROPERTYRESULT GetOutputDeviceTechnology(void* info, OUT_TECHNOLOGY* value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    
	*value = OUT_TECHNOLOGY_UNKNOWN;
	
	switch (outputDeviceInfo->caps->wTechnology)
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

OUT_GETPROPERTYRESULT GetOutputDeviceVoicesNumber(void* info, int* value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    *value = outputDeviceInfo->caps->wVoices;
	return OUT_GETPROPERTYRESULT_OK;
}

OUT_GETPROPERTYRESULT GetOutputDeviceNotesNumber(void* info, int* value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    *value = outputDeviceInfo->caps->wNotes;
	return OUT_GETPROPERTYRESULT_OK;
}

OUT_GETPROPERTYRESULT GetOutputDeviceChannelsMask(void* info, int* value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
    *value = outputDeviceInfo->caps->wChannelMask;
	return OUT_GETPROPERTYRESULT_OK;
}

OUT_GETPROPERTYRESULT GetOutputDeviceOptions(void* info, OUT_OPTION* value)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;
	
	int result = OUT_OPTION_UNKNOWN;
	
	DWORD support = outputDeviceInfo->caps->dwSupport;
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

OUT_OPENRESULT OpenOutputDevice_Win(void* info, void* sessionHandle, DWORD_PTR callback, void** handle)
{
    OutputDeviceInfo* outputDeviceInfo = (OutputDeviceInfo*)info;

    OutputDeviceHandle* outputDeviceHandle = malloc(sizeof(OutputDeviceHandle));
    outputDeviceHandle->info = outputDeviceInfo;

    HMIDIOUT outHandle;
    MMRESULT result = midiOutOpen(&outHandle, outputDeviceInfo->deviceIndex, callback, 0, CALLBACK_FUNCTION);
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MMSYSERR_ALLOCATED: return OUT_OPENRESULT_ALLOCATED;
            case MMSYSERR_BADDEVICEID: return OUT_OPENRESULT_BADDEVICEID;
            case MMSYSERR_INVALFLAG: return OUT_OPENRESULT_INVALIDFLAG;
            case MMSYSERR_INVALPARAM: return OUT_OPENRESULT_INVALIDSTRUCTURE;
            case MMSYSERR_NOMEM: return OUT_OPENRESULT_NOMEMORY;
        }
    }

    outputDeviceHandle->handle = outHandle;

    *handle = outputDeviceHandle;

    return OUT_OPENRESULT_OK;
}

OUT_CLOSERESULT CloseOutputDevice(void* handle)
{
    OutputDeviceHandle* outputDeviceHandle = (OutputDeviceHandle*)handle;

    MMRESULT result = midiOutReset(outputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MMSYSERR_INVALHANDLE: return OUT_CLOSERESULT_RESET_INVALIDHANDLE;
        }
    }

    result = midiOutClose(outputDeviceHandle->handle);
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MIDIERR_STILLPLAYING: return OUT_CLOSERESULT_CLOSE_STILLPLAYING;
            case MMSYSERR_INVALHANDLE: return OUT_CLOSERESULT_CLOSE_INVALIDHANDLE;
            case MMSYSERR_NOMEM: return OUT_CLOSERESULT_CLOSE_NOMEMORY;
        }
    }

    free(outputDeviceHandle->info);
    free(outputDeviceHandle);

    return OUT_CLOSERESULT_OK;
}

OUT_SENDSHORTRESULT SendShortEventToOutputDevice(void* handle, int message)
{
    OutputDeviceHandle* outputDeviceHandle = (OutputDeviceHandle*)handle;

    MMRESULT result = midiOutShortMsg(outputDeviceHandle->handle, (DWORD)message);
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MIDIERR_BADOPENMODE: return OUT_SENDSHORTRESULT_BADOPENMODE;
            case MIDIERR_NOTREADY: return OUT_SENDSHORTRESULT_NOTREADY;
            case MMSYSERR_INVALHANDLE: return OUT_SENDSHORTRESULT_INVALIDHANDLE;
        }
    }

    return OUT_SENDSHORTRESULT_OK;
}

OUT_SENDSYSEXRESULT SendSysExEventToOutputDevice_Win(void* handle, LPSTR data, int size)
{
    OutputDeviceHandle* outputDeviceHandle = (OutputDeviceHandle*)handle;

    LPMIDIHDR header = malloc(sizeof(MIDIHDR));
    header->lpData = data;
    header->dwBufferLength = size;
    header->dwBytesRecorded = size;
    header->dwFlags = 0;

    MMRESULT result = midiOutPrepareHeader(outputDeviceHandle->handle, header, sizeof(MIDIHDR));
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MMSYSERR_INVALHANDLE: return OUT_SENDSYSEXRESULT_PREPAREBUFFER_INVALIDHANDLE;
            case MMSYSERR_INVALPARAM: return OUT_SENDSYSEXRESULT_PREPAREBUFFER_INVALIDADDRESS;
            case MMSYSERR_NOMEM: return OUT_SENDSYSEXRESULT_PREPAREBUFFER_NOMEMORY;
        }
    }

    result = midiOutLongMsg(outputDeviceHandle->handle, header, sizeof(MIDIHDR));
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MIDIERR_NOTREADY: return OUT_SENDSYSEXRESULT_NOTREADY;
            case MIDIERR_UNPREPARED: return OUT_SENDSYSEXRESULT_UNPREPARED;
            case MMSYSERR_INVALHANDLE: return OUT_SENDSYSEXRESULT_INVALIDHANDLE;
            case MMSYSERR_INVALPARAM: return OUT_SENDSYSEXRESULT_INVALIDSTRUCTURE;
        }
    }

    return OUT_SENDSYSEXRESULT_OK;
}

OUT_GETSYSEXDATARESULT GetOutputDeviceSysExBufferData(void* handle, LPMIDIHDR header, LPSTR* data, int* size)
{
    OutputDeviceHandle* outputDeviceHandle = (OutputDeviceHandle*)handle;

    MMRESULT result = midiOutUnprepareHeader(outputDeviceHandle->handle, header, sizeof(MIDIHDR));
    if (result != MMSYSERR_NOERROR)
    {
        switch (result)
        {
            case MIDIERR_STILLPLAYING: return OUT_GETSYSEXDATARESULT_STILLPLAYING;
            case MMSYSERR_INVALPARAM: return OUT_GETSYSEXDATARESULT_INVALIDSTRUCTURE;
            case MMSYSERR_INVALHANDLE: return OUT_GETSYSEXDATARESULT_INVALIDHANDLE;
        }
    }

    *data = header->lpData;
    *size = header->dwBytesRecorded;

    free(header);
    return OUT_GETSYSEXDATARESULT_OK;
}

char IsOutputDevicePropertySupported(OUT_PROPERTY property)
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