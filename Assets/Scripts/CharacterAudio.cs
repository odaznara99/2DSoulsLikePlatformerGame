using UnityEngine;

public class CharacterAudio : MonoBehaviour
{
    //public AudioClip footstepSound;
    //public AudioSource audioSource;

    public void PlayFootstep()
    {
        //audioSource.PlayOneShot(footstepSound);
        AudioManager.Instance.PlaySFX("Step");
    }

    public void PlayJump()
    {
        //audioSource.PlayOneShot(jumpSound);
        AudioManager.Instance.PlaySFX("Jump");
    }

    public void PlayRoll()
    {
        //audioSource.PlayOneShot(jumpSound);
        AudioManager.Instance.PlaySFX("Jump");
    }

    public void PlayBlock()
    {
        //audioSource.PlayOneShot(jumpSound);
        AudioManager.Instance.PlaySFX("Block");
    }
}
