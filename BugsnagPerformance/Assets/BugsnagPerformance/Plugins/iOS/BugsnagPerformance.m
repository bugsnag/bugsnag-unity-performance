#import <sys/types.h>
#import <sys/sysctl.h>

char* convertNSStringToCString(const NSString* nsString)
{
    if (nsString == NULL)
        return NULL;

    const char* nsStringUtf8 = [nsString UTF8String];
    //create a null terminated C string on the heap so that our string's memory isn't wiped out right after method's return
    char* cString = (char*)malloc(strlen(nsStringUtf8) + 1);
    strcpy(cString, nsStringUtf8);

    return cString;
}

char* bugsnag_performance_getBundleVersion()
{
    NSDictionary *info = [[NSBundle mainBundle] infoDictionary];
    NSString *version = [info objectForKey:@"CFBundleVersion"];
    return convertNSStringToCString(version);
}

char* bugsnag_performance_get_arch () 
{

     size_t size;
    sysctlbyname("hw.cpusubtype", NULL, &size, NULL, 0);
    char *machine = malloc(size);
    sysctlbyname("hw.cpusubtype", machine, &size, NULL, 0);
    
    NSString *cpuArch = [NSString stringWithUTF8String:machine];
    
    free(machine);
    
    return convertNSStringToCString(cpuArch);
}



