using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PadlockScript : MonoBehaviour, IInteractable
{
    [Header("Ruller (Dial) References")]
    public List<GameObject> rullers; 
    public int numberOfDials = 4;

    [Header("Puzzle References")]
    public GameObject chest;  
    public Transform cameraFocusPoint;  
    public PlayerController playerController;

    [Header("Emission Blink Settings")]
    public float emissionBlinkTime = 1.2f;
    public Color emissionColorBase = new Color(1f, 0.85f, 0.4f);
    

    [Header("Heap Puzzle Settings")]
    public string clueWallText;
    public TextMeshPro clueTextObject;

    private List<int> startingHeap = new List<int>();
    private List<int> insertedValues = new List<int>(); 

    private int currentDialIndex = 0;
    private int[] currentNumbers;
    private bool isPlayerInteracting = false;

    public bool RequiresCameraFocus => true;
    public bool RequiresMovementStop => true;

    private int[] numberPassword = { 0, 0, 0, 0 };


    private List<Renderer> dialRenderers = new List<Renderer>();

    void Awake()
    {
        currentNumbers = new int[numberOfDials];

        // Cache renderers and enable emission
        foreach (var dial in rullers)
        {
            Renderer r = dial.GetComponent<Renderer>();
            Material matInstance = new Material(r.sharedMaterial);
            r.material.EnableKeyword("_EMISSION");
            dialRenderers.Add(r);
        }
    }

    void Start()
    {
        GenerateHeapPuzzle();
        GenerateClueWallText();
        clueTextObject.text = clueWallText;
    }

    void Update()
    {
        if (!isPlayerInteracting) return;

        UpdateEmissionEffects();
        HandleDialSelection();
        HandleDialRotation();
    }

    private void UpdateEmissionEffects()
    {
        for (int i = 0; i < dialRenderers.Count; i++)
        {
            Renderer r = dialRenderers[i];

            if (i == currentDialIndex)
            {
                float t = Mathf.PingPong(Time.time, emissionBlinkTime) / emissionBlinkTime;
                Color emission = emissionColorBase * Mathf.Lerp(0.1f, 0.15f, t);
                r.material.SetColor("_EmissionColor", emission);
            }
            else
            {
                r.material.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    private void HandleDialSelection()
    {
        if (Keyboard.current.dKey.wasPressedThisFrame)
            currentDialIndex = (currentDialIndex + 1) % numberOfDials;

        if (Keyboard.current.aKey.wasPressedThisFrame)
            currentDialIndex = (currentDialIndex - 1 + numberOfDials) % numberOfDials;
    }

    private void HandleDialRotation()
    {
        bool changed = false;

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            RotateCurrentDial(1);
            changed = true;
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            RotateCurrentDial(-1);
            changed = true;
        }

        if (changed)
            CheckPassword();
    }

    private void RotateCurrentDial(int direction)
    {
        int rotateAmount = 36 * direction;
        rullers[currentDialIndex].transform.Rotate(-rotateAmount, 0, 0, Space.Self);

        currentNumbers[currentDialIndex] += direction;

        if (currentNumbers[currentDialIndex] > 9) currentNumbers[currentDialIndex] = 0;
        if (currentNumbers[currentDialIndex] < 0) currentNumbers[currentDialIndex] = 9;
    }

    private void CheckPassword()
    {
        if (currentNumbers.SequenceEqual(numberPassword))
        {
            ExitInteraction();
            gameObject.SetActive(false);

            if (chest != null)
                chest.GetComponent<Animator>().enabled = true;
        }
    }

    public void Interact()
    {
        if (isPlayerInteracting) 
        {
            ExitInteraction();
            return;
        }

        isPlayerInteracting = true;

        playerController.flashLightMesh.gameObject.SetActive(false);
        playerController.flashLight.gameObject.SetActive(false);

        playerController.StartCameraFocus(cameraFocusPoint);
        playerController.isCameraFocused = RequiresCameraFocus;
        playerController.isMovementLocked = RequiresMovementStop;
        playerController.currentInteractable = this;
    }

    public void ExitInteraction()
    {
        isPlayerInteracting = false;
        playerController.currentInteractable = null;

        playerController.flashLightMesh.gameObject.SetActive(true);
        playerController.flashLight.gameObject.SetActive(true);

        playerController.StopCameraFocus();
    }

    private void GenerateHeapPuzzle()
    {
        HashSet<int> unique = new HashSet<int>();
        System.Random rng = new System.Random();

        while (unique.Count < 7)
            unique.Add(rng.Next(0, 10));

        var all = unique.ToList();

        startingHeap = all.Take(3).ToList();

        int maxVal = startingHeap.Max();
        int maxIndex = startingHeap.IndexOf(maxVal);
        if (maxIndex != 0)
        {
            int temp = startingHeap[0];
            startingHeap[0] = startingHeap[maxIndex];
            startingHeap[maxIndex] = temp;
        }

        insertedValues = all.Skip(3).Take(4).ToList();

        List<int> heap = new List<int>(startingHeap);

        foreach (int v in insertedValues)
            HeapInsert(heap, v);

        int firstLeaf = heap.Count / 2;
        numberPassword = heap.Skip(firstLeaf).Take(4).ToArray();
    }

    private void HeapInsert(List<int> heap, int value)
    {
        heap.Add(value);
        int i = heap.Count - 1;

        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (heap[parent] >= heap[i]) break;

            // swap
            int temp = heap[parent];
            heap[parent] = heap[i];
            heap[i] = temp;

            i = parent;
        }
    }

    private void GenerateClueWallText()
    {
        clueWallText =
        "       MAX-HEAP\n\n" +
        $"        {startingHeap[0]}\n" +
        $"       /   \\\n" +
        $"     {startingHeap[1]}     {startingHeap[2]}\n\n" +
        "Insert (in order):\n" +
        $"{string.Join(", ", insertedValues)}\n\n" +
        "leaf nodes\n" +
        "of the final heap.";
    }
}
