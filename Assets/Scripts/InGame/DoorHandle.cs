using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DoorHandle : MonoBehaviour, IInteractable
{
    [Header("문 제어")]
    public Animator doorAnimator;
    public Collider solidCollider;
    public Canvas hint;
    public string openTrigger = "Open";
    public string closeTrigger = "Close";
    public float openTime = 1f;
    public float closeTime = 1f;
    public float holdOpenTime = 2f;

    [Header("루프 연동")]
    public LoopManager loopManager;
    public string corridorID = "A";  // 이 문이 속한 복도 ID (A, B, C, D 등)

    bool isOpen;
    bool busy;

    public void Interact()
    {
        if (busy) return;
        if (!isOpen) StartCoroutine(OpenAndClose());
    }

    private IEnumerator OpenAndClose()
    {
        busy = true;
        isOpen = true;

        // 문 열기
        if (doorAnimator) doorAnimator.SetTrigger(openTrigger);
        yield return new WaitForSeconds(openTime * 0.3f);
        if (solidCollider) solidCollider.enabled = false;

        // 루프 진행 처리
        if (loopManager)
            loopManager.OnCorridorPassed(corridorID);

        // 열린 상태 유지
        yield return new WaitForSeconds(holdOpenTime);

        // 문 닫기
        if (doorAnimator) doorAnimator.SetTrigger(closeTrigger);
        yield return new WaitForSeconds(closeTime);
        if (solidCollider) solidCollider.enabled = true;

        isOpen = false;
        busy = false;
    }
}