using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BugsnagUnityPerformance
{
    internal class Logger : MonoBehaviour
    {

        private const string PREFIX = "BGS LOG: ";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static void I(string msg)
        {
            Debug.Log(PREFIX + msg);
        }
    }
}