using UnityEditor;
using UnityEngine;


[InitializeOnLoad]
public class AddScriptingSymbol : MonoBehaviour
{

    private const string DEFINE_SYMBOL = "BUGSNAG_PERFORMANCE";

    private static BuildTargetGroup[] _supportedPlatforms = { BuildTargetGroup.Android, BuildTargetGroup.Standalone, BuildTargetGroup.iOS, BuildTargetGroup.WebGL };

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
            var existingSymbols = BugsnagPlayerSettingsCompat.GetScriptingDefineSymbols(buildTargetGroup);
            if (string.IsNullOrEmpty(existingSymbols))
            {
                existingSymbols = DEFINE_SYMBOL;
            }
            else if (!existingSymbols.Contains(DEFINE_SYMBOL))
            {
                existingSymbols += ";" + DEFINE_SYMBOL;
            }
            BugsnagPlayerSettingsCompat.SetScriptingDefineSymbols(buildTargetGroup, existingSymbols);
        }
}
