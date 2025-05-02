#import <mach/mach.h>
#import <mach/mach_host.h>
#import <mach/mach_time.h>
#import <Foundation/Foundation.h>

#pragma mark ‑‑ helpers
// --------------------------------------------------------------------

// Convert a Mach time_value_t (sec / µsec) pair to micro‑seconds
static inline uint64_t tv_to_us(time_value_t tv)
{
    return ((uint64_t)tv.seconds * 1000000ULL) + tv.microseconds;
}

// Fast monotonic clock → seconds (double)
static inline double monotonic_seconds(void)
{
    static mach_timebase_info_data_t tb;
    mach_timebase_info(&tb);
    return ((double)mach_absolute_time() * tb.numer) /
           ((double)tb.denom * 1e9);
}

// Generic sampler that turns “total ticks” into “% since last call”
static inline double cpu_percent(uint64_t   nowTicks,
                                 uint64_t & lastTicks,
                                 double   & lastWall)
{
    const double nowWall = monotonic_seconds();

    if (lastWall == 0.0) {        // first sample
        lastTicks = nowTicks;
        lastWall  = nowWall;
        return 0.0;
    }

    const double deltaCpu  = (double)(nowTicks - lastTicks) / 1e6; // µs → s
    const double deltaWall = nowWall - lastWall;

    lastTicks = nowTicks;
    lastWall  = nowWall;

    return (deltaWall <= 0.0) ? 0.0 : (deltaCpu / deltaWall) * 100.0;
}

#pragma mark ‑‑ exported C API
// --------------------------------------------------------------------

extern "C" {

double bugsnag_unity_performance_process_cpu_percent(void)
{
    static uint64_t lastTicks = 0;
    static double   lastWall  = 0.0;

    task_basic_info_data_t tinfo;
    mach_msg_type_number_t count = TASK_BASIC_INFO_COUNT;
    if (task_info(mach_task_self(), TASK_BASIC_INFO,
                  (task_info_t)&tinfo, &count) != KERN_SUCCESS)
        return -1.0;

    const uint64_t nowTicks =
        tv_to_us(tinfo.user_time) + tv_to_us(tinfo.system_time);

    return cpu_percent(nowTicks, lastTicks, lastWall);
}

double bugsnag_unity_performance_main_thread_cpu_percent(void)
{
    static thread_t  mainThread = mach_thread_self(); // captured on first call
    static uint64_t  lastTicks  = 0;
    static double    lastWall   = 0.0;

    thread_basic_info_data_t info;
    mach_msg_type_number_t   count = THREAD_BASIC_INFO_COUNT;
    if (thread_info(mainThread, THREAD_BASIC_INFO,
                    (thread_info_t)&info, &count) != KERN_SUCCESS)
        return -1.0;

    const uint64_t nowTicks =
        tv_to_us(info.user_time) + tv_to_us(info.system_time);

    return cpu_percent(nowTicks, lastTicks, lastWall);
}

uint64_t bugsnag_unity_performance_physical_memory_in_use(void)
{
    mach_task_basic_info_data_t info;
    mach_msg_type_number_t      count = MACH_TASK_BASIC_INFO_COUNT;
    if (task_info(mach_task_self(), MACH_TASK_BASIC_INFO,
                  (task_info_t)&info, &count) != KERN_SUCCESS)
        return 0;
    return info.resident_size;
}

uint64_t bugsnag_unity_performance_total_device_memory(void)
{
    return [[NSProcessInfo processInfo] physicalMemory];
}

} // extern "C"
