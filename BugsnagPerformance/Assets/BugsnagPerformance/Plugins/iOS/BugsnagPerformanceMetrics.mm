#import <mach/mach.h>
#import <mach/mach_host.h>
#import <mach/mach_time.h>
#import <sys/sysctl.h>
#import <Foundation/Foundation.h>

#pragma mark – helpers
// -------------------------------------------------------------

static inline uint64_t tv_to_us(time_value_t tv)
{
    return ((uint64_t)tv.seconds * 1000000ULL) + tv.microseconds;
}

static inline double monotonic_seconds(void)
{
    static mach_timebase_info_data_t tb;
    static dispatch_once_t once;
    dispatch_once(&once, ^{ mach_timebase_info(&tb); });
    return ((double)mach_absolute_time() * tb.numer) /
           ((double)tb.denom * 1e9);
}

typedef struct {
    uint64_t lastTaskTicks;
    uint64_t lastMainTicks;
    double   lastWall;
} cpu_sample_state_t;

static cpu_sample_state_t g_state = { 0, 0, 0.0 };

static inline double cpu_percent(uint64_t nowTicks,
                                 uint64_t *lastTicks,
                                 double    deltaWall)
{
      /* do the subtraction in *signed* space */
    int64_t diff = (int64_t)nowTicks - (int64_t)*lastTicks;

    /* thread(s) ended → counter shrank → diff < 0 */
    if (diff < 0)
        diff = 0;                        // align with native sampler

    *lastTicks = nowTicks;

    return (deltaWall <= 0.0) ? 0.0
                              : ((double)diff / 1e6) / deltaWall * 100.0;
}

#pragma mark – exported C API
// -------------------------------------------------------------

extern "C" {

/**
 * Both results share the same sampling window and are on the
 * “one-logical-CPU = 100 %” scale.
 */
void bugsnag_unity_performance_cpu_percents(double *processPct,
                                            double *mainPct)
{
    /* ------------ task CPU time (live threads) ------------------ */
    task_thread_times_info_data_t tinfo;
    mach_msg_type_number_t tcount = TASK_THREAD_TIMES_INFO_COUNT;
    if (task_info(mach_task_self(), TASK_THREAD_TIMES_INFO,
                  (task_info_t)&tinfo, &tcount) != KERN_SUCCESS)
    {
        *processPct = *mainPct = -1.0;
        return;
    }
    const uint64_t taskTicks =
        tv_to_us(tinfo.user_time) + tv_to_us(tinfo.system_time);

    /* ------------ main-thread CPU time -------------------------- */
    static thread_t mainThread = mach_thread_self();
    thread_basic_info_data_t thinfo;
    mach_msg_type_number_t   thcount = THREAD_BASIC_INFO_COUNT;
    if (thread_info(mainThread, THREAD_BASIC_INFO,
                    (thread_info_t)&thinfo, &thcount) != KERN_SUCCESS)
    {
        *processPct = *mainPct = -1.0;
        return;
    }
    const uint64_t mainTicks =
        tv_to_us(thinfo.user_time) + tv_to_us(thinfo.system_time);

    /* ------------ convert to % ---------------------------------- */
    const double nowWall   = monotonic_seconds();
    const double deltaWall = nowWall - g_state.lastWall;
    g_state.lastWall       = nowWall;

    *processPct = cpu_percent(taskTicks, &g_state.lastTaskTicks, deltaWall);
    *mainPct    = cpu_percent(mainTicks, &g_state.lastMainTicks, deltaWall);
}

/* ---------------- memory helpers --------------------------------- */

uint64_t bugsnag_unity_performance_physical_memory_in_use(void)
{
    task_vm_info_data_t info;
    mach_msg_type_number_t count = TASK_VM_INFO_COUNT;
    if (task_info(mach_task_self(), TASK_VM_INFO,
                  (task_info_t)&info, &count) != KERN_SUCCESS)
        return 0;
    return info.phys_footprint;   
}

uint64_t bugsnag_unity_performance_total_device_memory(void)
{
    return [[NSProcessInfo processInfo] physicalMemory];
}

} // extern "C"