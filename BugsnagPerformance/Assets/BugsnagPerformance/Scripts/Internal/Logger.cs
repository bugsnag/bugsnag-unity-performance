using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BugsnagUnityPerformance
{
    internal class Logger : MonoBehaviour
    {

        private const bool ENABLED = true;

        private const string PREFIX = "BGS LOG: ";

        public static void I(string msg)
        {
            if (ENABLED)
            {
                Debug.Log(PREFIX + msg);
            }
        }
    }
}