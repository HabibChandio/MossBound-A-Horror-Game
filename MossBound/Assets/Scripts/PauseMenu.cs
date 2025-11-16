using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseUI;
    public Slider sensitivitySlider;
    public PlayerController player;

    private bool isPaused = false;

    private void Start()
    {
        sensitivitySlider.value = player.lookSpeed;
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        HideMenu();
    }

    public void TogglePause()
    {
        if (!isPaused) Pause(); else Unpause();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        player.isMovementLocked = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ShowMenu();
    }

    public void Unpause()
    {
        isPaused = false;
        Time.timeScale = 1f;
        player.isMovementLocked = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        HideMenu();
    }

    private void ShowMenu() => pauseUI.SetActive(true);
    private void HideMenu() => pauseUI.SetActive(false);

    public void OnSensitivityChanged(float value) => player.lookSpeed = value;

    public void OnContinueButton() => Unpause();

    public void OnMainMenuButton()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
