using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class CabinetScript : MonoBehaviour, IInteractable
{
    [Header("References")]
    public Animator cabinetAnimator;
    public Transform cameraPoint; 
    public PlayerController playerController;
    public AudioSource audioSource;
    public AudioClip openSFX;
    public AudioClip closeSFX;

    [Header("Settings")]
    public bool isHiding = false;
    private bool canInteract = true; 

    public bool RequiresCameraFocus => true;
    public bool RequiresMovementStop => true;

    public void Interact()
    {
        if (!canInteract) return;

        if (!isHiding)
            StartCoroutine(EnterCabinetRoutine());
        else
            StartCoroutine(ExitCabinetRoutine());
    }

    public void ExitInteraction()
    {
        playerController.currentInteractable = null;
        if (isHiding && canInteract)
            StartCoroutine(ExitCabinetRoutine());
    }

    private IEnumerator EnterCabinetRoutine()
    {
        canInteract = false;
        isHiding = true;
        playerController.isHiding = isHiding;

        cabinetAnimator.Play("CabinetOpen");
        yield return new WaitForSeconds(GetAnimationLength("CabinetOpen"));

        if (playerController != null && cameraPoint != null)
        {
            playerController.flashLightMesh.gameObject.SetActive(false);
            playerController.flashLight.gameObject.SetActive(false);

            playerController.StartCameraFocus(cameraPoint);
            playerController.isCameraFocused = RequiresCameraFocus;
            playerController.isMovementLocked = RequiresMovementStop;
            playerController.currentInteractable = this;
        }

        cabinetAnimator.Play("CabinetClose");
        yield return new WaitForSeconds(GetAnimationLength("CabinetClose"));

        canInteract = true;
    }

    private IEnumerator ExitCabinetRoutine()
    {
        canInteract = false;
        isHiding = false;
        playerController.isHiding = isHiding;

        cabinetAnimator.Play("CabinetOpen");
        yield return new WaitForSeconds(GetAnimationLength("CabinetOpen"));

        if (playerController != null)
        {

            playerController.flashLightMesh.gameObject.SetActive(true);
            playerController.flashLight.gameObject.SetActive(true);
            playerController.StopCameraFocus();

            playerController.isCameraFocused = false;
            playerController.isMovementLocked = false;
            playerController.currentInteractable = null;
        }

        cabinetAnimator.Play("CabinetClose");
        yield return new WaitForSeconds(GetAnimationLength("CabinetClose"));

        canInteract = true;
    }


    private float GetAnimationLength(string clipName)
    {
        if (cabinetAnimator == null || cabinetAnimator.runtimeAnimatorController == null)
            return 1f;

        foreach (var clip in cabinetAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }

        return 1f; // fallback
    }
    public void PlayOpenSFX()
    {
        if (audioSource != null && openSFX != null)
            audioSource.PlayOneShot(openSFX);
    }

    public void PlayCloseSFX()
    {
        if (audioSource != null && closeSFX != null)
            audioSource.PlayOneShot(closeSFX);
    }
}
