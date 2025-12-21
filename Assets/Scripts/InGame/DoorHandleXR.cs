using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRSimpleInteractable))]
public class LoopDoorInteraction : MonoBehaviour
{
    [Header("1. 시스템 연결")]
    public MonologueSystem monologueSystem;
    public LoopManager loopManager;
    public GameObject hintCanvas; // 상호작용 가능 표시 UI

    [Header("2. 설정")]
    public string corridorID = "A"; // 루프 시스템에 전달할 ID
    [TextArea] public string openMessage = ""; // 문 열 때 나올 대사

    [Header("3. 문 제어")]
    public Animator doorAnimator;
    public Collider solidCollider; // 문이 닫혀있을 때 못 지나가게 막는 콜라이더

    [Header("4. 애니메이션 및 시간 설정")]
    public string openTrigger = "Open";
    public string closeTrigger = "Close";
    [Tooltip("문이 열리는 애니메이션 재생 시간")]
    public float openAnimDuration = 1.0f;
    [Tooltip("문이 열린 채로 유지되는 시간")]
    public float stayOpenDuration = 2.0f;

    // 내부 상태 변수
    private bool isHovered = false;
    private bool wasPressed = false;
    private bool isBusy = false; // 현재 문이 작동 중인지 여부

    // XR Simple Interactable 이벤트 연결
    public void SetHoverState(bool state)
    {
        isHovered = state;

        // 문이 작동 중이 아닐 때만 힌트 캔버스 표시/숨김
        if (hintCanvas != null && !isBusy)
        {
            hintCanvas.SetActive(state);
        }
    }

    void Update()
    {
        // 호버 중이 아니거나, 이미 문이 작동 중이면 입력 무시
        if (!isHovered || isBusy) return;

        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool isPressed = false;

        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressedValue))
        {
            isPressed = pressedValue;
        }

        // 버튼을 막 눌렀을 때 실행
        if (isPressed && !wasPressed)
        {
            StartCoroutine(ProcessDoorSequence());
        }

        wasPressed = isPressed;
    }

    // 문 열림 -> 대기 -> 닫힘 시퀀스 처리
    private IEnumerator ProcessDoorSequence()
    {
        isBusy = true; // 중복 실행 방지

        // 힌트 캔버스 숨기기
        if (hintCanvas != null) hintCanvas.SetActive(false);

        // 1. 대사 출력
        if (monologueSystem != null && !string.IsNullOrEmpty(openMessage))
        {
            monologueSystem.ShowMonologue(openMessage);
        }

        // 2. 문 열기 애니메이션
        if (doorAnimator != null) doorAnimator.SetTrigger(openTrigger);

        // 3. 문이 살짝 열릴 때까지 대기 후 통과 가능하게 설정 (애니메이션 시간의 30% 지점)
        yield return new WaitForSeconds(openAnimDuration * 0.3f);
        if (solidCollider != null) solidCollider.enabled = false;

        // 4. 루프 매니저에 통과 신호 전달
        if (loopManager != null) loopManager.OnCorridorPassed(corridorID);

        // 5. 문이 완전히 열리고 유지되는 시간만큼 대기
        // (남은 애니메이션 시간 + 유지 시간)
        float remainingAnimTime = openAnimDuration * 0.7f;
        yield return new WaitForSeconds(remainingAnimTime + stayOpenDuration);

        // 6. 문 닫기 애니메이션
        if (doorAnimator != null) doorAnimator.SetTrigger(closeTrigger);

        // 7. 문이 닫히는 동안 대기
        yield return new WaitForSeconds(openAnimDuration);

        // 8. 다시 통과 불가능하게 막기
        if (solidCollider != null) solidCollider.enabled = true;

        isBusy = false; // 작동 완료

        // 여전히 바라보고 있다면 힌트 다시 켜기
        if (isHovered && hintCanvas != null) hintCanvas.SetActive(true);
    }
}