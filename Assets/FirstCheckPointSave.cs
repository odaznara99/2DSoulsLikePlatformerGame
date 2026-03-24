using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstCheckPointSave : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // The very first checkpoint save, right at the start of the game. This ensures that if the player dies immediately, they will respawn at the beginning of the level.
        GameManager.Instance.SaveCheckpointSnapshot();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
