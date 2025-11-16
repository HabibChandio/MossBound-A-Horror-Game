using System.Collections;
using UnityEngine;

public class DoorScript : MonoBehaviour, IInteractable
{
    [Header("Door Animators")]
    public Animator leftDoor;
    public Animator rightDoor;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSFX;
    public AudioClip closeSFX;

    private bool isOpen = false;
    private bool isBusy = false;

    public bool RequiresCameraFocus => false;
    public bool RequiresMovementStop => false;

    public void Interact()
    {
        if (isBusy) return;

        if (!isOpen)
            StartCoroutine(Open());
        else
            StartCoroutine(Close());
    }

    private IEnumerator Open()
    {
        isBusy = true;
        isOpen = true;

        leftDoor.SetBool("isOpen", true);
        rightDoor.SetBool("isOpen", true);

        PlayOpenSFX();

        yield return new WaitForSeconds(GetAnimLength());

        isBusy = false;
    }

    private IEnumerator Close()
    {
        isBusy = true;
        isOpen = false;

        leftDoor.SetBool("isOpen", false);
        rightDoor.SetBool("isOpen", false);

        PlayCloseSFX();

        yield return new WaitForSeconds(GetAnimLength());

        isBusy = false;
    }

    private float GetAnimLength()
    {
        leftDoor.Update(0);
        rightDoor.Update(0);

        float leftLen = leftDoor.GetCurrentAnimatorClipInfo(0)[0].clip.length;
        float rightLen = rightDoor.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        return Mathf.Max(leftLen, rightLen);
    }

    private void PlayOpenSFX()
    {
        if (audioSource != null && openSFX != null)
            audioSource.PlayOneShot(openSFX);
    }

    private void PlayCloseSFX()
    {
        if (audioSource != null && closeSFX != null)
            audioSource.PlayOneShot(closeSFX);
    }

    public void ExitInteraction() { }
}
