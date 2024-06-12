using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcedRestartSceneController : MonoBehaviour
{
    private void Awake()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
