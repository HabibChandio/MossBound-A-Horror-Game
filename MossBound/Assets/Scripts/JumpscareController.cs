using UnityEngine;
using System.Collections;

public class JumpscareController : MonoBehaviour
{
    [Header("References")]
    public Camera mainCam;
    public Camera jumpCam;
    public PlayerController playerController;
    public RetryMenu retryMenu;

    [Header("Enemy")]
    public GameObject enemy;

    [Header("Jumpscare Animation")]
    public Animator jumpAnimator;
    public AnimationClip jumpscareClip;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpscareSound;

    private bool active = false;

    public void StartJumpscare()
    {
        if (active) return;
        StartCoroutine(JumpscareRoutine());
    }

    private IEnumerator JumpscareRoutine()
    {
        active = true; 

        if (enemy != null)
            enemy.SetActive(false);

        if (playerController != null)
            playerController.enabled = false;

        if (mainCam != null) mainCam.gameObject.SetActive(false);
        if (jumpCam != null) jumpCam.gameObject.SetActive(true);

        if (audioSource != null && jumpscareSound != null)
        { 
            audioSource.clip = jumpscareSound;
            audioSource.Play();
        }

        float len = jumpscareClip != null ? jumpscareClip.length : 2f;
        yield return new WaitForSecondsRealtime(len);

        if (mainCam != null) mainCam.gameObject.SetActive(true);
        if (jumpCam != null) jumpCam.gameObject.SetActive(false);

        if (retryMenu != null)
            retryMenu.ShowRetryScreen();
    }
}
