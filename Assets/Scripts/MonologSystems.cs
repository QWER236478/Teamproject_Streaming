using UnityEngine;
using TMPro;
using System.Collections;

public class MonologueSystem : MonoBehaviour
{
    [Header("설정")]
    public float displayTime = 2.0f; // <-- 여기서 시간을 마음대로 바꿀 수 있습니다 (기본 2초)

    [Header("연결할 것들")]
    public GameObject subtitlePanel; // 검은색 배경 바
    public TextMeshProUGUI subtitleText; // 자막 텍스트

    void Start()
    {
        // 시작할 때 패널과 텍스트 숨기기
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        subtitleText.text = "";
    }

    public void ShowMonologue(string text)
    {
        StopAllCoroutines(); // 기존 대사 취소

        // 패널 켜기
        if (subtitlePanel != null) subtitlePanel.SetActive(true);

        StartCoroutine(DisplayMonologue(text));
    }

    IEnumerator DisplayMonologue(string text)
    {
        subtitleText.text = text;

        // 설정한 시간만큼 대기 (Inspector에서 조절 가능)
        yield return new WaitForSeconds(displayTime);

        // 끄기
        subtitleText.text = "";
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
    }
}