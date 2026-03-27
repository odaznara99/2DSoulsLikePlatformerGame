using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToLastCheckPoint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnLastCheckPoint()
    {
        AudioManager.Instance.PlaySFX("Click");

        if (SaveManager.Instance != null && SaveManager.Instance.HasSave() &&SaveManager.Instance.LoadSavedGame())
        {
            Debug.Log("Loaded save from disk.");
            return;
        }

        Debug.LogWarning("No checkpoint or saved game found.");
        //SceneLoader.Instance.LoadScene("Stage1");
        MessageManager.Instance.ShowMessage("No checkpoint or saved game found.", false, 24);
    }
}
