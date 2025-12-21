using UnityEngine;
using UnityEngine.XR; // 입력 감지용
using UnityEngine.XR.Interaction.Toolkit;

public class CameraMode : MonoBehaviour
{
    [Header("시스템 연결")]
    public MonologueSystem monologueSystem;
    public GameObject viewFinderCanvas; // 1단계에서 만든 눈앞의 캔버스

    [Header("잠금 해제할 문서")]
    public BoxCollider documentCollider; //여기에 문서(종이)를 연결하세요!

    [Header("대사 설정")]
    [TextArea]
    public string startMessage = "좋아 고장은 나지 않았어.";

    private bool isHovered = false;
    private bool wasPressed = false;
    private bool isCameraModeOn = false; // 현재 카메라 모드인지?

    // XR Simple Interactable - Hover 이벤트 연결
    public void SetHoverState(bool state)
    {
        isHovered = state;
    }

    void Update()
    {
        // 1. 카메라 모드가 켜져있지 않고 + 쳐다보고 있을 때만 입력 감지
        if (!isCameraModeOn && isHovered)
        {
            CheckInput();
        }
    }

    void CheckInput()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool isPressed = false;

        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressedValue))
        {
            isPressed = pressedValue;
        }

        // A버튼 클릭 시
        if (isPressed && !wasPressed)
        {
            ActivateCameraMode();
        }

        wasPressed = isPressed;
    }

    void ActivateCameraMode()
    {
        isCameraModeOn = true;

        // 1. 대사 출력
        if (monologueSystem != null)
        {
            monologueSystem.ShowMonologue(startMessage);
        }

        // 2. 뷰파인더 UI 켜기 (화면 전환 연출)
        if (viewFinderCanvas != null)
        {
            viewFinderCanvas.SetActive(true);
        }

        // 3. 문서 상호작용 잠금 해제 
        if (documentCollider != null)
        {
            documentCollider.enabled = true;
        }

        // 4. 바닥에 있는 카메라 모델 숨기기
        gameObject.SetActive(false);

        Debug.Log("카메라 모드 시작");
    }
}