#pragma comment(lib, "winmm.lib")

#include <windows.h>
#include <mmsystem.h>
#include <stdio.h>

#include "NativeApi-ReturnCodes.h"

/* ================================
   High-precision tick generator
================================ */

typedef struct
{
	UINT timerResolution;
	UINT timerId;
} TickGeneratorInfo;

void TimerCallback(UINT uTimerID, UINT uMsg, DWORD_PTR dwUser, DWORD_PTR dw1, DWORD_PTR dw2)
{
	void (*callback)(void) = (void (*)(void))dwUser;
	callback();
}

TG_STARTRESULT StartHighPrecisionTickGenerator(
	int interval,
	void (*callback)(void),
	TickGeneratorInfo** info)
{
	TIMECAPS tc;
	MMRESULT result = timeGetDevCaps(&tc, sizeof(TIMECAPS));
	if (result != TIMERR_NOERROR)
	{
		return WINDOWS_TG_STARTRESULT_CANTGETDEVICECAPABILITIES;
	}

	UINT wTimerRes = min(max(tc.wPeriodMin, interval), tc.wPeriodMax);
	
	timeBeginPeriod(wTimerRes);
	result = timeSetEvent(interval, wTimerRes, TimerCallback, (DWORD_PTR)callback, TIME_PERIODIC);
	if (result == 0)
	{
		return WINDOWS_TG_STARTRESULT_CANTSETTIMERCALLBACK;
	}

	TickGeneratorInfo* tickGeneratorInfo = malloc(sizeof(TickGeneratorInfo));
    tickGeneratorInfo->timerResolution = wTimerRes;
	tickGeneratorInfo->timerId = result;
	*info = tickGeneratorInfo;

	return WINDOWS_TG_STARTRESULT_OK;
}

TG_STOPRESULT StopHighPrecisionTickGenerator(
	TickGeneratorInfo* info)
{
	MMRESULT result = timeEndPeriod(info->timerResolution);
	if (result != TIMERR_NOERROR)
	{
		return WINDOWS_TG_STOPRESULT_CANTENDPERIOD;
	}

	result = timeKillEvent(info->timerId);
	if (result != TIMERR_NOERROR)
	{
		return WINDOWS_TG_STOPRESULT_CANTKILLEVENT;
	}

	free(info);

	return WINDOWS_TG_STOPRESULT_OK;
}

/*void TestCallback()
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
	printf("[stop = %d]", result);
}*/