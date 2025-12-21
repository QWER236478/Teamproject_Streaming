using UnityEngine;

public class TVwomanStart : MonoBehaviour
{
    [Header("트리거 밟으면 켜질 대상")]
    // [수정 1] MonoBehaviour 대신 구체적인 스크립트 이름을 씁니다.
    public HideAndSeekManager targetScript;

    public bool playOnce = true;
    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 태그 확인
        if (!other.CompareTag("Player")) return;

        // 한 번만 실행 옵션 체크
        if (playOnce && hasPlayed) return;

        if (targetScript != null)
        {
            // [수정 2] 단순히 스크립트를 켜는 게 아니라, 깨우는 함수를 호출합니다.
            targetScript.WakeUp();

            hasPlayed = true;
            Debug.Log("트리거 작동: 적이 깨어납니다!");
        }
    }
}