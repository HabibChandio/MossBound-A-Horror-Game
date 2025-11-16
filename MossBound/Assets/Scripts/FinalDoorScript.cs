using System.Collections;
using UnityEngine;

public class FinalDoorController : MonoBehaviour, IInteractable
{
    [Header("Locks")]
    public FinalDoorLock[] locks;
    private int unlockedLocks = 0;

    [Header("Keys")]
    public int keyCount = 0;

    [Header("Door Animators")]
    public Animator leftDoor;
    public Animator rightDoor;
    public string openBool = "Open";
    public float waitBeforeEnemy = 1.5f;

    [Header("SFX")]
    public AudioSource doorOpenSFX;

    [Header("Jumpscare")]
    public JumpscareController jumpscareController;

    private bool doorOpened = false;

    public bool RequiresCameraFocus => false;
    public bool RequiresMovementStop => false;

    public void OnLockUnlocked()
    {
        unlockedLocks++;
    }

    public void Interact()
    {
        if (doorOpened) return;

        if (unlockedLocks < locks.Length)
        {
            return;
        }

        doorOpened = true;

        if (leftDoor) leftDoor.SetBool(openBool, true);
        if (rightDoor) rightDoor.SetBool(openBool, true);

        if (doorOpenSFX) doorOpenSFX.Play();

        StartCoroutine(DelayedEnemyTeleport());
    }

    public void ExitInteraction() { }

    private IEnumerator DelayedEnemyTeleport()
    {
        yield return new WaitForSeconds(waitBeforeEnemy);

        jumpscareController.StartJumpscare();
    }
}
