using UnityEngine;
using UnityEngine.XR;

public class ItemPickup : MonoBehaviour
{
    [Header("자막 시스템 연결 (필수)")]
    public MonologueSystem monologueSystem; // 하단 자막 띄우기용
    public string pickupMessage = "감옥 열쇠를 획득했다."; // 줍고 나서 띄울 자막

    [Header("설정")]
    public string itemName = "감옥 열쇠 (A)";
    public GameObject pickupUI; // 열쇠 위에 뜨는 "줍는다 (A)" 팝업

    private bool isHovered = false;
    private bool wasPressed = false; // 중복 눌림 방지용

    void Start()
    {
        if (pickupUI != null) pickupUI.SetActive(false);
    }

    // XR Simple Interactable - Hover Entered에 연결
    public void ShowUI()
    {
        isHovered = true;
        if (pickupUI != null) pickupUI.SetActive(true);
    }

    // XR Simple Interactable - Hover Exited에 연결
    public void HideUI()
    {
        isHovered = false;
        if (pickupUI != null) pickupUI.SetActive(false);
    }

    void Update()
    {
        if (isHovered)
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            bool isPressed = false;

            if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressedValue))
            {
                isPressed = pressedValue;
            }

            // 버튼을 딱 눌렀을 때 (중복 실행 방지)
            if (isPressed && !wasPressed)
            {
                Pickup();
            }
            wasPressed = isPressed;
        }
    }

    void Pickup()
    {
        // 1. "줍는다(A)" 팝업 숨기기
        if (pickupUI != null) pickupUI.SetActive(false);

        // 2. 하단 자막에 "획득했다" 메시지 띄우기 (추가된 부분!)
        if (monologueSystem != null)
        {
            monologueSystem.ShowMonologue(pickupMessage);
        }

        Debug.Log($"{itemName} 획득!");

        // 3. 열쇠 오브젝트 비활성화 (인벤토리에 들어간 척)
        gameObject.SetActive(false);
    }
}