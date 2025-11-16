using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeverPuzzle : MonoBehaviour
{
    [Header("Puzzle Setup")]
    public Lever[] levers;
    public Animator safeAnimator;

    [Header("Clue Settings")]
    public GameObject nodePrefab;  
    public Transform clueWallPosition;
    public TMP_Text traversalText;  

    private List<int> correctOrder = new List<int>();
    private List<int> inputOrder = new List<int>();
    private bool puzzleSolved = false;
    public bool IsSolved => puzzleSolved;
    private string chosenTraversalType;
    private TreeNode root;

    private void Start()
    {
        GeneratePuzzle();
        SpawnClue();
    }

    private void GeneratePuzzle()
    {
        int count = levers.Length;
        List<int> available = new List<int>();
        for (int i = 1; i <= count; i++)
            available.Add(i);

        // Fisher-Yates shuffle
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = available[i];
            available[i] = available[j];
            available[j] = temp;
        }

        int[] leverIDs = available.ToArray();

        root = BuildTree(leverIDs);

        string[] traversals = { "Preorder", "Inorder", "Postorder" };
        chosenTraversalType = traversals[Random.Range(0, traversals.Length)];

        correctOrder.Clear();
        switch (chosenTraversalType)
        {
            case "Preorder": PreOrder(root, correctOrder); break;
            case "Inorder": InOrder(root, correctOrder); break;
            case "Postorder": PostOrder(root, correctOrder); break;
        }

        if (traversalText != null)
            traversalText.text = chosenTraversalType;
    }

    private class TreeNode
    {
        public int val;
        public TreeNode left, right;
        public TreeNode(int v) { val = v; }
    }

    private TreeNode BuildTree(int[] arr)
    {
        TreeNode root = new TreeNode(arr[0]);
        if (arr.Length > 1) root.left = new TreeNode(arr[1]);
        if (arr.Length > 2) root.right = new TreeNode(arr[2]);
        if (arr.Length > 3) root.left.left = new TreeNode(arr[3]);
        if (arr.Length > 4) root.left.right = new TreeNode(arr[4]);
        if (arr.Length > 5) root.right.left = new TreeNode(arr[5]);
        return root;
    }

    private void PreOrder(TreeNode node, List<int> list)
    {
        if (node == null) return;
        list.Add(node.val);
        PreOrder(node.left, list);
        PreOrder(node.right, list);
    }

    private void InOrder(TreeNode node, List<int> list)
    {
        if (node == null) return;
        InOrder(node.left, list);
        list.Add(node.val);
        InOrder(node.right, list);
    }

    private void PostOrder(TreeNode node, List<int> list)
    {
        if (node == null) return;
        PostOrder(node.left, list);
        PostOrder(node.right, list);
        list.Add(node.val);
    }

    private void SpawnClue()
    {
        if (nodePrefab == null || clueWallPosition == null) return;

        int[] leverIDs = new int[levers.Length];
        for (int i = 0; i < levers.Length; i++)
            leverIDs[i] = levers[i].leverID;

        Vector3 rootPos = clueWallPosition.position + new Vector3(0, 0.6f, 0);

        Dictionary<int, Color> idToColor = new Dictionary<int, Color>
    {
        {1, Color.yellow},
        {2, Color.blue},
        {3, Color.green},
        {4, Color.red},
        {5, new Color(1f, 0.5f, 0f)},
        {6, Color.magenta}
    };

        float verticalSpacing = -0.5f;
        float initialHSpacing = 0.6f;

        void SpawnNode(TreeNode node, Vector3 pos, float hSpacing)
        {
            if (node == null) return;

            GameObject n = Instantiate(nodePrefab, pos, Quaternion.identity, clueWallPosition);
            if (idToColor.TryGetValue(node.val, out Color c))
            {
                Renderer rend = n.GetComponent<Renderer>();
                if (rend != null) rend.material.color = c;
            }

            float childHSpacing = hSpacing / 1.5f; 

            if (node.left != null)
            {
                Vector3 leftPos = pos + new Vector3(-childHSpacing, verticalSpacing, 0);
                DrawLine(pos, leftPos);
                SpawnNode(node.left, leftPos, childHSpacing);
            }

            if (node.right != null)
            {
                Vector3 rightPos = pos + new Vector3(childHSpacing, verticalSpacing, 0);
                DrawLine(pos, rightPos);
                SpawnNode(node.right, rightPos, childHSpacing);
            }
        }

        SpawnNode(root, rootPos, initialHSpacing);

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 lookDir = mainCam.transform.position - clueWallPosition.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0)
                clueWallPosition.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    private Material lineMaterial;

    private void DrawLine(Vector3 start, Vector3 end)
    {
        GameObject line = new GameObject("Line");
        line.transform.parent = clueWallPosition;

        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.startWidth = lr.endWidth = 0.03f;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        if (lineMaterial == null)
            lineMaterial = Resources.Load<Material>("Materials/LineMaterial");

        lr.material = lineMaterial;

        Color lineColor = new Color(1f, 1f, 1f, 0.5f);
        lr.startColor = lr.endColor = lineColor;
    }

    public void OnLeverFlipped(Lever lever)
    {
        if (puzzleSolved) return;

        inputOrder.Add(lever.leverID);

        if (inputOrder.Count >= levers.Length)
            CheckOrder();
    }

    private void CheckOrder()
    {
        bool correct = true;
        for (int i = 0; i < correctOrder.Count; i++)
        {
            if (inputOrder[i] != correctOrder[i])
            {
                correct = false;
                break;
            }
        }

        if (correct)
            SolvePuzzle();
        else
            ResetLevers();
    }

    private void SolvePuzzle()
    {
        puzzleSolved = true;
        if (safeAnimator != null)
            safeAnimator.SetBool("Open", true);

        foreach (var lever in levers)
            lever.LockLever();
    }

    private void ResetLevers()
    {
        inputOrder.Clear();
        StartCoroutine(ResetLeversCoroutine());
    }

    private IEnumerator ResetLeversCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (var lever in levers)
            lever.ResetLever();
    }
}
