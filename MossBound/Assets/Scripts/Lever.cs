using System.Collections;
using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    [Header("Lever Settings")]
    public Animator animator;
    public AudioSource audioSource;
    public int leverID;
    public bool isFlipped = false; 
    private bool isFlipping = false;
    public bool canInteract = true;
    [Header("Puzzle Reference")]
    public LeverPuzzle puzzle;

    public bool RequiresCameraFocus => false;
    public bool RequiresMovementStop => false;

    private void Start()
    {
        animator.SetBool("Flipped", isFlipped);
    }

    public void Interact()
    {
        if (!canInteract || isFlipping) return;
        if (isFlipped) return;

        StartCoroutine(FlipLever());
    }

    private IEnumerator FlipLever()
    {
        isFlipping = true;
        canInteract = false;

        isFlipped = true;
        animator.SetBool("Flipped", true);

        float animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);

        if (puzzle != null)
            puzzle.OnLeverFlipped(this);

        canInteract = !puzzle.IsSolved;

        isFlipping = false;
    }

    public void ExitInteraction() { }

    public void ResetLever()
    {
        StopAllCoroutines();
        isFlipped = false;
        isFlipping = false;
        canInteract = true;
        animator.SetBool("Flipped", false);

        gameObject.tag = "Interactable";
    }

    public void LockLever()
    {
        canInteract = false;
        gameObject.tag = "Untagged"; 
    }

    public void PlayLeverSFX()
    {
        if(audioSource != null)
            audioSource.Play();
    }
}
