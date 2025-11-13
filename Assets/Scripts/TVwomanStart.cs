using UnityEngine;

public class TVwomanStart : MonoBehaviour
{
    [Header("트리거 밟으면 켜질 스크립트")]
    public MonoBehaviour targetScript; // HideAndSeekManager 같은 스크립트

    public bool playOnce = true;
    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (playOnce && hasPlayed) return;

        if (targetScript != null)
        {
            targetScript.enabled = true;   // 스크립트 켜기
            hasPlayed = true;
        }
    }
}