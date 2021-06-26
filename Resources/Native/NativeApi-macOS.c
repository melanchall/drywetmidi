#include <CoreFoundation/CoreFoundation.h>
#include <pthread.h>

#include "NativeApi-ReturnCodes.h"

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