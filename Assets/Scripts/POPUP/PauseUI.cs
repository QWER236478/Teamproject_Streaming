using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseUI : MonoBehaviour
{
    [Header("일시정지 팝업")]
    public GameObject pauseMenuPanel;

    [Header("입력 액션(왼손 메뉴 버튼)")]
    public InputActionReference pauseActionRef;   // ← 액션 레퍼런스로 변경

    private InputAction pauseAction;
    private bool isPaused = false;

    void OnEnable()
    {
        if (pauseActionRef != null)
        {
            pauseAction = pauseActionRef.action;
            pauseAction.Enable();
        }
    }

    void OnDisable()
    {
        if (pauseAction != null)
            pauseAction.Disable();
    }

    void Start()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    void Update()
    {
        // 왼손 메뉴 버튼 (Input System 액션)
        if (pauseAction != null && pauseAction.triggered)
        {
            TogglePause();
        }

        // 키보드 Q도 테스트용으로 남겨둠
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

    // UI 버튼용
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