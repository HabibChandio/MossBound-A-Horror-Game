# MossBound - A Horror Game

MossBound is a first-person horror game where players navigate dark environments, solve puzzles, and avoid a terrifying moss monster. This game was built as a course project in Unity.

---

## Project Overview

MossBound is a first-person horror escape game developed in Unity. The player must collect three keys—one obtained directly and two acquired through puzzle-solving—to unlock the final gate and “escape” the environment. The project demonstrates the practical use of classical data structures, including a binary tree and a heap, which form the basis of the game’s puzzle mechanics. The map was created in Blender, while most additional 3D models were sourced from Sketchfab. The game blends atmospheric exploration with algorithm-driven challenges to deliver an engaging horror experience.

---

## 1. Introduction & Gameplay Overview

Players explore a dark and foggy environment, interact with objects (doors, cabinets, levers, padlocks, keys), avoid an enemy that patrols and investigates noise, and solve puzzles to progress. The game uses Unity's new Input System and standard components such as CharacterController and NavMeshAgent for player and enemy movement.

**Core Objective:**  
Collect 3 keys → open 3 locks on the final door → trigger final jumpscare/escape.

**Key Gameplay Systems:**  
- Movement, looking, flashlight, zooming  
- Enemy AI with patrolling/investigating/chasing  
- Interaction interface `IInteractable` used by all interactable objects  
- Two puzzles: a heap-based padlock and a lever-traversal puzzle

**Controls:**  
- `W / A / S / D` – Move player  
- Mouse movement – Look around  
- `E` – Interact with objects  
- `Esc` – Pause game / exit interaction  
- `F` – Toggle flashlight  
- `Left Shift` – Sprint / run  

---

## 2. Scripts

- `RetryMenu.cs` – Shown after jumpscare  
- `PauseMenu.cs` – Shown when ESC is pressed and hidden when pressed again  
- `MainMenu.cs` – Main screen at the start of game  
- `PadlockScript.cs` – Heap-based padlock puzzle  
- `LeverPuzzle.cs` – Creates random tree, chooses random traversal, spawns clue nodes  
- `Lever.cs` – Individual lever behaviour  
- `KeyPickup.cs` – Manages key interaction  
- `JumpscareController.cs` – Manages jumpscare behavior  
- `IInteractable.cs` – Interface used by all interactables  
- `DoorScript.cs` – Controls doors  
- `CabinetScript.cs` – Controls hiding spaces  
- `PlayerController.cs` – Player controls  
- `EnemyAI.cs` – Enemy behavior  
- `FinalDoorController.cs` – Final door logic  
- `FinalDoorLock.cs` – Each lock consumes a key  

**High-level responsibilities:**  
- **PlayerController:** input, movement, camera, interactions, footsteps + noise notification  
- **EnemyAI:** patrol/investigate/chase, audio handling, jumpscare trigger  
- **PadlockScript:** generates max-heap puzzle, performs heap insertions, determines password from leaf nodes  
- **LeverPuzzle & Lever:** generate small fixed binary tree, compute chosen traversal order (pre/in/post), compare player input  
- **FinalDoorController & FinalDoorLock & KeyPickup:** key collection and lock unlocking for final escape  
- **UI and menus:** MainMenu, PauseMenu, RetryMenu — manage scenes, timeScale, and cursor  

---

## 3. Data Structures Used

### 3.1 List / Array
- Used for collections of objects (dials, levers, waypoints, audio clips)  
- Example: `List<GameObject> rullers` in `PadlockScript`  
- Random access O(1), insertion/removal O(n)  
- Unity-friendly for editor serialization  

### 3.2 HashSet
- Ensures unique digits in heap puzzle  
- O(1) average insert/membership checks  

### 3.3 Heap (Max-Heap implemented on a List)
- Core to the padlock puzzle  
- Leaf nodes determine 4-digit password  
- Heap insert: O(log n), building heap: O(n log n)  

### 3.4 Binary Tree
- Used in LeverPuzzle (≤ 6 nodes)  
- Random traversal type (preorder/inorder/postorder) determines lever flipping order  
- Traversal time: O(n)  

### 3.5 Dictionary<int, Color / string>
- Maps lever IDs to colors and names for clue nodes  
- O(1) lookup  

---

## 4. Puzzle Design

### 4.1 Padlock Puzzle (Heap)
- Generate 7 unique digits  
- Insert values into max-heap using `HeapInsert`  
- Leaf nodes form 4-digit code  
- Player rotates 4 dials to match password  

### 4.2 Lever Puzzle (Tree Traversal)
- Small binary tree generated from lever IDs  
- Random traversal selected  
- Clues spawned as colored nodes with traversal type  
- Player flips levers in correct order to solve puzzle  

**Educational Purpose:**  
- Demonstrates heap insertions and tree traversals interactively  

---

## 5. Algorithms & Complexity
- **Heap insertion:** O(log n) per insert, O(n log n) for repeated inserts  
- **Tree traversal:** O(n) time, O(n) stack for recursion  
- **Raycasts for interactions:** O(1) per frame  
- **Enemy patrol selection:** O(1)  

> Note: n is small (≤ 7), so performance is trivial  

---

## 6. Implementation Notes
- `IInteractable` ensures consistent interaction interface  
- `PlayerController.TryInteract()` handles raycast detection and camera focus  
- `PadlockScript` updates dials using keyboard input, checks password with `SequenceEqual`  
- `LeverPuzzle` spawns visual clue nodes with `LineRenderer` and displays traversal type  
- `EnemyAI` triggers jumpscare on contact  

---

## 7. Testing & Limitations
- Random puzzles use RNG; set seed for deterministic testing  
- Animator clip queries assume clips exist  
- Camera focus interactions must restore original transform  
- Some puzzles require mental simulation; consider optional hints  

---

## 8. Screenshots

*(Add images here as Markdown with `![alt-text](path)` or GitHub upload)*

**Enemy:** The moss monster (Rake)  
**Puzzles:** Padlock and lever puzzles  
**Gameplay:** Exploration and interactions  

---

## 9. Controls

| Action | Key |
|--------|-----|
| Move | W / A / S / D |
| Look Around | Mouse |
| Interact | E |
| Pause / Exit Interaction | Esc |
| Toggle Flashlight | F |
| Sprint / Run | Left Shift |

---

## 10. Credits & Disclaimer

- **Models, textures, and assets**: Most 3D models and textures used in this project were sourced from **Sketchfab** or other free/educational sources.  
- **Unity**: The game was developed using Unity Engine.  
- **Blender**: The map was created using Blender.  

> **Disclaimer:** I do **not** claim ownership of any third-party models, textures, or assets used in this project. All such assets are used solely for educational purposes as part of this course project.


