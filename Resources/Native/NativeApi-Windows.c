#pragma comment(lib, "winmm.lib")

#include <windows.h>
#include <mmsystem.h>
/*#include <stdio.h>*/

#include "NativeApi-ReturnCodes.h"

API_TYPE GetApiType()
{
    return API_TYPE_WINMM;
}

/* ================================
   High-precision tick generator
================================ */

typedef struct
{
	UINT timerResolution;
	UINT timerId;
} TickGeneratorInfo;

TG_STARTRESULT StartHighPrecisionTickGenerator_Winmm(int interval, LPTIMECALLBACK callback, TickGeneratorInfo** info)
{
	TIMECAPS tc;
	MMRESULT result = timeGetDevCaps(&tc, sizeof(TIMECAPS));
	if (result != TIMERR_NOERROR)
	{
		return TG_STARTRESULT_CANTGETDEVICECAPABILITIES;
	}

	UINT wTimerRes = min(max(tc.wPeriodMin, interval), tc.wPeriodMax);
	
	timeBeginPeriod(wTimerRes);
	result = timeSetEvent(interval, wTimerRes, callback, 0, TIME_PERIODIC);
	if (result == 0)
	{
		return TG_STARTRESULT_CANTSETTIMERCALLBACK;
	}

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
	{
		return TG_STOPRESULT_CANTENDPERIOD;
	}

	result = timeKillEvent(info->timerId);
	if (result != TIMERR_NOERROR)
	{
		return TG_STOPRESULT_CANTKILLEVENT;
	}

	free(info);

	return TG_STOPRESULT_OK;
}

/*void TestCallback(UINT uTimerID, UINT uMsg, DWORD_PTR dwUser, DWORD_PTR dw1, DWORD_PTR dw2)
{
	printf("A");
}

void main()
{
	TickGeneratorInfo* info;
	TG_STARTRESULT result = StartHighPrecisionTickGenerator(1000, TestCallback, &info);
	printf("[start = %d]", result);
	
	Sleep(10000);

	result = StopHighPrecisionTickGenerator(info);
	printf("[stop = %d]\n", result);
}*/