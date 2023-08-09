using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BugsnagUnityPerformance
{
    internal class Logger : MonoBehaviour
    {

        private const bool ENABLED = false;

        private const string PREFIX = "BGS LOG: ";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static void I(string msg)
        {
            if (ENABLED)
            {
#pragma warning disable CS0162 // Unreachable code detected
                Debug.Log(PREFIX + msg);
#pragma warning restore CS0162 // Unreachable code detected
            }
        }
    }
}