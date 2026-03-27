using System;
using UnityEngine;

public class EnableGlowCheckpoint : MonoBehaviour
{
    [SerializeField]
    private GameObject glowObject;
    [SerializeField]
    private GameObject lightObject;

    public void DisableGlowingLights()
        {
        if (glowObject != null)
        {
            glowObject.SetActive(false);
        }
        if (lightObject != null)
        {
            lightObject.SetActive(false);
        }
    }

    public void EnableGlowingLights()
    {
        if (glowObject != null)
        {
            glowObject.SetActive(true);
        }
        if (lightObject != null)
        {
            lightObject.SetActive(true);
        }
    }
}
