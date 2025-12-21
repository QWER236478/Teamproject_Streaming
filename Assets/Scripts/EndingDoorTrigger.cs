using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR; // VR 입력
using UnityEngine.XR.Interaction.Toolkit; // XR 상호작용

// 이 스크립트를 넣으면 XR Simple Interactable이 자동으로 붙습니다.
[RequireComponent(typeof(XRSimpleInteractable))]
public class EndingDoorTrigger : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    public string endingSceneName = "EndingSceneDemo";

    // 내부 변수
    private bool isHovered = false; // 지금 쳐다보고 있는가?
    private bool wasPressed = false; // 버튼 중복 눌림 방지

    // =============================================================
    // 1. XR Interaction Toolkit 이벤트 연결용 함수
    // (인스펙터의 Hover Entered / Exited에 등록해서 isHovered를 켜고 끕니다)
    // =============================================================
    public void SetHoverState(bool state)
    {
        isHovered = state;
    }

    // =============================================================
    // 2. 입력 감지 (A버튼) - Update에서 직접 처리
    // =============================================================
    void Update()
    {
        // 쳐다보고 있지 않으면(Hover 상태 아니면) 아무것도 안 함
        if (!isHovered) return;

        // 오른쪽 컨트롤러 감지
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool isPressed = false;

        // A버튼(PrimaryButton) 눌림 확인
        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressedValue))
        {
            isPressed = pressedValue;
        }

        // 버튼을 '딱' 눌렀을 때 실행
        if (isPressed && !wasPressed)
        {
            GoToEnding();
        }
        wasPressed = isPressed; // 상태 저장
    }

    void GoToEnding()
    {
        Debug.Log("엔딩 씬으로 이동합니다.");
        SceneManager.LoadScene(endingSceneName);
    }
}