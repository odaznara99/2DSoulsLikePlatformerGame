using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFollowRangeTrigger : MonoBehaviour
{


    [Header("Boss Follow Range Trigger")]
    [SerializeField]private bool playerInArea = false;
    [SerializeField ]private Transform player;

    public bool IsPlayerInArea
    {
        get { return playerInArea; }
    }

    private void Update()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform == player)
        {
            playerInArea = true;
            player = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform == player)
        {
            playerInArea = false;
            player = null;
        }
    }
}
