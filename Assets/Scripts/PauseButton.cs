using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void OnPauseButtonClicked()
    {
        if (GameManager.instance != null)
        {
            if (GameManager.instance.IsGamePaused())
            {
                GameManager.instance.ResumeGame();
            }
            else
            {
                GameManager.instance.PauseGame();
            }
        }
    }
}
