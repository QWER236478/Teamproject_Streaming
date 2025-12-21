using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class BossStartTriggerVR : MonoBehaviour
{
    [Header("플레이어 식별")]
    public string playerTag = "Player";

    [Header("UI 연결")]
    public GameObject subtitlePanel;    // 검은색 배경 (Image)
    public TMP_Text subtitleText;       // 글자 나오는 곳 (TextMeshPro)

    [Header("대사 설정")]
    [TextArea] public string message = "아까 이런 통로는 없었던 것 같은데... 확인해봐야겠어.";
    public float displayDuration = 4.0f; // 자막이 떠있는 시간(초)

    private bool used = false;          // 중복 실행 방지

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        // 시작할 때 UI 꺼두기 (안전장치)
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (subtitleText != null) subtitleText.text = "";
    }

    void OnTriggerEnter(Collider other)
    {
        // 이미 사용했거나, 플레이어가 아니면 무시
        if (used || !other.CompareTag(playerTag)) return;

        used = true;
        StartCoroutine(ShowSubtitleRoutine());
    }

    IEnumerator ShowSubtitleRoutine()
    {
        // 1. UI 켜기
        if (subtitlePanel != null) subtitlePanel.SetActive(true);

        // 2. 텍스트 바로 표시 (타자기 효과 없음)
        if (subtitleText != null) subtitleText.text = message;

        // 3. 설정한 시간만큼 대기
        yield return new WaitForSeconds(displayDuration);

        // 4. UI 끄기
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (subtitleText != null) subtitleText.text = "";

        // 5. 트리거 비활성화 (다시 실행 안 되게)
        GetComponent<Collider>().enabled = false;
    }
}