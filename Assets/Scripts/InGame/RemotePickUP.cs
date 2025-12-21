using UnityEngine;
using UnityEngine.XR; // VR 입력
using UnityEngine.XR.Interaction.Toolkit; // XR 상호작용

// 이 스크립트를 넣으면 XR Simple Interactable이 자동으로 붙습니다.
[RequireComponent(typeof(XRSimpleInteractable))]
public class RemotePickUP : MonoBehaviour
{
    [Header("자막 시스템 연결")]
    public MonologueSystem monologueSystem;
    [TextArea] public string pickupMessage = "리모컨을 획득했다.";

    [Header("UI 설정 (선택사항)")]
    public GameObject pickupUI; // 리모컨 위에 띄울 "줍기 (A)" 안내 UI

    // 내부 변수
    private bool isHovered = false;
    private bool wasPressed = false;

    void Start()
    {
        // 시작할 때 줍기 안내 UI는 꺼둠
        if (pickupUI != null) pickupUI.SetActive(false);
    }

    // =============================================================
    // 1. XR Interaction Toolkit 이벤트 연결용 함수
    // (인스펙터의 Hover Entered / Exited에 등록하세요)
    // =============================================================
    public void ShowUI()
    {
        isHovered = true;
        if (pickupUI != null) pickupUI.SetActive(true);
    }

    public void HideUI()
    {
        isHovered = false;
        if (pickupUI != null) pickupUI.SetActive(false);
    }

    // =============================================================
    // 2. 입력 감지 (A버튼)
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

        // 버튼을 '딱' 눌렀을 때 실행 (꾹 누르고 있어도 한 번만 실행)
        if (isPressed && !wasPressed)
        {
            Pickup();
        }
        wasPressed = isPressed; // 상태 저장
    }

    void Pickup()
    {
        // 1. UI 숨기기
        if (pickupUI != null) pickupUI.SetActive(false);

        // 2. 자막 띄우기
        if (monologueSystem != null)
        {
            monologueSystem.ShowMonologue(pickupMessage);
        }

        Debug.Log("리모컨 획득 완료");

        // 3. 리모컨 오브젝트 끄기 (획득 처리)
        // -> 이러면 HideAndSeekManager가 리모컨이 사라진 걸 감지합니다.
        gameObject.SetActive(false);
    }
}