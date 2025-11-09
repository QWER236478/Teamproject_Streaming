using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class BossStartTrigger_TMP : MonoBehaviour
{
    [Header("플레이어 식별")]
    public string playerTag = "Player";

    [Header("UI (TextMeshPro)")]
    public Canvas cutsceneCanvas;          // 전체 패널 (처음엔 비활성 권장)
    public TMP_Text uiText;                //TMP_Text로 변경
    [TextArea] public string message = "아까 이런 통로는 없었던 것 같은데... 확인해봐야겠어.";
    public float charsPerSec = 35f;
    public bool waitForConfirm = true;
    public KeyCode confirmKey = KeyCode.Space;

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
        StartCoroutine(Sequence(other));
    }

    IEnumerator Sequence(Collider playerCol)
    {
        // 1) 플레이어 컨트롤 잠시 종료
        var controller = playerCol.GetComponentInParent<MonoBehaviour>();
        // 실제 사용하는 컨트롤러로 교체 가능 (예: PlayerControllerKeyMa)
        if (controller) controller.enabled = false;

        // 2) UI 활성화 + 타자기 효과
        if (cutsceneCanvas) cutsceneCanvas.enabled = true;
        yield return StartCoroutine(Typewriter(message));

        // 3) 확인 입력 대기
        if (waitForConfirm)
            yield return new WaitUntil(() => Input.GetKeyDown(confirmKey));

        // 4) UI 비활성화 + 컨트롤 복귀
        if (cutsceneCanvas) cutsceneCanvas.enabled = false;
        if (controller) controller.enabled = true;

        // 5) 재사용 방지
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