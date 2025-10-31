using System.Collections;
using UnityEngine;

public class PrologLock : MonoBehaviour
{
    [Header("타이핑 관리")]
    public PrologManager prologManager;     // 연결된 PrologManager
    public float delayAfterTyping = 0.5f;   // 텍스트 끝난 후 대기 시간

    [Header("플레이어 제어 스크립트")]
    public MonoBehaviour[] playerScripts;   // 이동, 시점, 상호작용 등
    public GameObject playerRoot;           // 플레이어 루트 (선택)

    [Header("UI 제어")]
    public GameObject prologUI;             // 꺼질 UI 루트
    public GameObject pressAnyKeyText;      // "아무 키나 누르세요" 안내 텍스트 (선택)

    private bool isLocked = false;
    private bool waitingForInput = false;

    private void Start()
    {
        if (prologManager == null)
        {
            Debug.LogWarning("[PrologLock] PrologManager가 연결되지 않았습니다.");
            return;
        }

        LockPlayer(true);
        StartCoroutine(WatchProlog());
    }

    IEnumerator WatchProlog()
    {
        // PrologManager의 타이핑이 끝날 때까지 대기
        while (prologManager != null && prologManager.IsTyping())
            yield return null;

        // 살짝 지연 후 "아무 키나 누르세요" 상태 진입
        yield return new WaitForSecondsRealtime(delayAfterTyping);

        waitingForInput = true;
        if (pressAnyKeyText) pressAnyKeyText.SetActive(true);

        // 입력 기다림
        yield return StartCoroutine(WaitForAnyKey());

        // 입력이 들어오면 UI 끄기 + 잠금 해제
        if (pressAnyKeyText) pressAnyKeyText.SetActive(false);
        if (prologUI != null) prologUI.SetActive(false);
        LockPlayer(false);
    }

    IEnumerator WaitForAnyKey()
    {
        // 타임스케일 0이어도 동작하도록 Realtime 루프
        while (true)
        {
            if (Input.anyKeyDown)
                yield break;
            yield return null;
        }
    }

    public void LockPlayer(bool locked)
    {
        isLocked = locked;

        // 플레이어 스크립트 비활성화
        if (playerScripts != null)
        {
            foreach (var script in playerScripts)
            {
                if (script != null)
                    script.enabled = !locked;
            }
        }

        // 전체 루트 비활성화 (선택)
        if (playerRoot != null)
            playerRoot.SetActive(!locked);

        // 커서 상태
        Cursor.visible = locked;
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
    }
}