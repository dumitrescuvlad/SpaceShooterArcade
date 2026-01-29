using UnityEngine;

public class FullscreenManager : MonoBehaviour
{
    void Start()
    {
        Screen.fullScreen = true; 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
    }
}
