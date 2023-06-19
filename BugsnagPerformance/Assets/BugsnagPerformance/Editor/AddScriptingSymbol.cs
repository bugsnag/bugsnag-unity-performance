using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static log4net.Appender.ColoredConsoleAppender;

[InitializeOnLoad]
public class AddScriptingSymbol : MonoBehaviour
{

    private const string DEFINE_SYMBOL = "BUGSNAG_PERFORMANCE";

    private static BuildTargetGroup[] _supportedPlatforms = { BuildTargetGroup.Android, BuildTargetGroup.Standalone, BuildTargetGroup.iOS, BuildTargetGroup.WebGL};

    static AddScriptingSymbol()
    {
        foreach (var target in _supportedPlatforms)
        {
            try
            {
                SetScriptingSymbol(target);
            }
            catch
            {
                // Some users might not have a platform installed, in that case ignore the error
            }
        }
    }

    static void SetScriptingSymbol(BuildTargetGroup buildTargetGroup)
    {
        var existingSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        if (!existingSymbols.Contains(DEFINE_SYMBOL))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup,existingSymbols + ";" + DEFINE_SYMBOL);
        }
    }
}
