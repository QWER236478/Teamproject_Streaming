using UnityEngine;
using UnityEngine.XR; // 입력 감지용
using UnityEngine.XR.Interaction.Toolkit; // 상호작용 비활성화용
using System.Collections; // 시간 지연(Coroutine) 사용

public class CabinetInteraction : MonoBehaviour
{
    [Header("시스템 연결")]
    public MonologueSystem monologueSystem;

    [Header("대사 설정")]
    [TextArea]
    public string openMessage = "잠겨있지 않네. 안을 확인해보자.";

    [Header("애니메이션 설정")]
    public Animator doorAnimator;
    public string animTriggerName = "Open";
    public float animationDuration = 1.5f;  // 문이 열리는 데 걸리는 시간

    [Header("내부 아이템 (동시 잠금 해제)")]
    public BoxCollider itemCollider1; // 첫 번째 물건 (예: 노란 상자)
    public BoxCollider itemCollider2; // 두 번째 물건 (예: 빨간 상자)

    private bool isHovered = false;
    private bool wasPressed = false;

    // 시작하자마자 내부 물건들의 콜라이더를 끕니다.
    void Start()
    {
        if (itemCollider1 != null) itemCollider1.enabled = false;
        if (itemCollider2 != null) itemCollider2.enabled = false;
    }

    // XR Simple Interactable - Hover 이벤트 연결
    public void SetHoverState(bool state)
    {
        isHovered = state;
    }

    void Update()
    {
        if (!isHovered) return;

        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool isPressed = false;

        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressedValue))
        {
            isPressed = pressedValue;
        }

        if (isPressed && !wasPressed)
        {
            OpenCabinet();
        }

        wasPressed = isPressed;
    }

    void OpenCabinet()
    {
        // 1. 대사 출력
        if (monologueSystem != null)
        {
            monologueSystem.ShowMonologue(openMessage);
        }

        // 2. 애니메이션 실행
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(animTriggerName);
        }

        // 3. 문 열리는 시간만큼 기다렸다가 물건들 켜기
        StartCoroutine(EnableItems());

        // 4. 상호작용 비활성화 (문은 이제 볼일 없음)
        this.enabled = false;

        var interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.enabled = false;
        }
    }

    // 애니메이션 시간 뒤에 실행될 함수
    IEnumerator EnableItems()
    {
        yield return new WaitForSeconds(animationDuration);

        // 두 오브젝트 박스 콜라이더 켜기
        if (itemCollider1 != null) itemCollider1.enabled = true;
        if (itemCollider2 != null) itemCollider2.enabled = true;
    }
}