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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToLastCheckpoint();
        }
    }
}
