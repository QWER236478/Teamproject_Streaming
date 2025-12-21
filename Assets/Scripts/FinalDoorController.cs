using UnityEngine;
using UnityEngine.XR; // 입력 감지용

public class FinalDoorController : MonoBehaviour
{
    [Header("시스템 연결")]
    public MonologueSystem monologueSystem;

    [Header("열쇠 설정")]
    public GameObject targetKey;

    [Header("대사 설정")]
    [TextArea] public string lockedMessage = "열리지 않는다... 또 열쇠를 찾아야하는군.";
    [TextArea] public string openMessage = "문이 열렸다. 이 복도를 지나면 탈출 할 수 있는건가?";

    [Header("애니메이션")]
    public Animator doorAnimator;

    private bool isHovered = false; // 쳐다보고 있는지 체크
    private bool wasPressed = false; // 버튼 중복 눌림 방지

    // 1. XR Interaction Toolkit 이벤트에서 호출할 함수들
    public void SetHoverState(bool state)
    {
        isHovered = state;
    }

    void Update()
    {
        // 쳐다보고 있지 않으면 아무것도 안 함
        if (!isHovered) return;

        // 2. 오른쪽 컨트롤러의 'A' 버튼 감지 (Primary Button)
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool isPressed = false;

        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressedValue))
        {
            isPressed = pressedValue;
        }

        // 3. 버튼을 '딱' 눌렀을 때만 실행 (누르고 있는 동안 계속 실행 방지)
        if (isPressed && !wasPressed)
        {
            TryOpenDoor();
        }

        wasPressed = isPressed; // 상태 저장
    }

    void TryOpenDoor()
    {
        bool hasKey = (targetKey != null && !targetKey.activeSelf);

        if (hasKey)
        {
            monologueSystem.ShowMonologue(openMessage);
            if (doorAnimator) doorAnimator.SetTrigger("Open");

            // 문 열렸으면 더 이상 상호작용 끄기
            this.enabled = false;

            GetComponent<UnityEngine.XR.Interaction.Toolkit.XRSimpleInteractable>().enabled = false;

            this.enabled = false;
        }
        else
        {
            monologueSystem.ShowMonologue(lockedMessage);
        }
    }
}