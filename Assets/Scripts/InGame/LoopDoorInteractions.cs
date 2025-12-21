using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

// 이 컴포넌트를 넣으면 XR Simple Interactable도 같이 생깁니다.
[RequireComponent(typeof(XRSimpleInteractable))]
public class LoopDoorInteractions : MonoBehaviour
{
    [Header("1. 애니메이션 설정")]
    public Animator doorAnimator;       // 움직일 문(부모)의 애니메이터
    public string openTrigger = "Open";   // 문 열기 파라미터 이름
    public string closeTrigger = "Close"; // 문 닫기 파라미터 이름

    [Header("2. 시간 설정")]
    public float stayOpenTime = 3.0f;     // 문이 열려있는 시간
    public float closeAnimTime = 1.0f;    // 문이 닫히는 애니메이션 시간 (안전 대기용)

    // 내부 상태 변수
    private bool isHovered = false;
    private bool wasPressed = false;
    private bool isBusy = false; // 문이 움직이는 중인지 확인

    // XR 이벤트 연결용 함수
    public void SetHoverState(bool state)
    {
        isHovered = state;
    }

    void Update()
    {
        // 손이 닿지 않았거나, 이미 문이 작동 중이면 입력 무시
        if (!isHovered || isBusy) return;

        // 오른손 컨트롤러 입력 감지
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        // A버튼 (PrimaryButton) 상태 확인
        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed))
        {
            // 버튼을 막 누른 순간 (Press Down) 실행
            if (isPressed && !wasPressed)
            {
                StartCoroutine(OperateDoor());
            }
            wasPressed = isPressed;
        }
    }

    // 문 작동 코루틴
    IEnumerator OperateDoor()
    {
        isBusy = true; // 중복 실행 방지

        // 1. 문 열기 (Open 트리거 발동)
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(openTrigger);
        }

        // 2. 문이 열려있는 시간만큼 대기
        yield return new WaitForSeconds(stayOpenTime);

        // 3. 문 닫기 (Close 트리거 발동)
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(closeTrigger);
        }

        // 4. 문이 완전히 닫힐 때까지 대기 (다시 열기 방지)
        yield return new WaitForSeconds(closeAnimTime);

        isBusy = false; // 작동 완료, 다시 누를 수 있음
    }
}