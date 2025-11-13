using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class BossStartTrigger : MonoBehaviour
{
    [Header("플레이어 식별")]
    public string playerTag = "Player";

    [Header("UI (TextMeshPro)")]
    public Canvas cutsceneCanvas;          // 전체 패널
    public TMP_Text uiText;
    [TextArea] public string message = "아까 이런 통로는 없었던 것 같은데... 확인해봐야겠어.";
    public float charsPerSec = 35f;
    public bool waitForConfirm = true;
    public KeyCode confirmKey = KeyCode.Space;

    [Header("플레이어 잠금 제어 (선택)")]
    public PrologLock prologLock;          // 있으면 이걸로 잠금/해제 호출
    public MonoBehaviour[] playerScripts;  // 혹시 PrologLock 안 쓸 경우용

    [Header("재사용 방지")]
    public bool oneShot = true;

    bool used = false;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Awake()
    {
        if (cutsceneCanvas) cutsceneCanvas.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (oneShot && used) return;

        used = true;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // 1) 플레이어 잠금
        if (prologLock != null)
        {
            prologLock.LockPlayer(true);
        }
        else
        {
            // PrologLock 안 쓰고 이 스크립트에서 직접 잠그고 싶다면
            foreach (var s in playerScripts)
                if (s) s.enabled = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // 2) UI 켜고 타자기
        if (cutsceneCanvas) cutsceneCanvas.enabled = true;
        yield return StartCoroutine(Typewriter(message));

        // 3) 확인 입력 기다리기
        if (waitForConfirm)
            yield return new WaitUntil(() => Input.GetKeyDown(confirmKey));

        // 4) UI 끄기
        if (cutsceneCanvas) cutsceneCanvas.enabled = false;

        // 5) 플레이어 잠금 해제
        if (prologLock != null)
        {
            prologLock.LockPlayer(false);
        }
        else
        {
            foreach (var s in playerScripts)
                if (s) s.enabled = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // 6) 트리거 콜라이더 끄기
        if (oneShot)
            GetComponent<Collider>().enabled = false;
    }

    IEnumerator Typewriter(string text)
    {
        if (!uiText) yield break;

        uiText.text = "";
        float delay = 1f / Mathf.Max(1f, charsPerSec);

        for (int i = 0; i < text.Length; i++)
        {
            uiText.text = text.Substring(0, i + 1);
            yield return new WaitForSecondsRealtime(delay);
        }
    }
}