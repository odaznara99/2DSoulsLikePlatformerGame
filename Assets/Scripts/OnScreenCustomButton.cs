using UnityEngine;

public class OnScreenCustomButton : MonoBehaviour
{
    private PlayerControllerVersion2 playerController;
     void Awake()
    {
        playerController = FindAnyObjectByType<PlayerControllerVersion2>();
        if (playerController == null)
        {
            Debug.LogError("PlayerControllerVersion2 not found in the scene.");
        }
    }

    public void OnPlayerJump() { 
        playerController.OnJump();
    }
    public void OnPlayerAttack() { 
        playerController.AttackActions();
    }
    public void OnPlayerShieldPressed() { 
        playerController.OnHoldShield();
    }

    public void OnPlayerShieldRelease()
    {
        playerController.OnNeutral();
    }
    public void OnPlayerRoll() { 
        playerController.OnRoll();
    }
    public void OnPlayerInteract() { 
        //playerController.OnInteract();
    }

    public void OnPlayerMoveLeft() { 
        playerController.SetFloatInputX(-1f);
    }

    public void OnPlayerMoveRight()
    {
        playerController.SetFloatInputX(1f);
    }

    public void OnPlayerMoveStop()
    {
        //playerController.OnNeutral();
        playerController.SetFloatInputX(0f);
    }

    public void OnPlayerMoveDown()
    {
        playerController.SetFloatInputY(-1f);
    }

    public void OnPlayerMoveDownStop()
    {
        playerController.SetFloatInputY(0f);
    }
}
