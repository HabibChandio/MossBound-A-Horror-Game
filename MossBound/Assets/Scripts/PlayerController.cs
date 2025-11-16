using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCam;
    public GameObject defaultCrosshair;
    public GameObject grabCrosshair;
    public Light flashLight;
    public Transform footstepAudioPosition;
    public AudioSource audioSource;
    public EnemyAI enemyAI;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float gravity = 10f;
    public bool isHiding = false;

    [Header("Camera Settings")]
    public float lookSpeed = 2f;
    public float lookXLimit = 75f;
    public float cameraFocusSpeed = 5f;

    [Header("Zoom")]
    public int zoomFOV = 35;
    public float zoomSmooth = 4f;

    [Header("Flashlight")]
    public GameObject flashLightMesh;
    public float flashLightIntensity = 1f;
    public AudioSource flashLightSource;

    [Header("Footsteps")]
    public AudioClip[] carpetFootstepSounds;
    public float walkNoiseRadius;
    public float runNoiseRadius;

    [Header("Interaction")]
    public float interactDistance = 3f;

    [Header("Background Music")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    private PlayerControls controls;
    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 moveDirection;
    private float rotationX = 0;
    private bool isZoomed = false;
    private bool isFootstepCoroutineRunning = false;
    private AudioClip[] currentFootstepSounds;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;


    public IInteractable currentInteractable;
    public bool isMovementLocked = false;
    public bool isCameraFocused = false;


    private float initialFOV;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.Zoom.performed += ctx => isZoomed = true;
        controls.Player.Zoom.canceled += ctx => isZoomed = false;

        controls.Player.FlashLight.performed += ctx => ToggleFlashlight();
        controls.Player.Interact.performed += ctx => TryInteract();
        controls.Player.Pause.performed += ctx => HandleCancel();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        initialFOV = playerCam.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        flashLight.enabled = false;
        grabCrosshair.SetActive(false);
        currentFootstepSounds = carpetFootstepSounds;

        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    private void Update()
    {
        if (!Application.isFocused) return;
        if (Time.timeScale == 0) return;
        HandleCamera();
        HandleMovement();
        HandleZoom();
        HandleCrosshair();
        HandleFootsteps();
    }

    public void EndInteractionState()
    {
        isMovementLocked = false;
        isCameraFocused = false;
        currentInteractable = null;

        StopCameraFocus();
    }

    private void HandleCancel()
    {
        if (currentInteractable != null)
        {
            currentInteractable.ExitInteraction();
            currentInteractable = null;
        }
        else
        {
            // PauseMenu handles pause separately
            FindFirstObjectByType<PauseMenu>()?.TogglePause();
        }
    }

    private void HandleMovement()
    {
        if (isMovementLocked) return;

        bool isRunning = Keyboard.current.leftShiftKey.isPressed;
        float speed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveDirection.x = move.x * speed;
        moveDirection.z = move.z * speed;

        if (controller.isGrounded)
        {
            moveDirection.y = -0.5f;
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        controller.Move(moveDirection * Time.deltaTime);
    }

    private void HandleCamera()
    {
        if (isCameraFocused) return;

        float mouseX = lookInput.x * lookSpeed;
        float mouseY = lookInput.y * lookSpeed;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleZoom()
    {
        if (isCameraFocused) return;

        playerCam.fieldOfView = Mathf.Lerp(
            playerCam.fieldOfView,
            isZoomed ? zoomFOV : initialFOV,
            Time.deltaTime * zoomSmooth
        );

        float flicker = Mathf.PingPong(Time.time * UnityEngine.Random.Range(5f, 10f), 0.5f);
        flashLight.intensity = flashLightIntensity + flicker;
    }

    private void ToggleFlashlight()
    {
        if(isCameraFocused) return;
        flashLightSource.Play();
        flashLight.enabled = !flashLight.enabled;
    }

    private void HandleCrosshair()
    {
        Vector3 rayOrigin = playerCam.transform.position + playerCam.transform.forward * 0.5f;

        if (Physics.Raycast(rayOrigin, playerCam.transform.forward, out RaycastHit hit, interactDistance))
        {
            IInteractable interactable =
                hit.collider.GetComponent<IInteractable>() ??
                hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                defaultCrosshair.SetActive(false);
                grabCrosshair.SetActive(true);
                return;
            }
        }

        defaultCrosshair.SetActive(true);
        grabCrosshair.SetActive(false);
    }

    private void TryInteract()
    {
        Ray ray = new Ray(
            playerCam.transform.position + playerCam.transform.forward * 0.5f,
            playerCam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            IInteractable interactable =
                hit.collider.GetComponent<IInteractable>() ??
                hit.collider.GetComponentInParent<IInteractable>();

            if (interactable == null) return;

            interactable.Interact();
        }
    }

    public void StartCameraFocus(Transform target)
    {
        isCameraFocused = true;
        originalCameraPosition = playerCam.transform.position;
        originalCameraRotation = playerCam.transform.rotation;
        playerCam.transform.position = target.position;
        playerCam.transform.rotation = target.rotation;
    }

    public void StopCameraFocus()
    {
        isCameraFocused = false;
        isMovementLocked = false;
        playerCam.transform.position = originalCameraPosition;
        playerCam.transform.rotation = originalCameraRotation;
    }

    private void HandleFootsteps()
    {
        if (isMovementLocked) return;

        bool moving = moveInput.magnitude > 0.1f;
        if (moving && controller.isGrounded && !isFootstepCoroutineRunning)
            StartCoroutine(PlayFootsteps(1.3f / (Keyboard.current.leftShiftKey.isPressed ? runSpeed : walkSpeed)));
    }

    private IEnumerator PlayFootsteps(float delay)
    {
        isFootstepCoroutineRunning = true;
        while (moveInput.magnitude > 0.1f && controller.isGrounded)
        {
            if (currentFootstepSounds.Length > 0)
            {
                audioSource.transform.position = footstepAudioPosition.position;
                audioSource.clip = currentFootstepSounds[UnityEngine.Random.Range(0, currentFootstepSounds.Length)];
                audioSource.Play();
                NotifyEnemyOfNoise();
            }
            yield return new WaitForSeconds(delay);
        }
        isFootstepCoroutineRunning = false;
    }

    private void NotifyEnemyOfNoise()
    {
        bool isRunning = Keyboard.current.leftShiftKey.isPressed;
        float radius = isRunning ? runNoiseRadius : walkNoiseRadius;

        if (enemyAI != null)
        {
            float distance = Vector3.Distance(transform.position, enemyAI.transform.position);
            if (distance <= radius)
            {
                enemyAI.InvestigateNoise(transform.position);
            }
        }
    }
}
