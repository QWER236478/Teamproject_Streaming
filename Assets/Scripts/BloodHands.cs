using UnityEngine;

public class BloodHands : MonoBehaviour
{
    [Header("트리거가 밟히면 실행할 피 연출 스크립트")]
    public BloodStampAreaSpawnerWithSound spawner;

    public bool playOnce = true;   // 한 번만 실행할지
    bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어만 반응
        if (!other.CompareTag("Player"))
            return;

        if (playOnce && hasPlayed)
            return;

        if (spawner != null)
        {
            spawner.Begin();   // 여기서 손자국 + 사운드 연출 시작
            hasPlayed = true;
        }
    }
}