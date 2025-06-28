using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class UIButtonsManager : MonoBehaviour
{
    // Start is called before the first frame update
    public PlayerController playerController;
    public GameObject buttonAttack;
    public GameObject buttonBlock;
    public GameObject buttonRoll;

    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {

        //buttonAttack.button.SetEnabled(playerController.isAttacking);
        //buttonBlock.SetEnabled(playerController.m_grounded);
        buttonAttack.SetActive(!playerController.isAttacking);
        buttonBlock.SetActive(playerController.m_grounded);
        buttonRoll.SetActive(!playerController.m_rolling);





    }
}
