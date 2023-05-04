using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class OtherSceneController : MonoBehaviour
{
    public void LoadMainScene()
    {
        BugsnagSceneManager.LoadScene(0);
    }
}
