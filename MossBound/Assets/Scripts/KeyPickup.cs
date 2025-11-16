using System.Collections;
using UnityEngine;

public class KeyPickup : MonoBehaviour, IInteractable
{
    public FinalDoorController doorController;
    public AudioSource pickupSFX;

    public bool RequiresCameraFocus => false;
    public bool RequiresMovementStop => false;

    public void Interact()
    {
        doorController.keyCount++;

        pickupSFX.PlayOneShot(pickupSFX.clip);

        StartCoroutine(DisableAfterSound());
    }

    private IEnumerator DisableAfterSound()
    {
        yield return new WaitForSeconds(pickupSFX.clip.length);
        gameObject.SetActive(false);
    }

    public void ExitInteraction() { }
}
