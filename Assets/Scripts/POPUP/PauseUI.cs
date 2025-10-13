using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class PauseUI : MonoBehaviour
{
    [Header("일시정지 팝업")]
    public GameObject pauseMenuPanel;

    [Header("입력 액션(왼손 메뉴 버튼)")]
    public InputAction pauseAction; 

    private bool isPaused = false;

    void OnEnable() { pauseAction.Enable(); }
    void OnDisable() { pauseAction.Disable(); }

    void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    void Update()
    {
        // ▶ 왼손 메뉴 버튼(≡) OR 키보드 Z 로 토글
        if (pauseAction.triggered)
        {
            TogglePause();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
            pauseMenuPanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            pauseMenuPanel.SetActive(false);
        }
    }

    // UI 버튼
    public void OnResume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        pauseMenuPanel.SetActive(false);
    }

    public void OnOption()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void OnRestart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void OnMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartMenu");
    }
}