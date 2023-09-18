using System;
using System.Collections;
using System.Collections.Generic;
using Codice.Utils;
using System.Text.RegularExpressions;
using UnityEngine;

public class WindowsNative : MonoBehaviour
{
    public static string GetOsVersion()
    {
        // we expect that windows version strings look like:
        // "Microsoft Windows NT 10.0.17134.0"
        // if it does then we can parse out the version number into a separate field
        var matches = Regex.Match(Environment.OSVersion.VersionString, "\\A(?<osName>[a-zA-Z ]*) (?<osVersion>[\\d\\.]*)\\z");
        if (matches.Success)
        {
            return matches.Groups["osVersion"].Value;
        }
        else
        {
            return Environment.OSVersion.VersionString;
        }
    }

    public static string GetArch()
    {
        if (Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") == "AMD64"
        || Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432") == "AMD64")
        {
            return "64-bit";
        }
        else
        {
            return "32-bit";
        }
    }

}
