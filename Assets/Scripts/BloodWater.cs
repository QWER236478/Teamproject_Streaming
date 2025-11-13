using UnityEngine;

public class BloodWater : MonoBehaviour
{
    [Header("트리거가 밟혔을 때 실행할 타겟")]
    public Animator targetAnimator;   // 빨간색 오브젝트의 Animator
    public string triggerName = "BloodWater";

    private bool hasPlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasPlayed) return;
        if (!other.CompareTag("Player")) return;

        targetAnimator.SetTrigger(triggerName);
        hasPlayed = true;
    }
}