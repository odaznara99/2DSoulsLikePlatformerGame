using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerFightingMusic : MonoBehaviour
{
    public LayerMask playerLayer;
    public Transform bossReference;


    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            if (bossReference != null)
            {
                AudioManager.Instance.StopMusic();
                AudioManager.Instance.PlayFightMusic();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            if (bossReference != null)
            {
                AudioManager.Instance.PlayMusic("Ballad");
                AudioManager.Instance.StopFightMusic();
            }
            
        }
    }
}
