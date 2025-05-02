#import <mach/mach.h>
#import <mach/mach_host.h>
#import <mach/mach_time.h> 
#import <sys/time.h>
#import <pthread.h>
#import <Foundation/Foundation.h>

static inline double timeInSeconds()
{
    mach_timebase_info_data_t info;
    mach_timebase_info(&info);
    return (double)mach_absolute_time() * (double)info.numer / (double)info.denom / 1e9;
}

extern "C" {

/// Process CPU percent (user+system) since the last call.
double bugsnag_unity_performance_process_cpu_percent()
{
    static uint64_t lastTotalTicks = 0;
    static double   lastWallClock  = 0.0;

    task_basic_info_data_t tinfo;
    mach_msg_type_number_t count = TASK_BASIC_INFO_COUNT;
    if (task_info(mach_task_self(), TASK_BASIC_INFO,
                  (task_info_t)&tinfo, &count) != KERN_SUCCESS)
        return -1.0;

    uint64_t nowTicks = tinfo.user_time.seconds  * 1e6 + tinfo.user_time.microseconds +
                        tinfo.system_time.seconds * 1e6 + tinfo.system_time.microseconds;
    double   nowClock = timeInSeconds();

    if (lastWallClock == 0) { // first sample
        lastTotalTicks = nowTicks;
        lastWallClock  = nowClock;
        return 0.0;
    }

    double deltaCpu   = (double)(nowTicks - lastTotalTicks) / 1e6; // µs → s
    double deltaTime  = nowClock - lastWallClock;
    lastTotalTicks    = nowTicks;
    lastWallClock     = nowClock;

    if (deltaTime <= 0.0) return 0.0;
    return (deltaCpu / deltaTime) * 100.0;
}

/// CPU percent for the main Unity thread only.
double bugsnag_unity_performance_main_thread_cpu_percent()
{
    static thread_t mainThread      = mach_thread_self(); // first call happens on main
    static uint64_t lastTotalTicks  = 0;
    static double   lastWallClock   = 0.0;

    thread_basic_info_data_t info;
    mach_msg_type_number_t count = THREAD_BASIC_INFO_COUNT;
    if (thread_info(mainThread, THREAD_BASIC_INFO,
                    (thread_info_t)&info, &count) != KERN_SUCCESS)
        return -1.0;

    uint64_t nowTicks = info.user_time.seconds  * 1e6 + info.user_time.microseconds +
                        info.system_time.seconds * 1e6 + info.system_time.microseconds;
    double   nowClock = timeInSeconds();

    if (lastWallClock == 0) {
        lastTotalTicks = nowTicks;
        lastWallClock  = nowClock;
        return 0.0;
    }

    double deltaCpu   = (double)(nowTicks - lastTotalTicks) / 1e6;
    double deltaTime  = nowClock - lastWallClock;
    lastTotalTicks    = nowTicks;
    lastWallClock     = nowClock;

    if (deltaTime <= 0.0) return 0.0;
    return (deltaCpu / deltaTime) * 100.0;
}

/// Bytes of physical memory in use by this process (resident size)
uint64_t bugsnag_unity_performance_physical_memory_in_use()
{
    mach_task_basic_info_data_t info;
    mach_msg_type_number_t count = MACH_TASK_BASIC_INFO_COUNT;
    if (task_info(mach_task_self(), MACH_TASK_BASIC_INFO,
                  (task_info_t)&info, &count) != KERN_SUCCESS)
        return 0;
    return info.resident_size;
}

/// Total bytes of RAM on the device
uint64_t bugsnag_unity_performance_total_device_memory()
{
    return [[NSProcessInfo processInfo] physicalMemory];
}
} // extern "C"