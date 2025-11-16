using UnityEngine;
using System.Collections;

public class FinalDoorLock : MonoBehaviour, IInteractable
{
    public FinalDoorController doorController;
    public AudioSource unlockSFX;

    public bool RequiresCameraFocus => false;
    public bool RequiresMovementStop => false;

    public void Interact()
    {
        if (doorController.keyCount <= 0)
        {
            return;
        }

        doorController.keyCount--;

        if (unlockSFX)
        {
            unlockSFX.Play();
            StartCoroutine(DisableAfterSound(unlockSFX.clip.length));
        }
        else
        {
            gameObject.SetActive(false);
        }

        doorController.OnLockUnlocked();
    }

    private IEnumerator DisableAfterSound(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    public void ExitInteraction() { }
}
