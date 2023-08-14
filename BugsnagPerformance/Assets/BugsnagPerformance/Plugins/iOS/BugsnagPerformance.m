#import <sys/types.h>
#import <sys/sysctl.h>
#import <stdint.h>

#define CHECK_SYSCTL_NAME(TYPE, CALL)                                          \
    if (0 != (CALL)) {                                                         \
        NSLog(@"Could not get %s value for %s: %s", #CALL, name,      \
                        strerror(errno));                                      \
        return 0;                                                              \
    }

static NSString *CPUArchForCPUType(int32_t cpuType, int32_t subType) {
    switch (cpuType) {
    case CPU_TYPE_ARM: {
        switch (subType) {
        case CPU_SUBTYPE_ARM_V6:
            return @"armv6";
        case CPU_SUBTYPE_ARM_V7:
            return @"armv7";
        case CPU_SUBTYPE_ARM_V7F:
            return @"armv7f";
        case CPU_SUBTYPE_ARM_V7K:
            return @"armv7k";
#ifdef CPU_SUBTYPE_ARM_V7S
        case CPU_SUBTYPE_ARM_V7S:
            return @"armv7s";
#endif
        case CPU_SUBTYPE_ARM_V8:
            return @"armv8";
        }
        break;
    }
    case CPU_TYPE_ARM64: {
        switch (subType) {
        case CPU_SUBTYPE_ARM64E:
            return @"arm64e";
        default:
            return @"arm64";
        }
    }
    case CPU_TYPE_ARM64_32: {
        // Ignore arm64_32_v8 subtype
        return @"arm64_32";
    }
    case CPU_TYPE_X86:
        return @"x86";
    case CPU_TYPE_X86_64:
        return @"x86_64";
    }
    return nil;
}

static int32_t bsgsysctl_int32ForName(const char *const name) {
    int32_t value = 0;
    size_t size = sizeof(value);
    CHECK_SYSCTL_NAME(int32, sysctlbyname(name, &value, &size, NULL, 0));
    return value;
}

static char* convertNSStringToCString(const NSString* nsString)
{
    if (nsString == NULL)
        return NULL;
    // create a copy because otherwise we are tied to nsString's lifetime
    return strdup([nsString UTF8String]);
}

static NSString* getCpuArch(void) {
    return CPUArchForCPUType(bsgsysctl_int32ForName("hw.cputype"),
                             bsgsysctl_int32ForName("hw.cpusubtype"));
}

char* bugsnag_unity_performance_getBundleVersion()
{
    NSDictionary *info = [[NSBundle mainBundle] infoDictionary];
    NSString *version = [info objectForKey:@"CFBundleVersion"];
    return convertNSStringToCString(version);
}

char* bugsnag_unity_performance_get_arch () 
{
    return convertNSStringToCString(getCpuArch());
}

char* bugsnag_unity_performance_get_os_version()
{
    return convertNSStringToCString(UIDevice.currentDevice.systemVersion);
}
