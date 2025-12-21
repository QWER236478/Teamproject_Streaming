using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class DocumentClick : MonoBehaviour
{
    [Header("시스템 연결")]
    public MonologueSystem monologueSystem;

    // 새로 추가된 이미지 슬롯
    [Header("이미지 설정")]
    public GameObject documentImage; // 띄울 이미지 UI 오브젝트

    [Header("대사 설정")]
    [TextArea]
    public string message = "누가 이런 자료를 넣어놨지? 다시 돌아와서 봐야겠어.";

    private bool isHovered = false;
    private bool wasPressed = false;

    // 현재 상태 관리 (0: 시작전, 1: 이미지 켜짐, 2: 대사 출력됨)
    private int currentState = 0;

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

        // 버튼을 '딱' 눌렀을 때 실행
        if (isPressed && !wasPressed)
        {
            HandleInteraction();
        }

        wasPressed = isPressed;
    }

    void HandleInteraction()
    {
        if (currentState == 0)
        {
            // [1단계] 이미지 띄우기
            if (documentImage != null)
            {
                documentImage.SetActive(true);
                Debug.Log("문서 이미지 활성화");
            }
            currentState = 1;
        }
        else if (currentState == 1)
        {
            // [2단계] 이미지 끄고 대사 치기
            if (documentImage != null)
            {
                documentImage.SetActive(false);
            }

            if (monologueSystem != null)
            {
                monologueSystem.ShowMonologue(message);
            }

            Debug.Log("이미지 비활성화 및 대사 출력");
            currentState = 2; // 이후에는 클릭해도 반응 없거나 다시 0으로 돌려 반복 가능
        }
    }
}